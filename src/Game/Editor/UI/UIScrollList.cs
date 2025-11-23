using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Scrollable list with proper clipping and coordinate handling.
    /// Items are positioned in LOCAL coordinates and clipped to viewport.
    /// </summary>
    public class UIScrollList : UIElement
    {
        private List<UIElement> _items = new List<UIElement>();
        
        public Color BackgroundColor { get; set; } = new Color(30, 30, 30, 220);
        public int ItemHeight { get; set; } = 80;
        public int Padding { get; set; } = 10;
        public float ScrollOffset { get; set; }
        public float MaxScrollOffset { get; private set; }

        private int _previousScrollValue;
        private RasterizerState _scissorRasterizer;

        public UIScrollList()
        {
            _scissorRasterizer = new RasterizerState
            {
                CullMode = CullMode.None,
                ScissorTestEnable = true
            };
        }
        
        /// <summary>
        /// Adds an item and sets its Parent reference.
        /// </summary>
        public void AddItem(UIElement item)
        {
            if (item == null) return;
            item.Parent = this;
            _items.Add(item);
        }
        
        /// <summary>
        /// Clears all items.
        /// </summary>
        public void Clear()
        {
            foreach (var item in _items)
            {
                item.Parent = null;
            }
            _items.Clear();
            ScrollOffset = 0;
        }
        
        public override void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
        {
            if (!Visible || !Enabled) return;

            // Handle scroll wheel (using GlobalBounds for hit test)
            if (HitTest(mouseState.Position))
            {
                int scrollDelta = mouseState.ScrollWheelValue - _previousScrollValue;
                ScrollOffset -= scrollDelta / 10f; // Smooth scroll
                
                // Calculate max scroll based on content height
                int contentHeight = _items.Count * (ItemHeight + Padding) + Padding;
                MaxScrollOffset = Math.Max(0, contentHeight - Bounds.Height);
                
                ScrollOffset = Math.Max(0, Math.Min(ScrollOffset, MaxScrollOffset));
            }
            _previousScrollValue = mouseState.ScrollWheelValue;

            // Layout items in LOCAL coordinates with scroll offset
            int localY = Padding - (int)ScrollOffset;
            foreach (var item in _items)
            {
                // Set LOCAL position relative to this scroll list
                item.Bounds = new Rectangle(
                    Padding,
                    localY,
                    Bounds.Width - Padding * 2,
                    ItemHeight
                );

                // Only update if visible (check GlobalBounds against our GlobalBounds)
                Rectangle itemGlobal = item.GlobalBounds;
                Rectangle ourGlobal = GlobalBounds;
                
                if (itemGlobal.Bottom >= ourGlobal.Y && itemGlobal.Y <= ourGlobal.Bottom)
                {
                    item.Update(gameTime, mouseState, previousMouseState);
                }

                localY += ItemHeight + Padding;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            if (!Visible) return;

            Rectangle globalBounds = GlobalBounds;
            GraphicsDevice graphicsDevice = spriteBatch.GraphicsDevice;

            // Background
            spriteBatch.Draw(pixelTexture, globalBounds, BackgroundColor);

            // Save old scissor rectangle
            Rectangle oldScissor = graphicsDevice.ScissorRectangle;
            
            // End current batch to apply scissor
            spriteBatch.End();
            
            // Set scissor rectangle to clip items to list bounds
            graphicsDevice.ScissorRectangle = globalBounds;
            
            // Restart batch with scissor test enabled
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null,
                _scissorRasterizer
            );

            // Draw items - only visible ones
            foreach (var item in _items)
            {
                Rectangle itemGlobal = item.GlobalBounds;
                
                // Cull items outside viewport
                if (itemGlobal.Bottom >= globalBounds.Y && itemGlobal.Y <= globalBounds.Bottom)
                {
                    item.Draw(spriteBatch, font, pixelTexture);
                }
            }

            // End scissor batch
            spriteBatch.End();
            
            // Restore scissor
            graphicsDevice.ScissorRectangle = oldScissor;
            
            // Restart normal batch for scrollbar and rest of UI
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp
            );

            // Scrollbar indicator (drawn outside scissor)
            if (MaxScrollOffset > 0)
            {
                DrawScrollbar(spriteBatch, pixelTexture, globalBounds);
            }
        }

        private void DrawScrollbar(SpriteBatch spriteBatch, Texture2D pixelTexture, Rectangle globalBounds)
        {
            int scrollbarWidth = 8;
            int scrollbarX = globalBounds.Right - scrollbarWidth - 2;

            // Track
            Rectangle track = new Rectangle(scrollbarX, globalBounds.Y, scrollbarWidth, globalBounds.Height);
            spriteBatch.Draw(pixelTexture, track, new Color(60, 60, 60, 180));

            // Thumb
            float scrollPercentage = ScrollOffset / MaxScrollOffset;
            int thumbHeight = Math.Max(20, (int)(globalBounds.Height * ((float)globalBounds.Height / (globalBounds.Height + MaxScrollOffset))));
            int thumbY = globalBounds.Y + (int)((globalBounds.Height - thumbHeight) * scrollPercentage);

            Rectangle thumb = new Rectangle(scrollbarX, thumbY, scrollbarWidth, thumbHeight);
            spriteBatch.Draw(pixelTexture, thumb, new Color(120, 120, 120, 220));
        }
    }
}

