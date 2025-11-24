# Critical Fixes Applied - Texture Rendering Issues

## Problems Identified & Fixed

### 1. ✅ MapRenderSystem Not Using VariantResolver Correctly

**Problem**: MapRenderSystem was falling back to `GetBlockTexture()` which tried to load textures like "tree" and "rock" directly, but these don't exist (only "tree1/tree2" and "stone1/stone2" exist).

**Fix Applied**:
- Removed fallback to `GetBlockTexture()` that tried non-existent keys
- Now **only** uses VariantResolver when available
- If VariantResolver returns null, falls back to color (not a non-existent texture key)
- Added debug logging to identify when VariantResolver is null or returns null

**Files Changed**:
- `src/Systems/Rendering/MapRenderSystem.cs` - Removed problematic fallback

### 2. ✅ EnemySpawnSystem Spawning Wrong Enemy Types

**Problem**: EnemySpawnSystem was calling `CreateEnemy()` without specifying enemy type, defaulting to "default" which has no texture.

**Fix Applied**:
- Force spawn "glorb" type (which has textures) until other types get textures
- Added logging to show which enemy type is being spawned
- TODO: Add textures to default/fast/strong or make spawner choose based on region meta

**Files Changed**:
- `src/Systems/Core/EnemySpawnSystem.cs` - Force "glorb" type and add logging

### 3. ✅ RenderSystem Prioritizing SpriteAnimatorComponent

**Problem**: RenderSystem might not have been properly prioritizing animator frames.

**Fix Applied**:
- Explicitly check `animator.GetCurrentFrame()` first
- Only fall back to `sprite.Texture` if animator frame is null
- Added debug logging to show when AnimatorFrame is used vs SpriteTexture

**Files Changed**:
- `src/Systems/Rendering/RenderSystem.cs` - Improved animator priority logic

## Debug Logging Added

### MapRenderSystem
- Logs when VariantResolver is null (should never happen)
- Logs when VariantResolver returns null for a baseId (limited to 5 logs to avoid spam)
- Logs when baseId is null for a BlockType

### EnemySpawnSystem
- Logs enemy type being spawned: `[EnemySpawnSystem] Spawning enemyType='glorb' at (x,y)`

### RenderSystem
- Logs when using AnimatorFrame (limited to avoid spam)
- Logs when AnimatorFrame is null but animator exists
- Logs when no texture is available (pixel fallback)

## Expected Behavior After Fixes

1. **Walls**: Should render correctly (VariantResolver returns wall texture)
2. **Trees**: Should render with variants (tree1/tree2) and rotation
3. **Rocks/Stones**: Should render with variants (stone1/stone2) and rotation
4. **Glorbs**: Should spawn with "glorb" type (has textures) and animate when moving

## Testing Checklist

- [ ] Run game and check console logs
- [ ] Verify walls render in-game
- [ ] Verify trees render with variants in-game
- [ ] Verify rocks render with variants in-game
- [ ] Verify glorbs spawn (check log: `Spawning enemyType='glorb'`)
- [ ] Verify glorbs have texture (not color fallback)
- [ ] Verify glorbs animate when moving
- [ ] Check for any "MissingTexture" warnings in logs

## Next Steps (Future Improvements)

1. **Add textures to default/fast/strong enemy types** OR
2. **Make EnemySpawnSystem choose enemy type based on region meta** OR
3. **Remove default/fast/strong types if not needed**

