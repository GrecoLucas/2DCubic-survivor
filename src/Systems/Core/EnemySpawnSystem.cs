using System;
using System.Linq;
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
        private readonly IEnemySpawnExclusionProvider _exclusionProvider;
        private readonly CubeSurvivor.Systems.World.BiomeSystem _biomeSystem;
        private readonly Random _random = new Random();
        // Último multiplicador logado para evitar spam de logs
        private float _lastLoggedMultiplier = 0.01f;
        private float _spawnTimer;
        private readonly float _spawnInterval;
        private readonly int _maxEnemies;
        private readonly Rectangle _spawnArea;

        // Novo: tempo acumulado desde o início (segundos)
        private float _elapsedTime;

        public EnemySpawnSystem(Rectangle spawnArea, IEnemyFactory enemyFactory, float spawnInterval = 2f, int maxEnemies = 50, IEnemySpawnExclusionProvider exclusionProvider = null, CubeSurvivor.Systems.World.BiomeSystem biomeSystem = null)
        {
            _spawnArea = spawnArea;
            _enemyFactory = enemyFactory;
            _spawnInterval = spawnInterval;
            _maxEnemies = maxEnemies;
            _exclusionProvider = exclusionProvider;
            _biomeSystem = biomeSystem;
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

            // Se o jogador estiver dentro de uma caverna, acelerar spawn e permitir mais inimigos
            bool playerInCave = false;
            if (_biomeSystem != null)
            {
                Entity player = null;
                foreach (var entity in World.GetEntitiesWithComponent<PlayerInputComponent>())
                {
                    player = entity;
                    break;
                }

                if (player != null)
                {
                    var t = player.GetComponent<TransformComponent>();
                    if (t != null)
                    {
                        var pb = _biomeSystem.GetBiomeAt(t.Position);
                        if (pb != null && pb.Type == CubeSurvivor.World.Biomes.BiomeType.Cave)
                        {
                            playerInCave = true;
                        }
                    }
                }
            }

            if (playerInCave)
            {
                // spawn mais rápido dentro da caverna
                currentInterval *= 0.08f; // 2.5x mais rápido
            }

            // Calcular máximo efetivo de inimigos (aumentado em cavernas)
            int effectiveMax = _maxEnemies;
            if (playerInCave)
            {
                effectiveMax = Math.Max(_maxEnemies, (int)(_maxEnemies * 1.8f));
            }

            // Spawnar novo inimigo se possível
            if (_spawnTimer >= currentInterval && enemyCount < effectiveMax)
            {
                _spawnTimer = 0f;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            // Verificar se o jogador está próximo da caverna antes de spawnar
            if (_biomeSystem != null)
            {
                Entity player = null;
                foreach (var entity in World.GetEntitiesWithComponent<PlayerInputComponent>())
                {
                    player = entity;
                    break;
                }

                if (player != null)
                {
                    var playerTransform = player.GetComponent<TransformComponent>();
                    if (playerTransform != null)
                    {
                        var playerBiome = _biomeSystem.GetBiomeAt(playerTransform.Position);
                        
                        // Só spawnar inimigos se o jogador estiver na caverna ou próximo dela (dentro de 400 pixels)
                        if (playerBiome == null || playerBiome.Type != CubeSurvivor.World.Biomes.BiomeType.Cave)
                        {
                            // Verificar se jogador está próximo da borda da caverna (x=2000)
                            const float proximityThreshold = 400f;
                            if (playerTransform.Position.X < 2000 - proximityThreshold)
                            {
                                // Jogador está longe da caverna, não spawnar
                                return;
                            }
                        }
                    }
                }
            }

            // Gerar posição aleatória nas bordas do spawn area
            Vector2 position = GetRandomSpawnPosition();

            // Se há um sistema de biomas configurado, garantir que o bioma nessa posição permite spawns
            if (_biomeSystem != null && !_biomeSystem.AllowsEnemySpawnsAt(position))
            {
                // não spawnar aqui; somente tentar novamente no próximo ciclo
                return;
            }

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
            const int maxAttempts = 20;
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2 position = GetRandomEdgePosition();

                // Se não há provider de exclusão, retornar posição diretamente
                if (_exclusionProvider == null)
                {
                    return position;
                }

                // Verificar se a posição está dentro de alguma zona de exclusão
                bool insideExclusion = _exclusionProvider
                    .GetExclusionZones()
                    .Any(rect => rect.Contains(position));

                if (!insideExclusion)
                {
                    return position;
                }
            }

            // Fallback: retornar uma posição sem verificação de exclusão
            // (caso patológico onde todas as tentativas caíram em zonas de exclusão)
            return GetRandomEdgePosition();
        }

        private Vector2 GetRandomEdgePosition()
        {
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