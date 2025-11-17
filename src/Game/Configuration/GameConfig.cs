namespace CubeSurvivor
{
    /// <summary>
    /// Configurações globais do jogo
    /// </summary>
    public static class GameConfig
    {
        // Configurações de tela
        public const int ScreenWidth = 1280;
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
        public const float EnemySpawnInterval = 2f;
        public const int MaxEnemies = 100;

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
    }
}

