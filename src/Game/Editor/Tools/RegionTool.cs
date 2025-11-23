using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Map;

namespace CubeSurvivor.Game.Editor.Tools
{
    /// <summary>
    /// Region tool: click-drag to create spawn/safe regions.
    /// </summary>
    public class RegionTool : IEditorTool
    {
        private Vector2? _startWorld;
        private Vector2? _currentWorld;

        public void OnMouseDown(Point tilePos, MouseState mouseState, EditorContext context)
        {
            _startWorld = tilePos.ToVector2() * (context.MapDefinition?.TileSizePx ?? 32);
        }

        public void OnMouseDrag(Point tilePos, MouseState mouseState, EditorContext context)
        {
            _currentWorld = tilePos.ToVector2() * (context.MapDefinition?.TileSizePx ?? 32);
        }

        public void OnMouseUp(Point tilePos, MouseState mouseState, EditorContext context)
        {
            if (_startWorld.HasValue && context.MapDefinition != null)
            {
                Vector2 end = tilePos.ToVector2() * context.MapDefinition.TileSizePx;
                Rectangle rect = GetRect(_startWorld.Value, end);

                if (rect.Width > 0 && rect.Height > 0)
                {
                    // Create new region
                    string id = $"{context.PendingRegionType}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                    RegionDefinition region = new RegionDefinition
                    {
                        Id = id,
                        Type = context.PendingRegionType,
                        Area = rect,
                        Meta = new System.Collections.Generic.Dictionary<string, string>(context.PendingRegionMeta)
                    };

                    context.MapDefinition.Regions.Add(region);
                    context.IsDirty = true;
                }
            }

            _startWorld = null;
            _currentWorld = null;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font, EditorContext context, EditorCameraController camera, Rectangle canvasBounds)
        {
            if (_startWorld.HasValue && _currentWorld.HasValue)
            {
                Rectangle worldRect = GetRect(_startWorld.Value, _currentWorld.Value);

                Point screenTopLeft = camera.WorldToScreen(worldRect.Location.ToVector2(), canvasBounds);
                Point screenBottomRight = camera.WorldToScreen(new Vector2(worldRect.Right, worldRect.Bottom), canvasBounds);
                Rectangle screenRect = new Rectangle(
                    screenTopLeft.X,
                    screenTopLeft.Y,
                    screenBottomRight.X - screenTopLeft.X,
                    screenBottomRight.Y - screenTopLeft.Y
                );

                spriteBatch.Draw(pixelTexture, screenRect, new Color(255, 200, 100, 80));
                DrawBorder(spriteBatch, pixelTexture, screenRect, Color.Orange, 2);
            }
        }

        private Rectangle GetRect(Vector2 a, Vector2 b)
        {
            int minX = (int)Math.Min(a.X, b.X);
            int minY = (int)Math.Min(a.Y, b.Y);
            int maxX = (int)Math.Max(a.X, b.X);
            int maxY = (int)Math.Max(a.Y, b.Y);
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        private void DrawBorder(SpriteBatch sb, Texture2D px, Rectangle rect, Color color, int width)
        {
            sb.Draw(px, new Rectangle(rect.X, rect.Y, rect.Width, width), color);
            sb.Draw(px, new Rectangle(rect.X, rect.Bottom - width, rect.Width, width), color);
            sb.Draw(px, new Rectangle(rect.X, rect.Y, width, rect.Height), color);
            sb.Draw(px, new Rectangle(rect.Right - width, rect.Y, width, rect.Height), color);
        }
    }
}

