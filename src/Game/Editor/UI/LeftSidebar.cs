using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.Editor.Diagnostics;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Left sidebar: Layer Panel + Tools + Tabs (Tiles/Blocks/Items) + Palettes
    /// </summary>
    public class LeftSidebar
    {
        private UIPanel _panel;
        private List<UIToggleButton> _toolButtons = new List<UIToggleButton>();
        private List<UILayerRow> _layerRows = new List<UILayerRow>();
        private List<UIToggleButton> _tabButtons = new List<UIToggleButton>();
        private UIScrollGrid _palette;
        private EditorContext _context;
        private int _layerPanelHeight;
        private int _toolPanelHeight;

        private enum PaletteTab { Tiles, Blocks, Items }
        private PaletteTab _activeTab = PaletteTab.Tiles;

        public void Build(Rectangle bounds, EditorContext context)
        {
            _context = context;
            _panel = new UIPanel
            {
                Bounds = bounds,
                BackgroundColor = new Color(25, 25, 25, 240)
            };

            int y = bounds.Y + 10;
            int buttonHeight = 36;
            int buttonWidth = bounds.Width - 20;

            // ============================================================
            // LAYER PANEL (at top)
            // ============================================================
            BuildLayerPanel(bounds, ref y, buttonHeight, buttonWidth, context);
            _layerPanelHeight = y - bounds.Y;

            y += 10;

            // ============================================================
            // TOOLS SECTION
            // ============================================================
            int toolStartY = y;
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
            _toolPanelHeight = y - toolStartY;
            y += 10;

            // ============================================================
            // TAB BUTTONS (Tiles / Blocks / Items)
            // ============================================================
            int tabStartY = y;
            _tabButtons.Clear();
            string[] tabNames = { "Tiles", "Blocks", "Items" };
            PaletteTab[] tabValues = { PaletteTab.Tiles, PaletteTab.Blocks, PaletteTab.Items };
            int tabButtonWidth = (buttonWidth - 4) / 3;

            for (int i = 0; i < tabNames.Length; i++)
            {
                PaletteTab tab = tabValues[i];
                var tabBtn = new UIToggleButton
                {
                    Bounds = new Rectangle(bounds.X + 10 + i * (tabButtonWidth + 2), y, tabButtonWidth, buttonHeight),
                    Text = tabNames[i],
                    Selected = _activeTab == tab,
                    OnClick = () =>
                    {
                        _activeTab = tab;
                        UpdateActiveLayerKindFromTab();
                        RebuildPalette(context);
                        EditorLogger.Log("LeftSidebar", $"Tab selected: {tab}");
                    }
                };
                _tabButtons.Add(tabBtn);
                _panel.AddChild(tabBtn);
            }
            y += buttonHeight + 10;

            // ============================================================
            // PALETTE SECTION (scrollable)
            // ============================================================
            _palette = new UIScrollGrid
            {
                Bounds = new Rectangle(bounds.X + 10, y, buttonWidth, bounds.Bottom - y - 10),
                ItemSize = 56,
                Columns = 2,
                OnItemSelected = (index) =>
                {
                    int brushId = GetPaletteBrushId(index, _activeTab);
                    context.ActiveBrushId = brushId;
                    string itemName = GetItemName(brushId, _activeTab);
                    EditorLogger.Log("Palette", $"Selected: {itemName} (id={brushId}, tab={_activeTab})");
                }
            };

            RebuildPalette(context);
            _panel.AddChild(_palette);
        }

        private void BuildLayerPanel(Rectangle bounds, ref int y, int buttonHeight, int buttonWidth, EditorContext context)
        {
            // Layer panel header
            var headerLabel = new UILabel
            {
                Bounds = new Rectangle(bounds.X + 10, y, buttonWidth, 20),
                Text = "LAYERS",
                TextColor = new Color(200, 200, 200)
            };
            _panel.AddChild(headerLabel);
            y += 25;

            // Tiles section
            if (context.MapDefinition?.TileLayers != null && context.MapDefinition.TileLayers.Count > 0)
            {
                for (int i = 0; i < context.MapDefinition.TileLayers.Count; i++)
                {
                    var layer = context.MapDefinition.TileLayers[i];
                    var row = CreateLayerRow(bounds.X + 10, y, buttonWidth, buttonHeight, 
                        $"Tiles: {layer.Name}", 
                        EditableLayerKind.Tiles, 
                        i,
                        context);
                    _layerRows.Add(row);
                    _panel.AddChild(row.VisibilityButton);
                    _panel.AddChild(row.SelectButton);
                    y += buttonHeight + 2;
                }
            }

            // Items section
            if (context.MapDefinition?.ItemLayers != null && context.MapDefinition.ItemLayers.Count >= 2)
            {
                // ItemsLow
                var itemsLowRow = CreateLayerRow(bounds.X + 10, y, buttonWidth, buttonHeight,
                    "ItemsLow",
                    EditableLayerKind.ItemsLow,
                    0,
                    context);
                _layerRows.Add(itemsLowRow);
                _panel.AddChild(itemsLowRow.VisibilityButton);
                _panel.AddChild(itemsLowRow.SelectButton);
                y += buttonHeight + 2;

                // ItemsHigh
                var itemsHighRow = CreateLayerRow(bounds.X + 10, y, buttonWidth, buttonHeight,
                    "ItemsHigh",
                    EditableLayerKind.ItemsHigh,
                    1,
                    context);
                _layerRows.Add(itemsHighRow);
                _panel.AddChild(itemsHighRow.VisibilityButton);
                _panel.AddChild(itemsHighRow.SelectButton);
                y += buttonHeight + 2;
            }

            // Blocks section
            if (context.MapDefinition?.BlockLayers != null && context.MapDefinition.BlockLayers.Count > 0)
            {
                for (int i = 0; i < context.MapDefinition.BlockLayers.Count; i++)
                {
                    var layer = context.MapDefinition.BlockLayers[i];
                    var row = CreateLayerRow(bounds.X + 10, y, buttonWidth, buttonHeight,
                        $"Blocks: {layer.Name}",
                        EditableLayerKind.Blocks,
                        i,
                        context);
                    _layerRows.Add(row);
                    _panel.AddChild(row.VisibilityButton);
                    _panel.AddChild(row.SelectButton);
                    y += buttonHeight + 2;
                }
            }
        }

        private UILayerRow CreateLayerRow(int x, int y, int width, int height, string label, 
            EditableLayerKind layerKind, int layerIndex, EditorContext context)
        {
            int visibilityBtnWidth = 30;
            int selectBtnWidth = width - visibilityBtnWidth - 4;

            var visibilityBtn = new UIToggleButton
            {
                Bounds = new Rectangle(x, y, visibilityBtnWidth, height),
                Text = "V", // Visibility toggle - ASCII only to avoid SpriteFont crashes
                Selected = IsLayerVisible(layerKind, layerIndex, context),
                OnClick = () =>
                {
                    ToggleLayerVisibility(layerKind, layerIndex, context);
                    EditorLogger.Log("LayerPanel", $"Toggled visibility: {label}");
                }
            };

            var selectBtn = new UIToggleButton
            {
                Bounds = new Rectangle(x + visibilityBtnWidth + 4, y, selectBtnWidth, height),
                Text = label,
                Selected = IsLayerActive(layerKind, layerIndex, context),
                OnClick = () =>
                {
                    SetActiveLayer(layerKind, layerIndex, context);
                    UpdateTabFromActiveLayer();
                    RebuildPalette(context);
                    EditorLogger.Log("LayerPanel", $"Set active layer: {label}");
                }
            };

            return new UILayerRow
            {
                LayerKind = layerKind,
                LayerIndex = layerIndex,
                VisibilityButton = visibilityBtn,
                SelectButton = selectBtn
            };
        }

        private bool IsLayerVisible(EditableLayerKind kind, int index, EditorContext context)
        {
            return kind switch
            {
                EditableLayerKind.Tiles => index < context.TileLayerVisible.Length && context.TileLayerVisible[index],
                EditableLayerKind.Blocks => index < context.BlockLayerVisible.Length && context.BlockLayerVisible[index],
                EditableLayerKind.ItemsLow => context.ItemLayerVisible[0],
                EditableLayerKind.ItemsHigh => context.ItemLayerVisible[1],
                _ => true
            };
        }

        private void ToggleLayerVisibility(EditableLayerKind kind, int index, EditorContext context)
        {
            switch (kind)
            {
                case EditableLayerKind.Tiles:
                    if (index < context.TileLayerVisible.Length)
                        context.TileLayerVisible[index] = !context.TileLayerVisible[index];
                    break;
                case EditableLayerKind.Blocks:
                    if (index < context.BlockLayerVisible.Length)
                        context.BlockLayerVisible[index] = !context.BlockLayerVisible[index];
                    break;
                case EditableLayerKind.ItemsLow:
                    context.ItemLayerVisible[0] = !context.ItemLayerVisible[0];
                    break;
                case EditableLayerKind.ItemsHigh:
                    context.ItemLayerVisible[1] = !context.ItemLayerVisible[1];
                    break;
            }
        }

        private bool IsLayerActive(EditableLayerKind kind, int index, EditorContext context)
        {
            if (context.ActiveLayerKind != kind) return false;
            return kind switch
            {
                EditableLayerKind.Tiles => context.ActiveTileLayerIndex == index,
                EditableLayerKind.Blocks => context.ActiveBlockLayerIndex == index,
                EditableLayerKind.ItemsLow => true,
                EditableLayerKind.ItemsHigh => true,
                _ => false
            };
        }

        private void SetActiveLayer(EditableLayerKind kind, int index, EditorContext context)
        {
            context.ActiveLayerKind = kind;
            switch (kind)
            {
                case EditableLayerKind.Tiles:
                    context.ActiveTileLayerIndex = index;
                    context.EditMode = EditMode.Tiles;
                    break;
                case EditableLayerKind.Blocks:
                    context.ActiveBlockLayerIndex = index;
                    context.EditMode = EditMode.Blocks;
                    break;
                case EditableLayerKind.ItemsLow:
                case EditableLayerKind.ItemsHigh:
                    // Items don't have EditMode, but we can set it to Blocks for now
                    break;
            }
        }

        private void UpdateActiveLayerKindFromTab()
        {
            switch (_activeTab)
            {
                case PaletteTab.Tiles:
                    if (_context.MapDefinition?.TileLayers != null && _context.MapDefinition.TileLayers.Count > 0)
                    {
                        _context.ActiveLayerKind = EditableLayerKind.Tiles;
                        _context.ActiveTileLayerIndex = 0;
                        _context.EditMode = EditMode.Tiles;
                    }
                    break;
                case PaletteTab.Blocks:
                    if (_context.MapDefinition?.BlockLayers != null && _context.MapDefinition.BlockLayers.Count > 0)
                    {
                        _context.ActiveLayerKind = EditableLayerKind.Blocks;
                        _context.ActiveBlockLayerIndex = 0;
                        _context.EditMode = EditMode.Blocks;
                    }
                    break;
                case PaletteTab.Items:
                    _context.ActiveLayerKind = EditableLayerKind.ItemsLow;
                    break;
            }
        }

        private void UpdateTabFromActiveLayer()
        {
            switch (_context.ActiveLayerKind)
            {
                case EditableLayerKind.Tiles:
                    _activeTab = PaletteTab.Tiles;
                    break;
                case EditableLayerKind.Blocks:
                    _activeTab = PaletteTab.Blocks;
                    break;
                case EditableLayerKind.ItemsLow:
                case EditableLayerKind.ItemsHigh:
                    _activeTab = PaletteTab.Items;
                    break;
            }
        }

        private void RebuildPalette(EditorContext context)
        {
            _palette.Items.Clear();

            switch (_activeTab)
            {
                case PaletteTab.Tiles:
                    // All tile types
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "empty", Label = "Empty", FallbackColor = new Color(40, 40, 40) });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "grass", Label = "Grass", FallbackColor = new Color(50, 200, 50) });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "dirt", Label = "Dirt", FallbackColor = new Color(139, 90, 43) });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "stone", Label = "Stone", FallbackColor = new Color(128, 128, 128) });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "floor", Label = "Floor", FallbackColor = new Color(200, 200, 200) });
                    break;

                case PaletteTab.Blocks:
                    // All block types
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "empty", Label = "Empty", FallbackColor = new Color(40, 40, 40) });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "wall", Label = "Wall", FallbackColor = Color.Gray });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "crate", Label = "Crate", FallbackColor = new Color(150, 100, 50) });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "tree", Label = "Tree", FallbackColor = new Color(34, 139, 34) });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "rock", Label = "Rock", FallbackColor = new Color(100, 100, 120) });
                    break;

                case PaletteTab.Items:
                    // All item types
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "empty", Label = "Empty", FallbackColor = new Color(40, 40, 40) });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "hammer", Label = "Hammer", FallbackColor = new Color(139, 69, 19) });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "apple", Label = "Apple", FallbackColor = new Color(255, 0, 0) });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "wood", Label = "Wood", FallbackColor = new Color(160, 82, 45) });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "gold", Label = "Gold", FallbackColor = new Color(255, 215, 0) });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "brain", Label = "Brain", FallbackColor = new Color(255, 192, 203) });
                    _palette.Items.Add(new UIScrollGrid.GridItem { Id = "gun", Label = "Gun", FallbackColor = new Color(105, 105, 105) });
                    break;
            }

            _palette.SelectedIndex = FindPaletteIndex(context.ActiveBrushId, _activeTab);
        }

        private int GetPaletteBrushId(int paletteIndex, PaletteTab tab)
        {
            return tab switch
            {
                PaletteTab.Tiles => paletteIndex, // 0=Empty, 1=Grass, 2=Dirt, 3=Stone, 4=Floor
                PaletteTab.Blocks => paletteIndex, // 0=Empty, 1=Wall, 2=Crate, 3=Tree, 4=Rock
                PaletteTab.Items => paletteIndex, // 0=Empty, 1=Hammer, 2=Apple, 3=Wood, 4=Gold, 5=Brain, 6=Gun
                _ => 0
            };
        }

        private string GetItemName(int brushId, PaletteTab tab)
        {
            return tab switch
            {
                PaletteTab.Tiles => brushId switch
                {
                    0 => "Empty",
                    1 => "Grass",
                    2 => "Dirt",
                    3 => "Stone",
                    4 => "Floor",
                    _ => $"Tile{brushId}"
                },
                PaletteTab.Blocks => ((BlockType)brushId).ToString(),
                PaletteTab.Items => ((ItemType)brushId).ToString(),
                _ => "Unknown"
            };
        }

        private int FindPaletteIndex(int brushId, PaletteTab tab)
        {
            return brushId; // Direct mapping
        }

        public void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState, EditorContext context)
        {
            // Update tool selection
            foreach (var btn in _toolButtons)
            {
                btn.Selected = (GetToolTypeForButton(btn) == context.ActiveTool);
            }

            // Update tab selection
            foreach (var tabBtn in _tabButtons)
            {
                int index = _tabButtons.IndexOf(tabBtn);
                PaletteTab tab = (PaletteTab)index;
                tabBtn.Selected = _activeTab == tab;
            }

            // Update layer row states
            foreach (var row in _layerRows)
            {
                row.VisibilityButton.Selected = IsLayerVisible(row.LayerKind, row.LayerIndex, context);
                row.SelectButton.Selected = IsLayerActive(row.LayerKind, row.LayerIndex, context);
            }

            // Rebuild palette if needed
            _palette.SelectedIndex = FindPaletteIndex(context.ActiveBrushId, _activeTab);

            _panel.Update(gameTime, mouseState, previousMouseState);
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            _panel.Draw(spriteBatch, font, pixelTexture);
        }

        private ToolType GetToolTypeForButton(UIToggleButton btn)
        {
            int index = _toolButtons.IndexOf(btn);
            ToolType[] types = { ToolType.Brush, ToolType.Eraser, ToolType.BoxFill, ToolType.FloodFill, ToolType.Picker, ToolType.Region };
            return index >= 0 && index < types.Length ? types[index] : ToolType.Brush;
        }

        public bool HitTest(Point point) => _panel.HitTest(point);

        private class UILayerRow
        {
            public EditableLayerKind LayerKind { get; set; }
            public int LayerIndex { get; set; }
            public UIToggleButton VisibilityButton { get; set; }
            public UIToggleButton SelectButton { get; set; }
        }
    }
}
