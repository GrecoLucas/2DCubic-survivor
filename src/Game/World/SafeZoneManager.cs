using System.Collections.Generic;
using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using CubeSurvivor.Systems;
using Microsoft.Xna.Framework;

namespace CubeSurvivor
{
    /// <summary>
    /// Gerencia zonas seguras (safe zones) no mapa.
    /// Implementa IEnemySpawnExclusionProvider para impedir spawn de inimigos nas zonas.
    /// </summary>
    public sealed class SafeZoneManager : IEnemySpawnExclusionProvider
    {
        private readonly List<Rectangle> _exclusionZones = new();
        private readonly IWorldObjectFactory _worldObjectFactory;

        public SafeZoneManager(IWorldObjectFactory worldObjectFactory)
        {
            _worldObjectFactory = worldObjectFactory;
        }

        /// <summary>
        /// Retorna as zonas de exclusão para spawn de inimigos.
        /// </summary>
        public IReadOnlyList<Rectangle> GetExclusionZones() => _exclusionZones;

        /// <summary>
        /// Inicializa as zonas seguras a partir de uma definição de nível.
        /// Cria paredes ao redor de cada zona com uma abertura (porta).
        /// </summary>
        /// <param name="world">O mundo do jogo</param>
        /// <param name="levelDefinition">Definição do nível contendo as zonas</param>
        public void InitializeZones(IGameWorld world, LevelDefinition levelDefinition)
        {
            _exclusionZones.Clear();

            foreach (var zone in levelDefinition.SafeZones)
            {
                _exclusionZones.Add(zone.Area);
                CreateWallsForZone(world, zone);
            }

            if (levelDefinition.SafeZones.Count > 0)
            {
                System.Console.WriteLine($"[SafeZoneManager] {levelDefinition.SafeZones.Count} zona(s) segura(s) inicializada(s).");
            }
        }

        /// <summary>
        /// Cria paredes ao redor de uma zona segura, deixando uma abertura (porta).
        /// </summary>
        private void CreateWallsForZone(IGameWorld world, SafeZoneDefinition zone)
        {
            int wallBlockSize = GameConfig.WallBlockSize;

            Rectangle area = zone.Area;
            Rectangle opening = zone.OpeningArea;

            int wallsCreated = 0;

            // Criar paredes horizontais (topo e fundo)
            for (int x = area.Left; x < area.Right; x += wallBlockSize)
            {
                var topBlockRect = new Rectangle(x, area.Top, wallBlockSize, wallBlockSize);
                var bottomBlockRect = new Rectangle(x, area.Bottom - wallBlockSize, wallBlockSize, wallBlockSize);

                // Parede do topo
                if (!topBlockRect.Intersects(opening))
                {
                    CreateWallBlock(world, new Vector2(topBlockRect.Center.X, topBlockRect.Center.Y));
                    wallsCreated++;
                }

                // Parede do fundo
                if (!bottomBlockRect.Intersects(opening))
                {
                    CreateWallBlock(world, new Vector2(bottomBlockRect.Center.X, bottomBlockRect.Center.Y));
                    wallsCreated++;
                }
            }

            // Criar paredes verticais (esquerda e direita)
            for (int y = area.Top; y < area.Bottom; y += wallBlockSize)
            {
                var leftBlockRect = new Rectangle(area.Left, y, wallBlockSize, wallBlockSize);
                var rightBlockRect = new Rectangle(area.Right - wallBlockSize, y, wallBlockSize, wallBlockSize);

                // Parede da esquerda
                if (!leftBlockRect.Intersects(opening))
                {
                    CreateWallBlock(world, new Vector2(leftBlockRect.Center.X, leftBlockRect.Center.Y));
                    wallsCreated++;
                }

                // Parede da direita
                if (!rightBlockRect.Intersects(opening))
                {
                    CreateWallBlock(world, new Vector2(rightBlockRect.Center.X, rightBlockRect.Center.Y));
                    wallsCreated++;
                }
            }

            System.Console.WriteLine($"[SafeZoneManager] Criados {wallsCreated} blocos de parede para zona em ({area.X}, {area.Y}).");
        }

        /// <summary>
        /// Cria um bloco de parede individual.
        /// </summary>
        private void CreateWallBlock(IGameWorld world, Vector2 centerPosition)
        {
            _worldObjectFactory.CreateWall(world, centerPosition, GameConfig.WallBlockSize, GameConfig.WallBlockSize);
        }
    }
}

