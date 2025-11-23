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

            if (context.EditMode == EditMode.Tiles)
            {
                context.Map.SetTileAt(tilePos.X, tilePos.Y, context.ActiveBrushId, context.ActiveLayerIndex);
                EditorLogger.Log("Paint", $"Tile painted: pos={tilePos} layer={context.ActiveLayerIndex} tileId={context.ActiveBrushId}");
            }
            else
            {
                BlockType blockType = (BlockType)context.ActiveBrushId;
                context.Map.SetBlockAtTile(tilePos.X, tilePos.Y, blockType, context.ActiveLayerIndex);
                EditorLogger.Log("Paint", $"Block painted: pos={tilePos} layer={context.ActiveLayerIndex} type={blockType}");
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

