namespace EvolutionSimulator.Core.Rendering
{
    public sealed record SimulationRenderPalette(
        RenderColor Background,
        RenderColor WorldBorder,
        RenderColor Prey,
        RenderColor Predator,
        RenderColor Food,
        RenderColor ForestTerrain,
        RenderColor WaterTerrain,
        RenderColor RockyTerrain)
    {
        public static SimulationRenderPalette Default { get; } = new(
            Background: RenderColor.Sky,
            WorldBorder: RenderColor.Slate,
            Prey: RenderColor.ForestGreen,
            Predator: RenderColor.SunsetOrange,
            Food: RenderColor.Wheat,
            ForestTerrain: new RenderColor(109, 156, 112, 140),
            WaterTerrain: new RenderColor(82, 143, 204, 140),
            RockyTerrain: new RenderColor(125, 125, 125, 140));
    }
}
