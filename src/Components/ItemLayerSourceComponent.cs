using CubeSurvivor.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente que marca um item como vindo de uma ItemLayer.
    /// Quando o item é coletado, ele deve ser removido da ItemLayer correspondente.
    /// </summary>
    public sealed class ItemLayerSourceComponent : Component
    {
        public int LayerIndex { get; } // 0 = ItemsLow, 1 = ItemsHigh
        public int TileX { get; }
        public int TileY { get; }

        public ItemLayerSourceComponent(int layerIndex, int tileX, int tileY)
        {
            LayerIndex = layerIndex;
            TileX = tileX;
            TileY = tileY;
        }
    }
}

