using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CubeSurvivor.Core
{
    /// <summary>
    /// Gerencia todas as entidades e sistemas do jogo
    /// </summary>
    public class GameWorld
    {
        private readonly List<Entity> _entities = new List<Entity>();
        private readonly List<GameSystem> _systems = new List<GameSystem>();

        /// <summary>
        /// Adiciona uma entidade ao mundo
        /// </summary>
        public Entity CreateEntity(string name = "Entity")
        {
            var entity = new Entity(name);
            _entities.Add(entity);
            return entity;
        }

        /// <summary>
        /// Remove uma entidade do mundo
        /// </summary>
        public void RemoveEntity(Entity entity)
        {
            _entities.Remove(entity);
        }

        /// <summary>
        /// Obtém todas as entidades
        /// </summary>
        public IEnumerable<Entity> GetAllEntities()
        {
            return _entities.Where(e => e.Active);
        }

        /// <summary>
        /// Obtém entidades que possuem um componente específico
        /// </summary>
        public IEnumerable<Entity> GetEntitiesWithComponent<T>() where T : Component
        {
            return _entities.Where(e => e.Active && e.HasComponent<T>());
        }

        /// <summary>
        /// Adiciona um sistema ao mundo
        /// </summary>
        public void AddSystem(GameSystem system)
        {
            system.Initialize(this);
            _systems.Add(system);
        }

        /// <summary>
        /// Atualiza todos os sistemas
        /// </summary>
        public void Update(GameTime gameTime)
        {
            foreach (var system in _systems)
            {
                system.Update(gameTime);
            }
        }
    }
}
