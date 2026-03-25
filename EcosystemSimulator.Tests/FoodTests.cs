using EvolutionSimulator.Core.Models;
using Xunit;

namespace EcosystemSimulator.Tests;

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
