using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Inventory.Core
{
    /// <summary>
    /// Interface base para todos os itens do jogo.
    /// Segue o Interface Segregation Principle (ISP).
    /// </summary>
    public interface IItem
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        ItemType Type { get; }
        int MaxStackSize { get; }
        Color IconColor { get; }
        Texture2D IconTexture { get; set; }
        
        /// <summary>
        /// Chamado quando o item é equipado/segurado.
        /// </summary>
        void OnEquip(CubeSurvivor.Core.Entity holder);
        
        /// <summary>
        /// Chamado quando o item é desequipado.
        /// </summary>
        void OnUnequip(CubeSurvivor.Core.Entity holder);
        
        /// <summary>
        /// Chamado quando o item é usado (clique).
        /// </summary>
        bool OnUse(CubeSurvivor.Core.Entity user);
        
        /// <summary>
        /// Cria uma cópia do item.
        /// </summary>
        IItem Clone();
    }
}

