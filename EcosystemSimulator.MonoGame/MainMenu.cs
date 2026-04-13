using EcosystemSimulator.Models;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    }

    public class MainMenu
    {
        // Fonts
        private FontSystem _fontSystem;
        private SpriteFontBase _titleFont;
        private SpriteFontBase _menuFont;
        private SpriteFontBase _hintFont;
        private SpriteFontBase _labelFont;

        // Graphics
        private readonly GraphicsDevice _graphics;
        private readonly SpriteBatch _spriteBatch;

        // Parameters
        public SimulationParameters Parameters { get; private set; } = new();

        private readonly (string Label, bool IsFloat)[] _fields = {
            ("Initial Prey Count",       false),
            ("Initial Predator Count",   false),
            ("Prey Starting Energy",     true),
            ("Predator Starting Energy", true),
            ("Mutation Rate",            true),
            ("Initial Food Count",       false),
            ("Food Regeneration Rate",   true),
            ("Food Nutrition Value",     true),
            ("Max Food Count",           false),
        };

        private string[] _fieldValues;
        private int _focusedField = -1;
        private int _selectedButton = 0;

        // Layout — computed once in Draw, stored for hit testing
        private Rectangle[] _fieldBoxRects;
        private Rectangle[] _buttonRects;

        // Mouse state
        private MouseState _prevMouse;

        public MainMenu(GraphicsDevice graphics, string fontPath, string configPath)
        {
            _graphics = graphics;
            _spriteBatch = new SpriteBatch(graphics);

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
            };

            _fieldBoxRects = new Rectangle[_fields.Length];
            _buttonRects = new Rectangle[2];
        }

        public MenuAction Update(KeyboardState kb, KeyboardState prevKb)
        {
            MouseState mouse = Mouse.GetState();
            Point mousePos = mouse.Position;
            bool clicked = mouse.LeftButton == ButtonState.Released
                            && _prevMouse.LeftButton == ButtonState.Pressed;

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
                _ => _fieldValues[index]
            };
        }

        public void Draw()
        {
            int cx = _graphics.Viewport.Width / 2;
            int screenH = _graphics.Viewport.Height;
            Point mousePos = Mouse.GetState().Position;

            const int startY = 100;
            const int rowHeight = 48;
            const int labelWidth = 260;
            const int boxWidth = 160;
            const int boxHeight = 34;
            const int boxPadding = 8;
            const int columnGap = 20;
            const int columns = 2;
            int totalWidth = labelWidth + boxWidth + columnGap;
            int leftX = cx - totalWidth;
            int rightX = cx + columnGap;

            _spriteBatch.Begin();

            // Title
            string title = "Evolution Simulator";
            var titleSize = _titleFont.MeasureString(title);
            _spriteBatch.DrawString(_titleFont, title,
                new Vector2(cx - titleSize.X / 2, 30), Color.ForestGreen);

            // Parameter fields
            for (int i = 0; i < _fields.Length; i++)
            {
                int col = i % columns;
                int row = i / columns;
                int x = col == 0 ? leftX : rightX;
                int y = startY + row * rowHeight;
                int boxX = x + labelWidth;

                // Store rect for hit testing
                _fieldBoxRects[i] = new Rectangle(boxX, y, boxWidth, boxHeight);

                bool focused = _focusedField == i;
                bool hovered = _fieldBoxRects[i].Contains(mousePos);
                Color labelColor = focused ? Color.ForestGreen : Color.Black;
                Color boxColor = focused ? Color.LightYellow : hovered ? Color.Ivory : Color.WhiteSmoke;
                Color borderColor = focused ? Color.ForestGreen : hovered ? Color.DarkGray : Color.Gray;

                _spriteBatch.DrawString(_labelFont, _fields[i].Label,
                    new Vector2(x, y + 8), labelColor);

                DrawRect(boxX, y, boxWidth, boxHeight, boxColor);
                DrawRectBorder(boxX, y, boxWidth, boxHeight, borderColor);

                string display = _fieldValues[i] + (focused ? "|" : "");
                _spriteBatch.DrawString(_labelFont, display,
                    new Vector2(boxX + boxPadding, y + 8), Color.Black);
            }

            // Buttons
            string[] buttons = { "Start Simulation", "Quit" };
            int buttonY = startY + (((_fields.Length + 1) / columns) + 1) * rowHeight;

            for (int i = 0; i < buttons.Length; i++)
            {
                bool hovered = _buttonRects[i] != Rectangle.Empty && _buttonRects[i].Contains(mousePos);
                bool sel = _focusedField < 0 && _selectedButton == i;
                Color btnColor = (sel || hovered) ? Color.ForestGreen : Color.DarkGray;

                var size = _menuFont.MeasureString(buttons[i]);
                int btnW = (int)size.X + 40;
                int btnX = cx - btnW / 2;
                int btnYi = buttonY + i * 60;

                // Store rect for hit testing
                _buttonRects[i] = new Rectangle(btnX, btnYi, btnW, 44);

                DrawRect(btnX, btnYi, btnW, 44, btnColor);
                _spriteBatch.DrawString(_menuFont, buttons[i],
                    new Vector2(btnX + 20, btnYi + 8), Color.White);
            }

            // Hint
            string hint = _focusedField >= 0
                ? "Type value   Enter/Tab next field   Esc back"
                : "Click a field to edit   Click a button or press Enter";
            var hintSize = _hintFont.MeasureString(hint);
            _spriteBatch.DrawString(_hintFont, hint,
                new Vector2(cx - hintSize.X / 2, screenH - 40), Color.Gray);

            _spriteBatch.End();
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