using System.Linq;
using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using CubeSurvivor.Inventory.Components;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema responsável por processar solicitações de construção do jogador.
    /// Verifica recursos, alcance, colisões e cria caixas usando IWorldObjectFactory.
    /// </summary>
    public sealed class ConstructionSystem : GameSystem
    {
        private readonly IWorldObjectFactory _worldObjectFactory;

        public ConstructionSystem(IWorldObjectFactory worldObjectFactory)
        {
            _worldObjectFactory = worldObjectFactory;
        }

        public override void Update(GameTime gameTime)
        {
            // Criar snapshot da lista para evitar "Collection was modified" ao criar novas entidades
            var builders = World.GetEntitiesWithComponent<BuilderComponent>().ToList();
            
            foreach (var entity in builders)
            {
                var builder = entity.GetComponent<BuilderComponent>();
                var inventoryComp = entity.GetComponent<InventoryComponent>();
                var transform = entity.GetComponent<TransformComponent>();

                if (builder == null || inventoryComp == null || transform == null || !builder.Enabled)
                    continue;

                if (!builder.RequestedBuildPosition.HasValue)
                    continue;

                var targetPos = builder.RequestedBuildPosition.Value;

                try
                {
                    TryBuildCrate(entity, builder, inventoryComp, transform, targetPos);
                }
                finally
                {
                    // Limpar solicitação independente do sucesso
                    builder.ClearRequest();
                }
            }
        }

        private void TryBuildCrate(
            Entity player,
            BuilderComponent builder,
            InventoryComponent inventoryComp,
            TransformComponent playerTransform,
            Vector2 targetPos)
        {
            var inventory = inventoryComp.Inventory;
            if (inventory == null)
                return;

            // 1. Verificar se tem martelo no inventário
            if (!inventory.HasItem("hammer", 1))
            {
                System.Console.WriteLine("[Construction] ⚠ Precisa de um Hammer para construir!");
                return;
            }

            // 2. Verificar quantidade de madeira
            if (!inventory.HasItem("wood", GameConfig.WoodPerCrate))
            {
                System.Console.WriteLine($"[Construction] ⚠ Precisa de {GameConfig.WoodPerCrate} Wood para construir! (Tem: {inventory.GetAllStacks().Where(s => s.Item?.Id == "wood").Sum(s => s.Quantity)})");
                return;
            }

            // 3. Verificar alcance de construção
            float distance = Vector2.Distance(playerTransform.Position, targetPos);
            if (distance > builder.BuildRange)
            {
                System.Console.WriteLine($"[Construction] ⚠ Muito longe! Distância: {distance:F0} / {builder.BuildRange:F0}");
                return;
            }

            // 4. Encaixar posição numa grade baseada no tamanho da caixa
            float size = GameConfig.WallBlockSize;
            var snappedPos = new Vector2(
                (float)System.Math.Round(targetPos.X / size) * size,
                (float)System.Math.Round(targetPos.Y / size) * size
            );

            // 5. Verificar se a posição está livre (sem sobreposição de colliders)
            if (!IsPositionFree(snappedPos, size))
            {
                System.Console.WriteLine("[Construction] ⚠ Posição bloqueada!");
                return;
            }

            // 6. Consumir madeira
            if (!inventory.RemoveItem("wood", GameConfig.WoodPerCrate))
            {
                System.Console.WriteLine("[Construction] ✗ Falha ao consumir Wood");
                return;
            }

            // 7. Construir caixa (destrutível)
            _worldObjectFactory.CreateCrate(World, snappedPos, isDestructible: true, maxHealth: 50f);
            System.Console.WriteLine($"[Construction] ✓ Caixa construída em ({snappedPos.X}, {snappedPos.Y})!");
        }

        /// <summary>
        /// Verifica se a posição está livre de colliders existentes.
        /// </summary>
        private bool IsPositionFree(Vector2 center, float size)
        {
            var half = size / 2f;
            var rect = new Rectangle(
                (int)(center.X - half),
                (int)(center.Y - half),
                (int)size,
                (int)size
            );

            foreach (var e in World.GetEntitiesWithComponent<ColliderComponent>())
            {
                var c = e.GetComponent<ColliderComponent>();
                var t = e.GetComponent<TransformComponent>();
                if (c == null || t == null)
                    continue;

                var bounds = c.GetBounds(t.Position);
                if (bounds.Intersects(rect))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

