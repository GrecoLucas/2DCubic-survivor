using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using CubeSurvivor.Inventory.Components;
using CubeSurvivor.Inventory.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema que processa input do teclado e mouse para entidades controláveis pelo jogador
    /// </summary>
    public sealed class PlayerInputSystem : GameSystem
    {
        // Constantes para movimento e tiro
        private const float BulletSpawnOffset = 30f; // Fallback if no gun visual

        private readonly IBulletFactory _bulletFactory;
        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;
        private Matrix? _cameraTransform;

        public PlayerInputSystem(IBulletFactory bulletFactory)
        {
            _bulletFactory = bulletFactory;
        }

        public void SetScreenSize(int width, int height)
        {
            // Mantido para compatibilidade, mas usando readonly fields internos
        }

        public void SetCameraTransform(Matrix? transform)
        {
            _cameraTransform = transform;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            // Coletar entidades em uma lista para evitar modificação durante iteração
            var playerEntities = World.GetEntitiesWithComponent<PlayerInputComponent>().ToList();

            foreach (var entity in playerEntities)
            {
                var input = entity.GetComponent<PlayerInputComponent>();
                var velocity = entity.GetComponent<VelocityComponent>();
                var transform = entity.GetComponent<TransformComponent>();
                var xp = entity.GetComponent<XpComponent>();

                if (input == null || velocity == null || transform == null || !input.Enabled)
                    continue;

                // Detectar tecla P para abrir menu de upgrade se houver nível pendente
                if (xp != null && xp.HasPendingLevelUp && 
                    keyboardState.IsKeyDown(Keys.P) && 
                    _previousKeyboardState.IsKeyUp(Keys.P))
                {
                    // Verificar se já não tem um upgrade request ativo
                    if (!entity.HasComponent<UpgradeRequestComponent>())
                    {
                        entity.AddComponent(new UpgradeRequestComponent());
                        xp.HasPendingLevelUp = false; // Consumir o flag
                        System.Console.WriteLine("[PlayerInput] Menu de upgrade aberto (tecla P)");
                    }
                }

                // Calcular direção baseada em WASD
                var direction = Vector2.Zero;

                if (keyboardState.IsKeyDown(Keys.W))
                    direction.Y -= 1;
                if (keyboardState.IsKeyDown(Keys.S))
                    direction.Y += 1;
                if (keyboardState.IsKeyDown(Keys.A))
                    direction.X -= 1;
                if (keyboardState.IsKeyDown(Keys.D))
                    direction.X += 1;

                // Normalizar para movimento diagonal não ser mais rápido
                if (direction != Vector2.Zero)
                    direction.Normalize();

                velocity.Velocity = direction * velocity.Speed;

                // Calcular posição do mouse no mundo
                Vector2 mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);
                Vector2 mouseWorldPos = ScreenToWorld(mouseScreenPos);

                // Calcular direção do mouse em relação ao player
                Vector2 aimDirection = mouseWorldPos - transform.Position;
                if (aimDirection != Vector2.Zero)
                {
                    aimDirection.Normalize();
                    // Calcular rotação em radianos (Math.Atan2 retorna ângulo em radianos)
                    transform.Rotation = (float)System.Math.Atan2(aimDirection.Y, aimDirection.X);
                }

                // Atualizar cooldown de tiro
                if (input.ShootCooldown > 0)
                {
                    input.ShootCooldown -= deltaTime;
                }

                // Verificar se tem arma equipada antes de atirar
                var heldItemComp = entity.GetComponent<HeldItemComponent>();
                bool hasGunEquipped = heldItemComp != null && heldItemComp.CurrentItem is GunItem;
                
                // Ao segurar o botão esquerdo, continua atirando respeitando o cooldown
                if (hasGunEquipped && mouseState.LeftButton == ButtonState.Pressed && input.ShootCooldown <= 0)
                {
                    var gun = heldItemComp.CurrentItem as GunItem;

                    // Criar projéteis (suporta múltiplas balas por tiro)
                    Vector2 aim = mouseWorldPos - transform.Position;
                    if (aim != Vector2.Zero)
                    {
                        aim.Normalize();

                        int bulletsToFire = 1 + input.ExtraBullets;
                        float spreadDeg = 10f; // spread total em graus
                        float spreadRad = MathHelper.ToRadians(spreadDeg);

                        // Ângulo central
                        float baseAngle = (float)System.Math.Atan2(aim.Y, aim.X);

                        for (int i = 0; i < bulletsToFire; i++)
                        {
                            // distribuir os ângulos ao redor do centro
                            float offset = 0f;
                            if (bulletsToFire > 1)
                            {
                                float step = spreadRad / (bulletsToFire - 1);
                                offset = -spreadRad / 2f + step * i;
                            }

                            float angle = baseAngle + offset;
                            Vector2 dir = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));
                            
                            // Spawn bullets from gun muzzle if available, otherwise fallback to player center
                            Vector2 bulletStartPos;
                            var gunVisual = FindGunVisual(entity);
                            
                            if (gunVisual != null)
                            {
                                var gunT = gunVisual.GetComponent<TransformComponent>();
                                var muzzle = gunVisual.GetComponent<GunMuzzleComponent>();
                                
                                // Transform muzzle local offset to world space
                                Vector2 muzzleWorldOffset =
                                    Vector2.Transform(muzzle.LocalMuzzleOffset, Matrix.CreateRotationZ(gunT.Rotation));
                                
                                bulletStartPos = gunT.Position + muzzleWorldOffset;
                            }
                            else
                            {
                                // Fallback: spawn from player center with offset
                                bulletStartPos = transform.Position + dir * BulletSpawnOffset;
                            }

                            float speed = gun.BulletSpeed;
                            float damage = gun.Damage;
                            float size = input.BulletSize;
                            _bulletFactory.CreateBullet(World, bulletStartPos, dir, speed, damage, size);
                        }

                        input.ShootCooldown = gun.ShootCooldown;
                    }
                }

                // Input de construção (Right-click quando tem hammer equipado)
                bool buildClick = mouseState.RightButton == ButtonState.Pressed &&
                                 _previousMouseState.RightButton == ButtonState.Released;

                if (buildClick)
                {
                    var builder = entity.GetComponent<BuilderComponent>();
                    var inventoryComp = entity.GetComponent<InventoryComponent>();
                    
                    if (builder != null && builder.Enabled && inventoryComp != null)
                    {
                        // Verificar se tem hammer no inventário
                        var inventory = inventoryComp.Inventory;
                        if (inventory != null && inventory.HasItem("hammer", 1))
                        {
                            builder.RequestedBuildPosition = mouseWorldPos;
                            System.Console.WriteLine($"[PlayerInput] ✓ Build solicitado em ({mouseWorldPos.X:F0}, {mouseWorldPos.Y:F0})");
                        }
                        else
                        {
                            System.Console.WriteLine("[PlayerInput] ⚠ Precisa de Hammer no inventário para construir!");
                        }
                    }
                }
            }

            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
        }

        /// <summary>
        /// Find the gun visual entity attached to the player
        /// </summary>
        private Entity FindGunVisual(Entity player)
        {
            foreach (var e in World.GetEntitiesWithComponent<GunMuzzleComponent>())
            {
                var attach = e.GetComponent<AttachmentComponent>();
                if (attach != null && attach.Parent == player)
                    return e;
            }
            return null;
        }

        private Vector2 ScreenToWorld(Vector2 screenPos)
        {
            if (_cameraTransform.HasValue)
            {
                // Inverter a transformação da câmera para obter posição no mundo
                Matrix inverseTransform = Matrix.Invert(_cameraTransform.Value);
                return Vector2.Transform(screenPos, inverseTransform);
            }
            return screenPos;
        }
    }
}

