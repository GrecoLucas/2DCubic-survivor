# Texture Rendering Fixes Applied

## Summary

Fixed critical bugs preventing walls, rocks, and glorb enemies from rendering in-game.

## Phase 0 - Pipeline Map ✅

Created `docs/PIPELINE_MAP.md` documenting:
- Texture loading paths in EditorState vs PlayState
- BlockType → texture key mappings
- EnemyType → texture mappings
- Fallback chains

## Phase 1 - Walls + Rock Rendering ✅

### Fixes Applied:

1. **EnemyFactory TextureManager Bug** (CRITICAL)
   - **Issue**: `PlayState` created `EnemyFactory` without passing `TextureManager`
   - **Fix**: Changed `new EnemyFactory()` → `new EnemyFactory(_textureManager)`
   - **File**: `src/Game/States/PlayState.cs:313`

2. **Debug Logging Added**
   - **TileVisualCatalog**: Now logs what gets registered and what's missing
   - **MapRenderSystem**: Logs missing textures when VariantResolver/GetBlockTexture return null
   - **PlayState/EditorState**: Log all loaded texture keys with ✓/✗ status

3. **Catalog Initialization Logging**
   - Shows which block types are registered (tree, stone, rock, wall, crate)
   - Warns if textures are missing

## Phase 2 - Glorb Textures + Animation ✅

### Fixes Applied:

1. **EnemyFactory Debug Logging**
   - Logs when enemy spawns with animation frames
   - Warns if animation frames are missing
   - Shows texture name used

2. **SpriteAnimationSystem Debug Logging**
   - Logs frame changes (limited to avoid spam)
   - Shows moving/idle state and current frame

3. **RenderSystem Debug Logging**
   - Logs texture source (AnimatorFrame vs SpriteTexture vs PixelFallback)
   - Warns when using pixel fallback unexpectedly

4. **Enemy Registry Logging**
   - PlayState now logs all registered enemy types with texture names

## Debug Output Examples

### On Game Start:
```
[PlayState] Textures loaded (including variants)
[PlayState] Loaded texture keys:
  grass: ✓
  wall: ✓
  tree1: ✓
  ...
[TileVisualCatalog] Initializing defaults...
[TileVisualCatalog] Registered 'wall' (single texture, no rotation)
[TileVisualCatalog] Registered 'tree' with 2 variants + rotation
[PlayState] Enemy registry entries:
  glorb: textureName=glorb1, frames=3 (detected)
```

### On Enemy Spawn:
```
[EnemyFactory] Spawned "glorb" at (100.0,200.0) frames=3 idle=glorb1
```

### On Missing Texture:
```
[MapRenderSystem] MissingTexture key="wall" at tile=(10,5) layer=0 kind=Blocks (VariantResolver returned null)
```

## Remaining Work (Phase 3)

- [ ] Create shared texture loading utility to avoid duplication between EditorState and PlayState
- [ ] Consider extracting texture key constants to prevent string drift

## Testing Checklist

- [x] Build succeeds
- [ ] Test in-game: walls render
- [ ] Test in-game: rocks render with variants
- [ ] Test in-game: glorb enemies spawn with texture
- [ ] Test in-game: glorb enemies animate when moving
- [ ] Check console logs for any missing texture warnings

