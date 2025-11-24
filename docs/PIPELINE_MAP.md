# Texture Loading Pipeline Map

## Phase 0 - Current State Analysis

### Texture Loading

**EditorState:**
- Uses `TextureManager` (file-based loading from `assets/`)
- Loads: grass, dirt, stone, floor, wall, crate, tree1/2, stone1/2, glorb1/2/3, items
- Creates `TileVisualCatalog` and calls `InitializeDefaults()`
- Creates `VariantResolver` and attaches to `EditorContext`

**PlayState:**
- Uses `TextureManager` (file-based loading from `assets/`)
- Loads: grass, cave, player, wall, crate, tree1/2, stone1/2, glorb1/2/3, items
- Creates `TileVisualCatalog` and calls `InitializeDefaults()`
- Creates `VariantResolver` and passes to `MapRenderSystem`
- **BUG**: `EnemyFactory` created without `TextureManager` parameter!

### BlockType → Texture Key Mapping

**Single Source of Truth:**
- `MapRenderSystem.GetBlockBaseId()`: BlockType → baseId string
  - Wall → "wall"
  - Crate → "crate"
  - Tree → "tree"
  - Rock → "rock"

**TileVisualCatalog.InitializeDefaults():**
- Registers: "tree", "stone", "rock", "wall", "crate"
- Only registers if textures exist in TextureManager

**VariantResolver:**
- Resolves baseId → (Texture2D, rotation)
- Returns null if catalog doesn't have definition

**Fallback Chain:**
1. Try VariantResolver.Resolve(baseId, ...)
2. If null, try GetBlockTexture(blockType) → TextureManager.GetTexture(baseId)
3. If null, use color fallback

### EnemyType → Texture Mapping

**EnemyRegistry:**
- Stores `EnemyDefinition` with `TextureName` field
- "glorb" enemy has `TextureName = "glorb1"`

**EnemyFactory:**
- Detects animation frames if TextureName ends with "1"
- Tries to load baseName+"1", baseName+"2", baseName+"3"
- **BUG**: Needs TextureManager but PlayState doesn't pass it!

**RenderSystem:**
- Uses SpriteAnimatorComponent.GetCurrentFrame() if available
- Falls back to SpriteComponent.Texture

## Issues Identified

1. **EnemyFactory missing TextureManager** → Can't load glorb textures
2. **No debug logging** → Can't see what's failing
3. **Catalog might fail silently** → If textures not loaded, catalog has no entries
4. **No validation** → Don't know if textures loaded successfully

