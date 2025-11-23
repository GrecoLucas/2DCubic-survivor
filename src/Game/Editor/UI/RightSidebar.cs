using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.Editor.Diagnostics;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Right sidebar: Layers + Regions list with Focus/Delete buttons
    /// </summary>
    public class RightSidebar
    {
        private UIPanel _panel;
        private List<UIButton> _regionButtons = new List<UIButton>();
        private EditorContext _context;

        public void Build(Rectangle bounds, EditorContext context)
        {
            _context = context;
            _panel = new UIPanel
            {
                Bounds = bounds,
                BackgroundColor = new Color(25, 25, 25, 240),
                Scrollable = true
            };

            RebuildRegionsList(bounds, context);
        }

        private void RebuildRegionsList(Rectangle bounds, EditorContext context)
        {
            _panel.ClearChildren();
            _regionButtons.Clear();

            if (context.MapDefinition == null) return;

            int y = bounds.Y + 10;
            int itemHeight = 60;
            int buttonWidth = bounds.Width - 20;

            // Section header
            // (Could add UILabel here)

            foreach (var region in context.MapDefinition.Regions)
            {
                UIPanel regionPanel = new UIPanel
                {
                    Bounds = new Rectangle(bounds.X + 10, y, buttonWidth, itemHeight),
                    BackgroundColor = new Color(40, 40, 40)
                };

                // Region label
                // (Could add UILabel with region.Type + region.Id)

                // Focus button - LOCAL coords
                UIButton focusBtn = new UIButton
                {
                    Bounds = new Rectangle(5, 5, 80, 24),
                    Text = "Focus",
                    NormalColor = new Color(60, 120, 180),
                    OnClick = () => FocusRegion(region, context)
                };
                regionPanel.AddChild(focusBtn);

                // Delete button - LOCAL coords
                UIButton deleteBtn = new UIButton
                {
                    Bounds = new Rectangle(90, 5, 80, 24),
                    Text = "Delete",
                    NormalColor = new Color(180, 60, 60),
                    OnClick = () => DeleteRegion(region, context)
                };
                regionPanel.AddChild(deleteBtn);

                // Select on click
                bool isSelected = region.Id == context.SelectedRegionId;
                if (isSelected)
                {
                    regionPanel.BackgroundColor = new Color(60, 100, 140);
                }

                _panel.AddChild(regionPanel);
                y += itemHeight + 4;
            }

            _panel.CalculateScrollMax(y - bounds.Y);
        }

        private void FocusRegion(RegionDefinition region, EditorContext context)
        {
            EditorLogger.Log("RightSidebar", $"Focus requested for region '{region.Id}'");
            context.SelectedRegionId = region.Id;
            // EditorState will handle actual camera movement
        }

        private void DeleteRegion(RegionDefinition region, EditorContext context)
        {
            EditorLogger.Log("RightSidebar", $"Delete button clicked for region '{region.Id}'");
            context.DeleteRegion(region.Id); // Uses context method with logging
            RebuildRegionsList(_panel.Bounds, context);
        }

        public void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState, EditorContext context)
        {
            // Rebuild if regions changed
            if (_regionButtons.Count != context.MapDefinition?.Regions.Count)
            {
                RebuildRegionsList(_panel.Bounds, context);
            }

            _panel.Update(gameTime, mouseState, previousMouseState);
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            _panel.Draw(spriteBatch, font, pixelTexture);

            // Draw region labels with actual data from context
            if (_panel.Visible && font != null && _context?.MapDefinition != null)
            {
                int index = 0;
                foreach (var child in _panel.Children)
                {
                    if (child is UIPanel regionPanel && index < _context.MapDefinition.Regions.Count)
                    {
                        var region = _context.MapDefinition.Regions[index];
                        Rectangle globalBounds = regionPanel.GlobalBounds;
                        
                        // Draw region type and ID (sanitized to prevent SpriteFont crashes)
                        string label = $"{region.Type}: {region.Id}";
                        string safeLabel = FontUtil.SanitizeForFont(font, label);
                        spriteBatch.DrawString(font, safeLabel, new Vector2(globalBounds.X + 5, globalBounds.Y + 35), Color.White);
                        
                        // Draw area info (sanitized)
                        string areaInfo = $"Area: {region.Area.X},{region.Area.Y} {region.Area.Width}x{region.Area.Height}";
                        string safeAreaInfo = FontUtil.SanitizeForFont(font, areaInfo);
                        spriteBatch.DrawString(font, safeAreaInfo, new Vector2(globalBounds.X + 5, globalBounds.Y + 50), Color.LightGray);
                        
                        index++;
                    }
                }
            }
        }

        public bool HitTest(Point point) => _panel.HitTest(point);
    }
}

