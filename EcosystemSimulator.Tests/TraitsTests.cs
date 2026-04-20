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
        Assert.Equal(original.VisionDistance, clone.VisionDistance);
        Assert.Equal(original.Metabolism, clone.Metabolism);
    }

    [Theory]
    [InlineData(0.5f, 1.5f)]
    [InlineData(3f, 3f)]
    [InlineData(6f, 4.5f)]
    public void ConstructorClampsSizeIntoExpectedRange(float inputSize, float expectedSize)
    {
        Traits traits = new(7f, inputSize, 5f, 8f, 4f);

        Assert.Equal(expectedSize, traits.Size);
    }
}
