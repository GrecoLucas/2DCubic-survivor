using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente para itens que podem ser coletados no mundo.
    /// </summary>
    public sealed class PickupComponent : Component
    {
        public IItem Item { get; }
        public int Quantity { get; }
        public float PickupRadius { get; set; }
        
        public PickupComponent(IItem item, int quantity = 1, float pickupRadius = 50f)
        {
            Item = item;
            Quantity = quantity;
            PickupRadius = pickupRadius;
        }
    }
}

