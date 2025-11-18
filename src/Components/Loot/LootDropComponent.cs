using CubeSurvivor.Core;
using System;
using System.Collections.Generic;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente que define loot que pode ser dropado ao morrer.
    /// </summary>
    public sealed class LootDropComponent : Component
    {
        public List<LootEntry> PossibleLoots { get; }
        
        public LootDropComponent()
        {
            PossibleLoots = new List<LootEntry>();
        }
        
        public void AddLoot(string itemId, float dropChance)
        {
            PossibleLoots.Add(new LootEntry(itemId, dropChance));
        }
    }
    
    /// <summary>
    /// Representa um item que pode ser dropado com sua chance.
    /// </summary>
    public sealed class LootEntry
    {
        public string ItemId { get; }
        public float DropChance { get; } // 0.0 a 1.0 (ex: 0.1 = 10%)
        
        public LootEntry(string itemId, float dropChance)
        {
            ItemId = itemId;
            DropChance = Math.Clamp(dropChance, 0f, 1f);
        }
    }
}

