using System.Collections.Generic;
using CubeSurvivor.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Stores named sockets for attachment points on the player sprite
    /// </summary>
    public sealed class PlayerSocketsComponent : Component
    {
        private readonly Dictionary<AttachmentSocketId, SpriteSocket> _sockets;

        public PlayerSocketsComponent(IEnumerable<SpriteSocket> sockets)
        {
            _sockets = new Dictionary<AttachmentSocketId, SpriteSocket>();
            foreach (var s in sockets)
                _sockets[s.Id] = s;
        }

        public bool TryGetSocket(AttachmentSocketId id, out SpriteSocket socket)
            => _sockets.TryGetValue(id, out socket);
    }
}

