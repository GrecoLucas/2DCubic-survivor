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
    /// OR Region Meta Editor when Region tool is active and region is selected
    /// </summary>
    public class RightSidebar
    {
        private UIPanel _panel;
        private List<UIButton> _regionButtons = new List<UIButton>();
        private EditorContext _context;
        private Dictionary<string, UILabel> _metaFields = new Dictionary<string, UILabel>();

        public void Build(Rectangle bounds, EditorContext context)
        {
            _context = context;
            _panel = new UIPanel
            {
                Bounds = bounds,
                BackgroundColor = new Color(25, 25, 25, 240),
                Scrollable = true
            };

            RebuildContent(bounds, context);
        }

        private void RebuildContent(Rectangle bounds, EditorContext context)
        {
            _panel.ClearChildren();
            _metaFields.Clear();

            // If Region tool is active and a region is selected, show meta editor
            if (context.ActiveTool == ToolType.Region && context.SelectedRegionRef != null)
            {
                BuildRegionMetaEditor(bounds, context);
            }
            else
            {
                RebuildRegionsList(bounds, context);
            }
        }

        private void BuildRegionMetaEditor(Rectangle bounds, EditorContext context)
        {
            var region = context.SelectedRegionRef;
            int y = bounds.Y + 10;
            int buttonHeight = 30;
            int buttonWidth = bounds.Width - 20;

            // Header
            var headerLabel = new UILabel
            {
                Bounds = new Rectangle(bounds.X + 10, y, buttonWidth, 25),
                Text = "REGION EDITOR",
                TextColor = new Color(200, 200, 200)
            };
            _panel.AddChild(headerLabel);
            y += 35;

            // Region ID (editable)
            var idLabel = new UILabel
            {
                Bounds = new Rectangle(bounds.X + 10, y, 80, 20),
                Text = "ID:",
                TextColor = Color.White
            };
            _panel.AddChild(idLabel);
            
            // TODO: Make ID editable (for now just display)
            var idValueLabel = new UILabel
            {
                Bounds = new Rectangle(bounds.X + 95, y, buttonWidth - 95, 20),
                Text = FontUtil.SanitizeForFont(null, region.Id ?? "unnamed"),
                TextColor = Color.LightGray
            };
            _panel.AddChild(idValueLabel);
            y += 30;

            // Region Type (read-only for now, could be dropdown)
            var typeLabel = new UILabel
            {
                Bounds = new Rectangle(bounds.X + 10, y, 80, 20),
                Text = "Type:",
                TextColor = Color.White
            };
            _panel.AddChild(typeLabel);
            
            var typeValueLabel = new UILabel
            {
                Bounds = new Rectangle(bounds.X + 95, y, buttonWidth - 95, 20),
                Text = FontUtil.SanitizeForFont(null, region.Type.ToString()),
                TextColor = Color.LightGray
            };
            _panel.AddChild(typeValueLabel);
            y += 30;

            // Bounds (read-only, tile coordinates)
            var boundsLabel = new UILabel
            {
                Bounds = new Rectangle(bounds.X + 10, y, buttonWidth, 20),
                Text = FontUtil.SanitizeForFont(null, $"Bounds: {region.Area.X}, {region.Area.Y} ({region.Area.Width}x{region.Area.Height} tiles)"),
                TextColor = Color.LightGray
            };
            _panel.AddChild(boundsLabel);
            y += 35;

            // Meta fields (type-specific)
            var metaHeader = new UILabel
            {
                Bounds = new Rectangle(bounds.X + 10, y, buttonWidth, 20),
                Text = "SPAWN SETTINGS:",
                TextColor = new Color(180, 180, 180)
            };
            _panel.AddChild(metaHeader);
            y += 30;

            // Build meta fields based on region type
            BuildMetaFieldsForType(bounds, ref y, buttonHeight, buttonWidth, region, context);
        }

        private void BuildMetaFieldsForType(Rectangle bounds, ref int y, int buttonHeight, int buttonWidth, RegionDefinition region, EditorContext context)
        {
            // Get expected fields for this region type
            var defaults = RegionDefaults.GetDefaults(region.Type);
            
            // Ensure region.Meta has all default keys
            foreach (var kvp in defaults)
            {
                if (!region.Meta.ContainsKey(kvp.Key))
                {
                    region.Meta[kvp.Key] = kvp.Value;
                }
            }

            // Create UI fields for each meta key
            foreach (var kvp in defaults)
            {
                string key = kvp.Key;
                string currentValue = region.Meta.ContainsKey(key) ? region.Meta[key] : kvp.Value;

                // Label
                var label = new UILabel
                {
                    Bounds = new Rectangle(bounds.X + 10, y, 120, 20),
                    Text = FontUtil.SanitizeForFont(null, key + ":"),
                    TextColor = Color.White
                };
                _panel.AddChild(label);

                // Value field (simplified - just display for now, could be UITextBox later)
                var valueLabel = new UILabel
                {
                    Bounds = new Rectangle(bounds.X + 135, y, buttonWidth - 145, 20),
                    Text = FontUtil.SanitizeForFont(null, currentValue),
                    TextColor = Color.LightGray
                };
                _panel.AddChild(valueLabel);

                y += 25;
            }

            // Note: Full editable text boxes would require UITextBox component
            // For now, meta is set via defaults when creating regions
            // User can edit JSON directly or we can add UITextBox later
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

                // Select on click - make entire panel selectable
                bool isSelected = region.Id == context.SelectedRegionId;
                if (isSelected)
                {
                    regionPanel.BackgroundColor = new Color(60, 100, 140);
                }

                // Add a select button that covers the panel
                UIButton selectBtn = new UIButton
                {
                    Bounds = new Rectangle(175, 5, buttonWidth - 180, 24),
                    Text = "Select",
                    NormalColor = isSelected ? new Color(80, 140, 200) : new Color(50, 50, 50),
                    OnClick = () =>
                    {
                        context.SelectedRegionId = region.Id;
                        context.SelectedRegionRef = region;
                        RebuildContent(_panel.Bounds, context);
                        EditorLogger.Log("RightSidebar", $"Selected region: {region.Id}");
                    }
                };
                regionPanel.AddChild(selectBtn);

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
            // Rebuild if mode changed or regions changed
            bool shouldShowMetaEditor = context.ActiveTool == ToolType.Region && context.SelectedRegionRef != null;
            bool currentlyShowingMetaEditor = _panel.Children.Count > 0 && _panel.Children[0] is UILabel header && header.Text == "REGION EDITOR";
            
            if (shouldShowMetaEditor != currentlyShowingMetaEditor || 
                (context.MapDefinition != null && _regionButtons.Count != context.MapDefinition.Regions.Count))
            {
                RebuildContent(_panel.Bounds, context);
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

