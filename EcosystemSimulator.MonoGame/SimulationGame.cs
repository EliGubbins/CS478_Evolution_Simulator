using EcosystemSimulator.Analytics;
using EcosystemSimulator.Models;
using EcosystemSimulator.MonoGame;
using EvolutionSimulator.Core;
using EvolutionSimulator.Core.Rendering;
using EvolutionSimulator.MonoGameHost.Graphics;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Security.Cryptography.X509Certificates;

namespace EvolutionSimulator.MonoGameHost
{
    public enum SimulationMode
    {
        SliderMenu,
        Running,
        Paused,
        Results
    }
    public sealed class SimulationGame : Game
    {
        private const float DefaultSimulationTimeScale = 1.5f;
        private const float MinimumSimulationTimeScale = 0.25f;
        private const float MaximumSimulationTimeScale = 8f;
        private const float SimulationTimeScaleStep = 0.25f;

        private readonly GraphicsDeviceManager graphics;
        private readonly SimulationEngine engine;
        private readonly SimulationRenderBridge renderBridge;

        private MonoGameRenderFrameRenderer? renderer;
        private WorldRenderViewport viewport;
        private KeyboardState previousKeyboardState;
        private float simulationTimeScale;

        private SimulationMode _mode = SimulationMode.SliderMenu;
        private MainMenu _mainMenu = null!;

        private FontSystem _fontSystem;
        private SpriteFontBase _menuFont;

        public string configPath;

        public SimulationGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += HandleClientSizeChanged;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            string configPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..",
                "DefaultParametersMono.ini");

            configPath = Path.GetFullPath(configPath);
            DefaultParameters parameters = new DefaultParameters();
            parameters = DefaultParameters.LoadFromFile(configPath);

            engine = new SimulationEngine(
                worldWidth: parameters.WorldWidth,
                worldHeight: parameters.WorldHeight,
                initialPreyCount: parameters.InitialPreyCount,
                initialPredatorCount: parameters.InitialPredatorCount,
                preyStartingEnergy: parameters.PreyStartingEnergy,
                predatorStartingEnergy: parameters.PredatorStartingEnergy,
                mutationRate: parameters.MutationRate);

            engine.EnvironmentManager.MaxFoodCount = parameters.MaxFoodCount;
            engine.EnvironmentManager.DefaultFoodNutritionValue = parameters.FoodNutritionValue;
            engine.EnvironmentManager.FoodRegenerationRate = parameters.FoodRegenerationRate;
            engine.EnvironmentManager.SeedInitialFood(parameters.InitialFoodCount);

            renderBridge = new SimulationRenderBridge(engine);
            simulationTimeScale = DefaultSimulationTimeScale;
            viewport = renderBridge.CreateViewport(
                graphics.PreferredBackBufferWidth,
                graphics.PreferredBackBufferHeight);

            UpdateWindowTitle();
        }
        

        protected override void Initialize()
        {
            //engine.Start();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            renderer = new MonoGameRenderFrameRenderer(GraphicsDevice);
            renderer.Initialize(viewport, renderBridge.Palette);

            _fontSystem = new FontSystem();
            string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Figtree-VariableFont_wght.ttf");
            fontPath = Path.GetFullPath(fontPath);
            _fontSystem.AddFont(File.ReadAllBytes(fontPath));
            _menuFont = _fontSystem.GetFont(24);
            
            // Initialize MainMenu
            _mainMenu = new MainMenu(GraphicsDevice, fontPath, configPath);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            switch (_mode)
            {
                case SimulationMode.SliderMenu:
                    var action = _mainMenu.Update(keyboardState, previousKeyboardState);
                    if (action == MenuAction.Start)
                    {
                        engine.Start();
                        _mode = SimulationMode.Running;
                    }
                    else if (action == MenuAction.Quit)
                        Exit();
                    break;

                case SimulationMode.Running:
                case SimulationMode.Paused:
                    if (IsSingleKeyPress(keyboardState, Keys.Escape))
                    {
                        engine.Stop();
                        engine.MetricsManager.ExportToCsv();
                        var graphs = new Graphs(engine);
                        graphs.CreateAllGraphs();
                        Exit();
                    }
                    if (IsSingleKeyPress(keyboardState, Keys.Space))
                    {
                        if (engine.IsRunning) { engine.Stop(); _mode = SimulationMode.Paused; }
                        else { engine.Start(); _mode = SimulationMode.Running; }
                    }
                    if (IsSingleKeyPress(keyboardState, Keys.R))
                    {
                        engine.Reset();
                        engine.EnvironmentManager.MaxFoodCount = 180;
                        engine.EnvironmentManager.DefaultFoodNutritionValue = 10f;
                        engine.EnvironmentManager.FoodRegenerationRate = 8f;
                        engine.EnvironmentManager.SeedInitialFood(100);
                        engine.Start();
                        _mode = SimulationMode.Running;
                    }
                    if (IsSingleKeyPress(keyboardState, Keys.OemPlus) || IsSingleKeyPress(keyboardState, Keys.Add))
                        AdjustSimulationSpeed(SimulationTimeScaleStep);
                    if (IsSingleKeyPress(keyboardState, Keys.OemMinus) || IsSingleKeyPress(keyboardState, Keys.Subtract))
                        AdjustSimulationSpeed(-SimulationTimeScaleStep);

                    if (_mode == SimulationMode.Running)
                    {
                        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds * simulationTimeScale;
                        engine.Step(deltaTime);
                    }
                    break;
            }

            previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (renderer is null) return;

            if (_mode == SimulationMode.SliderMenu)
            {
                GraphicsDevice.Clear(Color.White);
                _mainMenu.Draw();
            }
            else
            {
                SimulationRenderFrame frame = renderBridge.CreateFrame();
                renderer.Render(frame, viewport);
            }

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Window.ClientSizeChanged -= HandleClientSizeChanged;
                renderer?.Dispose();
            }

            base.Dispose(disposing);
        }

        private bool IsSingleKeyPress(KeyboardState keyboardState, Keys key)
        {
            return keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
        }

        private void AdjustSimulationSpeed(float delta)
        {
            simulationTimeScale = Math.Clamp(
                simulationTimeScale + delta,
                MinimumSimulationTimeScale,
                MaximumSimulationTimeScale);

            UpdateWindowTitle();
        }

        private void HandleClientSizeChanged(object? sender, EventArgs e)
        {
            viewport = renderBridge.CreateViewport(
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight);

            renderer?.Resize(viewport);
        }

        private void UpdateWindowTitle()
        {
            Window.Title = $"Evolution Simulator | Speed {simulationTimeScale:0.00}x | Space Pause | R Reset | +/- Speed | ESC Close & Save Metrics";
        }
    }
}
