using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.Tools
{
    /// <summary>
    /// Picker (Eyedropper) tool: samples tile/block under cursor.
    /// </summary>
    public class PickerTool : IEditorTool
    {
        public void OnMouseDown(Point tilePos, MouseState mouseState, EditorContext context)
        {
            if (context.Map == null) return;

            if (context.EditMode == EditMode.Tiles)
            {
                int tileId = context.Map.GetTileAt(tilePos.X, tilePos.Y, context.ActiveLayerIndex);
                context.ActiveBrushId = tileId;
            }
            else
            {
                var blockType = context.Map.GetBlockAtTile(tilePos.X, tilePos.Y, context.ActiveLayerIndex);
                context.ActiveBrushId = (int)blockType;
            }
        }

        public void OnMouseDrag(Point tilePos, MouseState mouseState, EditorContext context) { }
        public void OnMouseUp(Point tilePos, MouseState mouseState, EditorContext context) { }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font, EditorContext context, EditorCameraController camera, Rectangle canvasBounds)
        {
            // No preview
        }
    }
}

