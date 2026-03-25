using EvolutionSimulator.Core.Models;

namespace EvolutionSimulator.Core.Rendering
{
    public enum RenderEntityKind
    {
        Prey,
        Predator
    }

    public sealed record SimulationHudSnapshot(
        int CurrentStep,
        float ElapsedTime,
        bool IsRunning,
        int LivingPreyCount,
        int LivingPredatorCount,
        int AvailableFoodCount);

    public sealed record OrganismRenderSnapshot(
        Guid Id,
        RenderEntityKind Kind,
        float X,
        float Y,
        float DirectionX,
        float DirectionY,
        float Radius,
        float VisionRadius,
        float VisionFieldOfViewDegrees,
        float Energy,
        int Age,
        bool IsAlive);

    public sealed record FoodRenderSnapshot(
        Guid Id,
        float X,
        float Y,
        float NutritionValue,
        bool IsConsumed);

    public sealed record TerrainRenderSnapshot(
        Guid Id,
        TerrainType TerrainType,
        float CenterX,
        float CenterY,
        float Radius);

    public sealed record SimulationRenderFrame(
        float WorldWidth,
        float WorldHeight,
        SimulationRenderPalette Palette,
        SimulationHudSnapshot Hud,
        IReadOnlyList<OrganismRenderSnapshot> Organisms,
        IReadOnlyList<FoodRenderSnapshot> FoodSources,
        IReadOnlyList<TerrainRenderSnapshot> TerrainRegions);
}
