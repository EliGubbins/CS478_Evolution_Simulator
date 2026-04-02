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
}
