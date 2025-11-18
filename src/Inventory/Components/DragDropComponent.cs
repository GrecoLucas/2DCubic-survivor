using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Inventory.Components
{
    /// <summary>
    /// Componente que rastreia o estado de drag-and-drop do inventário.
    /// </summary>
    public sealed class DragDropComponent : Component
    {
        public bool IsDragging { get; set; }
        public int SourceSlotIndex { get; set; }
        public IItemStack DraggedStack { get; set; }
        public Vector2 DragPosition { get; set; }
        
        public DragDropComponent()
        {
            IsDragging = false;
            SourceSlotIndex = -1;
            DraggedStack = null;
            DragPosition = Vector2.Zero;
        }
        
        public void StartDrag(int slotIndex, IItemStack stack, Vector2 position)
        {
            IsDragging = true;
            SourceSlotIndex = slotIndex;
            DraggedStack = stack;
            DragPosition = position;
        }
        
        public void UpdateDragPosition(Vector2 position)
        {
            DragPosition = position;
        }
        
        public void EndDrag()
        {
            IsDragging = false;
            SourceSlotIndex = -1;
            DraggedStack = null;
            DragPosition = Vector2.Zero;
        }
    }
}

