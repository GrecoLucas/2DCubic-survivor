using Microsoft.Xna.Framework;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Represents a socket in LOCAL sprite space.
    /// Positions are normalized (0..1) so they scale with sprite size.
    /// </summary>
    public readonly struct SpriteSocket
    {
        public AttachmentSocketId Id { get; }
        public Vector2 NormalizedPosition { get; } // (0..1, 0..1) in texture space
        public float LocalRotationOffset { get; }  // radians, relative to parent rotation

        public SpriteSocket(AttachmentSocketId id, Vector2 normalizedPosition, float localRotationOffset = 0f)
        {
            Id = id;
            NormalizedPosition = normalizedPosition;
            LocalRotationOffset = localRotationOffset;
        }
    }
}

