using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Scrollable grid of items with thumbnails (for palette).
    /// Each item shows icon + label.
    /// </summary>
    public class UIScrollGrid : UIElement
    {
        public class GridItem
        {
            public string Id { get; set; }
            public string Label { get; set; }
            public Texture2D Icon { get; set; }
            public Color FallbackColor { get; set; } = Color.Gray;
        }

        public List<GridItem> Items { get; } = new List<GridItem>();
        public int SelectedIndex { get; set; } = -1;
        public Action<int> OnItemSelected { get; set; }
        public Color BackgroundColor { get; set; } = new Color(40, 40, 40);
        public int ItemSize { get; set; } = 56;
        public int Padding { get; set; } = 4;
        public int Columns { get; set; } = 3;

        private int _scrollOffset;
        private int _previousScrollValue;

        public override void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
        {
            if (!Visible || !Enabled) return;

            Rectangle globalBounds = GlobalBounds;

            // Handle scroll
            if (HitTest(mouseState.Position))
            {
                int scrollDelta = mouseState.ScrollWheelValue - _previousScrollValue;
                _scrollOffset -= scrollDelta / 10;
                _scrollOffset = System.Math.Max(0, _scrollOffset);
            }
            _previousScrollValue = mouseState.ScrollWheelValue;

            // Handle clicks (use GlobalBounds for coordinate conversion)
            if (mouseState.LeftButton == ButtonState.Pressed && 
                previousMouseState.LeftButton == ButtonState.Released &&
                HitTest(mouseState.Position))
            {
                Point local = new Point(mouseState.X - globalBounds.X, mouseState.Y - globalBounds.Y + _scrollOffset);
                int col = local.X / (ItemSize + Padding);
                int row = local.Y / (ItemSize + Padding);
                int index = row * Columns + col;

                if (index >= 0 && index < Items.Count)
                {
                    SelectedIndex = index;
                    OnItemSelected?.Invoke(index);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            if (!Visible) return;

            Rectangle globalBounds = GlobalBounds;

            // Background
            spriteBatch.Draw(pixelTexture, globalBounds, BackgroundColor);

            // Draw items
            for (int i = 0; i < Items.Count; i++)
            {
                GridItem item = Items[i];
                int col = i % Columns;
                int row = i / Columns;

                int itemX = globalBounds.X + Padding + col * (ItemSize + Padding);
                int itemY = globalBounds.Y + Padding + row * (ItemSize + Padding) - _scrollOffset;

                // Skip if out of view
                if (itemY + ItemSize < globalBounds.Y || itemY > globalBounds.Bottom) continue;

                Rectangle itemRect = new Rectangle(itemX, itemY, ItemSize, ItemSize);

                // Draw icon or fallback
                if (item.Icon != null)
                {
                    spriteBatch.Draw(item.Icon, itemRect, Color.White);
                }
                else
                {
                    spriteBatch.Draw(pixelTexture, itemRect, item.FallbackColor);
                }

                // Selection highlight
                if (i == SelectedIndex)
                {
                    DrawBorder(spriteBatch, pixelTexture, itemRect, Color.Cyan, 2);
                }
                else
                {
                    DrawBorder(spriteBatch, pixelTexture, itemRect, new Color(60, 60, 60), 1);
                }

                // Label: draw inside the bottom area of the item to guarantee visibility
                if (font != null)
                {
                    string labelToUse = item.Label;
                    if (string.IsNullOrEmpty(labelToUse) && !string.IsNullOrEmpty(item.Id))
                    {
                        var parts = item.Id.Replace('-', ' ').Replace('_', ' ').Split(' ');
                        for (int p = 0; p < parts.Length; p++)
                        {
                            if (string.IsNullOrEmpty(parts[p])) continue;
                            parts[p] = char.ToUpper(parts[p][0]) + (parts[p].Length > 1 ? parts[p].Substring(1) : string.Empty);
                        }
                        labelToUse = string.Join(" ", parts);
                    }

                    if (!string.IsNullOrEmpty(labelToUse))
                    {
                        string safeLabel = FontUtil.SanitizeForFont(font, labelToUse);

                        // Truncate label if it's wider than the item area, adding ellipsis
                        float maxWidth = ItemSize - 6; // small padding
                        string finalLabel = safeLabel;
                        if (font.MeasureString(finalLabel).X > maxWidth)
                        {
                            finalLabel = TruncateTextToWidth(font, safeLabel, maxWidth);
                        }

                        Vector2 textSize = font.MeasureString(finalLabel);

                        // Position text inside the item rectangle near the bottom
                        Vector2 textPos = new Vector2(
                            itemX + (ItemSize - textSize.X) / 2,
                            itemY + ItemSize - textSize.Y - 4
                        );

                        // Ensure the text is at least within the visible control vertically
                        if (textPos.Y + textSize.Y >= globalBounds.Y && textPos.Y < globalBounds.Bottom)
                        {
                            spriteBatch.DrawString(font, finalLabel, textPos, Color.White);
                        }
                    }
                }
            }
        }

        private static string TruncateTextToWidth(SpriteFont font, string text, float maxWidth)
        {
            if (string.IsNullOrEmpty(text) || font == null) return text;
            if (font.MeasureString(text).X <= maxWidth) return text;

            const string ellipsis = "...";
            int lo = 0, hi = text.Length;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                string sub = text.Substring(0, mid) + ellipsis;
                if (font.MeasureString(sub).X > maxWidth) hi = mid;
                else lo = mid + 1;
            }

            int cut = Math.Max(0, lo - 1);
            string result = text.Substring(0, cut) + ellipsis;
            return result;
        }

        protected void DrawBorder(SpriteBatch sb, Texture2D px, Rectangle rect, Color color, int width)
        {
            sb.Draw(px, new Rectangle(rect.X, rect.Y, rect.Width, width), color);
            sb.Draw(px, new Rectangle(rect.X, rect.Bottom - width, rect.Width, width), color);
            sb.Draw(px, new Rectangle(rect.X, rect.Y, width, rect.Height), color);
            sb.Draw(px, new Rectangle(rect.Right - width, rect.Y, width, rect.Height), color);
        }
    }
}


