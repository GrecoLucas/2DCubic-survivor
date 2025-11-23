using CubeSurvivor.Core;
using CubeSurvivor.Game.Map;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems.World
{
    /// <summary>
    /// System that wraps the BlockEntityStreamer to stream block entities each frame.
    /// Integrates the streamer into the ECS update loop.
    /// </summary>
    public sealed class BlockStreamSystem : GameSystem
    {
        private readonly BlockEntityStreamer _streamer;

        public BlockStreamSystem(BlockEntityStreamer streamer)
        {
            _streamer = streamer;
        }

        public override void Update(GameTime gameTime)
        {
            _streamer.Update();
        }

        /// <summary>
        /// Gets the number of currently spawned block entities.
        /// Useful for debugging/monitoring.
        /// </summary>
        public int GetSpawnedBlockCount()
        {
            return _streamer.GetSpawnedBlockCount();
        }
    }
}





