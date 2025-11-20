# Socket/Attachment System Documentation

## Overview

The CubeSurvivor project now features a professional, SOLID, data-driven socket/attachment system for held items. This system allows any entity (like weapons) to be visually attached to specific points on other entities (like the player's hands).

## Architecture

### Components Created

1. **AttachmentSocketId.cs** - Enum defining named socket types:
   - `RightHand` - Player's right hand position
   - `LeftHand` - Player's left hand position  
   - `Muzzle` - Gun muzzle position (for bullet spawning)

2. **SpriteSocket.cs** - Struct representing a socket in local sprite space:
   - `NormalizedPosition` - (0..1, 0..1) position in texture space
   - `LocalRotationOffset` - Rotation offset in radians relative to parent
   - Normalized positions scale with sprite size automatically

3. **PlayerSocketsComponent.cs** - Component storing named sockets on an entity:
   - Dictionary-based socket lookup
   - `TryGetSocket()` method for safe access

4. **AttachmentComponent.cs** - Marks an entity as attached to a parent:
   - `Parent` - Reference to parent entity
   - `SocketId` - Which socket to attach to
   - `AdditionalRotationOffset` - Fine-tuning rotation (e.g., grip alignment)

5. **GunMuzzleComponent.cs** - Marks the muzzle position on gun visuals:
   - `LocalMuzzleOffset` - Offset in gun's local space
   - Used for precise bullet spawning

### Systems Created

**AttachmentSystem.cs** - Updates transforms of attached entities each frame:
- Runs after input/movement, before rendering
- Converts normalized socket positions to world space
- Applies parent rotation to socket offsets
- Updates child entity position and rotation

### Factories Created

**WeaponVisualFactory.cs** - Creates weapon visual entities:
- `CreateGunVisual()` - Spawns gun entity attached to player
- Gun is 25x6 units with muzzle at (12, 0)
- Purely visual - no collision
- **Important**: Gun visual is a BLACK RECTANGLE when held, texture is only for inventory/ground

### Systems Created (Part 2)

**WeaponVisualSystem.cs** - Controls visibility of weapon visuals:
- Shows/hides weapon visuals based on `HeldItemComponent`
- Gun visual only appears when GunItem is equipped
- Runs each frame to keep visual state in sync with inventory

## Key Features

### 1. Player Sprite Facing Correction

**Problem**: Player texture faces LEFT by default, but rotation system uses RIGHT (+X) as baseline.

**Solution**: Added `FacingOffsetRadians` property to `SpriteComponent`:
- Player sprite has `FacingOffsetRadians = π` (180°)
- RenderSystem applies: `rotation = transform.Rotation + sprite.FacingOffsetRadians`
- When aiming right (rotation=0), sprite is rotated π to face right

### 2. Normalized Socket Positions

Sockets use normalized coordinates (0..1) in texture space:
- `(0, 0)` = top-left of sprite
- `(0.5, 0.5)` = center of sprite
- `(1, 1)` = bottom-right of sprite

Current player socket positions:
```csharp
RightHand: (0.20f, 0.25f)  // Upper-left area (20% from left, 25% from top)
LeftHand:  (0.20f, 0.75f)  // Lower-left area (20% from left, 75% from top)
```

**Benefits**:
- Scales automatically with sprite size
- Easy to tune by editing just two numbers
- Can be data-driven via JSON later

### 3. Bullet Spawning from Gun Muzzle

**Old behavior**: Bullets spawned from player center + offset

**New behavior**:
- `PlayerInputSystem` finds gun visual entity via `AttachmentComponent`
- Reads `GunMuzzleComponent` for local muzzle offset
- Transforms muzzle offset to world space using gun rotation
- Spawns bullets from precise muzzle position
- Fallback to old behavior if gun visual not found

### 4. Weapon Rendering

**Old**: RenderSystem had hardcoded weapon drawing logic

**New**: 
- Weapons are entities with `SpriteComponent` + `AttachmentComponent`
- Drawn like any other sprite via standard rendering pipeline
- AttachmentSystem handles positioning automatically
- WeaponVisualSystem controls visibility based on equipped item
- Clean separation of concerns

**Important Design Decision**:
- Gun visual in player's hand = **BLACK RECTANGLE** (Color.Black)
- Gun texture is **ONLY** used for:
  - Inventory icon
  - Ground pickup (when dropped)
- This allows distinct visual representations for different contexts

## Changes to Existing Systems

### SpriteComponent.cs
- Added `FacingOffsetRadians` property (default: 0f)

### RenderSystem.cs
- Updated rotation calculation: `rotation = transform.Rotation + sprite.FacingOffsetRadians`
- Removed hardcoded weapon drawing block
- All sprites now rendered uniformly

### PlayerFactory.cs
- Added `WeaponVisualFactory` dependency injection
- Sets `FacingOffsetRadians = π` for player sprite
- Adds `PlayerSocketsComponent` with hand socket definitions
- Creates gun visual entity on player spawn

### PlayerInputSystem.cs
- Added `FindGunVisual()` helper method
- Updated bullet spawning to use gun muzzle position
- Maintains fallback to old behavior

### Game1.cs
- Creates `WeaponVisualFactory` instance
- Configures `PlayerFactory` with weapon factory
- Registers `AttachmentSystem` after movement, before collision
- Registers `WeaponVisualSystem` to control weapon visibility

## Tuning Socket Positions

To adjust weapon position:

1. **Edit PlayerFactory.cs**, find:
```csharp
new SpriteSocket(AttachmentSocketId.RightHand, new Vector2(0.20f, 0.25f))
```

2. **Change normalized coordinates**:
   - X: 0.0 (left edge) → 1.0 (right edge)
   - Y: 0.0 (top edge) → 1.0 (bottom edge)

3. **Rebuild and test** - no other code changes needed

## Future Extensions

### Adding New Attachments

1. **Define socket** in `AttachmentSocketId`:
```csharp
public enum AttachmentSocketId
{
    RightHand,
    LeftHand,
    Muzzle,
    BackSlot,  // NEW: For backpack/shield
}
```

2. **Add to PlayerSocketsComponent**:
```csharp
new SpriteSocket(AttachmentSocketId.BackSlot, new Vector2(0.5f, 0.8f))
```

3. **Create visual entity**:
```csharp
var item = world.CreateEntity("ShieldVisual");
item.AddComponent(new TransformComponent(playerPos));
item.AddComponent(new SpriteComponent(texture, 20f, 20f));
item.AddComponent(new AttachmentComponent(player, AttachmentSocketId.BackSlot));
```

### Data-Driven Sockets

To load sockets from JSON:

1. **Add to player definition**:
```json
{
  "sockets": [
    {"id": "RightHand", "x": 0.2, "y": 0.25, "rotation": 0},
    {"id": "LeftHand", "x": 0.20, "y": 0.75, "rotation": 0}
  ]
}
```

2. **Load in PlayerFactory**:
```csharp
var sockets = playerDef.Sockets.Select(s => 
    new SpriteSocket(
        ParseSocketId(s.Id),
        new Vector2(s.X, s.Y),
        s.Rotation
    ));
player.AddComponent(new PlayerSocketsComponent(sockets));
```

## Testing Checklist

- ✅ Game builds with 0 warnings/errors
- ✅ Player rotates smoothly toward mouse
- ✅ Rotation pivot is exact center of player sprite
- ✅ Gun is drawn at player's right hand
- ✅ Gun rotates with player correctly
- ✅ Bullets spawn from gun muzzle (not player center)
- ✅ Player sprite faces correct direction (LEFT→RIGHT conversion works)
- ✅ All existing gameplay preserved (collisions, spawning, safe zones, inventory)
- ✅ Game runs without crashes for 25+ seconds

## SOLID Principles Adherence

- **Single Responsibility**: Each component stores only data; systems do only logic
- **Open/Closed**: Easy to add new socket types without modifying existing code
- **Liskov Substitution**: All sprites rendered uniformly via RenderSystem
- **Interface Segregation**: `IWeaponVisualFactory` defines clear contract
- **Dependency Inversion**: PlayerFactory depends on abstraction (`IWeaponVisualFactory`), not concrete class

## Performance Considerations

- AttachmentSystem runs O(n) where n = number of attached entities (typically 1-3 per player)
- Socket lookup is O(1) via Dictionary
- No allocations during Update loop
- Normalized positions calculated once per frame per attachment

---

**Implementation Date**: November 2025  
**Status**: Complete and tested

