using Microsoft.Xna.Framework;

namespace CubeSurvivor.Core
{
    /// <summary>
    /// Classe base para todos os sistemas no ECS
    /// </summary>
    public abstract class GameSystem
    {
        protected GameWorld World { get; private set; }

        public void Initialize(GameWorld world)
        {
            World = world;
        }

        public abstract void Update(GameTime gameTime);
    }
}
