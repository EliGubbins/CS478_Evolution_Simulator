using EvolutionSimulator.Core;
using EvolutionSimulator.Core.Models;
using Xunit;

namespace EcosystemSimulator.Tests;

public sealed class SimulationEngineTests
{
    [Fact]
    public void InitializeSeedsPopulationAndResetsState()
    {
        SimulationEngine engine = new(100f, 100f);
        engine.EnvironmentManager.FoodSources.Add(new Food(1f, 1f, 5f));
        engine.Start();
        engine.Step(1f);

        engine.Initialize(3, 2, 45f, 90f, 0.2f);

        Assert.Equal(3, engine.PopulationManager.PreyPopulation.Count);
        Assert.Equal(2, engine.PopulationManager.PredatorPopulation.Count);
        Assert.Equal(45f, engine.PreyStartingEnergy);
        Assert.Equal(90f, engine.PredatorStartingEnergy);
        Assert.Equal(0.2f, engine.MutationRate);
        Assert.Equal(0, engine.CurrentStep);
        Assert.Equal(0f, engine.ElapsedTime);
        Assert.False(engine.IsRunning);
        Assert.Empty(engine.EnvironmentManager.FoodSources);
    }

    [Fact]
    public void StepDoesNothingWhileStopped()
    {
        SimulationEngine engine = new(100f, 100f, initialPreyCount: 2, initialPredatorCount: 1);
        int initialPreyCount = engine.PopulationManager.PreyPopulation.Count;
        int initialPredatorCount = engine.PopulationManager.PredatorPopulation.Count;

        engine.Step(1f);

        Assert.Equal(0, engine.CurrentStep);
        Assert.Equal(0f, engine.ElapsedTime);
        Assert.Equal(initialPreyCount, engine.PopulationManager.PreyPopulation.Count);
        Assert.Equal(initialPredatorCount, engine.PopulationManager.PredatorPopulation.Count);
    }

    [Fact]
    public void StartAndStopToggleRunningState()
    {
        SimulationEngine engine = new(100f, 100f);

        engine.Start();
        Assert.True(engine.IsRunning);

        engine.Stop();
        Assert.False(engine.IsRunning);
    }

    [Fact]
    public void StepIncrementsCountersWhileRunning()
    {
        SimulationEngine engine = new(100f, 100f, initialPreyCount: 0, initialPredatorCount: 0);
        engine.Start();

        engine.Step(0.5f);

        Assert.Equal(1, engine.CurrentStep);
        Assert.Equal(0.5f, engine.ElapsedTime);
    }

    [Fact]
    public void ResetRestoresInitialConfiguration()
    {
        SimulationEngine engine = new(
            100f,
            100f,
            initialPreyCount: 4,
            initialPredatorCount: 1,
            preyStartingEnergy: 55f,
            predatorStartingEnergy: 95f,
            mutationRate: 0.3f);

        engine.Start();
        engine.Step(1f);
        engine.Initialize(2, 3, 40f, 80f, 0.1f);

        engine.Reset();

        Assert.Equal(2, engine.PopulationManager.PreyPopulation.Count);
        Assert.Equal(3, engine.PopulationManager.PredatorPopulation.Count);
        Assert.Equal(40f, engine.PreyStartingEnergy);
        Assert.Equal(80f, engine.PredatorStartingEnergy);
        Assert.Equal(0.1f, engine.MutationRate);
        Assert.Equal(0, engine.CurrentStep);
        Assert.Equal(0f, engine.ElapsedTime);
        Assert.False(engine.IsRunning);
    }
}
