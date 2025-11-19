using CubeSurvivor.Components;
using CubeSurvivor.Inventory.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Inventory.Items.Weapons
{
    /// <summary>
    /// Item de arma de fogo (Gun).
    /// Implementação concreta de WeaponItem.
    /// </summary>
    public sealed class GunItem : WeaponItem
    {
        public float BulletSpeed { get; }
        public float ShootCooldown { get; }
        
        public GunItem()
            : base(
                id: "gun",
                name: "Gun",
                description: "A basic firearm",
                damage: 25f,
                attackSpeed: 5f, // 5 shots per second
                range: 600f,
                iconColor: Color.Black)
        {
            BulletSpeed = 600f;
            ShootCooldown = 0.2f;
        }
        
        private GunItem(GunItem original)
            : base(
                original.Id,
                original.Name,
                original.Description,
                original.Damage,
                original.AttackSpeed,
                original.Range,
                original.IconColor)
        {
            BulletSpeed = original.BulletSpeed;
            ShootCooldown = original.ShootCooldown;
            // Copiar textura do original
            IconTexture = original.IconTexture;
        }
        
        protected override void EquipVisuals(CubeSurvivor.Core.Entity holder)
        {
            // Adicionar o componente visual GunComponent
            if (!holder.HasComponent<GunComponent>())
            {
                holder.AddComponent(new GunComponent());
            }
            
            // Atualizar WeaponComponent se existir
            var weaponComp = holder.GetComponent<WeaponComponent>();
            if (weaponComp != null)
            {
                weaponComp.Enabled = true;
            }
        }
        
        protected override void UnequipVisuals(CubeSurvivor.Core.Entity holder)
        {
            // Desabilitar o componente visual
            var weaponComp = holder.GetComponent<WeaponComponent>();
            if (weaponComp != null)
            {
                weaponComp.Enabled = false;
            }
        }
        
        public override bool OnUse(CubeSurvivor.Core.Entity user)
        {
            // O disparo será tratado pelo PlayerInputSystem
            // Este método pode ser usado para lógica adicional se necessário
            return true;
        }
        
        public override IItem Clone()
        {
            return new GunItem(this);
        }
    }
}

