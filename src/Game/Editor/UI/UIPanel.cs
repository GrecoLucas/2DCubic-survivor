using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Container panel with background and child elements.
    /// Children use LOCAL coordinates relative to panel origin.
    /// </summary>
    public class UIPanel : UIElement
    {
        public Color BackgroundColor { get; set; } = new Color(30, 30, 30, 220);
        
        private List<UIElement> _children = new List<UIElement>();
        public List<UIElement> Children => _children;
        
        public bool Scrollable { get; set; }
        public int ScrollOffset { get; set; }
        public int MaxScrollOffset { get; private set; }

        private int _previousScrollValue;
        
        /// <summary>
        /// Adds a child and sets its Parent reference.
        /// </summary>
        public void AddChild(UIElement child)
        {
            if (child == null) return;
            child.Parent = this;
            _children.Add(child);
        }
        
        /// <summary>
        /// Removes a child.
        /// </summary>
        public void RemoveChild(UIElement child)
        {
            if (child == null) return;
            child.Parent = null;
            _children.Remove(child);
        }
        
        /// <summary>
        /// Clears all children.
        /// </summary>
        public void ClearChildren()
        {
            foreach (var child in _children)
            {
                child.Parent = null;
            }
            _children.Clear();
        }

        public override void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
        {
            if (!Visible || !Enabled) return;

            // Handle scroll wheel (using GlobalBounds for hit test)
            if (Scrollable && HitTest(mouseState.Position))
            {
                int scrollDelta = mouseState.ScrollWheelValue - _previousScrollValue;
                ScrollOffset -= scrollDelta / 10;
                ScrollOffset = System.Math.Max(0, System.Math.Min(ScrollOffset, MaxScrollOffset));
            }
            _previousScrollValue = mouseState.ScrollWheelValue;

            // Update children (they use GlobalBounds automatically)
            foreach (var child in Children)
            {
                child.Update(gameTime, mouseState, previousMouseState);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            if (!Visible) return;

            Rectangle globalBounds = GlobalBounds;

            // Background
            spriteBatch.Draw(pixelTexture, globalBounds, BackgroundColor);

            // Draw children (they use their GlobalBounds automatically)
            foreach (var child in Children)
            {
                child.Draw(spriteBatch, font, pixelTexture);
            }

            // Border
            DrawBorder(spriteBatch, pixelTexture, globalBounds, new Color(60, 60, 60), 2);
        }

        protected void DrawBorder(SpriteBatch sb, Texture2D px, Rectangle rect, Color color, int width)
        {
            sb.Draw(px, new Rectangle(rect.X, rect.Y, rect.Width, width), color);
            sb.Draw(px, new Rectangle(rect.X, rect.Bottom - width, rect.Width, width), color);
            sb.Draw(px, new Rectangle(rect.X, rect.Y, width, rect.Height), color);
            sb.Draw(px, new Rectangle(rect.Right - width, rect.Y, width, rect.Height), color);
        }

        public void CalculateScrollMax(int contentHeight)
        {
            MaxScrollOffset = System.Math.Max(0, contentHeight - Bounds.Height);
        }
    }
}
