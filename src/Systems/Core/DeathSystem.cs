using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema que remove entidades mortas (sem vida) e processa loot drops
    /// </summary>
    public sealed class DeathSystem : GameSystem
    {
        private readonly Random _random;
        private readonly BrainEntityFactory _brainFactory;
        
        public DeathSystem(TextureManager textureManager = null)
        {
            _random = new Random();
            _brainFactory = new BrainEntityFactory();
            if (textureManager != null)
            {
                _brainFactory.SetTextureManager(textureManager);
            }
        }
        
        public override void Update(GameTime gameTime)
        {
            var entitiesToRemove = new List<Entity>();

            // Usar ToList() para evitar modificação durante iteração
            foreach (var entity in World.GetEntitiesWithComponent<HealthComponent>().ToList())
            {
                var health = entity.GetComponent<HealthComponent>();
                if (health == null || !health.Enabled)
                    continue;

                // Se a entidade não está viva, marcar para remoção
                if (!health.IsAlive)
                {
                    // Processar loot drop antes de remover
                    ProcessLootDrop(entity);
                    
                    // Se for inimigo, conceder XP ao jogador antes de remover
                    if (entity.GetComponent<EnemyComponent>() != null)
                    {
                        // Buscar primeiro jogador disponível
                        Entity player = null;
                        foreach (var e in World.GetEntitiesWithComponent<PlayerInputComponent>())
                        {
                            player = e;
                            break;
                        }

                        if (player != null)
                        {
                            var xp = player.GetComponent<XpComponent>();
                            if (xp != null)
                            {
                                const float xpPerEnemy = 20f; // cada inimigo vale 20 XP
                                bool leveled = xp.AddXp(xpPerEnemy);
                                if (leveled)
                                {
                                    // Apenas marcar que há um nível pendente, sem abrir o menu automaticamente
                                    xp.HasPendingLevelUp = true;
                                }
                            }
                        }
                    }

                    entitiesToRemove.Add(entity);
                }
            }

            // Remover entidades mortas
            foreach (var entity in entitiesToRemove)
            {
                World.RemoveEntity(entity);
            }
        }
        
        private void ProcessLootDrop(Entity entity)
        {
            var lootComp = entity.GetComponent<LootDropComponent>();
            if (lootComp == null || !lootComp.Enabled)
                return;
            
            var transform = entity.GetComponent<TransformComponent>();
            if (transform == null)
                return;
            
            // Processar cada possível loot
            foreach (var loot in lootComp.PossibleLoots)
            {
                // Rolar chance de drop
                float roll = (float)_random.NextDouble();
                
                if (roll <= loot.DropChance)
                {
                    // Dropar item baseado no ID
                    SpawnLootItem(loot.ItemId, transform.Position);
                }
            }
        }
        
        private void SpawnLootItem(string itemId, Vector2 position)
        {
            switch (itemId)
            {
                case "brain":
                    _brainFactory.CreateBrain(World, position);
                    break;
                // Futuros items podem ser adicionados aqui
                default:
                    break;
            }
        }
    }
}