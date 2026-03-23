using EvolutionSimulator.Core;
using EvolutionSimulator.Core.Environment;
using EvolutionSimulator.Core.Entities;
using EvolutionSimulator.Core.Models;

namespace EcosystemSimulator.Tests;

internal static class Program
{
    private static readonly List<(string Name, Action Test)> Tests =
    [
        ("Traits.Clone returns a detached copy", TraitsCloneReturnsDetachedCopy),
        ("SeedInitialPopulation clears and reseeds populations", SeedInitialPopulationClearsAndReseedsPopulations),
        ("SeedInitialPopulation keeps prey and predator traits within baseline range", SeedInitialPopulationKeepsTraitsWithinBaselineRange),
        ("Prey offspring inherits traits and updates parent state", PreyOffspringInheritsTraitsAndUpdatesParentState),
        ("Predator offspring inherits traits and updates parent state", PredatorOffspringInheritsTraitsAndUpdatesParentState)
    ];

    private static int Main()
    {
        int passed = 0;
        List<string> failures = [];

        foreach ((string name, Action test) in Tests)
        {
            try
            {
                test();
                Console.WriteLine($"PASS {name}");
                passed += 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAIL {name}");
                Console.WriteLine($"  {ex.Message}");
                failures.Add(name);
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{passed}/{Tests.Count} tests passed.");

        return failures.Count == 0 ? 0 : 1;
    }

    private static void TraitsCloneReturnsDetachedCopy()
    {
        Traits original = new(7f, 3f, 5f, 8f, 4f);
        Traits clone = original.Clone();

        clone.Speed = 99f;

        Assert.NotSame(original, clone, "Clone should return a distinct object instance.");
        Assert.Equal(7f, original.Speed, "Changing the clone should not mutate the original.");
        Assert.Equal(original.Size, clone.Size, "Clone should copy all original values.");
        Assert.Equal(original.Stamina, clone.Stamina, "Clone should copy all original values.");
        Assert.Equal(original.VisionRadius, clone.VisionRadius, "Clone should copy all original values.");
        Assert.Equal(original.Metabolism, clone.Metabolism, "Clone should copy all original values.");
    }

    private static void SeedInitialPopulationClearsAndReseedsPopulations()
    {
        PopulationManager populationManager = new();
        EnvironmentManager environmentManager = new(100f, 100f);

        populationManager.SeedInitialPopulation(4, 2, environmentManager, 50f, 70f);
        populationManager.SeedInitialPopulation(3, 1, environmentManager, 45f, 80f);

        Assert.Equal(3, populationManager.PreyPopulation.Count, "Reseeding should replace the prey population.");
        Assert.Equal(1, populationManager.PredatorPopulation.Count, "Reseeding should replace the predator population.");
        Assert.True(
            populationManager.PreyPopulation.All(prey => prey.Energy == 45f),
            "Reseeded prey should use the new starting energy.");
        Assert.True(
            populationManager.PredatorPopulation.All(predator => predator.Energy == 80f),
            "Reseeded predators should use the new starting energy.");
    }

    private static void SeedInitialPopulationKeepsTraitsWithinBaselineRange()
    {
        PopulationManager populationManager = new();
        EnvironmentManager environmentManager = new(100f, 100f);

        populationManager.SeedInitialPopulation(200, 200, environmentManager, 50f, 80f);

        foreach (Prey prey in populationManager.PreyPopulation)
        {
            Assert.InRange(prey.Traits.Speed, 5f, 9f, "Prey speed should stay within baseline +/- 2.");
            Assert.InRange(prey.Traits.Size, 0.5f, 4.5f, "Prey size should stay within baseline +/- 2.");
            Assert.InRange(prey.Traits.Stamina, 3f, 7f, "Prey stamina should stay within baseline +/- 2.");
            Assert.InRange(prey.Traits.VisionRadius, 6f, 10f, "Prey vision should stay within baseline +/- 2.");
            Assert.InRange(prey.Traits.Metabolism, 2f, 6f, "Prey metabolism should stay within baseline +/- 2.");
        }

        foreach (Predator predator in populationManager.PredatorPopulation)
        {
            Assert.InRange(predator.Traits.Speed, 4f, 8f, "Predator speed should stay within baseline +/- 2.");
            Assert.InRange(predator.Traits.Size, 2.5f, 6.5f, "Predator size should stay within baseline +/- 2.");
            Assert.InRange(predator.Traits.Stamina, 3f, 7f, "Predator stamina should stay within baseline +/- 2.");
            Assert.InRange(predator.Traits.VisionRadius, 4.5f, 8.5f, "Predator vision should stay within baseline +/- 2.");
            Assert.InRange(predator.Traits.Metabolism, 2f, 6f, "Predator metabolism should stay within baseline +/- 2.");
        }
    }

    private static void PreyOffspringInheritsTraitsAndUpdatesParentState()
    {
        Traits traits = new(7f, 2.5f, 5f, 8f, 4f);
        Prey parent = new(traits, 10f, 20f, 80f);

        Prey child = parent.CreateOffspring(0f);

        Assert.NotSame(parent.Traits, child.Traits, "Offspring should receive a cloned trait object.");
        Assert.Equal(parent.Traits.Speed, child.Traits.Speed, "Mutation rate 0 should preserve speed.");
        Assert.Equal(parent.Traits.Size, child.Traits.Size, "Mutation rate 0 should preserve size.");
        Assert.Equal(parent.Traits.Stamina, child.Traits.Stamina, "Mutation rate 0 should preserve stamina.");
        Assert.Equal(parent.Traits.VisionRadius, child.Traits.VisionRadius, "Mutation rate 0 should preserve vision.");
        Assert.Equal(parent.Traits.Metabolism, child.Traits.Metabolism, "Mutation rate 0 should preserve metabolism.");
        Assert.Equal(60f, parent.Energy, "Parent should keep 75% of its energy after reproducing.");
        Assert.Equal(20f, child.Energy, "Child should receive 25% of the original parent energy.");
        Assert.True(parent.ReproductionCooldown > 0f, "Parent reproduction cooldown should be reset.");
    }

    private static void PredatorOffspringInheritsTraitsAndUpdatesParentState()
    {
        Traits traits = new(6f, 4.5f, 5f, 6.5f, 4f);
        Predator parent = new(traits, 5f, 15f, 120f);

        Predator child = parent.CreateOffspring(0f);

        Assert.NotSame(parent.Traits, child.Traits, "Offspring should receive a cloned trait object.");
        Assert.Equal(parent.Traits.Speed, child.Traits.Speed, "Mutation rate 0 should preserve speed.");
        Assert.Equal(parent.Traits.Size, child.Traits.Size, "Mutation rate 0 should preserve size.");
        Assert.Equal(parent.Traits.Stamina, child.Traits.Stamina, "Mutation rate 0 should preserve stamina.");
        Assert.Equal(parent.Traits.VisionRadius, child.Traits.VisionRadius, "Mutation rate 0 should preserve vision.");
        Assert.Equal(parent.Traits.Metabolism, child.Traits.Metabolism, "Mutation rate 0 should preserve metabolism.");
        Assert.Equal(90f, parent.Energy, "Parent should keep 75% of its energy after reproducing.");
        Assert.Equal(30f, child.Energy, "Child should receive 25% of the original parent energy.");
        Assert.True(parent.ReproductionCooldown > 0f, "Parent reproduction cooldown should be reset.");
    }
}

internal static class Assert
{
    public static void True(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    public static void Equal<T>(T expected, T actual, string message)
        where T : notnull
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"{message} Expected {expected} but found {actual}.");
    }

    public static void NotSame(object expectedDifferent, object actual, string message)
    {
        if (ReferenceEquals(expectedDifferent, actual))
            throw new InvalidOperationException(message);
    }

    public static void InRange(float actual, float minimum, float maximum, string message)
    {
        if (actual < minimum || actual > maximum)
            throw new InvalidOperationException($"{message} Value {actual} was outside [{minimum}, {maximum}].");
    }
}
