using EcosystemSimulator.Analytics;
using EcosystemSimulator.Models;
using EvolutionSimulator.Core;
using EvolutionSimulator.Core.Rendering;
using EvolutionSimulator.MonoGameHost.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Security.Cryptography.X509Certificates;

namespace EvolutionSimulator.MonoGameHost
{
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
            engine.Start();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            renderer = new MonoGameRenderFrameRenderer(GraphicsDevice);
            renderer.Initialize(viewport, renderBridge.Palette);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (IsSingleKeyPress(keyboardState, Keys.Space))
            {
                if (engine.IsRunning)
                    engine.Stop();
                else
                    engine.Start();
            }

            if (IsSingleKeyPress(keyboardState, Keys.R))
            {
                engine.Reset();
                engine.EnvironmentManager.MaxFoodCount = 180;
                engine.EnvironmentManager.DefaultFoodNutritionValue = 10f;
                engine.EnvironmentManager.FoodRegenerationRate = 8f;
                engine.EnvironmentManager.SeedInitialFood(100);
                engine.Start();
            }

            if (IsSingleKeyPress(keyboardState, Keys.OemPlus) || IsSingleKeyPress(keyboardState, Keys.Add))
                AdjustSimulationSpeed(SimulationTimeScaleStep);

            if (IsSingleKeyPress(keyboardState, Keys.OemMinus) || IsSingleKeyPress(keyboardState, Keys.Subtract))
                AdjustSimulationSpeed(-SimulationTimeScaleStep);

            if (IsSingleKeyPress(keyboardState, Keys.OemMinus) || IsSingleKeyPress(keyboardState, Keys.Escape))
            {
                engine.MetricsManager.ExportToCsv();
                var graphs = new Graphs(engine);
                graphs.CreateAllGraphs();
                Exit();
            }
            if (engine.IsRunning)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds * simulationTimeScale;
                engine.Step(deltaTime);
            }

            previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (renderer is null)
                return;

            SimulationRenderFrame frame = renderBridge.CreateFrame();
            renderer.Render(frame, viewport);

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
