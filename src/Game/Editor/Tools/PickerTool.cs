using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Editor.Diagnostics;

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

            // Use ActiveLayerKind to determine what to sample
            switch (context.ActiveLayerKind)
            {
                case EditableLayerKind.Tiles:
                    int tileId = context.Map.GetTileAt(tilePos.X, tilePos.Y, context.ActiveTileLayerIndex);
                    context.ActiveBrushId = tileId;
                    EditorLogger.Log("PickerTool", $"Picked tile: {tileId}");
                    break;

                case EditableLayerKind.Blocks:
                    var blockType = context.Map.GetBlockAtTile(tilePos.X, tilePos.Y, context.ActiveBlockLayerIndex);
                    context.ActiveBrushId = (int)blockType;
                    EditorLogger.Log("PickerTool", $"Picked block: {blockType}");
                    break;

                case EditableLayerKind.ItemsLow:
                    var itemTypeLow = context.Map.GetItemAtTile(tilePos.X, tilePos.Y, 0);
                    context.ActiveBrushId = (int)itemTypeLow;
                    EditorLogger.Log("PickerTool", $"Picked item (Low): {itemTypeLow}");
                    break;

                case EditableLayerKind.ItemsHigh:
                    var itemTypeHigh = context.Map.GetItemAtTile(tilePos.X, tilePos.Y, 1);
                    context.ActiveBrushId = (int)itemTypeHigh;
                    EditorLogger.Log("PickerTool", $"Picked item (High): {itemTypeHigh}");
                    break;
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

