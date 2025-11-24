using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Builders
{
    /// <summary>
    /// Builder para criar entidades seguindo o padrão Builder
    /// Princípio: Single Responsibility Principle (SRP) - Responsável apenas por construir entidades
    /// Princípio: Open/Closed Principle (OCP) - Facilmente extensível
    /// 
    /// Uso:
    /// <code>
    /// var entity = new EntityBuilder(world, "MyEntity")
    ///     .WithTransform(new Vector2(100, 100)) -- faz a entidade ter uma posição
    ///     .WithSprite(Color.Blue, 32, 32, RenderLayer.Entities) -- faz a entidade ter um sprite
    ///     .WithVelocity(200f) -- faz a entidade ter uma velocidade
    ///     .WithCollider(32, 32, ColliderTag.Default) -- faz a entidade ter um collider
    ///     .Build(); -- constroi a entidade
    /// </code>
    /// </summary>
    public class EntityBuilder
    {
        private readonly IGameWorld _world;
        private readonly Entity _entity;

        public EntityBuilder(IGameWorld world, string name)
        {
            _world = world;
            _entity = world.CreateEntity(name);
        }

        /// <summary>
        /// Adiciona um componente Transform à entidade
        /// </summary>
        public EntityBuilder WithTransform(Vector2 position, float rotation = 0f)
        {
            _entity.AddComponent(new TransformComponent(position) { Rotation = rotation });
            return this;
        }

        /// <summary>
        /// Adiciona um componente Sprite com cor à entidade
        /// </summary>
        public EntityBuilder WithSprite(Color color, float width, float height, RenderLayer layer)
        {
            _entity.AddComponent(new SpriteComponent(color, width, height, layer));
            return this;
        }

        /// <summary>
        /// Adiciona um componente Sprite com textura à entidade
        /// </summary>
        public EntityBuilder WithSprite(Texture2D texture, float width, float height, Color? tintColor, RenderLayer layer)
        {
            _entity.AddComponent(new SpriteComponent(texture, new Vector2(width, height), tintColor, layer));
            return this;
        }

        /// <summary>
        /// Adiciona um componente Velocity à entidade
        /// </summary>
        public EntityBuilder WithVelocity(float speed)
        {
            _entity.AddComponent(new VelocityComponent(speed));
            return this;
        }

        /// <summary>
        /// Adiciona um componente Collider à entidade
        /// </summary>
        public EntityBuilder WithCollider(float width, float height, ColliderTag tag)
        {
            _entity.AddComponent(new ColliderComponent(width, height, tag));
            return this;
        }

        /// <summary>
        /// Adiciona um componente Health à entidade
        /// </summary>
        public EntityBuilder WithHealth(float maxHealth)
        {
            _entity.AddComponent(new HealthComponent(maxHealth));
            return this;
        }

        /// <summary>
        /// Adiciona um componente AI à entidade
        /// </summary>
        public EntityBuilder WithAI(float chaseSpeed)
        {
            _entity.AddComponent(new AIComponent(chaseSpeed));
            return this;
        }

        /// <summary>
        /// Adiciona um componente Enemy à entidade
        /// </summary>
        public EntityBuilder WithEnemy(float damage, float attackCooldown)
        {
            _entity.AddComponent(new EnemyComponent(damage, attackCooldown));
            return this;
        }

        /// <summary>
        /// Adiciona um componente LootDrop à entidade
        /// </summary>
        public EntityBuilder WithLootDrop(params (string itemType, float chance)[] lootEntries)
        {
            var lootDrop = new LootDropComponent();
            foreach (var (itemType, chance) in lootEntries)
            {
                lootDrop.AddLoot(itemType, chance);
            }
            _entity.AddComponent(lootDrop);
            return this;
        }

        /// <summary>
        /// Adiciona um componente Pickup à entidade
        /// </summary>
        public EntityBuilder WithPickup(Inventory.Core.IItem item, int quantity, float pickupRadius)
        {
            _entity.AddComponent(new PickupComponent(item, quantity, pickupRadius));
            return this;
        }

        /// <summary>
        /// Adiciona um componente customizado à entidade
        /// </summary>
        public EntityBuilder WithComponent<T>(T component) where T : Component
        {
            _entity.AddComponent(component);
            return this;
        }

        /// <summary>
        /// Constrói e retorna a entidade
        /// </summary>
        public Entity Build()
        {
            return _entity;
        }
    }
}
