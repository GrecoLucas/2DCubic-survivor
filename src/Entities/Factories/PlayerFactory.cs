using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Components;
using CubeSurvivor.Inventory.Core;
using CubeSurvivor.Inventory.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar a entidade do jogador
    /// </summary>
    public sealed class PlayerFactory : IPlayerFactory
    {
        private TextureManager _textureManager;
        private IWeaponVisualFactory _weaponVisualFactory;
        
        public void SetTextureManager(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }
        
        public void SetWeaponVisualFactory(IWeaponVisualFactory weaponVisualFactory)
        {
            _weaponVisualFactory = weaponVisualFactory;
        }
        
        public Entity CreatePlayer(IGameWorld world, Vector2 position)
        {
            var player = world.CreateEntity("Player");

            // Adicionar componentes ao jogador
            player.AddComponent(new TransformComponent(position));
            
            // Usar textura se disponível, senão usar cor
            // Jogador usa a camada Entities (padrão)
            Texture2D playerTexture = _textureManager?.GetTexture("player");
            SpriteComponent playerSprite;
            if (playerTexture != null)
            {
                playerSprite = new SpriteComponent(playerTexture, 32f, 32f, null, RenderLayer.Entities);
                // Player texture faces LEFT, so add PI offset to make it face RIGHT when aiming right
                playerSprite.FacingOffsetRadians = MathHelper.Pi;
            }
            else
            {
                playerSprite = new SpriteComponent(Color.Blue, 32f, 32f, RenderLayer.Entities);
                // Color squares face right by default, no offset needed
            }
            player.AddComponent(playerSprite);
            player.AddComponent(new VelocityComponent(200f)); // Velocidade de 200 pixels/segundo
            // Player input with initial projectile properties
            player.AddComponent(new PlayerInputComponent(bulletSpeed: 600f, bulletDamage: 25f, bulletSize: 8f, shootCooldownTime: 0.5f));
            player.AddComponent(new HealthComponent(100f)); // 100 de vida
            player.AddComponent(new ColliderComponent(32f, 32f, ColliderTag.Player));
            
            // Add attachment sockets for player (normalized positions in texture space)
            player.AddComponent(new PlayerSocketsComponent(new[]
            {
                // Right hand: top-left corner of sprite (when facing left)
                new SpriteSocket(AttachmentSocketId.RightHand, new Vector2(0.95f, 0.1f)),
                // Left hand: bottom-left corner of sprite (when facing left)
                new SpriteSocket(AttachmentSocketId.LeftHand, new Vector2(0.95f, 0.9f))
            }));
            
            // Componente de construção
            player.AddComponent(new BuilderComponent(GameConfig.PlayerBuildRange));
            
            // Componente de XP
            player.AddComponent(new XpComponent(20f));
            
            // Criar e inicializar inventário
            var inventory = new PlayerInventory(totalSlots: 16, hotbarSize: 4);
            player.AddComponent(new InventoryComponent(inventory));
            player.AddComponent(new HeldItemComponent());
            
            // Adicionar gun ao inventário (no primeiro slot da hotbar)
            var gun = new GunItem();
            // Atribuir textura ao item (para inventário)
            if (_textureManager != null)
            {
                gun.IconTexture = _textureManager.GetTexture("gun");
            }
            inventory.AddItem(gun, 1);
            
            // Equipar automaticamente a gun ao criar o jogador
            inventory.SelectHotbarSlot(0);
            gun.OnEquip(player);
            player.GetComponent<HeldItemComponent>().SetHeldItem(gun, 0);
            
            // Create gun visual entity attached to player's right hand
            // Note: gun texture is only for inventory/drops, not for equipped visual
            if (_weaponVisualFactory != null)
            {
                _weaponVisualFactory.CreateGunVisual(world, player, null);
            }

            return player;
        }
    }
}