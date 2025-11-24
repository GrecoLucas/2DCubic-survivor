using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Game.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar objetos do mundo como obstáculos, caixas e paredes.
    /// </summary>
    public sealed class WorldObjectFactory : IWorldObjectFactory
    {
        private readonly TextureManager _textureManager;
        private readonly TileVisualCatalog _catalog;
        private readonly IVariantResolver _variantResolver;
        private readonly int _worldSeed;
        private int _wallLogCount = 0;
        private int _rockLogCount = 0;

        public WorldObjectFactory(TextureManager textureManager = null, TileVisualCatalog catalog = null, IVariantResolver variantResolver = null, int worldSeed = 0)
        {
            _textureManager = textureManager;
            _catalog = catalog;
            _variantResolver = variantResolver;
            _worldSeed = worldSeed;
        }
        public Entity CreateCrate(IGameWorld world, Vector2 position, float width = 32f, float height = 32f, bool isDestructible = false, float maxHealth = 50f)
        {
            var crate = world.CreateEntity("Crate");

            // Transformação
            crate.AddComponent(new TransformComponent(position));

            // Try to resolve texture using VariantResolver
            Texture2D crateTexture = null;
            Point tilePos = new Point((int)(position.X / width), (int)(position.Y / height));
            
            if (_catalog != null && _variantResolver != null)
            {
                if (_catalog.TryResolveFromBaseKey("crate", tilePos, 0, _variantResolver, _worldSeed, out var tex, out var rot))
                {
                    crateTexture = tex;
                    // Crate doesn't rotate typically, but we can apply it if needed
                }
            }
            
            // Fallback to texture manager if resolver didn't work
            if (crateTexture == null && _textureManager != null)
            {
                crateTexture = _textureManager.GetTexture("crate");
            }

            // Create sprite component with texture or color fallback
            if (crateTexture != null)
            {
                crate.AddComponent(new SpriteComponent(crateTexture, width, height, null, RenderLayer.GroundEffects));
            }
            else
            {
                crate.AddComponent(new SpriteComponent(new Color(139, 69, 19), width, height, RenderLayer.GroundEffects)); // Cor marrom
            }

            // Componentes de física e colisão
            crate.AddComponent(new ColliderComponent(width, height, ColliderTag.Default));
            crate.AddComponent(new ObstacleComponent(
                blocksMovement: true,
                blocksBullets: true,
                isDestructible: isDestructible
            ));

            // Se destrutível, adicionar vida
            if (isDestructible)
            {
                crate.AddComponent(new HealthComponent(maxHealth));
            }

            return crate;
        }

        public Entity CreateWall(IGameWorld world, Vector2 position, float width = 32f, float height = 32f)
        {
            var wall = world.CreateEntity("Wall");

            // Transformação
            var transform = new TransformComponent(position);
            wall.AddComponent(transform);

            // Try to resolve texture using VariantResolver
            Texture2D wallTexture = null;
            float rotation = 0f;
            Point tilePos = new Point((int)(position.X / width), (int)(position.Y / height));
            
            if (_catalog != null && _variantResolver != null)
            {
                if (_catalog.TryResolveFromBaseKey("wall", tilePos, 0, _variantResolver, _worldSeed, out var tex, out var rot))
                {
                    wallTexture = tex;
                    rotation = rot;
                    transform.Rotation = rotation;
                }
            }
            
            // Fallback to texture manager if resolver didn't work
            if (wallTexture == null && _textureManager != null)
            {
                wallTexture = _textureManager.GetTexture("wall");
            }

            // Create sprite component with texture or color fallback
            if (wallTexture != null)
            {
                wall.AddComponent(new SpriteComponent(wallTexture, width, height, null, RenderLayer.GroundEffects));
                // Debug log (limited to avoid spam)
                if (_wallLogCount++ < 3)
                {
                    System.Console.WriteLine($"[WorldObjectFactory] Wall at ({position.X:F1},{position.Y:F1}) tile=({tilePos.X},{tilePos.Y}) resolved texture rot={rotation * 180f / System.MathF.PI:F1}°");
                }
            }
            else
            {
                wall.AddComponent(new SpriteComponent(new Color(80, 80, 80), width, height, RenderLayer.GroundEffects)); // Cor cinza escuro
                System.Console.WriteLine($"[WorldObjectFactory] ⚠ Wall at ({position.X:F1},{position.Y:F1}) using color fallback (texture not found)");
            }

            // Componentes de física e colisão
            wall.AddComponent(new ColliderComponent(width, height, ColliderTag.Default));
            wall.AddComponent(new ObstacleComponent(
                blocksMovement: true,
                blocksBullets: true,
                isDestructible: false // Paredes são indestrutíveis
            ));

            return wall;
        }
        
        public Entity CreateRock(IGameWorld world, Vector2 position, float size = 32f)
        {
            var rock = world.CreateEntity("Rock");

            // Transformação
            var transform = new TransformComponent(position);
            rock.AddComponent(transform);

            // Try to resolve texture using VariantResolver (rock/stone)
            Texture2D rockTexture = null;
            float rotation = 0f;
            Point tilePos = new Point((int)(position.X / size), (int)(position.Y / size));
            
            if (_catalog != null && _variantResolver != null)
            {
                // Try "rock" first, fallback to "stone"
                if (!_catalog.TryResolveFromBaseKey("rock", tilePos, 0, _variantResolver, _worldSeed, out var tex, out var rot))
                {
                    _catalog.TryResolveFromBaseKey("stone", tilePos, 0, _variantResolver, _worldSeed, out tex, out rot);
                }
                rockTexture = tex;
                rotation = rot;
                transform.Rotation = rotation;
            }
            
            // Fallback to texture manager if resolver didn't work
            if (rockTexture == null && _textureManager != null)
            {
                rockTexture = _textureManager.GetTexture("stone1"); // Default to stone1
            }

            // Create sprite component with texture or color fallback
            if (rockTexture != null)
            {
                rock.AddComponent(new SpriteComponent(rockTexture, size, size, null, RenderLayer.GroundEffects));
                // Debug log (limited to avoid spam)
                if (_rockLogCount++ < 3)
                {
                    System.Console.WriteLine($"[WorldObjectFactory] Rock at ({position.X:F1},{position.Y:F1}) tile=({tilePos.X},{tilePos.Y}) resolved texture rot={rotation * 180f / System.MathF.PI:F1}°");
                }
            }
            else
            {
                rock.AddComponent(new SpriteComponent(new Color(105, 105, 105), size, size, RenderLayer.GroundEffects)); // Cor cinza pedra
                System.Console.WriteLine($"[WorldObjectFactory] ⚠ Rock at ({position.X:F1},{position.Y:F1}) using color fallback (texture not found)");
            }

            // Componentes de física e colisão
            rock.AddComponent(new ColliderComponent(size, size, ColliderTag.Default));
            rock.AddComponent(new ObstacleComponent(
                blocksMovement: true,
                blocksBullets: true,
                isDestructible: false // Rochas são indestrutíveis
            ));

            return rock;
        }
    }
}

