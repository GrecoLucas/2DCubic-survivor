# Entity Variant Resolution - Complete Fix

## Problem Identified

**Root Cause**: Entities spawned from BlockLayers (via `BlockEntityStreamer`) were using **solid colors** instead of textures, because:
1. `WorldObjectFactory.CreateWall/CreateRock` used hardcoded colors
2. `BlockEntityStreamer.CreateTree` used hardcoded colors
3. No VariantResolver was being applied to entity creation

This caused walls, rocks, and trees to appear as colored squares instead of textured sprites.

## Solution Applied

### Phase A - Pipeline Mapping ✅
- **Identified**: `BlockEntityStreamer.SpawnBlockEntity()` creates entities from BlockType
- **Identified**: `WorldObjectFactory` creates wall/rock/crate entities
- **Identified**: `BlockEntityStreamer.CreateTree()` creates tree entities

### Phase B - Variant Resolution for Entities ✅

#### B1: TileVisualCatalog.TryResolveFromBaseKey()
- Added method to resolve base keys ("tree", "rock", "stone", "wall") to textures + rotation
- Handles synonyms: "rock" and "stone" map to same definition
- Returns Texture2D directly (more efficient than key lookup)

#### B2: WorldObjectFactory Updated
- **CreateWall**: Now uses VariantResolver to get "wall" texture
- **CreateRock**: Now uses VariantResolver to get "rock"/"stone" texture with variants + rotation
- **CreateCrate**: Updated to use VariantResolver (for consistency)
- All methods apply rotation from resolver to TransformComponent

#### B3: BlockEntityStreamer Updated
- **CreateTree**: Now uses VariantResolver to get "tree" texture with variants + rotation
- Applies rotation to TransformComponent

#### B4: PlayState Integration
- Passes `TileVisualCatalog`, `VariantResolver`, and `worldSeed` to:
  - `WorldObjectFactory` constructor
  - `BlockEntityStreamer` constructor

### Phase C - Debug Logging ✅
- **WorldObjectFactory**: Logs first 3 wall/rock creations with resolved texture and rotation
- **BlockEntityStreamer**: Logs first 3 tree creations with resolved texture and rotation
- All logs show: position, tile coordinates, rotation in degrees

## Files Modified

1. **src/Game/Map/TileVisualCatalog.cs**
   - Added `TryResolveFromBaseKey()` method

2. **src/Entities/Factories/WorldObjectFactory.cs**
   - Added constructor parameters for TextureManager, Catalog, VariantResolver, worldSeed
   - Updated `CreateWall()` to use VariantResolver
   - Updated `CreateRock()` to use VariantResolver
   - Updated `CreateCrate()` to use VariantResolver
   - Added debug logging

3. **src/Game/Map/BlockEntityStreamer.cs**
   - Added constructor parameters for TextureManager, Catalog, VariantResolver, worldSeed
   - Updated `CreateTree()` to use VariantResolver
   - Added debug logging

4. **src/Game/States/PlayState.cs**
   - Passes catalog, resolver, and seed to WorldObjectFactory and BlockEntityStreamer

## Expected Results

1. **Walls**: Render with "wall" texture (no rotation)
2. **Rocks/Stones**: Render with "stone1" or "stone2" variants + random rotation (0°/90°/180°/270°)
3. **Trees**: Render with "tree1" or "tree2" variants + random rotation (0°/90°/180°/270°)
4. **Crates**: Render with "crate" texture (no rotation)

All variants are **deterministic** - same tile position = same variant and rotation, always.

## Debug Output

On first few entity creations, you'll see:
```
[WorldObjectFactory] Wall at (100.0,200.0) tile=(3,6) resolved texture rot=0.0°
[WorldObjectFactory] Rock at (150.0,250.0) tile=(4,7) resolved texture rot=90.0°
[BlockEntityStreamer] Tree at (200.0,300.0) tile=(6,9) resolved texture rot=180.0°
```

## Testing Checklist

- [ ] Run game and check console logs
- [ ] Verify walls render with texture (not gray squares)
- [ ] Verify rocks render with stone1/stone2 variants + rotation
- [ ] Verify trees render with tree1/tree2 variants + rotation
- [ ] Check logs show resolved textures (not color fallbacks)
- [ ] Verify PixelFallback spam is reduced/eliminated

