using System;
using Microsoft.Xna.Framework;
using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using CubeSurvivor.Components;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema de spawn de inimigos
    /// </summary>
    public sealed class EnemySpawnSystem : GameSystem
    {
        private readonly IEnemyFactory _enemyFactory;
        private readonly Random _random = new Random();
        // Último multiplicador logado para evitar spam de logs
        private float _lastLoggedMultiplier = 0.01f;
        private float _spawnTimer;
        private readonly float _spawnInterval;
        private readonly int _maxEnemies;
        private readonly Rectangle _spawnArea;

        // Novo: tempo acumulado desde o início (segundos)
        private float _elapsedTime;

        public EnemySpawnSystem(Rectangle spawnArea, IEnemyFactory enemyFactory, float spawnInterval = 2f, int maxEnemies = 50)
        {
            _spawnArea = spawnArea;
            _enemyFactory = enemyFactory;
            _spawnInterval = spawnInterval;
            _maxEnemies = maxEnemies;
            _spawnTimer = 0f;
            _elapsedTime = 0f;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _spawnTimer += deltaTime;
            _elapsedTime += deltaTime;

            // Contar inimigos ativos
            int enemyCount = 0;
            foreach (var entity in World.GetEntitiesWithComponent<EnemyComponent>())
            {
                enemyCount++;
            }

            // Usar intervalo dinâmico que diminui conforme a dificuldade sobe
            float currentInterval = GameConfig.GetSpawnInterval(_spawnInterval, _elapsedTime);

            // Spawnar novo inimigo se possível
            if (_spawnTimer >= currentInterval && enemyCount < _maxEnemies)
            {
                _spawnTimer = 0f;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            // Gerar posição aleatória nas bordas do spawn area
            Vector2 position = GetRandomSpawnPosition();

            // Criar inimigo via factory (retorna a entidade criada)
            var enemy = _enemyFactory.CreateEnemy(World, position);

            // Aplicar escala de dificuldade à vida/dano do inimigo baseado no tempo decorrido
            float multiplier = GameConfig.GetEnemyDifficultyMultiplier(_elapsedTime);

            var health = enemy.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.MaxHealth *= multiplier;
                health.CurrentHealth = health.MaxHealth;
            }

            var enemyComp = enemy.GetComponent<EnemyComponent>();
            if (enemyComp != null)
            {
                enemyComp.Damage *= multiplier;
            }

            // Logar quando a dificuldade (multiplicador) aumentar o suficiente
            // para avisar que os inimigos ficaram mais fortes.
            const float logThreshold = 0.05f; // loga a cada +5% de aumento
            if (multiplier >= _lastLoggedMultiplier + logThreshold)
            {
                var ts = System.TimeSpan.FromSeconds(_elapsedTime);
                Console.WriteLine($"[EnemySpawn] {ts:mm\\:ss} - Dificuldade aumentou: multiplicador = {multiplier:F2}. Inimigos mais fortes e spawn mais rápido.");
                _lastLoggedMultiplier = multiplier;
            }

            // (Opcional) pode também ajustar velocidade/ai etc.
        }

        private Vector2 GetRandomSpawnPosition()
        {
            // ...existing implementation...
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