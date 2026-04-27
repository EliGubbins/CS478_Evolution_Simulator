using EcosystemSimulator.Analytics;
using EcosystemSimulator.Models;
using EcosystemSimulator.MonoGame;
using EvolutionSimulator.Core;
using EvolutionSimulator.Core.Entities;
using EvolutionSimulator.Core.Models;
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
        Clicked,
        Results
    }
    public sealed class SimulationGame : Game
    {
        private const float DefaultSimulationTimeScale = 1.5f;
        private const float MinimumSimulationTimeScale = 0.25f;
        private const float MaximumSimulationTimeScale = 8f;
        private const float SimulationTimeScaleStep = 0.25f;
        private const float ClickSelectionWorldRadius = 3f;
        private const float DetailPanelPadding = 10f;
        private const float DetailPanelWidth = 260f;
        private const float EmptyPopulationResetDelaySeconds = 5f;
        private const int TopBarHeight = 52;
        private const int TopBarPadding = 12;
        private const int ToggleSpacing = 18;
        private const int CheckboxSize = 18;
        private const int TopBarReservedSpace = 72;

        private readonly GraphicsDeviceManager graphics;
        private readonly SimulationEngine engine;
        private readonly SimulationRenderBridge renderBridge;

        private MonoGameRenderFrameRenderer? renderer;
        private WorldRenderViewport viewport;
        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;
        private float simulationTimeScale;

        private SimulationMode _mode = SimulationMode.SliderMenu;
        private MainMenu _mainMenu = null!;

        private FontSystem _fontSystem;
        private SpriteFontBase _menuFont;
        private SpriteFontBase _detailFont = null!;
        private SpriteBatch? _uiSpriteBatch;
        private Texture2D? _panelTexture;

        private Guid? _selectedOrganismId;
        private bool _showDirectionVectors = true;
        private bool _showFieldOfViews = true;
        private bool _showBorderBox = true;
        private bool _loopWhenExtinct = false;

        // Auto-reset tracking
        private float _emptyPopulationTimer = 0f;
        private bool _isEmptyPopulationPhase = false;

        public string configPath = string.Empty;
        private SimulationParameters _activeParameters = new();

        public SimulationGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += HandleClientSizeChanged;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            configPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..",
                "DefaultParametersMono.ini");

            configPath = Path.GetFullPath(configPath);
            DefaultParameters parameters = new DefaultParameters();
            parameters = DefaultParameters.LoadFromFile(configPath);
            _activeParameters = CreateSimulationParameters(parameters);

            engine = new SimulationEngine(
                worldWidth: parameters.WorldWidth,
                worldHeight: parameters.WorldHeight,
                initialPreyCount: parameters.InitialPreyCount,
                initialPredatorCount: parameters.InitialPredatorCount,
                preyStartingEnergy: parameters.PreyStartingEnergy,
                predatorStartingEnergy: parameters.PredatorStartingEnergy,
                mutationRate: parameters.MutationRate);

            ApplySimulationParameters(_activeParameters);

            renderBridge = new SimulationRenderBridge(engine);
            simulationTimeScale = DefaultSimulationTimeScale;
            viewport = renderBridge.CreateViewport(
                graphics.PreferredBackBufferWidth,
                graphics.PreferredBackBufferHeight,
                topInset: TopBarReservedSpace);

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
            _detailFont = _fontSystem.GetFont(18);

            _uiSpriteBatch = new SpriteBatch(GraphicsDevice);
            _panelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _panelTexture.SetData([Color.White]);
            
            // Initialize MainMenu
            _mainMenu = new MainMenu(GraphicsDevice, fontPath, configPath);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            switch (_mode)
            {
                case SimulationMode.SliderMenu:
                    var action = _mainMenu.Update(keyboardState, previousKeyboardState);
                    if (action == MenuAction.Start)
                    {
                        _activeParameters = CloneParameters(_mainMenu.Parameters);
                        ApplySimulationParameters(_activeParameters);
                        _selectedOrganismId = null;
                        engine.Start();
                        _mode = SimulationMode.Running;
                        _emptyPopulationTimer = 0f;
                        _isEmptyPopulationPhase = false;
                        
               
                    }
                    else if (action == MenuAction.Quit)
                        Exit();
                    break;

                case SimulationMode.Running:
                case SimulationMode.Paused:
                case SimulationMode.Clicked:
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
                        if (engine.IsRunning) { engine.Stop(); _mode = _selectedOrganismId.HasValue ? SimulationMode.Clicked : SimulationMode.Paused; }
                        else { engine.Start(); _mode = _selectedOrganismId.HasValue ? SimulationMode.Clicked : SimulationMode.Running; }
                    }
                    if (IsSingleKeyPress(keyboardState, Keys.R))
                    {
                        _selectedOrganismId = null;
                        ApplySimulationParameters(_activeParameters);
                        engine.Start();
                        _mode = SimulationMode.Running;
                        _emptyPopulationTimer = 0f;
                        _isEmptyPopulationPhase = false;
                    }
                    if (IsSingleKeyPress(keyboardState, Keys.OemPlus) || IsSingleKeyPress(keyboardState, Keys.Add))
                        AdjustSimulationSpeed(SimulationTimeScaleStep);
                    if (IsSingleKeyPress(keyboardState, Keys.OemMinus) || IsSingleKeyPress(keyboardState, Keys.Subtract))
                        AdjustSimulationSpeed(-SimulationTimeScaleStep);

                    if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                    {
                        if (!HandleSimulationOverlayClick(mouseState.X, mouseState.Y))
                            HandleOrganismClick(mouseState.X, mouseState.Y);
                    }

                    if (_mode == SimulationMode.Running || (_mode == SimulationMode.Clicked && engine.IsRunning))
                    {
                        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds * simulationTimeScale;
                        engine.Step(deltaTime);
                    }

                    // Clear selection if the organism died
                    if (_selectedOrganismId.HasValue && FindOrganismById(_selectedOrganismId.Value) is null)
                        _selectedOrganismId = null;

                    // Check for empty population and handle auto-reset if enabled
                    if (_loopWhenExtinct)
                        HandleAutoReset(gameTime);

                    break;
            }

            previousKeyboardState = keyboardState;
            previousMouseState = mouseState;
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
                renderer.ShowDirectionIndicators = _showDirectionVectors;
                renderer.ShowVisionCones = _showFieldOfViews;
                renderer.ShowWorldBorder = _showBorderBox;
                renderer.Render(frame, viewport);
                DrawSimulationTopBar();

                // Draw organism detail panel overlay
                if (_selectedOrganismId.HasValue)
                {
                    Organism? selected = FindOrganismById(_selectedOrganismId.Value);
                    if (selected is not null)
                        DrawOrganismDetailPanel(selected);
                }
            }

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Window.ClientSizeChanged -= HandleClientSizeChanged;
                renderer?.Dispose();
                _uiSpriteBatch?.Dispose();
                _panelTexture?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void HandleAutoReset(GameTime gameTime)
        {
            int preyCount = 0;
            int predatorCount = 0;

            foreach (Organism organism in engine.PopulationManager.GetAllLivingOrganisms())
            {
                if (organism is Predator)
                    predatorCount++;
                else
                    preyCount++;
            }

            bool populationEmpty = preyCount == 0 || predatorCount == 0;

            if (populationEmpty)
            {
                if (!_isEmptyPopulationPhase)
                {
                    _isEmptyPopulationPhase = true;
                    _emptyPopulationTimer = 0f;
                }

                _emptyPopulationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_emptyPopulationTimer >= EmptyPopulationResetDelaySeconds)
                {
                    _selectedOrganismId = null;
                    engine.Stop();
                    engine.Reset();
                    engine.EnvironmentManager.MaxFoodCount = 180;
                    engine.EnvironmentManager.DefaultFoodNutritionValue = 10f;
                    engine.EnvironmentManager.FoodRegenerationRate = 8f;
                    engine.EnvironmentManager.SeedInitialFood(100);
                    engine.Start();
                    _mode = SimulationMode.Running;
                    _emptyPopulationTimer = 0f;
                    _isEmptyPopulationPhase = false;
                }
            }
            else
            {
                _isEmptyPopulationPhase = false;
                _emptyPopulationTimer = 0f;
            }
        }

        private void HandleOrganismClick(int screenX, int screenY)
        {
            float worldX = viewport.ToWorldX(screenX);
            float worldY = viewport.ToWorldY(screenY);

            Organism? closest = null;
            float closestDistance = float.MaxValue;

            foreach (Organism organism in engine.PopulationManager.GetAllLivingOrganisms())
            {
                float distance = organism.DistanceTo(worldX, worldY);
                if (distance < closestDistance && distance <= ClickSelectionWorldRadius + organism.Traits.Size)
                {
                    closest = organism;
                    closestDistance = distance;
                }
            }

            if (closest is not null)
            {
                _selectedOrganismId = closest.Id;
                _mode = SimulationMode.Clicked;
            }
            else
            {
                _selectedOrganismId = null;
                _mode = engine.IsRunning ? SimulationMode.Running : SimulationMode.Paused;
            }
        }

        private Organism? FindOrganismById(Guid id)
        {
            foreach (Organism organism in engine.PopulationManager.GetAllLivingOrganisms())
            {
                if (organism.Id == id)
                    return organism;
            }
            return null;
        }

        private void DrawOrganismDetailPanel(Organism organism)
        {
            if (_uiSpriteBatch is null || _panelTexture is null || _detailFont is null)
                return;

            string kind = organism is Predator ? "Predator" : "Prey";

            string[] lines =
            [
                $"=== {kind} ===",
                $"Energy:   {organism.Energy:F1}",
                $"Age:      {organism.Age}",
                $"Speed:    {organism.Traits.Speed:F2}",
                $"Size:     {organism.Traits.Size:F2}",
                $"Stamina:  {organism.Traits.Stamina:F2}",
                $"Vision:   {organism.Traits.VisionDistance:F2}",
                $"Metab:    {organism.Traits.Metabolism:F2}",
                $"FOV:      {organism.VisionFieldOfViewDegrees:F0}°",
                $"Pos:      ({organism.X:F1}, {organism.Y:F1})"
            ];

            float lineHeight = _detailFont.LineHeight;
            float panelHeight = (lines.Length * lineHeight) + (DetailPanelPadding * 2);
            float panelX = GraphicsDevice.PresentationParameters.BackBufferWidth - DetailPanelWidth - DetailPanelPadding;
            float panelY = TopBarHeight + DetailPanelPadding;

            _uiSpriteBatch.Begin(blendState: BlendState.AlphaBlend);

            // Draw semi-transparent background
            _uiSpriteBatch.Draw(
                _panelTexture,
                new Rectangle((int)panelX, (int)panelY, (int)DetailPanelWidth, (int)panelHeight),
                Color.Black * 0.75f);

            // Draw each line of text
            float textX = panelX + DetailPanelPadding;
            float textY = panelY + DetailPanelPadding;

            foreach (string line in lines)
            {
                _uiSpriteBatch.DrawString(_detailFont, line, new Vector2(textX, textY), Color.White);
                textY += lineHeight;
            }

            // Draw highlight ring around selected organism in world space
            float ringScreenX = viewport.ToScreenX(organism.X);
            float ringScreenY = viewport.ToScreenY(organism.Y);
            float ringSize = viewport.ToScreenSize(organism.Traits.Size * 2.5f);
            Color ringColor = organism is Predator ? Color.Red : Color.LimeGreen;

            _uiSpriteBatch.Draw(
                _panelTexture,
                new Rectangle((int)(ringScreenX - ringSize / 2), (int)(ringScreenY - ringSize / 2), (int)ringSize, 2),
                ringColor);
            _uiSpriteBatch.Draw(
                _panelTexture,
                new Rectangle((int)(ringScreenX - ringSize / 2), (int)(ringScreenY + ringSize / 2), (int)ringSize, 2),
                ringColor);
            _uiSpriteBatch.Draw(
                _panelTexture,
                new Rectangle((int)(ringScreenX - ringSize / 2), (int)(ringScreenY - ringSize / 2), 2, (int)ringSize),
                ringColor);
            _uiSpriteBatch.Draw(
                _panelTexture,
                new Rectangle((int)(ringScreenX + ringSize / 2), (int)(ringScreenY - ringSize / 2), 2, (int)ringSize),
                ringColor);

            _uiSpriteBatch.End();
        }

        private void DrawSimulationTopBar()
        {
            if (_uiSpriteBatch is null || _panelTexture is null || _detailFont is null)
                return;

            Rectangle barRect = new(
                TopBarPadding,
                TopBarPadding,
                GraphicsDevice.PresentationParameters.BackBufferWidth - (TopBarPadding * 2),
                TopBarHeight);

            Rectangle directionRect = GetToggleBounds(TopBarPadding + 16, "Direction Vectors");
            Rectangle fieldOfViewRect = GetToggleBounds(directionRect.Right + ToggleSpacing, "Field of Views");
            Rectangle borderRect = GetToggleBounds(fieldOfViewRect.Right + ToggleSpacing, "Border Box");
            Rectangle loopRect = GetToggleBounds(borderRect.Right + ToggleSpacing, "Loop on Extinction");

            _uiSpriteBatch.Begin(blendState: BlendState.AlphaBlend);

            _uiSpriteBatch.Draw(_panelTexture, barRect, new Color(255, 255, 255, 220));
            DrawToggle(directionRect, "Direction Vectors", _showDirectionVectors);
            DrawToggle(fieldOfViewRect, "Field of Views", _showFieldOfViews);
            DrawToggle(borderRect, "Border Box", _showBorderBox);
            DrawToggle(loopRect, "Loop on Extinction", _loopWhenExtinct);

            _uiSpriteBatch.End();
        }

        private void DrawToggle(Rectangle bounds, string label, bool isChecked)
        {
            if (_uiSpriteBatch is null || _panelTexture is null || _detailFont is null)
                return;

            Rectangle checkboxRect = new(
                bounds.X,
                bounds.Y + ((bounds.Height - CheckboxSize) / 2),
                CheckboxSize,
                CheckboxSize);

            _uiSpriteBatch.Draw(_panelTexture, checkboxRect, Color.White);
            _uiSpriteBatch.Draw(_panelTexture, new Rectangle(checkboxRect.X, checkboxRect.Y, checkboxRect.Width, 2), Color.DarkSlateGray);
            _uiSpriteBatch.Draw(_panelTexture, new Rectangle(checkboxRect.X, checkboxRect.Bottom - 2, checkboxRect.Width, 2), Color.DarkSlateGray);
            _uiSpriteBatch.Draw(_panelTexture, new Rectangle(checkboxRect.X, checkboxRect.Y, 2, checkboxRect.Height), Color.DarkSlateGray);
            _uiSpriteBatch.Draw(_panelTexture, new Rectangle(checkboxRect.Right - 2, checkboxRect.Y, 2, checkboxRect.Height), Color.DarkSlateGray);

            if (isChecked)
            {
                Rectangle fillRect = new(
                    checkboxRect.X + 4,
                    checkboxRect.Y + 4,
                    checkboxRect.Width - 8,
                    checkboxRect.Height - 8);
                _uiSpriteBatch.Draw(_panelTexture, fillRect, Color.ForestGreen);
            }

            Vector2 textPosition = new(checkboxRect.Right + 8f, bounds.Y + ((bounds.Height - _detailFont.LineHeight) / 2f));
            _uiSpriteBatch.DrawString(_detailFont, label, textPosition, Color.Black);
        }

        private bool HandleSimulationOverlayClick(int mouseX, int mouseY)
        {
            Rectangle directionRect = GetToggleBounds(TopBarPadding + 16, "Direction Vectors");
            Rectangle fieldOfViewRect = GetToggleBounds(directionRect.Right + ToggleSpacing, "Field of Views");
            Rectangle borderRect = GetToggleBounds(fieldOfViewRect.Right + ToggleSpacing, "Border Box");
            Rectangle loopRect = GetToggleBounds(borderRect.Right + ToggleSpacing, "Loop on Extinction");
            Point clickPoint = new(mouseX, mouseY);

            if (directionRect.Contains(clickPoint))
            {
                _showDirectionVectors = !_showDirectionVectors;
                return true;
            }

            if (fieldOfViewRect.Contains(clickPoint))
            {
                _showFieldOfViews = !_showFieldOfViews;
                return true;
            }

            if (borderRect.Contains(clickPoint))
            {
                _showBorderBox = !_showBorderBox;
                return true;
            }

            if (loopRect.Contains(clickPoint))
            {
                _loopWhenExtinct = !_loopWhenExtinct;
                return true;
            }

            return false;
        }

        private Rectangle GetToggleBounds(int left, string label)
        {
            float textWidth = _detailFont is null ? 120f : _detailFont.MeasureString(label).X;
            int width = CheckboxSize + 8 + (int)MathF.Ceiling(textWidth) + 12;

            return new Rectangle(
                left,
                TopBarPadding + 8,
                width,
                TopBarHeight - 16);
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
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                topInset: TopBarReservedSpace);

            renderer?.Resize(viewport);
        }

        private void UpdateWindowTitle()
        {
            Window.Title = $"Evolution Simulator | Speed {simulationTimeScale:0.00}x | Space Pause | R Reset | +/- Speed | ESC Close & Save Metrics";
        }

        private void ApplySimulationParameters(SimulationParameters parameters)
        {
            engine.PopulationManager.InitialTraitVariance = parameters.InitialTraitVariance;
            engine.PopulationManager.DefaultPreyTraits = new Traits(
                parameters.PreySpeed,
                parameters.PreySize,
                parameters.PreyStamina,
                parameters.PreyVisionDistance,
                parameters.PreyMetabolism);
            engine.PopulationManager.DefaultPredatorTraits = new Traits(
                parameters.PredatorSpeed,
                parameters.PredatorSize,
                parameters.PredatorStamina,
                parameters.PredatorVisionDistance,
                parameters.PredatorMetabolism);

            engine.Initialize(
                parameters.InitialPreyCount,
                parameters.InitialPredatorCount,
                parameters.PreyStartingEnergy,
                parameters.PredatorStartingEnergy,
                parameters.MutationRate);

            engine.EnvironmentManager.MaxFoodCount = parameters.MaxFoodCount;
            engine.EnvironmentManager.DefaultFoodNutritionValue = parameters.FoodNutritionValue;
            engine.EnvironmentManager.FoodRegenerationRate = parameters.FoodRegenerationRate;
            engine.EnvironmentManager.ClearAllTerrain();
            engine.EnvironmentManager.SeedInitialFood(parameters.InitialFoodCount);
        }

        private static SimulationParameters CreateSimulationParameters(DefaultParameters parameters)
        {
            return new SimulationParameters
            {
                InitialPreyCount = parameters.InitialPreyCount,
                InitialPredatorCount = parameters.InitialPredatorCount,
                PreyStartingEnergy = parameters.PreyStartingEnergy,
                PredatorStartingEnergy = parameters.PredatorStartingEnergy,
                MutationRate = parameters.MutationRate,
                InitialFoodCount = parameters.InitialFoodCount,
                FoodRegenerationRate = parameters.FoodRegenerationRate,
                FoodNutritionValue = parameters.FoodNutritionValue,
                MaxFoodCount = parameters.MaxFoodCount,
                InitialTraitVariance = parameters.InitialTraitVariance,
                PreySpeed = parameters.PreySpeed,
                PreySize = parameters.PreySize,
                PreyStamina = parameters.PreyStamina,
                PreyVisionDistance = parameters.PreyVisionDistance,
                PreyMetabolism = parameters.PreyMetabolism,
                PredatorSpeed = parameters.PredatorSpeed,
                PredatorSize = parameters.PredatorSize,
                PredatorStamina = parameters.PredatorStamina,
                PredatorVisionDistance = parameters.PredatorVisionDistance,
                PredatorMetabolism = parameters.PredatorMetabolism,
            };
        }

        private static SimulationParameters CloneParameters(SimulationParameters parameters)
        {
            return new SimulationParameters
            {
                InitialPreyCount = parameters.InitialPreyCount,
                InitialPredatorCount = parameters.InitialPredatorCount,
                PreyStartingEnergy = parameters.PreyStartingEnergy,
                PredatorStartingEnergy = parameters.PredatorStartingEnergy,
                MutationRate = parameters.MutationRate,
                InitialFoodCount = parameters.InitialFoodCount,
                FoodRegenerationRate = parameters.FoodRegenerationRate,
                FoodNutritionValue = parameters.FoodNutritionValue,
                MaxFoodCount = parameters.MaxFoodCount,
                InitialTraitVariance = parameters.InitialTraitVariance,
                PreySpeed = parameters.PreySpeed,
                PreySize = parameters.PreySize,
                PreyStamina = parameters.PreyStamina,
                PreyVisionDistance = parameters.PreyVisionDistance,
                PreyMetabolism = parameters.PreyMetabolism,
                PredatorSpeed = parameters.PredatorSpeed,
                PredatorSize = parameters.PredatorSize,
                PredatorStamina = parameters.PredatorStamina,
                PredatorVisionDistance = parameters.PredatorVisionDistance,
                PredatorMetabolism = parameters.PredatorMetabolism,
            };
        }
    }
}

