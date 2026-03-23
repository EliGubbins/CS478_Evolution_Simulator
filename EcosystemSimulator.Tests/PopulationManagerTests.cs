using EvolutionSimulator.Core;
using EvolutionSimulator.Core.Entities;
using EvolutionSimulator.Core.Environment;
using Xunit;

namespace EcosystemSimulator.Tests;

public sealed class PopulationManagerTests
{
    [Fact]
    public void SeedInitialPopulationClearsAndReseedsPopulations()
    {
        PopulationManager populationManager = new();
        EnvironmentManager environmentManager = new(100f, 100f);

        populationManager.SeedInitialPopulation(4, 2, environmentManager, 50f, 70f);
        populationManager.SeedInitialPopulation(3, 1, environmentManager, 45f, 80f);

        Assert.Equal(3, populationManager.PreyPopulation.Count);
        Assert.Single(populationManager.PredatorPopulation);
        Assert.All(populationManager.PreyPopulation, prey => Assert.Equal(45f, prey.Energy));
        Assert.All(populationManager.PredatorPopulation, predator => Assert.Equal(80f, predator.Energy));
    }

    [Fact]
    public void SeedInitialPopulationKeepsTraitsWithinBaselineRange()
    {
        PopulationManager populationManager = new();
        EnvironmentManager environmentManager = new(100f, 100f);

        populationManager.SeedInitialPopulation(200, 200, environmentManager, 50f, 80f);

        foreach (Prey prey in populationManager.PreyPopulation)
        {
            Assert.InRange(prey.Traits.Speed, 4f, 8f);
            Assert.InRange(prey.Traits.Size, 0.5f, 4.5f);
            Assert.InRange(prey.Traits.Stamina, 4f, 8f);
            Assert.InRange(prey.Traits.VisionRadius, 5f, 9f);
            Assert.InRange(prey.Traits.Metabolism, 2f, 6f);
        }

        foreach (Predator predator in populationManager.PredatorPopulation)
        {
            Assert.InRange(predator.Traits.Speed, 6f, 10f);
            Assert.InRange(predator.Traits.Size, 2.5f, 6.5f);
            Assert.InRange(predator.Traits.Stamina, 2f, 6f);
            Assert.InRange(predator.Traits.VisionRadius, 7f, 11f);
            Assert.InRange(predator.Traits.Metabolism, 2f, 6f);
        }
    }

    [Fact]
    public void RemoveDeadOrganismsFiltersOutDeadPreyAndPredators()
    {
        PopulationManager populationManager = new();
        Prey livingPrey = new(new EvolutionSimulator.Core.Models.Traits(7f, 2.5f, 5f, 8f, 4f), 0f, 0f, 50f);
        Prey deadPrey = new(new EvolutionSimulator.Core.Models.Traits(7f, 2.5f, 5f, 8f, 4f), 0f, 0f, 50f);
        Predator livingPredator = new(new EvolutionSimulator.Core.Models.Traits(6f, 4.5f, 5f, 6.5f, 4f), 0f, 0f, 60f);
        Predator deadPredator = new(new EvolutionSimulator.Core.Models.Traits(6f, 4.5f, 5f, 6.5f, 4f), 0f, 0f, 60f);

        deadPrey.Die();
        deadPredator.Die();

        populationManager.AddPrey(livingPrey);
        populationManager.AddPrey(deadPrey);
        populationManager.AddPredator(livingPredator);
        populationManager.AddPredator(deadPredator);

        populationManager.RemoveDeadOrganisms();

        Assert.Single(populationManager.PreyPopulation);
        Assert.Single(populationManager.PredatorPopulation);
        Assert.Same(livingPrey, populationManager.PreyPopulation[0]);
        Assert.Same(livingPredator, populationManager.PredatorPopulation[0]);
    }
}
