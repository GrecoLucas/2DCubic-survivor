using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.Tools
{
    /// <summary>
    /// Interface for all editor tools (Brush, Eraser, etc.)
    /// </summary>
    public interface IEditorTool
    {
        void OnMouseDown(Point tilePos, MouseState mouseState, EditorContext context);
        void OnMouseDrag(Point tilePos, MouseState mouseState, EditorContext context);
        void OnMouseUp(Point tilePos, MouseState mouseState, EditorContext context);
        void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font, EditorContext context, EditorCameraController camera, Rectangle canvasBounds);
    }
}

