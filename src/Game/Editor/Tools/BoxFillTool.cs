using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.Editor.Diagnostics;

namespace CubeSurvivor.Game.Editor.Tools
{
    /// <summary>
    /// Box Fill tool: click-drag to define rectangle, fill on release.
    /// </summary>
    public class BoxFillTool : IEditorTool
    {
        private Point? _startTile;
        private Point? _currentTile;

        public void OnMouseDown(Point tilePos, MouseState mouseState, EditorContext context)
        {
            _startTile = tilePos;
            _currentTile = tilePos;
            EditorLogger.Log("BoxFillTool", $"Start drag at tile={tilePos}");
        }

        public void OnMouseDrag(Point tilePos, MouseState mouseState, EditorContext context)
        {
            _currentTile = tilePos;
        }

        public void OnMouseUp(Point tilePos, MouseState mouseState, EditorContext context)
        {
            if (_startTile.HasValue && context.Map != null)
            {
                Rectangle rect = GetRect(_startTile.Value, tilePos);
                EditorLogger.Log("BoxFillTool", $"=== BOX FILL === from {_startTile.Value} to {tilePos} -> rect={rect}");
                FillRect(rect, context);
                context.IsDirty = true;
            }

            _startTile = null;
            _currentTile = null;
        }

        private void FillRect(Rectangle rect, EditorContext context)
        {
            if (context.MapDefinition == null)
            {
                EditorLogger.LogError("BoxFillTool", "MapDefinition is null!");
                return;
            }

            int maxX = context.MapDefinition.MapWidth;
            int maxY = context.MapDefinition.MapHeight;
            int filledCount = 0;

            EditorLogger.Log("BoxFillTool", $"Filling rect: LayerKind={context.ActiveLayerKind} BrushId={context.ActiveBrushId}");

            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    // Bounds check
                    if (x < 0 || y < 0 || x >= maxX || y >= maxY)
                    {
                        continue;
                    }

                    // Use ActiveLayerKind to determine what to fill
                    switch (context.ActiveLayerKind)
                    {
                        case EditableLayerKind.Tiles:
                            context.Map.SetTileAt(x, y, context.ActiveBrushId, context.ActiveTileLayerIndex);
                            break;

                        case EditableLayerKind.Blocks:
                            context.Map.SetBlockAtTile(x, y, (BlockType)context.ActiveBrushId, context.ActiveBlockLayerIndex);
                            break;

                        case EditableLayerKind.ItemsLow:
                            context.Map.SetItemAtTile(x, y, (ItemType)context.ActiveBrushId, 0);
                            break;

                        case EditableLayerKind.ItemsHigh:
                            context.Map.SetItemAtTile(x, y, (ItemType)context.ActiveBrushId, 1);
                            break;
                    }
                    filledCount++;
                }
            }

            EditorLogger.Log("BoxFillTool", $"=== BOX FILL COMPLETE === filled {filledCount} tiles");
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font, EditorContext context, EditorCameraController camera, Rectangle canvasBounds)
        {
            // Draw preview rectangle
            if (_startTile.HasValue && _currentTile.HasValue)
            {
                Rectangle tileRect = GetRect(_startTile.Value, _currentTile.Value);
                int tileSize = context.MapDefinition?.TileSizePx ?? 32;

                Rectangle worldRect = new Rectangle(
                    tileRect.X * tileSize,
                    tileRect.Y * tileSize,
                    tileRect.Width * tileSize,
                    tileRect.Height * tileSize
                );

                Point screenTopLeft = camera.WorldToScreen(worldRect.Location.ToVector2(), canvasBounds);
                Point screenBottomRight = camera.WorldToScreen(new Vector2(worldRect.Right, worldRect.Bottom), canvasBounds);
                Rectangle screenRect = new Rectangle(
                    screenTopLeft.X,
                    screenTopLeft.Y,
                    screenBottomRight.X - screenTopLeft.X,
                    screenBottomRight.Y - screenTopLeft.Y
                );

                spriteBatch.Draw(pixelTexture, screenRect, new Color(100, 200, 255, 80));
                DrawBorder(spriteBatch, pixelTexture, screenRect, Color.Cyan, 2);
            }
        }

        private Rectangle GetRect(Point a, Point b)
        {
            int minX = System.Math.Min(a.X, b.X);
            int minY = System.Math.Min(a.Y, b.Y);
            int maxX = System.Math.Max(a.X, b.X) + 1;
            int maxY = System.Math.Max(a.Y, b.Y) + 1;
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

