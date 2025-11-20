using CubeSurvivor.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Marks an entity as visually attached to a parent entity at a specific socket
    /// </summary>
    public sealed class AttachmentComponent : Component
    {
        public Entity Parent { get; }
        public AttachmentSocketId SocketId { get; }
        public float AdditionalRotationOffset { get; } // for grip alignment

        public AttachmentComponent(Entity parent, AttachmentSocketId socketId, float additionalRotationOffset = 0f)
        {
            Parent = parent;
            SocketId = socketId;
            AdditionalRotationOffset = additionalRotationOffset;
        }
    }
}

