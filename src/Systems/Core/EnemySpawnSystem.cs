using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using Microsoft.Xna.Framework;
using System;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema de spawn de inimigos
    /// </summary>
    public sealed class EnemySpawnSystem : GameSystem
    {
        private readonly IEnemyFactory _enemyFactory;
        private readonly Random _random = new Random();
        private float _spawnTimer;
        private readonly float _spawnInterval;
        private readonly int _maxEnemies;
        private readonly Rectangle _spawnArea;

        public EnemySpawnSystem(Rectangle spawnArea, IEnemyFactory enemyFactory, float spawnInterval = 2f, int maxEnemies = 50)
        {
            _spawnArea = spawnArea;
            _enemyFactory = enemyFactory;
            _spawnInterval = spawnInterval;
            _maxEnemies = maxEnemies;
            _spawnTimer = 0f;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _spawnTimer += deltaTime;

            // Contar inimigos ativos
            int enemyCount = 0;
            foreach (var entity in World.GetEntitiesWithComponent<EnemyComponent>())
            {
                enemyCount++;
            }

            // Spawnar novo inimigo se possível
            if (_spawnTimer >= _spawnInterval && enemyCount < _maxEnemies)
            {
                _spawnTimer = 0f;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            // Gerar posição aleatória nas bordas do spawn area
            Vector2 position = GetRandomSpawnPosition();
            _enemyFactory.CreateEnemy(World, position);
        }

        private Vector2 GetRandomSpawnPosition()
        {
            // Spawnar nas bordas da área
            int side = _random.Next(4); // 0=top, 1=right, 2=bottom, 3=left

            float x, y;

            switch (side)
            {
                case 0: // Top
                    x = _random.Next(_spawnArea.Left, _spawnArea.Right);
                    y = _spawnArea.Top;
                    break;
                case 1: // Right
                    x = _spawnArea.Right;
                    y = _random.Next(_spawnArea.Top, _spawnArea.Bottom);
                    break;
                case 2: // Bottom
                    x = _random.Next(_spawnArea.Left, _spawnArea.Right);
                    y = _spawnArea.Bottom;
                    break;
                default: // Left
                    x = _spawnArea.Left;
                    y = _random.Next(_spawnArea.Top, _spawnArea.Bottom);
                    break;
            }

            return new Vector2(x, y);
        }
    }
}