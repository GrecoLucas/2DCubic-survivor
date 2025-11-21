namespace CubeSurvivor
{
    /// <summary>
    /// Configurações globais do jogo
    /// </summary>
    public static class GameConfig
    {
        // Configurações de tela
        public const int ScreenWidth = 1080;
        public const int ScreenHeight = 720;

        // Limites do mapa
        public const int MapWidth = 2000;
        public const int MapHeight = 2000;

        // Configurações de projéteis do jogador
        public const float PlayerBulletSpeed = 600f;
        public const float PlayerBulletDamage = 25f;
        public const float PlayerBulletSpawnOffset = 30f;
        public const float PlayerBulletLifetime = 5f;

        // Configurações de spawn de inimigos
        public const float EnemySpawnInterval = 5f; // Reduzido: spawn a cada 5 segundos (antes era 2s)
        public const int MaxEnemies = 50; // Reduzido: máximo de 50 inimigos (antes 100)

        // Configurações do jogador
        public const float PlayerMoveSpeed = 200f;
        public const float PlayerHealth = 100f;
        public const float PlayerSize = 50f;

        // Configurações de inimigos
        public const float EnemyMoveSpeed = 150f;
        public const float EnemyHealth = 50f;
        public const float EnemySize = 40f;
        public const float EnemyDamage = 10f;
        public const float EnemyAttackCooldown = 1f;

        // Configurações de zonas seguras (casas, estruturas)
        public const int WallBlockSize = 50;
        public const int DefaultHouseWidth = 400;
        public const int DefaultHouseHeight = 300;

        // Configurações de construção e recursos
        public const float PlayerBuildRange = 300f;
        public const int WoodPerCrate = 4;
        public const float WoodSpawnIntervalSeconds = 30f;

        // Taxa de aumento da dificuldade por minuto (ex: 0.1 = +10% por minuto)
        public const float EnemyDifficultyIncreasePerMinute = 0.90f;

        // Menor intervalo possível entre spawns (segundos)
        public const float EnemySpawnIntervalMin = 0.02f;

        /// <summary>
        /// Retorna o multiplicador de dificuldade baseado no tempo (segundos).
        /// Ex: após 2 minutos e r=0.1 => m = 1 + 2 * 0.1 = 1.2
        /// </summary>
        public static float GetEnemyDifficultyMultiplier(float elapsedSeconds)
        {
            float minutes = elapsedSeconds / 60f;
            return 1f + minutes * EnemyDifficultyIncreasePerMinute;
        }
    
            /// <summary>
        /// Retorna o intervalo atual de spawn aplicado sobre um intervalo base,
        /// reduzido conforme a dificuldade aumenta, com um piso em EnemySpawnIntervalMin.
        /// </summary>
        public static float GetSpawnInterval(float baseInterval, float elapsedSeconds)
        {
            float m = GetEnemyDifficultyMultiplier(elapsedSeconds);
            float scaled = baseInterval / m;
            return System.MathF.Max(scaled, EnemySpawnIntervalMin);
        }
    }
    
}

