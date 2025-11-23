using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.Editor.Diagnostics;

namespace CubeSurvivor.Game.Editor.Tools
{
    /// <summary>
    /// Brush tool: paint tiles/blocks on drag.
    /// Tracks last painted tile to avoid redundant writes.
    /// </summary>
    public class BrushTool : IEditorTool
    {
        private Point? _lastPainted;

        public void OnMouseDown(Point tilePos, MouseState mouseState, EditorContext context)
        {
            EditorLogger.Log("BrushTool", $"MouseDown at tile={tilePos}");
            Paint(tilePos, context);
        }

        public void OnMouseDrag(Point tilePos, MouseState mouseState, EditorContext context)
        {
            if (_lastPainted != tilePos)
            {
                EditorLogger.Log("BrushTool", $"Drag to tile={tilePos}");
                Paint(tilePos, context);
            }
        }

        public void OnMouseUp(Point tilePos, MouseState mouseState, EditorContext context)
        {
            EditorLogger.Log("BrushTool", "MouseUp - stroke complete");
            _lastPainted = null;
        }

        private void Paint(Point tilePos, EditorContext context)
        {
            if (context.Map == null)
            {
                EditorLogger.LogError("BrushTool", "Map is null!");
                return;
            }

            if (!context.IsValidTile(tilePos))
            {
                EditorLogger.LogWarning("BrushTool", $"Ignored out-of-bounds tile={tilePos}");
                return;
            }

            // Use ActiveLayerKind to determine what to edit
            switch (context.ActiveLayerKind)
            {
                case EditableLayerKind.Tiles:
                    context.Map.SetTileAt(tilePos.X, tilePos.Y, context.ActiveBrushId, context.ActiveTileLayerIndex);
                    EditorLogger.Log("Paint", $"Tile painted: pos={tilePos} layer={context.ActiveTileLayerIndex} tileId={context.ActiveBrushId}");
                    break;

                case EditableLayerKind.Blocks:
                    BlockType blockType = (BlockType)context.ActiveBrushId;
                    context.Map.SetBlockAtTile(tilePos.X, tilePos.Y, blockType, context.ActiveBlockLayerIndex);
                    EditorLogger.Log("Paint", $"Block painted: pos={tilePos} layer={context.ActiveBlockLayerIndex} type={blockType}");
                    break;

                case EditableLayerKind.ItemsLow:
                    ItemType itemTypeLow = (ItemType)context.ActiveBrushId;
                    context.Map.SetItemAtTile(tilePos.X, tilePos.Y, itemTypeLow, 0); // ItemsLow = index 0
                    EditorLogger.Log("Paint", $"Item (Low) painted: pos={tilePos} type={itemTypeLow}");
                    break;

                case EditableLayerKind.ItemsHigh:
                    ItemType itemTypeHigh = (ItemType)context.ActiveBrushId;
                    context.Map.SetItemAtTile(tilePos.X, tilePos.Y, itemTypeHigh, 1); // ItemsHigh = index 1
                    EditorLogger.Log("Paint", $"Item (High) painted: pos={tilePos} type={itemTypeHigh}");
                    break;
            }

            _lastPainted = tilePos;
            context.IsDirty = true;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font, EditorContext context, EditorCameraController camera, Rectangle canvasBounds)
        {
            // Ghost preview handled by EditorRenderer
        }
    }
}

