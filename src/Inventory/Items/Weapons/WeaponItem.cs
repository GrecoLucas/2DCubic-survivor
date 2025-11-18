using CubeSurvivor.Inventory.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Inventory.Items.Weapons
{
    /// <summary>
    /// Classe base para itens de arma.
    /// Extende Item e adiciona propriedades específicas de armas.
    /// </summary>
    public abstract class WeaponItem : Item
    {
        public float Damage { get; protected set; }
        public float AttackSpeed { get; protected set; }
        public float Range { get; protected set; }
        
        protected WeaponItem(
            string id,
            string name,
            string description,
            float damage,
            float attackSpeed,
            float range,
            Color? iconColor = null)
            : base(id, name, description, ItemType.Weapon, maxStackSize: 1, iconColor: iconColor)
        {
            Damage = damage;
            AttackSpeed = attackSpeed;
            Range = range;
        }
        
        public override void OnEquip(CubeSurvivor.Core.Entity holder)
        {
            base.OnEquip(holder);
            // Adicionar componente visual da arma ao holder
            EquipVisuals(holder);
        }
        
        public override void OnUnequip(CubeSurvivor.Core.Entity holder)
        {
            base.OnUnequip(holder);
            // Remover componente visual da arma do holder
            UnequipVisuals(holder);
        }
        
        protected abstract void EquipVisuals(CubeSurvivor.Core.Entity holder);
        protected abstract void UnequipVisuals(CubeSurvivor.Core.Entity holder);
    }
}

