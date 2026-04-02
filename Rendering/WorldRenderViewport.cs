namespace EvolutionSimulator.Core.Rendering
{
    public readonly record struct WorldRenderViewport(
        int PixelWidth,
        int PixelHeight,
        float Padding,
        float WorldWidth,
        float WorldHeight,
        float Scale,
        float OffsetX,
        float OffsetY)
    {
        public static WorldRenderViewport Create(
            float worldWidth,
            float worldHeight,
            int pixelWidth,
            int pixelHeight,
            float padding = 24f)
        {
            float safeWorldWidth = MathF.Max(1f, worldWidth);
            float safeWorldHeight = MathF.Max(1f, worldHeight);
            float safePixelWidth = MathF.Max(1, pixelWidth);
            float safePixelHeight = MathF.Max(1, pixelHeight);
            float safePadding = MathF.Max(0f, padding);

            float drawableWidth = MathF.Max(1f, safePixelWidth - (safePadding * 2f));
            float drawableHeight = MathF.Max(1f, safePixelHeight - (safePadding * 2f));
            float scale = MathF.Min(drawableWidth / safeWorldWidth, drawableHeight / safeWorldHeight);

            float contentWidth = safeWorldWidth * scale;
            float contentHeight = safeWorldHeight * scale;
            float offsetX = (safePixelWidth - contentWidth) * 0.5f;
            float offsetY = (safePixelHeight - contentHeight) * 0.5f;

            return new WorldRenderViewport(
                pixelWidth,
                pixelHeight,
                safePadding,
                safeWorldWidth,
                safeWorldHeight,
                scale,
                offsetX,
                offsetY);
        }

        public float ToScreenX(float worldX) => OffsetX + (worldX * Scale);

        public float ToScreenY(float worldY) => OffsetY + (worldY * Scale);

        public float ToScreenSize(float worldSize) => MathF.Max(1f, worldSize * Scale);
    }
}
