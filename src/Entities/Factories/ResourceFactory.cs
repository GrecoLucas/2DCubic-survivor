using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Entities.Factories;
using CubeSurvivor.Inventory.Items.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory unificada para criação de recursos
    /// Princípio: Single Responsibility Principle (SRP) - Responsável apenas por criar recursos
    /// Princípio: Open/Closed Principle (OCP) - Aberto para novos tipos de recursos
    /// Princípio: Dependency Inversion Principle (DIP) - Depende de IResourceFactory
    /// </summary>
    public sealed class ResourceFactory : IResourceFactory
    {
        private readonly TextureManager _textureManager;

        public ResourceFactory(TextureManager textureManager = null)
        {
            _textureManager = textureManager;
        }

        public Entity CreateResource(IGameWorld world, Vector2 position, string resourceType)
        {
            return resourceType.ToLowerInvariant() switch
            {
                "wood" => CreateWood(world, position),
                "gold" => CreateGold(world, position),
                "apple" => CreateApple(world, position),
                "brain" => CreateBrain(world, position),
                _ => throw new System.ArgumentException($"Unknown resource type: {resourceType}")
            };
        }

        private Entity CreateWood(IGameWorld world, Vector2 position)
        {
            var wood = world.CreateEntity("Wood");

            wood.AddComponent(new TransformComponent(position));

            Texture2D woodTexture = _textureManager?.GetTexture("wood");
            if (woodTexture != null)
            {
                wood.AddComponent(new SpriteComponent(woodTexture, new Vector2(32f, 32f), null, RenderLayer.GroundItems));
            }
            else
            {
                wood.AddComponent(new SpriteComponent(new Color(139, 69, 19), 32f, 32f, RenderLayer.GroundItems));
            }

            var woodItem = new WoodItem();
            if (_textureManager != null)
            {
                woodItem.IconTexture = _textureManager.GetTexture("wood");
            }

            wood.AddComponent(new PickupComponent(woodItem, quantity: 1, pickupRadius: 50f));
            wood.AddComponent(new ColliderComponent(32f, 32f, ColliderTag.Default));

            return wood;
        }

        private Entity CreateGold(IGameWorld world, Vector2 position)
        {
            var gold = world.CreateEntity("Gold");

            gold.AddComponent(new TransformComponent(position));

            // TUDO 32x32
            Texture2D goldTexture = _textureManager?.GetTexture("gold");
            if (goldTexture != null)
            {
                gold.AddComponent(new SpriteComponent(goldTexture, new Vector2(32f, 32f), null, RenderLayer.GroundItems));
            }
            else
            {
                gold.AddComponent(new SpriteComponent(Color.Gold, 32f, 32f, RenderLayer.GroundItems));
            }

            var goldItem = new GoldItem();
            if (_textureManager != null)
            {
                goldItem.IconTexture = _textureManager.GetTexture("gold");
            }

            gold.AddComponent(new PickupComponent(goldItem, quantity: 1, pickupRadius: 50f));
            gold.AddComponent(new ColliderComponent(32f, 32f, ColliderTag.Default));

            return gold;
        }

        private Entity CreateApple(IGameWorld world, Vector2 position)
        {
            var apple = world.CreateEntity("Apple");

            apple.AddComponent(new TransformComponent(position));

            // TUDO 32x32
            Texture2D appleTexture = _textureManager?.GetTexture("apple");
            if (appleTexture != null)
            {
                apple.AddComponent(new SpriteComponent(appleTexture, new Vector2(32f, 32f), null, RenderLayer.GroundItems));
            }
            else
            {
                apple.AddComponent(new SpriteComponent(Color.Red, 32f, 32f, RenderLayer.GroundItems));
            }

            var appleItem = new Inventory.Items.Consumables.AppleItem();
            if (_textureManager != null)
            {
                appleItem.IconTexture = _textureManager.GetTexture("apple");
            }

            apple.AddComponent(new PickupComponent(appleItem, quantity: 1, pickupRadius: 50f));
            apple.AddComponent(new ColliderComponent(32f, 32f, ColliderTag.Default));

            return apple;
        }

        private Entity CreateBrain(IGameWorld world, Vector2 position)
        {
            var brain = world.CreateEntity("Brain");

            brain.AddComponent(new TransformComponent(position));

            // TUDO 32x32
            Texture2D brainTexture = _textureManager?.GetTexture("brain");
            if (brainTexture != null)
            {
                brain.AddComponent(new SpriteComponent(brainTexture, new Vector2(32f, 32f), null, RenderLayer.GroundItems));
            }
            else
            {
                brain.AddComponent(new SpriteComponent(Color.Pink, 32f, 32f, RenderLayer.GroundItems));
            }

            var brainItem = new Inventory.Items.Consumables.BrainItem();
            if (_textureManager != null)
            {
                brainItem.IconTexture = _textureManager.GetTexture("brain");
            }

            brain.AddComponent(new PickupComponent(brainItem, quantity: 1, pickupRadius: 50f));
            brain.AddComponent(new ColliderComponent(32f, 32f, ColliderTag.Default));

            return brain;
        }
    }
}
