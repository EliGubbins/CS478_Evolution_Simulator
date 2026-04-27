using EcosystemSimulator.Models;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace EcosystemSimulator.MonoGame
{
    public enum MenuAction { None, Start, Quit }

    public class SimulationParameters
    {
        public int InitialPreyCount = 35;
        public int InitialPredatorCount = 10;
        public float PreyStartingEnergy = 55;
        public float PredatorStartingEnergy = 70;
        public float MutationRate = 0.1f;
        public int InitialFoodCount = 100;
        public float FoodRegenerationRate = 5f;
        public float FoodNutritionValue = 30f;
        public int MaxFoodCount = 180;

        // Trait variance
        public float InitialTraitVariance = 2f;

        // Prey traits
        public float PreySpeed = 6f;
        public float PreySize = 2.5f;
        public float PreyStamina = 6f;
        public float PreyVisionDistance = 7f;
        public float PreyMetabolism = 4f;

        // Predator traits
        public float PredatorSpeed = 8f;
        public float PredatorSize = 4.5f;
        public float PredatorStamina = 4f;
        public float PredatorVisionDistance = 12f;
        public float PredatorMetabolism = 4f;

        // Visualization options
        public bool DrawVisionRadiusCones = false;
        public bool LoopWhenLowOnOrganisms = false;
    }

    public class MainMenu
    {
        // Layout constants (shared between Update and Draw)
        private const int StartY = 100;
        private const int RowHeight = 44;
        private const int SectionHeaderHeight = 38;
        private const int ScrollAreaTop = 90;
        private const int HintAreaHeight = 50;
        private const int LabelWidth = 260;
        private const int BoxWidth = 160;
        private const int BoxHeight = 34;
        private const int BoxPadding = 8;
        private const int ColumnGap = 20;
        private const int Columns = 2;
        private const int CheckboxSize = 24;
        private const int CheckboxGap = 8;

        // Fonts
        private FontSystem _fontSystem;
        private SpriteFontBase _titleFont;
        private SpriteFontBase _menuFont;
        private SpriteFontBase _hintFont;
        private SpriteFontBase _labelFont;

        // Graphics
        private readonly GraphicsDevice _graphics;
        private readonly SpriteBatch _spriteBatch;
        private readonly RasterizerState _scissorRasterizer;

        // Parameters
        public SimulationParameters Parameters { get; private set; } = new();

        private readonly (string Label, bool IsFloat)[] _fields = {
            // Simulation (0–8)
            ("Initial Prey Count",       false),
            ("Initial Predator Count",   false),
            ("Prey Starting Energy",     true),
            ("Predator Starting Energy", true),
            ("Mutation Rate",            true),
            ("Initial Food Count",       false),
            ("Food Regeneration Rate",   true),
            ("Food Nutrition Value",     true),
            ("Max Food Count",           false),
            // Prey Traits (9–14)
            ("Initial Trait Variance",   true),
            ("Prey Speed",               true),
            ("Prey Size",                true),
            ("Prey Stamina",             true),
            ("Prey Vision Distance",     true),
            ("Prey Metabolism",          true),
            // Predator Traits (15–19)
            ("Predator Speed",           true),
            ("Predator Size",            true),
            ("Predator Stamina",         true),
            ("Predator Vision Distance", true),
            ("Predator Metabolism",      true),
        };

        private readonly (string Header, int Start, int Count)[] _sections = {
            ("Simulation",      0,  9),
            ("Prey Traits",     9,  6),
            ("Predator Traits", 15, 5),
        };

        private string[] _fieldValues;
        private int _focusedField = -1;
        private int _selectedButton = 0;

        // Checkbox state
        private bool[] _checkboxStates;
        private Rectangle[] _checkboxRects;
        private readonly string[] _checkboxLabels = {
            "Draw Vision Radius Cones",
            "Loop When Low On Organisms"
        };

        // Layout — recomputed each Draw, used for hit testing
        private Rectangle[] _fieldBoxRects;
        private Rectangle[] _buttonRects;

        // Scroll state
        private float _scrollOffset;
        private float _contentHeight;

        // Mouse state
        private MouseState _prevMouse;

        public MainMenu(GraphicsDevice graphics, string fontPath, string configPath)
        {
            _graphics = graphics;
            _spriteBatch = new SpriteBatch(graphics);
            _scissorRasterizer = new RasterizerState { ScissorTestEnable = true };

            _fontSystem = new FontSystem();
            _fontSystem.AddFont(File.ReadAllBytes(fontPath));

            _titleFont = _fontSystem.GetFont(42);
            _menuFont = _fontSystem.GetFont(26);
            _hintFont = _fontSystem.GetFont(16);
            _labelFont = _fontSystem.GetFont(20);

            var defaults = DefaultParameters.LoadFromFile(configPath);
            Parameters = new SimulationParameters
            {
                InitialPreyCount = defaults.InitialPreyCount,
                InitialPredatorCount = defaults.InitialPredatorCount,
                PreyStartingEnergy = defaults.PreyStartingEnergy,
                PredatorStartingEnergy = defaults.PredatorStartingEnergy,
                MutationRate = defaults.MutationRate,
                InitialFoodCount = defaults.InitialFoodCount,
                FoodRegenerationRate = defaults.FoodRegenerationRate,
                FoodNutritionValue = defaults.FoodNutritionValue,
                MaxFoodCount = defaults.MaxFoodCount,
                InitialTraitVariance = defaults.InitialTraitVariance,
                PreySpeed = defaults.PreySpeed,
                PreySize = defaults.PreySize,
                PreyStamina = defaults.PreyStamina,
                PreyVisionDistance = defaults.PreyVisionDistance,
                PreyMetabolism = defaults.PreyMetabolism,
                PredatorSpeed = defaults.PredatorSpeed,
                PredatorSize = defaults.PredatorSize,
                PredatorStamina = defaults.PredatorStamina,
                PredatorVisionDistance = defaults.PredatorVisionDistance,
                PredatorMetabolism = defaults.PredatorMetabolism,
                DrawVisionRadiusCones = false,
                LoopWhenLowOnOrganisms = false,
            };

            _fieldValues = new string[]
            {
                Parameters.InitialPreyCount.ToString(),
                Parameters.InitialPredatorCount.ToString(),
                Parameters.PreyStartingEnergy.ToString(),
                Parameters.PredatorStartingEnergy.ToString(),
                Parameters.MutationRate.ToString(),
                Parameters.InitialFoodCount.ToString(),
                Parameters.FoodRegenerationRate.ToString(),
                Parameters.FoodNutritionValue.ToString(),
                Parameters.MaxFoodCount.ToString(),
                Parameters.InitialTraitVariance.ToString(),
                Parameters.PreySpeed.ToString(),
                Parameters.PreySize.ToString(),
                Parameters.PreyStamina.ToString(),
                Parameters.PreyVisionDistance.ToString(),
                Parameters.PreyMetabolism.ToString(),
                Parameters.PredatorSpeed.ToString(),
                Parameters.PredatorSize.ToString(),
                Parameters.PredatorStamina.ToString(),
                Parameters.PredatorVisionDistance.ToString(),
                Parameters.PredatorMetabolism.ToString(),
            };

            _fieldBoxRects = new Rectangle[_fields.Length];
            _checkboxStates = new bool[2];
            _checkboxRects = new Rectangle[2];
            _buttonRects = new Rectangle[2];
        }

        public MenuAction Update(KeyboardState kb, KeyboardState prevKb)
        {
            MouseState mouse = Mouse.GetState();
            Point mousePos = mouse.Position;
            bool clicked = mouse.LeftButton == ButtonState.Released
                            && _prevMouse.LeftButton == ButtonState.Pressed;

            // Mouse-wheel scroll
            int scrollDelta = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;
            if (scrollDelta != 0)
            {
                _scrollOffset -= scrollDelta * 0.25f;
                ClampScroll();
            }

            // Click on field
            if (clicked)
            {
                bool hitField = false;
                for (int i = 0; i < _fieldBoxRects.Length; i++)
                {
                    if (_fieldBoxRects[i].Contains(mousePos))
                    {
                        if (_focusedField >= 0 && _focusedField != i)
                            CommitField(_focusedField);
                        _focusedField = i;
                        _selectedButton = -1;
                        hitField = true;
                        break;
                    }
                }

                // Click on a checkbox
                if (!hitField)
                {
                    for (int i = 0; i < _checkboxRects.Length; i++)
                    {
                        if (_checkboxRects[i].Contains(mousePos))
                        {
                            if (_focusedField >= 0)
                                CommitField(_focusedField);
                            _focusedField = -1;
                            _checkboxStates[i] = !_checkboxStates[i];
                            Parameters.DrawVisionRadiusCones = _checkboxStates[0];
                            Parameters.LoopWhenLowOnOrganisms = _checkboxStates[1];
                            hitField = true;
                            break;
                        }
                    }
                }

                // Click on a button
                if (!hitField)
                {
                    for (int i = 0; i < _buttonRects.Length; i++)
                    {
                        if (_buttonRects[i].Contains(mousePos))
                        {
                            if (_focusedField >= 0)
                                CommitField(_focusedField);
                            _focusedField = -1;
                            _selectedButton = i;

                            _prevMouse = mouse;
                            return i == 0 ? MenuAction.Start : MenuAction.Quit;
                        }
                    }

                    // Clicked blank area — unfocus field
                    if (_focusedField >= 0)
                    {
                        CommitField(_focusedField);
                        _focusedField = -1;
                    }
                }
            }

            // highlight buttons
            for (int i = 0; i < _buttonRects.Length; i++)
            {
                if (_buttonRects[i].Contains(mousePos))
                    _selectedButton = i;
            }

            // typing into field
            if (_focusedField >= 0)
            {
                HandleTextInput(kb, prevKb);

                if (IsPressed(kb, prevKb, Keys.Enter) || IsPressed(kb, prevKb, Keys.Tab))
                {
                    CommitField(_focusedField);
                    _focusedField++;
                    if (_focusedField >= _fields.Length)
                        _focusedField = -1;
                    EnsureFieldVisible(_focusedField);
                }

                if (IsPressed(kb, prevKb, Keys.Escape))
                {
                    CommitField(_focusedField);
                    _focusedField = -1;
                }

                _prevMouse = mouse;
                return MenuAction.None;
            }

            // navigate buttons with keyboard
            if (IsPressed(kb, prevKb, Keys.Down))
                _selectedButton = (_selectedButton + 1) % 2;
            if (IsPressed(kb, prevKb, Keys.Up))
                _selectedButton = (_selectedButton - 1 + 2) % 2;
            if (IsPressed(kb, prevKb, Keys.Tab))
            {
                _focusedField = 0;
                EnsureFieldVisible(0);
                _prevMouse = mouse;
                return MenuAction.None;
            }
            if (IsPressed(kb, prevKb, Keys.Enter))
            {
                _prevMouse = mouse;
                return _selectedButton == 0 ? MenuAction.Start : MenuAction.Quit;
            }

            _prevMouse = mouse;
            return MenuAction.None;
        }

        private void HandleTextInput(KeyboardState kb, KeyboardState prevKb)
        {
            bool isFloat = _fields[_focusedField].IsFloat;
            string current = _fieldValues[_focusedField];

            if (IsPressed(kb, prevKb, Keys.Back) && current.Length > 0)
            {
                _fieldValues[_focusedField] = current[..^1];
                return;
            }

            for (Keys k = Keys.D0; k <= Keys.D9; k++)
                if (IsPressed(kb, prevKb, k))
                    _fieldValues[_focusedField] += (char)('0' + (k - Keys.D0));

            for (Keys k = Keys.NumPad0; k <= Keys.NumPad9; k++)
                if (IsPressed(kb, prevKb, k))
                    _fieldValues[_focusedField] += (char)('0' + (k - Keys.NumPad0));

            if (isFloat && (IsPressed(kb, prevKb, Keys.OemPeriod) || IsPressed(kb, prevKb, Keys.Decimal)))
                if (!_fieldValues[_focusedField].Contains('.'))
                    _fieldValues[_focusedField] += '.';
        }

        private void CommitField(int index)
        {
            string v = _fieldValues[index];
            switch (index)
            {
                case 0: Parameters.InitialPreyCount = int.TryParse(v, out int a) ? a : Parameters.InitialPreyCount; break;
                case 1: Parameters.InitialPredatorCount = int.TryParse(v, out int b) ? b : Parameters.InitialPredatorCount; break;
                case 2: Parameters.PreyStartingEnergy = float.TryParse(v, out float c) ? c : Parameters.PreyStartingEnergy; break;
                case 3: Parameters.PredatorStartingEnergy = float.TryParse(v, out float d) ? d : Parameters.PredatorStartingEnergy; break;
                case 4: Parameters.MutationRate = float.TryParse(v, out float e) ? e : Parameters.MutationRate; break;
                case 5: Parameters.InitialFoodCount = int.TryParse(v, out int f) ? f : Parameters.InitialFoodCount; break;
                case 6: Parameters.FoodRegenerationRate = float.TryParse(v, out float g) ? g : Parameters.FoodRegenerationRate; break;
                case 7: Parameters.FoodNutritionValue = float.TryParse(v, out float h) ? h : Parameters.FoodNutritionValue; break;
                case 8: Parameters.MaxFoodCount = int.TryParse(v, out int i) ? i : Parameters.MaxFoodCount; break;
                case 9: Parameters.InitialTraitVariance = float.TryParse(v, out float j) ? j : Parameters.InitialTraitVariance; break;
                case 10: Parameters.PreySpeed = float.TryParse(v, out float k) ? k : Parameters.PreySpeed; break;
                case 11: Parameters.PreySize = float.TryParse(v, out float l) ? l : Parameters.PreySize; break;
                case 12: Parameters.PreyStamina = float.TryParse(v, out float m) ? m : Parameters.PreyStamina; break;
                case 13: Parameters.PreyVisionDistance = float.TryParse(v, out float n) ? n : Parameters.PreyVisionDistance; break;
                case 14: Parameters.PreyMetabolism = float.TryParse(v, out float o) ? o : Parameters.PreyMetabolism; break;
                case 15: Parameters.PredatorSpeed = float.TryParse(v, out float p) ? p : Parameters.PredatorSpeed; break;
                case 16: Parameters.PredatorSize = float.TryParse(v, out float q) ? q : Parameters.PredatorSize; break;
                case 17: Parameters.PredatorStamina = float.TryParse(v, out float r) ? r : Parameters.PredatorStamina; break;
                case 18: Parameters.PredatorVisionDistance = float.TryParse(v, out float s) ? s : Parameters.PredatorVisionDistance; break;
                case 19: Parameters.PredatorMetabolism = float.TryParse(v, out float t) ? t : Parameters.PredatorMetabolism; break;
            }

            _fieldValues[index] = index switch
            {
                0 => Parameters.InitialPreyCount.ToString(),
                1 => Parameters.InitialPredatorCount.ToString(),
                2 => Parameters.PreyStartingEnergy.ToString(),
                3 => Parameters.PredatorStartingEnergy.ToString(),
                4 => Parameters.MutationRate.ToString(),
                5 => Parameters.InitialFoodCount.ToString(),
                6 => Parameters.FoodRegenerationRate.ToString(),
                7 => Parameters.FoodNutritionValue.ToString(),
                8 => Parameters.MaxFoodCount.ToString(),
                9 => Parameters.InitialTraitVariance.ToString(),
                10 => Parameters.PreySpeed.ToString(),
                11 => Parameters.PreySize.ToString(),
                12 => Parameters.PreyStamina.ToString(),
                13 => Parameters.PreyVisionDistance.ToString(),
                14 => Parameters.PreyMetabolism.ToString(),
                15 => Parameters.PredatorSpeed.ToString(),
                16 => Parameters.PredatorSize.ToString(),
                17 => Parameters.PredatorStamina.ToString(),
                18 => Parameters.PredatorVisionDistance.ToString(),
                19 => Parameters.PredatorMetabolism.ToString(),
                _ => _fieldValues[index]
            };
        }

        public void Draw()
        {
            int screenW = _graphics.Viewport.Width;
            int screenH = _graphics.Viewport.Height;
            int cx = screenW / 2;
            Point mousePos = Mouse.GetState().Position;

            int totalWidth = LabelWidth + BoxWidth + ColumnGap;
            int leftX = cx - totalWidth;
            int rightX = cx + ColumnGap;
            int scrollY = (int)_scrollOffset;

            // ── Fixed title ──
            _spriteBatch.Begin();
            string title = "Evolution Simulator";
            var titleSize = _titleFont.MeasureString(title);
            _spriteBatch.DrawString(_titleFont, title,
                new Vector2(cx - titleSize.X / 2, 30), Color.ForestGreen);
            _spriteBatch.End();

            // ── Scrollable content (scissor-clipped) ──
            var prevScissor = _graphics.ScissorRectangle;
            _graphics.ScissorRectangle = new Rectangle(
                0, ScrollAreaTop, screenW, screenH - HintAreaHeight - ScrollAreaTop);
            _spriteBatch.Begin(rasterizerState: _scissorRasterizer);

            int currentY = StartY - scrollY;
            foreach (var section in _sections)
            {
                // Section header
                _spriteBatch.DrawString(_menuFont, section.Header,
                    new Vector2(leftX, currentY), Color.ForestGreen);
                currentY += SectionHeaderHeight;

                for (int j = 0; j < section.Count; j++)
                {
                    int i = section.Start + j;
                    int col = j % Columns;
                    int row = j / Columns;
                    int x = col == 0 ? leftX : rightX;
                    int y = currentY + row * RowHeight;
                    int boxX = x + LabelWidth;

                    // Store rect in screen-space for hit testing
                    _fieldBoxRects[i] = new Rectangle(boxX, y, BoxWidth, BoxHeight);

                    bool focused = _focusedField == i;
                    bool hovered = _fieldBoxRects[i].Contains(mousePos);
                    Color labelColor = focused ? Color.ForestGreen : Color.Black;
                    Color boxColor = focused ? Color.LightYellow : hovered ? Color.Ivory : Color.WhiteSmoke;
                    Color borderColor = focused ? Color.ForestGreen : hovered ? Color.DarkGray : Color.Gray;

                    _spriteBatch.DrawString(_labelFont, _fields[i].Label,
                        new Vector2(x, y + 8), labelColor);

                    DrawRect(boxX, y, BoxWidth, BoxHeight, boxColor);
                    DrawRectBorder(boxX, y, BoxWidth, BoxHeight, borderColor);

                    string display = _fieldValues[i] + (focused ? "|" : "");
                    _spriteBatch.DrawString(_labelFont, display,
                        new Vector2(boxX + BoxPadding, y + 8), Color.Black);
                }

                int sectionRows = (section.Count + Columns - 1) / Columns;
                currentY += sectionRows * RowHeight;
            }

            // Checkboxes
            int checkboxStartY = currentY + RowHeight;
            for (int i = 0; i < _checkboxStates.Length; i++)
            {
                int checkboxX = leftX;
                int checkboxY = checkboxStartY + i * 40;
                bool hovered = _checkboxRects[i].Contains(mousePos);
                bool isChecked = _checkboxStates[i];

                // Store rect for hit testing
                _checkboxRects[i] = new Rectangle(checkboxX, checkboxY, CheckboxSize, CheckboxSize);

                // Draw checkbox background — different color when checked
                Color boxBg = isChecked 
                    ? Color.ForestGreen 
                    : (hovered ? Color.LightGray : Color.White);
                DrawRect(checkboxX, checkboxY, CheckboxSize, CheckboxSize, boxBg);
                
                // Draw border — thicker/more prominent when checked
                Color borderColor = isChecked ? Color.DarkGreen : (hovered ? Color.DarkGray : Color.Gray);
                DrawRectBorder(checkboxX, checkboxY, CheckboxSize, CheckboxSize, borderColor);

           

                // Draw label
                _spriteBatch.DrawString(_labelFont, _checkboxLabels[i],
                    new Vector2(checkboxX + CheckboxSize + CheckboxGap, checkboxY + 2), Color.Black);
            }

            // Buttons
            string[] buttons = { "Start Simulation", "Quit" };
            int buttonY = checkboxStartY + 100;

            for (int i = 0; i < buttons.Length; i++)
            {
                bool hovered = _buttonRects[i] != Rectangle.Empty && _buttonRects[i].Contains(mousePos);
                bool sel = _focusedField < 0 && _selectedButton == i;
                Color btnColor = (sel || hovered) ? Color.ForestGreen : Color.DarkGray;

                var size = _menuFont.MeasureString(buttons[i]);
                int btnW = (int)size.X + 40;
                int btnX = cx - btnW / 2;
                int btnYi = buttonY + i * 60;

                _buttonRects[i] = new Rectangle(btnX, btnYi, btnW, 44);

                DrawRect(btnX, btnYi, btnW, 44, btnColor);
                _spriteBatch.DrawString(_menuFont, buttons[i],
                    new Vector2(btnX + 20, btnYi + 8), Color.White);
            }

            // Track total content height (logical, without scroll offset)
            _contentHeight = (currentY + scrollY) - StartY
                           + RowHeight + 100 + (buttons.Length - 1) * 60 + 44;

            _spriteBatch.End();
            _graphics.ScissorRectangle = prevScissor;

            // ── Fixed hint (bottom-right corner) ──
            _spriteBatch.Begin();
            string hint = _focusedField >= 0
                ? "Type value · Enter/Tab next · Esc back · Scroll ↕"
                : "Click a field or checkbox to interact · Scroll ↕ · Enter to select";
            var hintSize = _hintFont.MeasureString(hint);
            _spriteBatch.DrawString(_hintFont, hint,
                new Vector2(screenW - hintSize.X - 20, screenH - 30), Color.Gray);
            _spriteBatch.End();
        }

        // ── Scroll helpers ──

        private void ClampScroll()
        {
            int screenH = _graphics.Viewport.Height;
            int visibleHeight = screenH - HintAreaHeight - ScrollAreaTop;
            float maxScroll = Math.Max(0f, _contentHeight - visibleHeight);
            _scrollOffset = MathHelper.Clamp(_scrollOffset, 0f, maxScroll);
        }

        /// <summary>
        /// Auto-scroll so the given field index is within the visible area.
        /// </summary>
        private void EnsureFieldVisible(int fieldIndex)
        {
            if (fieldIndex < 0 || fieldIndex >= _fields.Length)
                return;

            // Walk the layout to find the logical Y of the field
            int logicalY = StartY;
            foreach (var section in _sections)
            {
                logicalY += SectionHeaderHeight;
                int local = fieldIndex - section.Start;
                if (local >= 0 && local < section.Count)
                {
                    logicalY += (local / Columns) * RowHeight;
                    break;
                }
                int rows = (section.Count + Columns - 1) / Columns;
                logicalY += rows * RowHeight;
            }

            int screenH = _graphics.Viewport.Height;
            int scrollAreaBottom = screenH - HintAreaHeight;

            int screenY = logicalY - (int)_scrollOffset;
            if (screenY < ScrollAreaTop)
                _scrollOffset = logicalY - ScrollAreaTop;
            else if (screenY + BoxHeight > scrollAreaBottom)
                _scrollOffset = logicalY + BoxHeight - scrollAreaBottom;

            ClampScroll();
        }

        private bool IsPressed(KeyboardState kb, KeyboardState prev, Keys key)
            => kb.IsKeyDown(key) && prev.IsKeyUp(key);

        private void DrawRect(int x, int y, int w, int h, Color color)
        {
            var tex = new Texture2D(_graphics, 1, 1);
            tex.SetData(new[] { color });
            _spriteBatch.Draw(tex, new Rectangle(x, y, w, h), Color.White);
        }

        private void DrawRectBorder(int x, int y, int w, int h, Color color)
        {
            DrawRect(x, y, w, 2, color);
            DrawRect(x, y + h - 2, w, 2, color);
            DrawRect(x, y, 2, h, color);
            DrawRect(x + w - 2, y, 2, h, color);
        }
    }
}