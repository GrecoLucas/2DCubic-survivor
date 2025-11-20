using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Updates transforms of attached entities to follow their parent's sockets
    /// </summary>
    public sealed class AttachmentSystem : GameSystem
    {
        public override void Update(GameTime gameTime)
        {
            foreach (var child in World.GetEntitiesWithComponent<AttachmentComponent>())
            {
                var attach = child.GetComponent<AttachmentComponent>();
                var childTransform = child.GetComponent<TransformComponent>();
                var childSprite = child.GetComponent<SpriteComponent>();

                if (attach == null || childTransform == null || childSprite == null || !attach.Enabled)
                    continue;

                var parent = attach.Parent;
                if (parent == null || !parent.Active)
                    continue;

                var parentTransform = parent.GetComponent<TransformComponent>();
                var parentSprite = parent.GetComponent<SpriteComponent>();
                var sockets = parent.GetComponent<PlayerSocketsComponent>();

                if (parentTransform == null || parentSprite == null || sockets == null)
                    continue;

                if (!sockets.TryGetSocket(attach.SocketId, out var socket))
                    continue;

                // Convert normalized socket pos -> local world offset
                // localOffset = (normalized - 0.5) * parentSprite.Size
                Vector2 localOffset =
                    (socket.NormalizedPosition - new Vector2(0.5f, 0.5f))
                    * parentSprite.Size;

                // Rotate this offset by parent rotation
                float r = parentTransform.Rotation;
                Vector2 rotatedOffset = Vector2.Transform(localOffset, Matrix.CreateRotationZ(r));

                // Place child at parent pos + rotatedOffset
                childTransform.Position = parentTransform.Position + rotatedOffset;

                // Child rotation follows parent + socket offset + grip offset
                childTransform.Rotation = r + socket.LocalRotationOffset + attach.AdditionalRotationOffset;
            }
        }
    }
}

