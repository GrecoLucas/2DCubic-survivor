using System;
using System.IO;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace CubeSurvivor
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Texture2D _pixelTexture;
        
        private StateManager _stateManager;

        // Controle para evitar loops de redimensionamento
        private bool _isResizing = false;

        // Estado de maximização
        private bool _isMaximized = false;
        private Rectangle _restoreBounds; // Guarda posição/tamanho antes de maximizar

        public Game1()
        {
            Console.WriteLine("[Game1] Constructor started");
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Define resolução inicial (modo janela)
            _graphics.PreferredBackBufferWidth = GameConfig.ScreenWidth;
            _graphics.PreferredBackBufferHeight = GameConfig.ScreenHeight;
            _graphics.HardwareModeSwitch = false; // Importante para transições suaves
            
            Console.WriteLine("[Game1V2] Constructor completed");
        }

        #region Platform-Specific P/Invoke

        // ===== WINDOWS =====
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(int uAction, int uParam, ref RECT lpvParam, int fuWinIni);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private const int SW_RESTORE = 9;
        private const int SW_MAXIMIZE = 3;
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private const int SPI_GETWORKAREA = 48;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        // ===== LINUX/SDL2 =====
        // Tentamos múltiplos nomes de biblioteca SDL2
        [DllImport("libSDL2-2.0.so.0", EntryPoint = "SDL_MaximizeWindow", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_MaximizeWindow_Linux(IntPtr window);

        [DllImport("libSDL2-2.0.so.0", EntryPoint = "SDL_RestoreWindow", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_RestoreWindow_Linux(IntPtr window);

        [DllImport("libSDL2-2.0.so.0", EntryPoint = "SDL_GetWindowFlags", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint SDL_GetWindowFlags_Linux(IntPtr window);

        // SDL_WINDOW_MAXIMIZED = 0x00000080
        private const uint SDL_WINDOW_MAXIMIZED = 0x00000080;

        #endregion

        /// <summary>
        /// Alterna entre janela maximizada e janela normal.
        /// Funciona em Windows e Linux.
        /// </summary>
        private void ToggleMaximize()
        {
            try
            {
                Console.WriteLine($"[Game1] ToggleMaximize called. Current state: {(_isMaximized ? "Maximized" : "Normal")}");

                // Garantir que redimensionamento é permitido
                if (!Window.AllowUserResizing)
                {
                    Window.AllowUserResizing = true;
                    Console.WriteLine("[Game1] Enabled AllowUserResizing");
                }

                bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

                if (isWindows)
                {
                    ToggleMaximize_Windows();
                }
                else if (isLinux)
                {
                    ToggleMaximize_Linux();
                }
                else
                {
                    // macOS ou outro: fallback para fullscreen borderless
                    ToggleMaximize_Fallback();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Game1] ToggleMaximize error: {ex}");
                ShowToast("Failed to toggle maximize");
            }
        }

        private void ToggleMaximize_Windows()
        {
            IntPtr hWnd = Window.Handle;
            Console.WriteLine($"[Game1] Windows handle: 0x{hWnd:X}");

            if (!_isMaximized)
            {
                // Salvar posição atual antes de maximizar
                _restoreBounds = new Rectangle(
                    Window.Position.X,
                    Window.Position.Y,
                    _graphics.PreferredBackBufferWidth,
                    _graphics.PreferredBackBufferHeight
                );
                Console.WriteLine($"[Game1] Saved restore bounds: {_restoreBounds}");

                // Obter área de trabalho (exclui taskbar)
                RECT workArea = new RECT();
                SystemParametersInfo(SPI_GETWORKAREA, 0, ref workArea, 0);
                
                int workWidth = workArea.Right - workArea.Left;
                int workHeight = workArea.Bottom - workArea.Top;
                Console.WriteLine($"[Game1] Work area: {workWidth}x{workHeight} at ({workArea.Left},{workArea.Top})");

                // Mover janela para preencher área de trabalho (sem cobrir taskbar)
                Window.Position = new Point(workArea.Left, workArea.Top);
                _graphics.PreferredBackBufferWidth = workWidth;
                _graphics.PreferredBackBufferHeight = workHeight;
                _graphics.ApplyChanges();

                _isMaximized = true;
                Console.WriteLine($"[Game1] Window maximized to {workWidth}x{workHeight}");
                ShowToast("Window Maximized");
            }
            else
            {
                // Restaurar tamanho e posição anteriores
                Window.Position = new Point(_restoreBounds.X, _restoreBounds.Y);
                _graphics.PreferredBackBufferWidth = _restoreBounds.Width;
                _graphics.PreferredBackBufferHeight = _restoreBounds.Height;
                _graphics.ApplyChanges();

                _isMaximized = false;
                Console.WriteLine($"[Game1] Window restored to {_restoreBounds}");
                ShowToast("Window Restored");
            }
        }

        private void ToggleMaximize_Linux()
        {
            IntPtr hWnd = Window.Handle;
            Console.WriteLine($"[Game1] Linux/SDL handle: 0x{hWnd:X}");

            try
            {
                // Verificar estado atual via SDL
                uint flags = SDL_GetWindowFlags_Linux(hWnd);
                bool isCurrentlyMaximized = (flags & SDL_WINDOW_MAXIMIZED) != 0;
                Console.WriteLine($"[Game1] SDL flags: 0x{flags:X}, Maximized: {isCurrentlyMaximized}");

                if (!isCurrentlyMaximized)
                {
                    // Salvar bounds antes de maximizar
                    _restoreBounds = new Rectangle(
                        Window.Position.X,
                        Window.Position.Y,
                        _graphics.PreferredBackBufferWidth,
                        _graphics.PreferredBackBufferHeight
                    );

                    SDL_MaximizeWindow_Linux(hWnd);
                    _isMaximized = true;
                    Console.WriteLine("[Game1] SDL_MaximizeWindow called");
                    ShowToast("Window Maximized");
                }
                else
                {
                    SDL_RestoreWindow_Linux(hWnd);
                    _isMaximized = false;
                    Console.WriteLine("[Game1] SDL_RestoreWindow called");
                    ShowToast("Window Restored");
                }
            }
            catch (DllNotFoundException ex)
            {
                Console.WriteLine($"[Game1] SDL2 library not found: {ex.Message}");
                ToggleMaximize_Fallback();
            }
        }

        private void ToggleMaximize_Fallback()
        {
            Console.WriteLine("[Game1] Using fallback maximize (borderless fullscreen toggle)");

            if (!_isMaximized)
            {
                // Salvar estado atual
                _restoreBounds = new Rectangle(
                    Window.Position.X,
                    Window.Position.Y,
                    _graphics.PreferredBackBufferWidth,
                    _graphics.PreferredBackBufferHeight
                );

                // Preencher tela toda (pseudo-maximize)
                var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                Window.Position = Point.Zero;
                _graphics.PreferredBackBufferWidth = displayMode.Width;
                _graphics.PreferredBackBufferHeight = displayMode.Height;
                _graphics.ApplyChanges();

                _isMaximized = true;
                ShowToast("Window Maximized (Fallback)");
            }
            else
            {
                // Restaurar
                Window.Position = new Point(_restoreBounds.X, _restoreBounds.Y);
                _graphics.PreferredBackBufferWidth = _restoreBounds.Width;
                _graphics.PreferredBackBufferHeight = _restoreBounds.Height;
                _graphics.ApplyChanges();

                _isMaximized = false;
                ShowToast("Window Restored");
            }
        }

        // Evento disparado quando a janela muda de tamanho (arrastando ou maximizando)
        private void OnClientSizeChanged(object sender, EventArgs e)
        {
            if (_isResizing) return;
            if (Window.ClientBounds.Width <= 0 || Window.ClientBounds.Height <= 0) return;

            _isResizing = true;

            int newWidth = Window.ClientBounds.Width;
            int newHeight = Window.ClientBounds.Height;

            // Atualiza o BackBuffer para coincidir com o tamanho da janela
            _graphics.PreferredBackBufferWidth = newWidth;
            _graphics.PreferredBackBufferHeight = newHeight;
            _graphics.ApplyChanges();
            
            Console.WriteLine($"[Game1] Resized to: {newWidth}x{newHeight}");

            // Notificar o estado atual sobre a mudança de tamanho
            if (_stateManager?.CurrentState is PlayState playState)
            {
                playState.SetScreenSize(newWidth, newHeight);
            }

            _isResizing = false;
        }

        protected override void Initialize()
        {
            Console.WriteLine("[Game1V2] Initialize() started");

            // --- CORREÇÃO PRINCIPAL ---
            // MonoGame inicia com AllowUserResizing = false. 
            // O Windows BLOQUEIA o comando de maximizar se isso for false.
            Window.AllowUserResizing = true; 

            // Escutar mudança de tamanho para ajustar resolução automaticamente
            Window.ClientSizeChanged += OnClientSizeChanged;

            base.Initialize();
            Console.WriteLine("[Game1V2] Initialize() completed");
        }

        protected override void LoadContent()
        {
            Console.WriteLine("[Game1V2] LoadContent() started");
            
            try
            {
                _spriteBatch = new SpriteBatch(GraphicsDevice);
                _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
                
                try
                {
                    _font = Content.Load<SpriteFont>("DefaultFont");
                }
                catch 
                {
                    // Fallback silencioso
                }
                
                // Ensure assets
                string mapsDir = Path.Combine("assets", "maps");
                if (!Directory.Exists(mapsDir)) Directory.CreateDirectory(mapsDir);
                
                _stateManager = new StateManager();
                
                // Passamos o método MaximizeWindow corrigido
                var mainMenuState = CreateMainMenuState();
                _stateManager.SwitchState(mainMenuState);
                
                Console.WriteLine("[Game1V2] LoadContent() completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Game1V2] FATAL LOAD ERROR: {ex.Message}");
                throw;
            }
        }

        // --- Helpers de Estado (Mantidos iguais ao seu código original) ---
        private IGameState CreateMainMenuState()
        {
            var mainMenu = new MainMenuState(
                GraphicsDevice,
                _spriteBatch,
                _font,
                _pixelTexture
            );
            
            mainMenu.OnPlayMap += (path) => OpenPlayState(path);
            mainMenu.OnEditMap += (path) => OpenEditorState(path);
            mainMenu.OnNewMapCreated += (path) => OpenEditorState(path);
            mainMenu.OnExit += Exit;
            
            return mainMenu;
        }

        private void OpenPlayState(string mapPath)
        {
            try
            {
                var playState = new PlayState(GraphicsDevice, _spriteBatch, _font, _pixelTexture, mapPath);
                playState.OnReturnToMenu += () => _stateManager.SwitchState(CreateMainMenuState());
                _stateManager.SwitchState(playState);
            }
            catch { _stateManager.SwitchState(CreateMainMenuState()); }
        }

        private void OpenEditorState(string mapPath)
        {
            try
            {
                MapDefinition mapDef = MapLoader.Load(mapPath) ?? MapLoader.CreateDefaultMap(256, 256, 32, 64);
                if (!File.Exists(mapPath)) MapSaver.Save(mapPath, mapDef);

                var chunkedMap = new ChunkedTileMap(mapDef);
                
                var editorState = new EditorState(
                    GraphicsDevice, 
                    _spriteBatch, 
                    _font, 
                    _pixelTexture, 
                    chunkedMap, 
                    mapDef, 
                    mapPath,
                    () => {
                        // Toggle Fullscreen action for Editor
                        _graphics.IsFullScreen = !_graphics.IsFullScreen;
                        _graphics.ApplyChanges();
                    }
                );
                
                editorState.OnReturnToMenu += () => _stateManager.SwitchState(CreateMainMenuState());
                _stateManager.SwitchState(editorState);
            }
            catch { _stateManager.SwitchState(CreateMainMenuState()); }
        }

        protected override void Update(GameTime gameTime)
        {
            _stateManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _stateManager.Draw(gameTime);
            DrawToast();
            base.Draw(gameTime);
        }

        // --- Toast System ---
        private string _toastMessage;
        private DateTime _toastExpireUtc = DateTime.MinValue;

        public void ShowToast(string message, double seconds = 2.5)
        {
            _toastMessage = message;
            _toastExpireUtc = DateTime.UtcNow.AddSeconds(seconds);
        }

        private void DrawToast()
        {
            if (string.IsNullOrEmpty(_toastMessage) || DateTime.UtcNow >= _toastExpireUtc) return;

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            int w = _graphics.PreferredBackBufferWidth;
            int h = _graphics.PreferredBackBufferHeight;
            Vector2 size = _font?.MeasureString(_toastMessage) ?? Vector2.Zero;
            
            int boxW = (int)size.X + 40;
            int boxH = 40;
            Rectangle rect = new Rectangle((w - boxW) / 2, h - 80, boxW, boxH);
            
            _spriteBatch.Draw(_pixelTexture, rect, new Color(0, 0, 0, 200));
            if (_font != null)
                _spriteBatch.DrawString(_font, _toastMessage, new Vector2(rect.X + 20, rect.Y + (boxH - size.Y)/2), Color.White);
            
            _spriteBatch.End();
        }
    }
}