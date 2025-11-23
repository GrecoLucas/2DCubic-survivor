using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.Editor.Diagnostics;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Left sidebar: Tools + Edit Mode + Palette
    /// NO HOTKEY TEXT - clean professional UI!
    /// </summary>
    public class LeftSidebar
    {
        private UIPanel _panel;
        private List<UIToggleButton> _toolButtons = new List<UIToggleButton>();
        private List<UIToggleButton> _modeButtons = new List<UIToggleButton>();
        private UIScrollGrid _palette;

        public void Build(Rectangle bounds, EditorContext context)
        {
            _panel = new UIPanel
            {
                Bounds = bounds,
                BackgroundColor = new Color(25, 25, 25, 240)
            };

            int y = bounds.Y + 10;
            int buttonHeight = 36;
            int buttonWidth = bounds.Width - 20;

            // TOOLS SECTION
            string[] toolNames = { "Brush", "Eraser", "Box Fill", "Flood Fill", "Picker", "Region" };
            ToolType[] toolTypes = { ToolType.Brush, ToolType.Eraser, ToolType.BoxFill, ToolType.FloodFill, ToolType.Picker, ToolType.Region };

            _toolButtons.Clear();
            for (int i = 0; i < toolNames.Length; i++)
            {
                ToolType toolType = toolTypes[i];
                var btn = new UIToggleButton
                {
                    Bounds = new Rectangle(bounds.X + 10, y, buttonWidth, buttonHeight),
                    Text = toolNames[i],
                    Selected = context.ActiveTool == toolType,
                    OnClick = () =>
                    {
                        context.ActiveTool = toolType;
                        EditorLogger.Log("LeftSidebar", $"Tool selected: {toolType}");
                    }
                };
                _toolButtons.Add(btn);
                _panel.AddChild(btn);
                y += buttonHeight + 4;
            }

            y += 10;

            // EDIT MODE SECTION
            _modeButtons.Clear();
            var tilesBtn = new UIToggleButton
            {
                Bounds = new Rectangle(bounds.X + 10, y, buttonWidth / 2 - 2, buttonHeight),
                Text = "Tiles",
                Selected = context.EditMode == EditMode.Tiles,
                OnClick = () =>
                {
                    context.EditMode = EditMode.Tiles;
                    EditorLogger.Log("LeftSidebar", "Edit mode: Tiles");
                }
            };
            var blocksBtn = new UIToggleButton
            {
                Bounds = new Rectangle(bounds.X + 10 + buttonWidth / 2 + 2, y, buttonWidth / 2 - 2, buttonHeight),
                Text = "Blocks",
                Selected = context.EditMode == EditMode.Blocks,
                OnClick = () =>
                {
                    context.EditMode = EditMode.Blocks;
                    EditorLogger.Log("LeftSidebar", "Edit mode: Blocks");
                }
            };
            _modeButtons.Add(tilesBtn);
            _modeButtons.Add(blocksBtn);
            _panel.AddChild(tilesBtn);
            _panel.AddChild(blocksBtn);
            y += buttonHeight + 10;

            // PALETTE SECTION
            _palette = new UIScrollGrid
            {
                Bounds = new Rectangle(bounds.X + 10, y, buttonWidth, bounds.Bottom - y - 10),
                ItemSize = 56,
                Columns = 2,
                OnItemSelected = (index) =>
                {
                    int brushId = GetPaletteBrushId(index, context.EditMode);
                    context.ActiveBrushId = brushId;
                    string itemName = context.EditMode == EditMode.Tiles ? 
                        (brushId == 0 ? "Empty" : brushId == 1 ? "Grass" : $"Tile{brushId}") :
                        ((BlockType)brushId).ToString();
                    EditorLogger.Log("Palette", $"Selected: {itemName} (id={brushId}, mode={context.EditMode})");
                }
            };

            RebuildPalette(context);
            _panel.AddChild(_palette);
        }

        private void RebuildPalette(EditorContext context)
        {
            _palette.Items.Clear();

            if (context.EditMode == EditMode.Tiles)
            {
                _palette.Items.Add(new UIScrollGrid.GridItem { Id = "empty", Label = "Empty", FallbackColor = new Color(40, 40, 40) });
                _palette.Items.Add(new UIScrollGrid.GridItem { Id = "grass", Label = "Grass", FallbackColor = new Color(50, 200, 50) });
            }
            else
            {
                _palette.Items.Add(new UIScrollGrid.GridItem { Id = "empty", Label = "Empty", FallbackColor = new Color(40, 40, 40) });
                _palette.Items.Add(new UIScrollGrid.GridItem { Id = "wall", Label = "Wall", FallbackColor = Color.Gray });
                _palette.Items.Add(new UIScrollGrid.GridItem { Id = "crate", Label = "Crate", FallbackColor = new Color(150, 100, 50) });
                _palette.Items.Add(new UIScrollGrid.GridItem { Id = "tree", Label = "Tree", FallbackColor = new Color(34, 139, 34) });
                _palette.Items.Add(new UIScrollGrid.GridItem { Id = "rock", Label = "Rock", FallbackColor = new Color(100, 100, 120) });
            }

            // Set selected based on context
            _palette.SelectedIndex = FindPaletteIndex(context.ActiveBrushId, context.EditMode);
        }

        private int GetPaletteBrushId(int paletteIndex, EditMode mode)
        {
            if (mode == EditMode.Tiles)
            {
                return paletteIndex; // 0=Empty, 1=Grass
            }
            else
            {
                return paletteIndex; // 0=Empty, 1=Wall, 2=Crate, 3=Tree, 4=Rock
            }
        }

        private int FindPaletteIndex(int brushId, EditMode mode)
        {
            return brushId; // Direct mapping for now
        }

        public void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState, EditorContext context)
        {
            // Update selection states
            foreach (var btn in _toolButtons)
            {
                btn.Selected = (GetToolTypeForButton(btn) == context.ActiveTool);
            }

            _modeButtons[0].Selected = context.EditMode == EditMode.Tiles;
            _modeButtons[1].Selected = context.EditMode == EditMode.Blocks;

            // Rebuild palette if mode changed
            EditMode prevMode = _palette.Items.Count <= 2 ? EditMode.Tiles : EditMode.Blocks;
            if (prevMode != context.EditMode)
            {
                RebuildPalette(context);
            }

            _palette.SelectedIndex = FindPaletteIndex(context.ActiveBrushId, context.EditMode);

            _panel.Update(gameTime, mouseState, previousMouseState);
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            _panel.Draw(spriteBatch, font, pixelTexture);
        }

        private ToolType GetToolTypeForButton(UIToggleButton btn)
        {
            int index = _toolButtons.IndexOf(btn);
            ToolType[] types = { ToolType.Brush, ToolType.Eraser, ToolType.BoxFill, ToolType.FloodFill, ToolType.Picker, ToolType.Region, ToolType.SelectMove };
            return types[index];
        }

        public bool HitTest(Point point) => _panel.HitTest(point);
    }
}

