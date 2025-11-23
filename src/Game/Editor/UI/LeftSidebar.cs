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
    /// OR Region Palette when Region tool is active
    /// </summary>
    public class LeftSidebar
    {
        private UIPanel _panel;
        private List<UIToggleButton> _toolButtons = new List<UIToggleButton>();
        private List<UILayerRow> _layerRows = new List<UILayerRow>();
        private List<UIToggleButton> _tabButtons = new List<UIToggleButton>();
        private UIScrollGrid _palette;
        private UIScrollList _regionPalette; // NEW: Region palette list
        private EditorContext _context;
        private int _layerPanelHeight;
        private int _toolPanelHeight;

        private enum PaletteTab { Tiles, Blocks, Items }
        private PaletteTab _activeTab = PaletteTab.Tiles;

        private enum LeftSidebarMode { LayersPalette, RegionPalette }
        private LeftSidebarMode _currentMode = LeftSidebarMode.LayersPalette;
        
        // RegionPalette state tracking (to avoid rebuilding every frame)
        private RegionType _lastActiveRegionType;
        private bool _lastRegionEraseMode;
        private bool _regionPaletteDirty;

        public void Build(Rectangle bounds, EditorContext context)
        {
            _context = context;
            
            // Clear old state to prevent desync
            _layerRows.Clear();
            _toolButtons.Clear();
            _tabButtons.Clear();
            
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
                        // DO NOT reset ActiveRegionTypeToPlace when switching tools
                        // It should persist across tool switches
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
            // MODE-SPECIFIC CONTENT (LayersPalette or RegionPalette)
            // ============================================================
            BuildModeContent(bounds, ref y, buttonHeight, buttonWidth, context);
        }

        private void BuildModeContent(Rectangle bounds, ref int y, int buttonHeight, int buttonWidth, EditorContext context)
        {
            // Determine mode based on active tool
            _currentMode = context.ActiveTool == ToolType.Region ? LeftSidebarMode.RegionPalette : LeftSidebarMode.LayersPalette;

            if (_currentMode == LeftSidebarMode.RegionPalette)
            {
                BuildRegionPalette(bounds, ref y, buttonHeight, buttonWidth, context);
            }
            else
            {
                BuildLayersPalette(bounds, ref y, buttonHeight, buttonWidth, context);
            }
        }

        private void BuildLayersPalette(Rectangle bounds, ref int y, int buttonHeight, int buttonWidth, EditorContext context)
        {
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

        private void BuildRegionPalette(Rectangle bounds, ref int y, int buttonHeight, int buttonWidth, EditorContext context)
        {
            // ============================================================
            // REGION MODE HEADER
            // ============================================================
            var headerLabel = new UILabel
            {
                Bounds = new Rectangle(bounds.X + 10, y, buttonWidth, 25),
                Text = "REGION MODE",
                TextColor = new Color(200, 200, 200)
            };
            _panel.AddChild(headerLabel);
            y += 30;

            // ============================================================
            // REGION TYPE PALETTE (scrollable list)
            // ============================================================
            _regionPalette = new UIScrollList
            {
                Bounds = new Rectangle(bounds.X + 10, y, buttonWidth, bounds.Bottom - y - 10),
                BackgroundColor = new Color(30, 30, 30, 240)
            };

            RebuildRegionPalette(context);
            _panel.AddChild(_regionPalette);
            
            // Initialize last-known state to prevent unnecessary rebuilds
            _lastActiveRegionType = context.ActiveRegionTypeToPlace;
            _lastRegionEraseMode = context.RegionEraseMode;
            _regionPaletteDirty = false;
        }

        private void RebuildRegionPalette(EditorContext context)
        {
            if (_regionPalette == null) return;

            // DO NOT modify ActiveRegionTypeToPlace here - it's the single source of truth
            // Only read it to determine which card should be highlighted
            _regionPalette.Clear();
            
            // Add Eraser toggle button at the top
            var eraserButton = new UIToggleButton
            {
                Bounds = new Rectangle(0, 0, _regionPalette.Bounds.Width - 20, 30),
                Text = "Eraser",
                Selected = context.RegionEraseMode,
                OnClick = () =>
                {
                    context.RegionEraseMode = !context.RegionEraseMode;
                    EditorLogger.Log("RegionPalette", $"RegionEraseMode={context.RegionEraseMode}");
                    // Mark dirty for rebuild on next frame (without killing click)
                    _regionPaletteDirty = true;
                }
            };
            _regionPalette.AddItem(eraserButton);

            // All region types
            RegionType[] regionTypes = {
                RegionType.PlayerSpawn,
                RegionType.EnemySpawn,
                RegionType.GoldSpawn,
                RegionType.WoodSpawn,
                RegionType.AppleSpawn,
                RegionType.TreeSpawn,
                RegionType.ItemSpawn,
                RegionType.SafeZone,
                RegionType.Biome
            };

            foreach (var regionType in regionTypes)
            {
                var rt = regionType; // ✅ capture-safe for lambda
                
                var defaults = RegionDefaults.GetDefaults(rt);
                string metaSummary = RegionDefaults.GetMetaSummary(rt);
                string description = RegionDefaults.GetDescription(rt);

                // Check if this is the selected type
                bool isSelected = context.ActiveRegionTypeToPlace == rt;

                // Create a card-like panel for each region type
                var cardPanel = new UIPanel
                {
                    Bounds = new Rectangle(0, 0, _regionPalette.Bounds.Width - 20, 80),
                    BackgroundColor = isSelected 
                        ? new Color(60, 120, 180, 255) 
                        : new Color(40, 40, 40, 255)
                };

                // Region type name button
                var typeButton = new UIButton
                {
                    Bounds = new Rectangle(5, 5, cardPanel.Bounds.Width - 10, 30),
                    Text = FontUtil.SanitizeForFont(null, rt.ToString()),
                    NormalColor = isSelected 
                        ? new Color(80, 140, 200) 
                        : new Color(50, 50, 50),
                    HoverColor = new Color(100, 160, 220),
                    OnClick = () =>
                    {
                        try
                        {
                            // SINGLE SOURCE OF TRUTH: Only set ActiveRegionTypeToPlace
                            context.ActiveRegionTypeToPlace = rt;
                            // Clear selection when changing type
                            context.SelectedRegionId = null;
                            context.SelectedRegionRef = null;
                            // PendingRegionType is now an alias, so this is redundant but harmless
                            context.PendingRegionMeta = new Dictionary<string, string>(defaults);
                            
                            // Mark dirty for rebuild on next frame (without killing click)
                            _regionPaletteDirty = true;
                            
                            EditorLogger.Log("RegionPalette", $"CLICK -> ActiveRegionTypeToPlace = {rt} ({(int)rt})");
                        }
                        catch (System.Exception ex)
                        {
                            EditorLogger.LogError("RegionPalette", $"Error selecting region type {rt}: {ex.Message}");
                        }
                    }
                };
                cardPanel.AddChild(typeButton);

                // Description label
                var descLabel = new UILabel
                {
                    Bounds = new Rectangle(5, 35, cardPanel.Bounds.Width - 10, 20),
                    Text = FontUtil.SanitizeForFont(null, description.Length > 50 ? description.Substring(0, 50) + "..." : description),
                    TextColor = new Color(180, 180, 180)
                };
                cardPanel.AddChild(descLabel);

                // Meta summary label
                var metaLabel = new UILabel
                {
                    Bounds = new Rectangle(5, 55, cardPanel.Bounds.Width - 10, 20),
                    Text = FontUtil.SanitizeForFont(null, metaSummary),
                    TextColor = new Color(150, 150, 150)
                };
                cardPanel.AddChild(metaLabel);

                _regionPalette.AddItem(cardPanel);
            }
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

        public void HandleScroll(int scrollDelta)
        {
            if (_currentMode == LeftSidebarMode.RegionPalette && _regionPalette != null)
            {
                // Scroll the region palette list
                _regionPalette.ScrollBy(scrollDelta);
            }
            else if (_currentMode == LeftSidebarMode.LayersPalette && _palette != null)
            {
                // Scroll the palette grid
                _palette.ScrollBy(scrollDelta);
            }
        }

        public void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState, EditorContext context)
        {
            // Check if tool changed - if so, rebuild mode content
            LeftSidebarMode newMode = context.ActiveTool == ToolType.Region ? LeftSidebarMode.RegionPalette : LeftSidebarMode.LayersPalette;
            if (newMode != _currentMode)
            {
                // Mode changed - need to rebuild content
                EditorLogger.Log("LeftSidebar", $"Mode changed: {_currentMode} -> {newMode}");
                _currentMode = newMode;
                
                // Remove old mode content (palette or region palette)
                if (_palette != null)
                {
                    _panel.RemoveChild(_palette);
                    _palette = null;
                }
                if (_regionPalette != null)
                {
                    _panel.RemoveChild(_regionPalette);
                    _regionPalette = null;
                }
                foreach (var tabBtn in _tabButtons)
                {
                    _panel.RemoveChild(tabBtn);
                }
                _tabButtons.Clear();

                // Rebuild mode content
                int y = _panel.Bounds.Y + 10 + _layerPanelHeight + 10 + _toolPanelHeight + 10;
                int buttonHeight = 36;
                int buttonWidth = _panel.Bounds.Width - 20;
                BuildModeContent(_panel.Bounds, ref y, buttonHeight, buttonWidth, context);
                
                EditorLogger.Log("LeftSidebar", $"Mode content rebuilt. RegionPalette={_regionPalette != null}, Palette={_palette != null}");
            }

            // Update tool selection
            foreach (var btn in _toolButtons)
            {
                btn.Selected = (GetToolTypeForButton(btn) == context.ActiveTool);
            }

            // Update tab selection (only if in LayersPalette mode)
            if (_currentMode == LeftSidebarMode.LayersPalette)
            {
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
                if (_palette != null)
                {
                    _palette.SelectedIndex = FindPaletteIndex(context.ActiveBrushId, _activeTab);
                }
            }
            else
            {
                // RegionPalette mode - only rebuild when state actually changes
                if (_regionPalette != null)
                {
                    bool needsRebuild =
                        _regionPaletteDirty ||
                        context.ActiveRegionTypeToPlace != _lastActiveRegionType ||
                        context.RegionEraseMode != _lastRegionEraseMode;

                    if (needsRebuild)
                    {
                        EditorLogger.Log("LeftSidebar",
                            $"RegionPalette rebuild: type {_lastActiveRegionType}->{context.ActiveRegionTypeToPlace}, " +
                            $"erase {_lastRegionEraseMode}->{context.RegionEraseMode}");

                        RebuildRegionPalette(context);

                        _lastActiveRegionType = context.ActiveRegionTypeToPlace;
                        _lastRegionEraseMode = context.RegionEraseMode;
                        _regionPaletteDirty = false;
                    }
                }
            }

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
