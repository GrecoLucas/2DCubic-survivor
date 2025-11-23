using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Map;
using System.Linq;

namespace CubeSurvivor.Game.Editor.Tools
{
    /// <summary>
    /// Select/Move tool: click to select region, drag to move.
    /// </summary>
    public class SelectMoveTool : IEditorTool
    {
        private RegionDefinition _selectedRegion;
        private Vector2 _dragOffset;
        private bool _isDragging;

        public void OnMouseDown(Point tilePos, MouseState mouseState, EditorContext context)
        {
            if (context.MapDefinition == null) return;

            int tileSize = context.MapDefinition.TileSizePx;
            Vector2 worldPos = tilePos.ToVector2() * tileSize;

            // Find region under cursor
            _selectedRegion = context.MapDefinition.Regions
                .FirstOrDefault(r => r.Area.Contains(worldPos.ToPoint()));

            if (_selectedRegion != null)
            {
                context.SelectedRegionId = _selectedRegion.Id;
                _dragOffset = worldPos - _selectedRegion.Area.Location.ToVector2();
                _isDragging = true;
            }
        }

        public void OnMouseDrag(Point tilePos, MouseState mouseState, EditorContext context)
        {
            if (_isDragging && _selectedRegion != null && context.MapDefinition != null)
            {
                int tileSize = context.MapDefinition.TileSizePx;
                Vector2 worldPos = tilePos.ToVector2() * tileSize;
                Vector2 newPos = worldPos - _dragOffset;

                _selectedRegion.Area = new Rectangle(
                    (int)newPos.X,
                    (int)newPos.Y,
                    _selectedRegion.Area.Width,
                    _selectedRegion.Area.Height
                );

                context.IsDirty = true;
            }
        }

        public void OnMouseUp(Point tilePos, MouseState mouseState, EditorContext context)
        {
            _isDragging = false;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font, EditorContext context, EditorCameraController camera, Rectangle canvasBounds)
        {
            // Selection highlight handled by EditorRenderer
        }
    }
}

