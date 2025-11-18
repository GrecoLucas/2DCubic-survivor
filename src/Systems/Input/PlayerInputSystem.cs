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
        private const float BulletSpawnOffset = 30f;

        private readonly IBulletFactory _bulletFactory;
        private MouseState _previousMouseState;
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

                if (input == null || velocity == null || transform == null || !input.Enabled)
                    continue;

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
                    
                    // Criar projétil na direção do mouse
                    Vector2 bulletDirection = mouseWorldPos - transform.Position;
                    if (bulletDirection != Vector2.Zero)
                    {
                        bulletDirection.Normalize();
                        // Criar projétil ligeiramente à frente do player
                        Vector2 bulletStartPos = transform.Position + bulletDirection * BulletSpawnOffset;
                        // Usar propriedades da arma (permite diferentes armas com diferentes stats)
                        float speed = gun.BulletSpeed;
                        float damage = gun.Damage;
                        float size = input.BulletSize;
                        _bulletFactory.CreateBullet(World, bulletStartPos, bulletDirection, speed, damage, size);
                        input.ShootCooldown = gun.ShootCooldown;
                    }
                }
            }

            _previousMouseState = mouseState;
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

