using Microsoft.Xna.Framework;

namespace CubeSurvivor.Core
{
    /// <summary>
    /// Classe base para todos os sistemas no ECS
    /// </summary>
    public abstract class GameSystem
    {
        protected IGameWorld World { get; private set; }

        public void Initialize(IGameWorld world)
        {
            World = world;
        }

        public abstract void Update(GameTime gameTime);
    }
}
