using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Base class for all UI elements.
    /// CRITICAL: Bounds are LOCAL to parent. Use GlobalBounds for screen-space operations.
    /// </summary>
    public abstract class UIElement
    {
        /// <summary>
        /// Local bounds relative to parent. (0,0) is parent's top-left.
        /// </summary>
        public Rectangle Bounds { get; set; }
        
        /// <summary>
        /// Parent element in UI tree. Null for root elements.
        /// </summary>
        public UIElement Parent { get; set; }
        
        /// <summary>
        /// Computed global bounds in screen space.
        /// Recursively adds parent offsets.
        /// </summary>
        public Rectangle GlobalBounds
        {
            get
            {
                if (Parent == null)
                {
                    return Bounds; // Root element - bounds are already global
                }
                
                Rectangle parentGlobal = Parent.GlobalBounds;
                return new Rectangle(
                    parentGlobal.X + Bounds.X,
                    parentGlobal.Y + Bounds.Y,
                    Bounds.Width,
                    Bounds.Height
                );
            }
        }
        
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        
        public virtual void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState) { }
        public virtual void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture) { }
        
        /// <summary>
        /// Hit test using GLOBAL bounds (screen space).
        /// </summary>
        public virtual bool HitTest(Point screenPoint) => GlobalBounds.Contains(screenPoint);
        
        public bool IsMouseOver(MouseState mouseState) => Visible && Enabled && HitTest(mouseState.Position);
    }
}
