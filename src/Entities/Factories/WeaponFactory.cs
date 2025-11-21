using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Entities.Factories;
using CubeSurvivor.Game.Registries;
using CubeSurvivor.Inventory.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar armas baseada em definições do WeaponRegistry
    /// Princípio: Single Responsibility Principle (SRP) - Responsável apenas por criar armas
    /// Princípio: Open/Closed Principle (OCP) - Aberto para novos tipos via registry
    /// Princípio: Dependency Inversion Principle (DIP) - Depende de IWeaponFactory
    /// </summary>
    public sealed class WeaponFactory : IWeaponFactory
    {
        private readonly TextureManager _textureManager;

        public WeaponFactory(TextureManager textureManager = null)
        {
            _textureManager = textureManager;
        }

        public Entity CreateWeapon(IGameWorld world, Vector2 position, string weaponType)
        {
            if (!WeaponRegistry.Instance.Contains(weaponType))
            {
                throw new ArgumentException($"Weapon type '{weaponType}' not registered.");
            }

            var definition = WeaponRegistry.Instance.Get(weaponType);
            var weapon = world.CreateEntity(definition.Name);

            // Transformação e visual
            weapon.AddComponent(new TransformComponent(position));

            // Usar textura se disponível
            Texture2D weaponTexture = null;
            if (!string.IsNullOrEmpty(definition.TextureName) && _textureManager != null)
            {
                weaponTexture = _textureManager.GetTexture(definition.TextureName);
            }

            if (weaponTexture != null)
            {
                weapon.AddComponent(new SpriteComponent(weaponTexture, definition.Width, definition.Height, null, RenderLayer.GroundItems));
            }
            else
            {
                var color = new Color(definition.ColorR, definition.ColorG, definition.ColorB);
                weapon.AddComponent(new SpriteComponent(color, definition.Width, definition.Height, RenderLayer.GroundItems));
            }

            // Componente de pickup
            var weaponItem = CreateWeaponItem(weaponType, definition, weaponTexture);
            weapon.AddComponent(new PickupComponent(weaponItem, quantity: 1, pickupRadius: 50f));

            // Collider para detecção de proximidade
            weapon.AddComponent(new ColliderComponent(definition.Width, definition.Height, ColliderTag.Default));

            return weapon;
        }

        private WeaponItem CreateWeaponItem(string weaponType, WeaponDefinition definition, Texture2D texture)
        {
            // Por agora, criar GunItem genérico
            // Pode ser estendido para tipos específicos de armas
            var item = new GunItem
            {
                IconTexture = texture
            };

            return item;
        }
    }
}
