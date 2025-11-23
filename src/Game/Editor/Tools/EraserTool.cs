using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.Editor.Diagnostics;

namespace CubeSurvivor.Game.Editor.Tools
{
    /// <summary>
    /// Eraser tool: sets tiles/blocks to Empty on drag.
    /// </summary>
    public class EraserTool : IEditorTool
    {
        private Point? _lastErased;

        public void OnMouseDown(Point tilePos, MouseState mouseState, EditorContext context)
        {
            EditorLogger.Log("EraserTool", $"MouseDown at tile={tilePos}");
            Erase(tilePos, context);
        }

        public void OnMouseDrag(Point tilePos, MouseState mouseState, EditorContext context)
        {
            if (_lastErased != tilePos)
            {
                EditorLogger.Log("EraserTool", $"Drag to tile={tilePos}");
                Erase(tilePos, context);
            }
        }

        public void OnMouseUp(Point tilePos, MouseState mouseState, EditorContext context)
        {
            EditorLogger.Log("EraserTool", "MouseUp - erase complete");
            _lastErased = null;
        }

        private void Erase(Point tilePos, EditorContext context)
        {
            if (context.Map == null)
            {
                EditorLogger.LogError("EraserTool", "Map is null!");
                return;
            }

            if (!context.IsValidTile(tilePos))
            {
                EditorLogger.LogWarning("EraserTool", $"Ignored out-of-bounds tile={tilePos}");
                return;
            }

            // Use ActiveLayerKind to determine what to erase
            switch (context.ActiveLayerKind)
            {
                case EditableLayerKind.Tiles:
                    context.Map.SetTileAt(tilePos.X, tilePos.Y, 0, context.ActiveTileLayerIndex); // Empty
                    EditorLogger.Log("Erase", $"Tile erased: pos={tilePos} layer={context.ActiveTileLayerIndex}");
                    break;

                case EditableLayerKind.Blocks:
                    context.Map.SetBlockAtTile(tilePos.X, tilePos.Y, BlockType.Empty, context.ActiveBlockLayerIndex);
                    EditorLogger.Log("Erase", $"Block erased: pos={tilePos} layer={context.ActiveBlockLayerIndex}");
                    break;

                case EditableLayerKind.ItemsLow:
                    context.Map.SetItemAtTile(tilePos.X, tilePos.Y, ItemType.Empty, 0);
                    EditorLogger.Log("Erase", $"Item (Low) erased: pos={tilePos}");
                    break;

                case EditableLayerKind.ItemsHigh:
                    context.Map.SetItemAtTile(tilePos.X, tilePos.Y, ItemType.Empty, 1);
                    EditorLogger.Log("Erase", $"Item (High) erased: pos={tilePos}");
                    break;
            }

            _lastErased = tilePos;
            context.IsDirty = true;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font, EditorContext context, EditorCameraController camera, Rectangle canvasBounds)
        {
            // Ghost preview handled by EditorRenderer
        }
    }
}

