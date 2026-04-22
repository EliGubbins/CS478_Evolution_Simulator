using EvolutionSimulator.Core.Models;
using EvolutionSimulator.Core.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EvolutionSimulator.MonoGameHost.Graphics
{
    public sealed class MonoGameRenderFrameRenderer : IRenderFrameRenderer, IDisposable
    {
        private readonly GraphicsDevice graphicsDevice;

        private SpriteBatch? spriteBatch;
        private Texture2D? pixelTexture;
        private Texture2D? filledCircleTexture;
        private Texture2D? ringTexture;
        private SimulationRenderPalette palette = SimulationRenderPalette.Default;
        
        // Vision cone rendering control
        public bool DrawVisionCones { get; set; } = false;

        public MonoGameRenderFrameRenderer(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void Initialize(WorldRenderViewport viewport, SimulationRenderPalette palette)
        {
            spriteBatch = new SpriteBatch(graphicsDevice);
            pixelTexture = CreateSolidTexture(1, 1);
            filledCircleTexture = CreateCircleTexture(96, filled: true);
            ringTexture = CreateCircleTexture(96, filled: false);
            this.palette = palette;
        }

        public void Render(SimulationRenderFrame frame, WorldRenderViewport viewport)
        {
            if (spriteBatch is null || pixelTexture is null || filledCircleTexture is null || ringTexture is null)
                throw new InvalidOperationException("Renderer must be initialized before rendering.");

            palette = frame.Palette;
            graphicsDevice.Clear(ToXnaColor(palette.Background));

            spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.LinearClamp);

            DrawTerrain(frame, viewport);
            DrawWorldBorder(frame, viewport);
            DrawFood(frame, viewport);
            DrawOrganisms(frame, viewport);

            spriteBatch.End();
        }

        public void Resize(WorldRenderViewport viewport)
        {
        }

        public void Dispose()
        {
            spriteBatch?.Dispose();
            pixelTexture?.Dispose();
            filledCircleTexture?.Dispose();
            ringTexture?.Dispose();
        }

        private void DrawTerrain(SimulationRenderFrame frame, WorldRenderViewport viewport)
        {
            foreach (TerrainRenderSnapshot terrain in frame.TerrainRegions)
            {
                RenderColor terrainColor = terrain.TerrainType switch
                {
                    TerrainType.Forest => palette.ForestTerrain,
                    TerrainType.Water => palette.WaterTerrain,
                    TerrainType.Rocky => palette.RockyTerrain,
                    _ => palette.Background
                };

                DrawCircle(
                    terrain.CenterX,
                    terrain.CenterY,
                    terrain.Radius,
                    terrainColor,
                    viewport,
                    filled: true);
            }
        }

        private void DrawWorldBorder(SimulationRenderFrame frame, WorldRenderViewport viewport)
        {
            if (pixelTexture is null)
                return;

            float left = viewport.ToScreenX(0f);
            float top = viewport.ToScreenY(0f);
            float width = frame.WorldWidth * viewport.Scale;
            float height = frame.WorldHeight * viewport.Scale;
            Color color = ToXnaColor(palette.WorldBorder);
            int thickness = 2;

            spriteBatch!.Draw(pixelTexture, new Rectangle((int)left, (int)top, (int)width, thickness), color);
            spriteBatch.Draw(pixelTexture, new Rectangle((int)left, (int)(top + height - thickness), (int)width, thickness), color);
            spriteBatch.Draw(pixelTexture, new Rectangle((int)left, (int)top, thickness, (int)height), color);
            spriteBatch.Draw(pixelTexture, new Rectangle((int)(left + width - thickness), (int)top, thickness, (int)height), color);
        }

        private void DrawFood(SimulationRenderFrame frame, WorldRenderViewport viewport)
        {
            foreach (FoodRenderSnapshot food in frame.FoodSources)
            {
                if (food.IsConsumed)
                    continue;

                float radius = MathF.Max(0.8f, food.NutritionValue * 0.08f);
                DrawCircle(food.X, food.Y, radius, palette.Food, viewport, filled: true);
            }
        }

        private void DrawOrganisms(SimulationRenderFrame frame, WorldRenderViewport viewport)
        {
            foreach (OrganismRenderSnapshot organism in frame.Organisms)
            {
                if (!organism.IsAlive)
                    continue;

                RenderColor bodyColor = organism.Kind == RenderEntityKind.Predator
                    ? palette.Predator
                    : palette.Prey;

                // Only draw vision cone if enabled
                if (DrawVisionCones)
                {
                    RenderColor visionColor = organism.Kind == RenderEntityKind.Predator
                        ? new RenderColor(palette.Predator.R, palette.Predator.G, palette.Predator.B, 55)
                        : new RenderColor(palette.Prey.R, palette.Prey.G, palette.Prey.B, 55);

                    DrawVisionCone(organism, viewport, visionColor);
                }

                DrawCircle(organism.X, organism.Y, organism.Radius, bodyColor, viewport, filled: true);
                DrawDirectionIndicator(organism, viewport, bodyColor);
            }
        }

        private void DrawDirectionIndicator(
            OrganismRenderSnapshot organism,
            WorldRenderViewport viewport,
            RenderColor color)
        {
            if (pixelTexture is null)
                return;

            float startX = viewport.ToScreenX(organism.X);
            float startY = viewport.ToScreenY(organism.Y);
            float directionLength = MathF.Max(10f, viewport.ToScreenSize(organism.Radius * 1.8f));
            float endX = startX + (organism.DirectionX * directionLength);
            float endY = startY + (organism.DirectionY * directionLength);

            DrawLine(startX, startY, endX, endY, ToXnaColor(color), 2f);
        }

        private void DrawVisionCone(
            OrganismRenderSnapshot organism,
            WorldRenderViewport viewport,
            RenderColor color)
        {
            if (pixelTexture is null)
                return;

            float startX = viewport.ToScreenX(organism.X);
            float startY = viewport.ToScreenY(organism.Y);
            float radius = viewport.ToScreenSize(organism.VisionDistance);
            (float facingX, float facingY) = GetFacingVector(organism);
            float facingAngle = MathF.Atan2(facingY, facingX);
            float halfAngleRadians = MathHelper.ToRadians(organism.VisionFieldOfViewDegrees * 0.5f);
            int segments = Math.Max(10, (int)(organism.VisionFieldOfViewDegrees / 8f));
            Color lineColor = ToXnaColor(color);

            Vector2 previousPoint = new(
                startX + MathF.Cos(facingAngle - halfAngleRadians) * radius,
                startY + MathF.Sin(facingAngle - halfAngleRadians) * radius);

            DrawLine(startX, startY, previousPoint.X, previousPoint.Y, lineColor, 1.5f);

            for (int i = 1; i <= segments; i++)
            {
                float progress = i / (float)segments;
                float angle = facingAngle - halfAngleRadians + (progress * halfAngleRadians * 2f);
                Vector2 nextPoint = new(
                    startX + MathF.Cos(angle) * radius,
                    startY + MathF.Sin(angle) * radius);

                DrawLine(previousPoint.X, previousPoint.Y, nextPoint.X, nextPoint.Y, lineColor, 1.5f);
                previousPoint = nextPoint;
            }

            DrawLine(startX, startY, previousPoint.X, previousPoint.Y, lineColor, 1.5f);
        }

        private void DrawLine(float startX, float startY, float endX, float endY, Color color, float thickness)
        {
            if (pixelTexture is null)
                return;

            float deltaX = endX - startX;
            float deltaY = endY - startY;
            float angle = MathF.Atan2(deltaY, deltaX);
            float length = MathF.Sqrt((deltaX * deltaX) + (deltaY * deltaY));

            spriteBatch!.Draw(
                pixelTexture,
                new Vector2(startX, startY),
                null,
                color,
                angle,
                Vector2.Zero,
                new Vector2(length, thickness),
                SpriteEffects.None,
                0f);
        }

        private void DrawCircle(
            float worldX,
            float worldY,
            float worldRadius,
            RenderColor color,
            WorldRenderViewport viewport,
            bool filled)
        {
            Texture2D? texture = filled ? filledCircleTexture : ringTexture;
            if (texture is null)
                return;

            float radius = viewport.ToScreenSize(worldRadius);
            Rectangle destination = new(
                (int)(viewport.ToScreenX(worldX) - radius),
                (int)(viewport.ToScreenY(worldY) - radius),
                Math.Max(1, (int)(radius * 2f)),
                Math.Max(1, (int)(radius * 2f)));

            spriteBatch!.Draw(texture, destination, ToXnaColor(color));
        }

        private Texture2D CreateSolidTexture(int width, int height)
        {
            Texture2D texture = new(graphicsDevice, width, height);
            Color[] colors = Enumerable.Repeat(Color.White, width * height).ToArray();
            texture.SetData(colors);
            return texture;
        }

        private Texture2D CreateCircleTexture(int diameter, bool filled)
        {
            Texture2D texture = new(graphicsDevice, diameter, diameter);
            Color[] colors = new Color[diameter * diameter];
            float radius = diameter / 2f;
            float innerRadius = MathF.Max(0f, radius - 3f);
            Vector2 center = new(radius, radius);

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    Vector2 point = new(x + 0.5f, y + 0.5f);
                    float distance = Vector2.Distance(point, center);

                    bool shouldFill = filled
                        ? distance <= radius
                        : distance <= radius && distance >= innerRadius;

                    colors[(y * diameter) + x] = shouldFill ? Color.White : Color.Transparent;
                }
            }

            texture.SetData(colors);
            return texture;
        }

        private static Color ToXnaColor(RenderColor color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }

        private static (float X, float Y) GetFacingVector(OrganismRenderSnapshot organism)
        {
            float magnitudeSquared = (organism.DirectionX * organism.DirectionX) + (organism.DirectionY * organism.DirectionY);

            if (magnitudeSquared < 0.001f)
                return (0f, -1f);

            return (organism.DirectionX, organism.DirectionY);
        }
    }
}
