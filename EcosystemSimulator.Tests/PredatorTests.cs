using EvolutionSimulator.Core;
using EvolutionSimulator.Core.Entities;
using EvolutionSimulator.Core.Models;
using Xunit;

namespace EcosystemSimulator.Tests;

public sealed class PredatorTests
{
    [Fact]
    public void CreateOffspringInheritsTraitsAndUpdatesParentState()
    {
        Traits traits = new(6f, 4.5f, 5f, 6.5f, 4f);
        Predator parent = new(traits, 5f, 15f, 120f);

        Predator child = parent.CreateOffspring(0f);

        Assert.NotSame(parent.Traits, child.Traits);
        Assert.Equal(parent.Traits.Speed, child.Traits.Speed);
        Assert.Equal(parent.Traits.Size, child.Traits.Size);
        Assert.Equal(parent.Traits.Stamina, child.Traits.Stamina);
        Assert.Equal(parent.Traits.VisionDistance, child.Traits.VisionDistance);
        Assert.Equal(parent.Traits.Metabolism, child.Traits.Metabolism);
        Assert.Equal(60f, parent.Energy);
        Assert.Equal(60f, child.Energy);
        Assert.True(parent.ReproductionCooldown > 0f);
    }

    [Fact]
    public void FindNearestPreyReturnsClosestLivingPreyWithinVisionRange()
    {
        PopulationManager populationManager = new();
        Predator predator = new(new Traits(6f, 4.5f, 5f, 10f, 4f), 0f, 0f, 60f);
        Prey nearest = new(new Traits(7f, 2.5f, 5f, 8f, 4f), 2f, 0f, 50f);
        Prey farther = new(new Traits(7f, 2.5f, 5f, 8f, 4f), 5f, 0f, 50f);
        Prey dead = new(new Traits(7f, 2.5f, 5f, 8f, 4f), 1f, 0f, 50f);
        Prey outOfRange = new(new Traits(7f, 2.5f, 5f, 8f, 4f), 20f, 0f, 50f);
        dead.Die();

        populationManager.AddPrey(farther);
        populationManager.AddPrey(nearest);
        populationManager.AddPrey(dead);
        populationManager.AddPrey(outOfRange);

        Prey? result = predator.FindNearestPrey(populationManager);

        Assert.Same(nearest, result);
    }

    [Fact]
    public void CanReproduceRequiresLifeEnergyAndNoCooldown()
    {
        Predator lowEnergyPredator = new(new Traits(6f, 4.5f, 5f, 6.5f, 4f), 0f, 0f, 59f);
        Predator reproducingPredator = new(new Traits(6f, 4.5f, 5f, 6.5f, 4f), 0f, 0f, 120f);
        Predator deadPredator = new(new Traits(6f, 4.5f, 5f, 6.5f, 4f), 0f, 0f, 120f);

        reproducingPredator.CreateOffspring(0f);
        deadPredator.Die();

        Assert.False(lowEnergyPredator.CanReproduce());
        Assert.False(reproducingPredator.CanReproduce());
        Assert.False(deadPredator.CanReproduce());
    }

    [Fact]
    public void CalculateCatchChanceFavorsStrongerPredators()
    {
        Predator strongPredator = new(new Traits(9f, 5f, 7f, 8f, 4f), 0f, 0f, 60f);
        Predator weakPredator = new(new Traits(4f, 3f, 3f, 4f, 4f), 0f, 0f, 60f);
        Prey prey = new(new Traits(6f, 3f, 4f, 8f, 4f), 1f, 0f, 50f);

        float strongChance = strongPredator.CalculateCatchChance(prey);
        float weakChance = weakPredator.CalculateCatchChance(prey);

        Assert.True(strongChance > weakChance);
        Assert.InRange(strongChance, 0.1f, 0.9f);
        Assert.InRange(weakChance, 0.1f, 0.9f);
    }

    [Fact]
    public void CalculateEnergyGainReturnsMoreThanPreyEnergy()
    {
        Predator predator = new(new Traits(8f, 4.5f, 4f, 9f, 4f), 0f, 0f, 60f);
        Prey prey = new(new Traits(6f, 2.5f, 6f, 7f, 4f), 1f, 0f, 50f);

        float energyGained = predator.CalculateEnergyGain(prey);

        Assert.Equal(86.25f, energyGained);
        Assert.True(energyGained > prey.Energy);
    }

    [Fact]
    public void CalculateEnergyGainIncreasesWithPreySize()
    {
        Predator predator = new(new Traits(8f, 4.5f, 4f, 9f, 4f), 0f, 0f, 60f);
        Prey smallPrey = new(new Traits(6f, 1f, 6f, 7f, 4f), 1f, 0f, 50f);
        Prey largePrey = new(new Traits(6f, 4f, 6f, 7f, 4f), 1f, 0f, 50f);

        float smallEnergyGain = predator.CalculateEnergyGain(smallPrey);
        float largeEnergyGain = predator.CalculateEnergyGain(largePrey);

        Assert.True(largeEnergyGain > smallEnergyGain);
        Assert.Equal(73.75f, smallEnergyGain);
        Assert.Equal(100f, largeEnergyGain);
    }
}
