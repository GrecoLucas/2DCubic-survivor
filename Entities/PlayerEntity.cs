using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar a entidade do jogador
    /// </summary>
    public static class PlayerEntity
    {
        public static Entity Create(GameWorld world, Vector2 position)
        {
            var player = world.CreateEntity("Player");

            // Adicionar componentes ao jogador
            player.AddComponent(new TransformComponent(position));
            player.AddComponent(new SpriteComponent(Color.Blue, 50f, 50f)); // Quadrado azul 50x50
            player.AddComponent(new VelocityComponent(200f)); // Velocidade de 200 pixels/segundo
            player.AddComponent(new InputComponent());
            player.AddComponent(new HealthComponent(100f)); // 100 de vida
            player.AddComponent(new ColliderComponent(50f, 50f, "Player"));
            player.AddComponent(new GunComponent()); // Arma preta

            return player;
        }
    }
}