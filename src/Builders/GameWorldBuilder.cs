using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeSurvivor.Builders
{
    /// <summary>
    /// Builder para configurar o GameWorld
    /// Princípio: Single Responsibility Principle (SRP)
    /// Princípio: Open/Closed Principle (OCP)
    /// 
    /// Uso:
    /// <code>
    /// var world = new GameWorldBuilder()
    ///     .AddSystem(new MovementSystem())
    ///     .AddSystem(new RenderSystem(spriteBatch, camera))
    ///     .Build();
    /// </code>
    /// </summary>
    public class GameWorldBuilder
    {
        private readonly List<GameSystem> _systems = new List<GameSystem>();

        /// <summary>
        /// Adiciona um sistema ao mundo
        /// </summary>
        public GameWorldBuilder AddSystem(GameSystem system)
        {
            _systems.Add(system);
            return this;
        }

        /// <summary>
        /// Constrói e retorna o GameWorld configurado
        /// </summary>
        public GameWorld Build()
        {
            var world = new GameWorld();
            foreach (var system in _systems)
            {
                world.AddSystem(system);
            }
            return world;
        }
    }
}
