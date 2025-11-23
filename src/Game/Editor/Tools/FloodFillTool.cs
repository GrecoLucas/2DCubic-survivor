using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.Editor.Diagnostics;

namespace CubeSurvivor.Game.Editor.Tools
{
    /// <summary>
    /// Flood Fill tool: fills contiguous area matching clicked tile.
    /// </summary>
    public class FloodFillTool : IEditorTool
    {
        public void OnMouseDown(Point tilePos, MouseState mouseState, EditorContext context)
        {
            EditorLogger.Log("FloodFillTool", $"=== FLOOD FILL START === tile={tilePos}");
            FloodFill(tilePos, context);
        }

        public void OnMouseDrag(Point tilePos, MouseState mouseState, EditorContext context) { }
        public void OnMouseUp(Point tilePos, MouseState mouseState, EditorContext context) { }

        private void FloodFill(Point startTile, EditorContext context)
        {
            if (context.Map == null || context.MapDefinition == null)
            {
                EditorLogger.LogError("FloodFillTool", "Map or MapDefinition is null!");
                return;
            }

            EditorLogger.Log("FloodFillTool", $"LayerKind={context.ActiveLayerKind} BrushId={context.ActiveBrushId}");

            int targetValue = GetValue(startTile, context);
            int replacementValue = context.ActiveBrushId;

            EditorLogger.Log("FloodFillTool", $"TargetValue={targetValue} ReplacementValue={replacementValue}");

            if (targetValue == replacementValue)
            {
                EditorLogger.LogWarning("FloodFillTool", "NO-OP: target same as replacement!");
                return;
            }

            Queue<Point> queue = new Queue<Point>();
            HashSet<Point> visited = new HashSet<Point>();
            queue.Enqueue(startTile);

            int maxX = context.MapDefinition.MapWidth;
            int maxY = context.MapDefinition.MapHeight;

            EditorLogger.Log("FloodFillTool", $"Map bounds: {maxX}x{maxY}");

            int filledCount = 0;

            while (queue.Count > 0)
            {
                Point tile = queue.Dequeue();
                if (visited.Contains(tile)) continue;
                if (tile.X < 0 || tile.Y < 0 || tile.X >= maxX || tile.Y >= maxY) continue;

                if (GetValue(tile, context) != targetValue) continue;

                SetValue(tile, replacementValue, context);
                visited.Add(tile);
                filledCount++;

                // Enqueue 4 neighbors
                queue.Enqueue(new Point(tile.X + 1, tile.Y));
                queue.Enqueue(new Point(tile.X - 1, tile.Y));
                queue.Enqueue(new Point(tile.X, tile.Y + 1));
                queue.Enqueue(new Point(tile.X, tile.Y - 1));
            }

            context.IsDirty = true;
            EditorLogger.Log("FloodFillTool", $"=== FLOOD FILL COMPLETE === filled {filledCount} tiles");
        }

        private int GetValue(Point tile, EditorContext context)
        {
            return context.ActiveLayerKind switch
            {
                EditableLayerKind.Tiles => context.Map.GetTileAt(tile.X, tile.Y, context.ActiveTileLayerIndex),
                EditableLayerKind.Blocks => (int)context.Map.GetBlockAtTile(tile.X, tile.Y, context.ActiveBlockLayerIndex),
                EditableLayerKind.ItemsLow => (int)context.Map.GetItemAtTile(tile.X, tile.Y, 0),
                EditableLayerKind.ItemsHigh => (int)context.Map.GetItemAtTile(tile.X, tile.Y, 1),
                _ => 0
            };
        }

        private void SetValue(Point tile, int value, EditorContext context)
        {
            switch (context.ActiveLayerKind)
            {
                case EditableLayerKind.Tiles:
                    context.Map.SetTileAt(tile.X, tile.Y, value, context.ActiveTileLayerIndex);
                    break;

                case EditableLayerKind.Blocks:
                    context.Map.SetBlockAtTile(tile.X, tile.Y, (BlockType)value, context.ActiveBlockLayerIndex);
                    break;

                case EditableLayerKind.ItemsLow:
                    context.Map.SetItemAtTile(tile.X, tile.Y, (ItemType)value, 0);
                    break;

                case EditableLayerKind.ItemsHigh:
                    context.Map.SetItemAtTile(tile.X, tile.Y, (ItemType)value, 1);
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font, EditorContext context, EditorCameraController camera, Rectangle canvasBounds)
        {
            // No preview
        }
    }
}

