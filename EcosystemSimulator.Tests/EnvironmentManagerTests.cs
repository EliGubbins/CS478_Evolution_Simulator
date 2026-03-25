using EvolutionSimulator.Core.Environment;
using EvolutionSimulator.Core.Models;
using Xunit;

namespace EcosystemSimulator.Tests;

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
