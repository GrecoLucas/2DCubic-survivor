using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeSurvivor.Core
{
    /// <summary>
    /// Interface para o mundo do jogo (Dependency Inversion Principle)
    /// </summary>
    public interface IGameWorld
    {
        Entity CreateEntity(string name = "Entity");
        void RemoveEntity(Entity entity);
        IEnumerable<Entity> GetAllEntities();
        IEnumerable<Entity> GetEntitiesWithComponent<T>() where T : Component;
        void AddSystem(GameSystem system);
        void Update(GameTime gameTime);
    }
}

