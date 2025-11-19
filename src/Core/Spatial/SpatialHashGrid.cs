using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Core.Spatial
{
    /// <summary>
    /// Implementação de spatial hash grid para consultas espaciais eficientes.
    /// Divide o espaço do mundo em células de tamanho fixo e indexa entidades por células.
    /// Isso reduz a complexidade de colisões de O(n²) para aproximadamente O(n) em casos típicos.
    /// </summary>
    public sealed class SpatialHashGrid : ISpatialIndex
    {
        private readonly Dictionary<Point, List<Entity>> _cells = new();
        private readonly int _cellSize;

        /// <summary>
        /// Cria uma nova spatial hash grid com o tamanho de célula especificado.
        /// </summary>
        /// <param name="cellSize">Tamanho de cada célula em unidades do mundo (ex: 128, 256)</param>
        public SpatialHashGrid(int cellSize)
        {
            if (cellSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(cellSize), "Cell size must be positive");

            _cellSize = cellSize;
        }

        public void Clear()
        {
            _cells.Clear();
        }

        public void Register(Entity entity, Rectangle bounds)
        {
            if (entity == null)
                return;

            // Determinar todas as células sobrepostas pelos bounds
            int minX = bounds.Left / _cellSize;
            int maxX = bounds.Right / _cellSize;
            int minY = bounds.Top / _cellSize;
            int maxY = bounds.Bottom / _cellSize;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var cell = new Point(x, y);
                    if (!_cells.TryGetValue(cell, out var list))
                    {
                        list = new List<Entity>();
                        _cells[cell] = list;
                    }

                    list.Add(entity);
                }
            }
        }

        public IEnumerable<Entity> Query(Rectangle bounds)
        {
            int minX = bounds.Left / _cellSize;
            int maxX = bounds.Right / _cellSize;
            int minY = bounds.Top / _cellSize;
            int maxY = bounds.Bottom / _cellSize;

            HashSet<Entity> unique = new();

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var cell = new Point(x, y);
                    if (_cells.TryGetValue(cell, out var list))
                    {
                        foreach (var e in list)
                        {
                            unique.Add(e);
                        }
                    }
                }
            }

            return unique;
        }
    }
}

