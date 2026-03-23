using EvolutionSimulator.Core;
using EvolutionSimulator.Core.Entities;
using EvolutionSimulator.Core.Environment;
using EvolutionSimulator.Core.Models;
using Xunit;

namespace EcosystemSimulator.Tests;

public sealed class TraitsTests
{
    [Fact]
    public void CloneReturnsDetachedCopy()
    {
        Traits original = new(7f, 3f, 5f, 8f, 4f);
        Traits clone = original.Clone();

        clone.Speed = 99f;

        Assert.NotSame(original, clone);
        Assert.Equal(7f, original.Speed);
        Assert.Equal(original.Size, clone.Size);
        Assert.Equal(original.Stamina, clone.Stamina);
        Assert.Equal(original.VisionRadius, clone.VisionRadius);
        Assert.Equal(original.Metabolism, clone.Metabolism);
    }
}

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
            Assert.InRange(prey.Traits.Speed, 5f, 9f);
            Assert.InRange(prey.Traits.Size, 0.5f, 4.5f);
            Assert.InRange(prey.Traits.Stamina, 3f, 7f);
            Assert.InRange(prey.Traits.VisionRadius, 6f, 10f);
            Assert.InRange(prey.Traits.Metabolism, 2f, 6f);
        }

        foreach (Predator predator in populationManager.PredatorPopulation)
        {
            Assert.InRange(predator.Traits.Speed, 4f, 8f);
            Assert.InRange(predator.Traits.Size, 2.5f, 6.5f);
            Assert.InRange(predator.Traits.Stamina, 3f, 7f);
            Assert.InRange(predator.Traits.VisionRadius, 4.5f, 8.5f);
            Assert.InRange(predator.Traits.Metabolism, 2f, 6f);
        }
    }
}

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
}

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
        Assert.Equal(parent.Traits.VisionRadius, child.Traits.VisionRadius);
        Assert.Equal(parent.Traits.Metabolism, child.Traits.Metabolism);
        Assert.Equal(90f, parent.Energy);
        Assert.Equal(30f, child.Energy);
        Assert.True(parent.ReproductionCooldown > 0f);
    }
}

public sealed class FoodTests
{
    [Fact]
    public void ConstructorSetsIdentityPositionNutritionAndConsumptionState()
    {
        Food food = new(12.5f, 9.75f, 18f);

        Assert.NotEqual(Guid.Empty, food.Id);
        Assert.Equal(12.5f, food.X);
        Assert.Equal(9.75f, food.Y);
        Assert.Equal(18f, food.NutritionValue);
        Assert.False(food.IsConsumed);
    }
}

public sealed class EnvironmentManagerTests
{
    [Fact]
    public void SeedInitialFoodRespectsMaxFoodCountAndDefaultNutrition()
    {
        EnvironmentManager environmentManager = new(100f, 80f)
        {
            MaxFoodCount = 3,
            DefaultFoodNutritionValue = 14f
        };

        environmentManager.SeedInitialFood(10);

        Assert.Equal(3, environmentManager.FoodSources.Count);
        Assert.All(environmentManager.FoodSources, food => Assert.Equal(14f, food.NutritionValue));
        Assert.All(
            environmentManager.FoodSources,
            food => Assert.True(environmentManager.IsInsideBounds(food.X, food.Y)));
    }

    [Fact]
    public void UpdateRemovesConsumedFoodAndRegeneratesFood()
    {
        EnvironmentManager environmentManager = new(100f, 100f)
        {
            FoodRegenerationRate = 2f,
            MaxFoodCount = 5,
            DefaultFoodNutritionValue = 11f
        };

        Food consumed = new(5f, 5f, 4f) { IsConsumed = true };
        Food remaining = new(15f, 10f, 6f);

        environmentManager.FoodSources.Add(consumed);
        environmentManager.FoodSources.Add(remaining);

        environmentManager.Update(1f);

        Assert.DoesNotContain(consumed, environmentManager.FoodSources);
        Assert.Contains(remaining, environmentManager.FoodSources);
        Assert.Equal(3, environmentManager.FoodSources.Count);
        Assert.Equal(2, environmentManager.FoodSources.Count(food => food.NutritionValue == 11f));
    }

    [Fact]
    public void GetNearestAvailableFoodSkipsConsumedFoodAndHonorsRange()
    {
        EnvironmentManager environmentManager = new(100f, 100f);

        Food consumedCloser = new(2f, 0f, 5f) { IsConsumed = true };
        Food available = new(4f, 0f, 5f);

        environmentManager.FoodSources.Add(consumedCloser);
        environmentManager.FoodSources.Add(available);

        Food? nearest = environmentManager.GetNearestAvailableFood(0f, 0f, 10f);
        Food? outOfRange = environmentManager.GetNearestAvailableFood(0f, 0f, 3f);

        Assert.Same(available, nearest);
        Assert.Null(outOfRange);
    }

    [Fact]
    public void GetFoodInRangeReturnsOnlyNearbyAvailableFood()
    {
        EnvironmentManager environmentManager = new(100f, 100f);

        Food nearby = new(3f, 4f, 5f);
        Food consumedNearby = new(1f, 1f, 5f) { IsConsumed = true };
        Food farAway = new(8f, 8f, 5f);

        environmentManager.FoodSources.Add(nearby);
        environmentManager.FoodSources.Add(consumedNearby);
        environmentManager.FoodSources.Add(farAway);

        List<Food> foodsInRange = environmentManager.GetFoodInRange(0f, 0f, 5f);

        Assert.Single(foodsInRange);
        Assert.Same(nearby, foodsInRange[0]);
    }

    [Fact]
    public void ClearAllFoodRemovesFoodAndResetsRegenerationProgress()
    {
        EnvironmentManager environmentManager = new(100f, 100f)
        {
            FoodRegenerationRate = 1f,
            MaxFoodCount = 10
        };

        environmentManager.Update(0.5f);
        environmentManager.SeedInitialFood(2);

        environmentManager.ClearAllFood();
        environmentManager.Update(0.5f);

        Assert.Empty(environmentManager.FoodSources);

        environmentManager.Update(1f);

        Assert.Single(environmentManager.FoodSources);
    }
}
