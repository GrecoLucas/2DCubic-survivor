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

            if (context.EditMode == EditMode.Tiles)
            {
                context.Map.SetTileAt(tilePos.X, tilePos.Y, 0, context.ActiveLayerIndex); // Empty
                EditorLogger.Log("Erase", $"Tile erased: pos={tilePos} layer={context.ActiveLayerIndex}");
            }
            else
            {
                context.Map.SetBlockAtTile(tilePos.X, tilePos.Y, BlockType.Empty, context.ActiveLayerIndex);
                EditorLogger.Log("Erase", $"Block erased: pos={tilePos} layer={context.ActiveLayerIndex}");
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

