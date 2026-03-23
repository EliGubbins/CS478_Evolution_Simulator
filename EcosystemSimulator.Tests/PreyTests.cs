using EvolutionSimulator.Core;
using EvolutionSimulator.Core.Entities;
using EvolutionSimulator.Core.Environment;
using EvolutionSimulator.Core.Models;
using Xunit;

namespace EcosystemSimulator.Tests;

public sealed class PreyTests
{
    [Fact]
    public void CreateOffspringInheritsTraitsAndUpdatesParentState()
    {
        Traits traits = new(7f, 2.5f, 5f, 8f, 4f);
        Prey parent = new(traits, 10f, 20f, 80f);

        Prey child = parent.CreateOffspring(0f);

        Assert.NotSame(parent.Traits, child.Traits);
        Assert.Equal(parent.Traits.Speed, child.Traits.Speed);
        Assert.Equal(parent.Traits.Size, child.Traits.Size);
        Assert.Equal(parent.Traits.Stamina, child.Traits.Stamina);
        Assert.Equal(parent.Traits.VisionRadius, child.Traits.VisionRadius);
        Assert.Equal(parent.Traits.Metabolism, child.Traits.Metabolism);
        Assert.Equal(60f, parent.Energy);
        Assert.Equal(20f, child.Energy);
        Assert.True(parent.ReproductionCooldown > 0f);
    }

    [Fact]
    public void FindNearestFoodReturnsClosestAvailableFoodWithinVisionRange()
    {
        EnvironmentManager environmentManager = new(100f, 100f);
        Prey prey = new(new Traits(7f, 3f, 5f, 10f, 4f), 0f, 0f, 50f);
        Food fartherFood = new(6f, 0f, 8f);
        Food nearestFood = new(2f, 0f, 10f);
        Food consumedFood = new(1f, 0f, 3f) { IsConsumed = true };
        Food outOfRangeFood = new(20f, 0f, 12f);

        environmentManager.FoodSources.Add(fartherFood);
        environmentManager.FoodSources.Add(nearestFood);
        environmentManager.FoodSources.Add(consumedFood);
        environmentManager.FoodSources.Add(outOfRangeFood);

        Food? result = prey.FindNearestFood(environmentManager);

        Assert.Same(nearestFood, result);
    }

    [Fact]
    public void TryEatFoodConsumesNearbyFoodAndIncreasesEnergy()
    {
        EnvironmentManager environmentManager = new(100f, 100f);
        Prey prey = new(new Traits(7f, 3f, 5f, 10f, 4f), 0f, 0f, 50f);
        Food nearbyFood = new(2f, 0f, 15f);

        environmentManager.FoodSources.Add(nearbyFood);

        prey.TryEatFood(environmentManager);

        Assert.True(nearbyFood.IsConsumed);
        Assert.Equal(65f, prey.Energy);
    }

    [Fact]
    public void CanReproduceRequiresLifeEnergyAndNoCooldown()
    {
        Prey lowEnergyPrey = new(new Traits(7f, 2.5f, 5f, 8f, 4f), 0f, 0f, 59f);
        Prey reproducingPrey = new(new Traits(7f, 2.5f, 5f, 8f, 4f), 0f, 0f, 80f);
        Prey deadPrey = new(new Traits(7f, 2.5f, 5f, 8f, 4f), 0f, 0f, 80f);

        reproducingPrey.CreateOffspring(0f);
        deadPrey.Die();

        Assert.False(lowEnergyPrey.CanReproduce());
        Assert.False(reproducingPrey.CanReproduce());
        Assert.False(deadPrey.CanReproduce());
    }

    [Fact]
    public void FleeFromPredatorSetsDirectionAwayFromPredator()
    {
        Prey prey = new(new Traits(7f, 3f, 5f, 10f, 4f), 5f, 5f, 50f);
        Predator predator = new(new Traits(6f, 4f, 5f, 8f, 4f), 7f, 9f, 60f);

        prey.FleeFromPredator(predator);

        Assert.Equal(-0.447f, prey.DirectionX, 3);
        Assert.Equal(-0.894f, prey.DirectionY, 3);
    }

    [Fact]
    public void FindNearestPredatorReturnsClosestLivingPredatorWithinVisionRange()
    {
        PopulationManager populationManager = new();
        Prey prey = new(new Traits(7f, 3f, 5f, 10f, 4f), 0f, 0f, 50f);
        Predator nearest = new(new Traits(6f, 4.5f, 5f, 6.5f, 4f), 3f, 0f, 60f);
        Predator farther = new(new Traits(6f, 4.5f, 5f, 6.5f, 4f), 6f, 0f, 60f);
        Predator dead = new(new Traits(6f, 4.5f, 5f, 6.5f, 4f), 1f, 0f, 60f);
        Predator outOfRange = new(new Traits(6f, 4.5f, 5f, 6.5f, 4f), 20f, 0f, 60f);
        dead.Die();

        populationManager.AddPredator(farther);
        populationManager.AddPredator(nearest);
        populationManager.AddPredator(dead);
        populationManager.AddPredator(outOfRange);

        Predator? result = prey.FindNearestPredator(populationManager);

        Assert.Same(nearest, result);
    }
}
