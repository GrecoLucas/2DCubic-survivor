using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema que gerencia projéteis (movimento, lifetime, remoção)
    /// </summary>
    public sealed class BulletSystem : GameSystem
    {
        private int _mapWidth;
        private int _mapHeight;

        public BulletSystem(int mapWidth = 2000, int mapHeight = 2000)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
        }

        /// <summary>
        /// Atualiza as dimensões do mapa (útil quando carregado dinamicamente).
        /// </summary>
        public void SetMapSize(int mapWidth, int mapHeight)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            System.Console.WriteLine($"[BulletSystem] Dimensões do mapa atualizadas para {mapWidth}x{mapHeight}");
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var bulletsToRemove = new List<Entity>();

            foreach (var entity in World.GetEntitiesWithComponent<BulletComponent>())
            {
                // Pular se a entidade não está ativa (já foi marcada para remoção)
                if (!entity.Active)
                {
                    bulletsToRemove.Add(entity);
                    continue;
                }

                var bullet = entity.GetComponent<BulletComponent>();
                var transform = entity.GetComponent<TransformComponent>();

                if (bullet == null || transform == null || !bullet.Enabled)
                    continue;

                // Atualizar lifetime
                bullet.Lifetime += deltaTime;

                // Remover se expirou ou saiu do mapa
                if (bullet.Lifetime >= bullet.MaxLifetime ||
                    transform.Position.X < 0 || transform.Position.X > _mapWidth ||
                    transform.Position.Y < 0 || transform.Position.Y > _mapHeight)
                {
                    bulletsToRemove.Add(entity);
                }
            }

            // Remover projéteis que devem ser removidos
            foreach (var bullet in bulletsToRemove)
            {
                World.RemoveEntity(bullet);
            }
        }
    }
}

