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
        
        public void SetTextureManager(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }
        
        public Entity CreatePlayer(IGameWorld world, Vector2 position)
        {
            var player = world.CreateEntity("Player");

            // Adicionar componentes ao jogador
            player.AddComponent(new TransformComponent(position));
            
            // Usar textura se disponível, senão usar cor
            // Jogador usa a camada Entities (padrão)
            Texture2D playerTexture = _textureManager?.GetTexture("player");
            if (playerTexture != null)
            {
                player.AddComponent(new SpriteComponent(playerTexture, 50f, 50f, null, RenderLayer.Entities));
            }
            else
            {
                player.AddComponent(new SpriteComponent(Color.Blue, 50f, 50f, RenderLayer.Entities));
            }
            player.AddComponent(new VelocityComponent(200f)); // Velocidade de 200 pixels/segundo
            // Player input with initial projectile properties
            player.AddComponent(new PlayerInputComponent(bulletSpeed: 600f, bulletDamage: 25f, bulletSize: 8f, shootCooldownTime: 0.5f));
            player.AddComponent(new HealthComponent(100f)); // 100 de vida
            player.AddComponent(new ColliderComponent(50f, 50f, ColliderTag.Player));
            
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

            return player;
        }
    }
}