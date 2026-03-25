using EvolutionSimulator.Core;
using EvolutionSimulator.Core.Environment;
using EvolutionSimulator.Core.Models;
using Xunit;

namespace EcosystemSimulator.Tests;

public sealed class OrganismTests
{
    [Fact]
    public void SetDirectionNormalizesMovementVector()
    {
        TestOrganism organism = CreateOrganism();

        organism.SetDirection(3f, 4f);

        Assert.Equal(0.6f, organism.DirectionX, 3);
        Assert.Equal(0.8f, organism.DirectionY, 3);
    }

    [Fact]
    public void SetDirectionWithZeroVectorStopsMovement()
    {
        TestOrganism organism = CreateOrganism();
        organism.SetDirection(1f, 1f);

        organism.SetDirection(0f, 0f);

        Assert.Equal(0f, organism.DirectionX);
        Assert.Equal(0f, organism.DirectionY);
    }

    [Fact]
    public void MoveUsesSpeedDirectionAndDeltaTime()
    {
        TestOrganism organism = CreateOrganism();
        organism.SetDirection(1f, 0f);

        organism.Move(2f);

        Assert.Equal(14f, organism.X);
        Assert.Equal(10f, organism.Y);
    }

    [Fact]
    public void ClampToWorldBoundsKeepsPositionInsideEnvironment()
    {
        TestOrganism organism = CreateOrganism(startX: 99f, startY: 1f);
        EnvironmentManager environmentManager = new(100f, 100f);
        organism.SetDirection(1f, -1f);

        organism.Move(1f);
        organism.ClampToWorld(environmentManager);

        Assert.InRange(organism.X, 0f, 100f);
        Assert.InRange(organism.Y, 0f, 100f);
        Assert.Equal(100f, organism.X);
        Assert.Equal(0f, organism.Y);
        Assert.Equal(-0.707f, organism.DirectionX, 3);
        Assert.Equal(0.707f, organism.DirectionY, 3);
    }

    [Fact]
    public void ConsumeEnergyReducesEnergyAndKillsAtZero()
    {
        TestOrganism organism = CreateOrganism(startingEnergy: 8f);

        organism.ConsumeEnergy(2f);

        Assert.False(organism.IsAlive);
        Assert.Equal(0f, organism.Energy);
    }

    [Fact]
    public void WanderKeepsDirectionForLongerDurationInsteadOfRerollingImmediately()
    {
        TestOrganism organism = CreateOrganism();

        organism.WanderForTest(0.1f);
        float firstDirectionX = organism.DirectionX;
        float firstDirectionY = organism.DirectionY;

        organism.WanderForTest(1f);
        organism.WanderForTest(1f);
        organism.WanderForTest(1f);

        Assert.Equal(firstDirectionX, organism.DirectionX);
        Assert.Equal(firstDirectionY, organism.DirectionY);
    }

    [Fact]
    public void CanSeePointRequiresTargetToBeInsideVisionCone()
    {
        TestOrganism organism = CreateOrganism(startX: 0f, startY: 0f);
        organism.SetDirection(1f, 0f);

        Assert.True(organism.CanSeePointForTest(4f, 0f));
        Assert.False(organism.CanSeePointForTest(0f, 4f));
        Assert.False(organism.CanSeePointForTest(-4f, 0f));
    }

    [Fact]
    public void AgeOneStepIncrementsOnlyWhileAlive()
    {
        TestOrganism organism = CreateOrganism();

        organism.AgeOneStep();
        organism.Die();
        organism.AgeOneStep();

        Assert.Equal(1, organism.Age);
    }

    private static TestOrganism CreateOrganism(float startX = 10f, float startY = 10f, float startingEnergy = 20f)
    {
        return new TestOrganism(
            new Traits(speed: 2f, size: 1f, stamina: 3f, visionRadius: 5f, metabolism: 4f),
            startX,
            startY,
            startingEnergy);
    }
}
