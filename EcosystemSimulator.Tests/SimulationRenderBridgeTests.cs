using EvolutionSimulator.Core;
using EvolutionSimulator.Core.Models;
using EvolutionSimulator.Core.Rendering;
using Xunit;

namespace EcosystemSimulator.Tests;

public sealed class SimulationRenderBridgeTests
{
    [Fact]
    public void CreateFrameCapturesEngineWorldState()
    {
        SimulationEngine engine = new(120f, 80f, initialPreyCount: 2, initialPredatorCount: 1);
        engine.EnvironmentManager.FoodSources.Add(new Food(10f, 20f, 5f));
        engine.EnvironmentManager.TerrainRegions.Add(new TerrainRegion(TerrainType.Forest, 30f, 40f, 12f));

        SimulationRenderBridge bridge = new(engine);

        SimulationRenderFrame frame = bridge.CreateFrame();

        Assert.Equal(120f, frame.WorldWidth);
        Assert.Equal(80f, frame.WorldHeight);
        Assert.Equal(3, frame.Organisms.Count);
        Assert.Single(frame.FoodSources);
        Assert.Single(frame.TerrainRegions);
        Assert.Equal(2, frame.Hud.LivingPreyCount);
        Assert.Equal(1, frame.Hud.LivingPredatorCount);
        Assert.Equal(1, frame.Hud.AvailableFoodCount);
    }

    [Fact]
    public void CreateViewportFitsWorldInsideRequestedPixelBounds()
    {
        SimulationEngine engine = new(200f, 100f, initialPreyCount: 0, initialPredatorCount: 0);
        SimulationRenderBridge bridge = new(engine);

        WorldRenderViewport viewport = bridge.CreateViewport(1000, 800, padding: 50f);

        Assert.Equal(4.5f, viewport.Scale, 3);
        Assert.Equal(50f, viewport.OffsetX, 3);
        Assert.Equal(175f, viewport.OffsetY, 3);
        Assert.Equal(950f, viewport.ToScreenX(200f), 3);
        Assert.Equal(625f, viewport.ToScreenY(100f), 3);
    }
}
