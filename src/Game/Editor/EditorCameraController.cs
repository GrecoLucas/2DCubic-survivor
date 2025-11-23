using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor
{
    /// <summary>
    /// Handles editor camera pan and zoom with mouse.
    /// </summary>
    public class EditorCameraController
    {
        public Vector2 Position { get; set; }
        public float Zoom { get; set; } = 1.0f;
        public float MinZoom { get; set; } = 0.25f;
        public float MaxZoom { get; set; } = 4.0f;

        private bool _isPanning;
        private Point _panStartMouse;
        private Vector2 _panStartCamera;
        private int _previousScrollValue;

        public void Update(MouseState mouseState, MouseState previousMouseState, Rectangle canvasBounds)
        {
            // Check if mouse is over canvas
            if (!canvasBounds.Contains(mouseState.Position)) return;

            // Pan with RMB drag
            if (mouseState.RightButton == ButtonState.Pressed)
            {
                if (!_isPanning)
                {
                    _isPanning = true;
                    _panStartMouse = mouseState.Position;
                    _panStartCamera = Position;
                }
                else
                {
                    Vector2 delta = (mouseState.Position - _panStartMouse).ToVector2();
                    Position = _panStartCamera - delta / Zoom;
                }
            }
            else
            {
                _isPanning = false;
            }

            // Zoom with scroll wheel
            int scrollDelta = mouseState.ScrollWheelValue - _previousScrollValue;
            if (scrollDelta != 0)
            {
                float zoomFactor = 1.0f + (scrollDelta / 1200f);
                float newZoom = MathHelper.Clamp(Zoom * zoomFactor, MinZoom, MaxZoom);
                
                // Zoom toward mouse position
                Vector2 mouseWorld = ScreenToWorld(mouseState.Position, canvasBounds);
                Position += mouseWorld * (1 - newZoom / Zoom);
                Zoom = newZoom;
            }
            _previousScrollValue = mouseState.ScrollWheelValue;
        }

        public Vector2 ScreenToWorld(Point screenPos, Rectangle canvasBounds)
        {
            Vector2 canvasLocal = (screenPos - canvasBounds.Location).ToVector2();
            return Position + canvasLocal / Zoom;
        }

        public Point WorldToScreen(Vector2 worldPos, Rectangle canvasBounds)
        {
            Vector2 canvasLocal = (worldPos - Position) * Zoom;
            return (canvasLocal + canvasBounds.Location.ToVector2()).ToPoint();
        }

        public void FocusOn(Vector2 worldPos, Rectangle canvasBounds)
        {
            Position = worldPos - new Vector2(canvasBounds.Width / 2, canvasBounds.Height / 2) / Zoom;
        }
    }
}

