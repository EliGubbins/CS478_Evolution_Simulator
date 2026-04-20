namespace EvolutionSimulator.Core.Rendering
{
    public readonly record struct RenderColor(byte R, byte G, byte B, byte A = 255)
    {
        public static readonly RenderColor ForestGreen = new(74, 140, 84);
        public static readonly RenderColor PastelGrass = new(196, 226, 184);
        public static readonly RenderColor SunsetOrange = new(217, 91, 67);
        public static readonly RenderColor Wheat = new(232, 211, 162);
        public static readonly RenderColor Slate = new(70, 78, 92);
        public static readonly RenderColor Sky = new(167, 215, 244);
        public static readonly RenderColor Water = new(82, 143, 204);
        public static readonly RenderColor Stone = new(125, 125, 125);
    }
}
