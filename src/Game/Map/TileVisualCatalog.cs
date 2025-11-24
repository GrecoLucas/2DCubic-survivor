using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Central registry for tile/block visual definitions.
    /// Provides a single source of truth for texture variants and rotation rules.
    /// </summary>
    public sealed class TileVisualCatalog
    {
        private readonly Dictionary<string, TileVisualDefinition> _definitions;
        private readonly TextureManager _textureManager;

        public TileVisualCatalog(TextureManager textureManager)
        {
            _textureManager = textureManager ?? throw new System.ArgumentNullException(nameof(textureManager));
            _definitions = new Dictionary<string, TileVisualDefinition>();
        }

        /// <summary>
        /// Registers a visual definition for a tile/block type.
        /// </summary>
        public void Register(string baseId, TileVisualDefinition definition)
        {
            if (string.IsNullOrEmpty(baseId))
                throw new System.ArgumentException("BaseId cannot be null or empty", nameof(baseId));
            
            _definitions[baseId] = definition;
        }

        /// <summary>
        /// Gets a visual definition by base ID, or null if not found.
        /// </summary>
        public TileVisualDefinition GetDefinition(string baseId)
        {
            return _definitions.TryGetValue(baseId, out var def) ? def : null;
        }

        /// <summary>
        /// Tries to resolve a base key to a texture and rotation using VariantResolver.
        /// Handles synonyms (e.g., "rock" and "stone" map to same definition).
        /// Returns the resolved Texture2D directly (more efficient than key lookup).
        /// </summary>
        public bool TryResolveFromBaseKey(
            string baseKey,
            Point tilePos,
            int layer,
            IVariantResolver resolver,
            int worldSeed,
            out Microsoft.Xna.Framework.Graphics.Texture2D resolvedTexture,
            out float rotationRad)
        {
            resolvedTexture = null;
            rotationRad = 0f;

            if (string.IsNullOrWhiteSpace(baseKey) || resolver == null)
                return false;

            // Normalize base key
            baseKey = baseKey.Trim().ToLowerInvariant();

            // Handle synonyms
            string catalogKey = baseKey switch
            {
                "rock" => "rock",      // rock maps to rock definition
                "stone" => "stone",    // stone maps to stone definition
                "cave" => "stone",     // cave can map to stone if needed
                _ => baseKey
            };

            // Get definition from catalog
            var definition = GetDefinition(catalogKey);
            if (definition == null || definition.Variants == null || definition.Variants.Length == 0)
                return false;

            // Use resolver to get texture and rotation
            var (texture, rotation) = resolver.Resolve(catalogKey, tilePos.X, tilePos.Y, layer, worldSeed);
            if (texture == null)
                return false;

            resolvedTexture = texture;
            rotationRad = rotation;
            return true;
        }

        /// <summary>
        /// Initializes default visual definitions for known block types.
        /// Call this after textures are loaded.
        /// </summary>
        public void InitializeDefaults()
        {
            System.Console.WriteLine("[TileVisualCatalog] Initializing defaults...");
            
            // Tree: tree1, tree2 with rotation
            var tree1 = _textureManager.GetTexture("tree1");
            var tree2 = _textureManager.GetTexture("tree2");
            if (tree1 != null && tree2 != null)
            {
                Register("tree", new TileVisualDefinition("tree", new[] { tree1, tree2 }, allowRandomRotation: true));
                System.Console.WriteLine("[TileVisualCatalog] Registered 'tree' with 2 variants + rotation");
            }
            else
            {
                System.Console.WriteLine($"[TileVisualCatalog] ⚠ 'tree' variants missing: tree1={tree1 != null}, tree2={tree2 != null}");
            }

            // Stone/Rock: stone1, stone2 with rotation
            var stone1 = _textureManager.GetTexture("stone1");
            var stone2 = _textureManager.GetTexture("stone2");
            if (stone1 != null && stone2 != null)
            {
                Register("stone", new TileVisualDefinition("stone", new[] { stone1, stone2 }, allowRandomRotation: true));
                Register("rock", new TileVisualDefinition("rock", new[] { stone1, stone2 }, allowRandomRotation: true));
                System.Console.WriteLine("[TileVisualCatalog] Registered 'stone' and 'rock' with 2 variants + rotation");
            }
            else
            {
                System.Console.WriteLine($"[TileVisualCatalog] ⚠ 'stone' variants missing: stone1={stone1 != null}, stone2={stone2 != null}");
            }

            // Wall: single texture, no rotation (optional)
            var wall = _textureManager.GetTexture("wall");
            if (wall != null)
            {
                Register("wall", new TileVisualDefinition("wall", new[] { wall }, allowRandomRotation: false));
                System.Console.WriteLine("[TileVisualCatalog] Registered 'wall' (single texture, no rotation)");
            }
            else
            {
                System.Console.WriteLine("[TileVisualCatalog] ⚠ 'wall' texture not found in TextureManager");
            }

            // Crate: single texture, no rotation (optional - can add variants later)
            var crate = _textureManager.GetTexture("crate");
            if (crate != null)
            {
                Register("crate", new TileVisualDefinition("crate", new[] { crate }, allowRandomRotation: false));
                System.Console.WriteLine("[TileVisualCatalog] Registered 'crate' (single texture, no rotation)");
            }
            else
            {
                System.Console.WriteLine("[TileVisualCatalog] ⚠ 'crate' texture not found in TextureManager");
            }
            
            System.Console.WriteLine($"[TileVisualCatalog] Initialization complete. Total definitions: {_definitions.Count}");
        }
    }
}

