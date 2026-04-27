using EvolutionSimulator.Core.Entities;
using EvolutionSimulator.Core.Models;

namespace EvolutionSimulator.Core.Rendering
{
    public sealed class SimulationRenderBridge
    {
        private readonly SimulationEngine engine;

        public SimulationRenderBridge(SimulationEngine engine, SimulationRenderPalette? palette = null)
        {
            this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
            Palette = palette ?? SimulationRenderPalette.Default;
        }

        public SimulationRenderPalette Palette { get; }

        public SimulationRenderFrame CreateFrame()
        {
            return new SimulationRenderFrame(
                engine.EnvironmentManager.Width,
                engine.EnvironmentManager.Height,
                Palette,
                BuildHud(),
                BuildOrganismLayer(),
                BuildFoodLayer(),
                BuildTerrainLayer());
        }

        public SimulationHudSnapshot BuildHud()
        {
            return new SimulationHudSnapshot(
                engine.CurrentStep,
                engine.ElapsedTime,
                engine.IsRunning,
                engine.PopulationManager.GetLivingPreyCount(),
                engine.PopulationManager.GetLivingPredatorCount(),
                engine.EnvironmentManager.GetAvailableFoodCount());
        }

        public IReadOnlyList<OrganismRenderSnapshot> BuildOrganismLayer()
        {
            List<OrganismRenderSnapshot> organisms = [];

            organisms.AddRange(
                engine.PopulationManager.PreyPopulation.Select(prey => BuildOrganismSnapshot(prey, RenderEntityKind.Prey)));

            organisms.AddRange(
                engine.PopulationManager.PredatorPopulation.Select(predator => BuildOrganismSnapshot(predator, RenderEntityKind.Predator)));

            return organisms;
        }

        public IReadOnlyList<FoodRenderSnapshot> BuildFoodLayer()
        {
            return engine.EnvironmentManager.FoodSources
                .Select(food => new FoodRenderSnapshot(
                    food.Id,
                    food.X,
                    food.Y,
                    food.NutritionValue,
                    food.IsConsumed))
                .ToArray();
        }

        public IReadOnlyList<TerrainRenderSnapshot> BuildTerrainLayer()
        {
            return engine.EnvironmentManager.TerrainRegions
                .Select(region => new TerrainRenderSnapshot(
                    region.Id,
                    region.TerrainType,
                    region.CenterX,
                    region.CenterY,
                    region.Radius))
                .ToArray();
        }

        public WorldRenderViewport CreateViewport(int pixelWidth, int pixelHeight, float padding = 24f, float topInset = 0f)
        {
            return WorldRenderViewport.Create(
                engine.EnvironmentManager.Width,
                engine.EnvironmentManager.Height,
                pixelWidth,
                pixelHeight,
                padding,
                topInset);
        }

        public RenderColor GetTerrainColor(TerrainType terrainType)
        {
            return terrainType switch
            {
                TerrainType.Forest => Palette.ForestTerrain,
                TerrainType.Water => Palette.WaterTerrain,
                TerrainType.Rocky => Palette.RockyTerrain,
                _ => Palette.Background
            };
        }

        private static OrganismRenderSnapshot BuildOrganismSnapshot(Organism organism, RenderEntityKind kind)
        {
            return new OrganismRenderSnapshot(
                organism.Id,
                kind,
                organism.X,
                organism.Y,
                organism.DirectionX,
                organism.DirectionY,
                organism.Traits.Size,
                organism.Traits.VisionDistance,
                organism.VisionFieldOfViewDegrees,
                organism.Energy,
                organism.Age,
                organism.IsAlive);
        }
    }
}
