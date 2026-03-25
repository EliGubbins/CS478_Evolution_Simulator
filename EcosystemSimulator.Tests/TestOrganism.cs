using EvolutionSimulator.Core;
using EvolutionSimulator.Core.Entities;
using EvolutionSimulator.Core.Environment;
using EvolutionSimulator.Core.Models;

namespace EcosystemSimulator.Tests;

internal sealed class TestOrganism : Organism
{
    public override float VisionFieldOfViewDegrees => 90f;

    public TestOrganism(Traits traits, float startX, float startY, float startingEnergy)
        : base(traits, startX, startY, startingEnergy)
    {
    }

    public void ClampToWorld(EnvironmentManager environmentManager)
    {
        ClampToWorldBounds(environmentManager);
    }

    public void WanderForTest(float deltaTime)
    {
        Wander(deltaTime);
    }

    public bool CanSeePointForTest(float x, float y)
    {
        return CanSeePoint(x, y);
    }

    public override void Update(EnvironmentManager environmentManager, PopulationManager populationManager, float deltaTime)
    {
        base.Update(environmentManager, populationManager, deltaTime);
    }
}
