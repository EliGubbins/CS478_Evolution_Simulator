using EvolutionSimulator.Core.Environment;
using EvolutionSimulator.Core.Models;

namespace EvolutionSimulator.Core.Entities
{
    public class Predator : Organism
    {
        public float ReproductionCooldown { get; private set; }

        public Predator(Traits traits, float startX, float startY, float startingEnergy)
            : base(traits, startX, startY, startingEnergy)
        {
            ReproductionCooldown = 0;
        }

        public override void Update(EnvironmentManager environmentManager, PopulationManager populationManager, float deltaTime)
        {
            // Run one predator update step:
            // - detect prey
            // - choose pursuit or wandering
            // - consume energy
            // - age
            // - attempt reproduction
            if (!IsAlive)
                return;
            
            if (ReproductionCooldown > 0)
                ReproductionCooldown -= deltaTime;

            DecideMovement(populationManager);

            TryCatchPrey(populationManager);

            base.Update(environmentManager, populationManager, deltaTime);
        }

        public void DecideMovement(PopulationManager populationManager)
        {
            // Choose whether to hunt nearby prey or wander.
            Prey? prey = FindNearestPrey(populationManager);

            if (prey != null)
            {
                HuntPrey(prey);
                return;
            }

            Wander();
        }

        public void HuntPrey(Prey prey)
        {
            // Set movement direction toward a prey target.
            float dx = prey.X - X;
            float dy = prey.Y - Y;

            SetDirection(dx, dy);
        }

        public void Wander()
        {
            // Set a semi-random movement direction when no prey is nearby.
            float dx = Random.Shared.NextSingle() * 2 - 1;
            float dy = Random.Shared.NextSingle() * 2 - 1;

            SetDirection(dx, dy);
        }

        public void TryCatchPrey(PopulationManager populationManager)
        {
            // Attempt to catch nearby prey and gain energy on success.
            Prey? prey = FindNearestPrey(populationManager);

            if (prey == null)
                return;
            
            float distance = DistanceTo(prey);

            // TODO: determine what trait will effect the radius preds are willing to hunt
            float catchRange = Traits.Size;

            if (distance > catchRange)
                return;

            float catchChance = CalculateCatchChance(prey);

            if (Random.Shared.NextSingle() < catchChance)
            {
                float preyEnergy = prey.Energy;
                prey.Die();
                Energy += preyEnergy * 0.8f;
                // could use a fixed nutrition value later for better balancing
                // I think in the long term though larger prey would provide more energy so
                // we will need to factor in size for our formula
            }
        }

        public override bool CanReproduce()
        {
            // Return whether this predator has enough energy and meets other reproduction rules.
            if (!IsAlive)
                return false;

            if (Energy < 60)
                return false;
            
            if (ReproductionCooldown > 0)
                return false;
            
            return true;
        }

        public Predator CreateOffspring(float mutationRate)
        {
            // Create a new predator instance with inherited and possibly mutated traits.

            // Copy traits from parent
            Traits childTraits = Traits.Clone();

            // Apply mutation
            childTraits.Mutate(mutationRate);
            
            // Spawn near parent
            float offsetX = Random.Shared.NextSingle() * 2 - 1;
            float offsetY = Random.Shared.NextSingle() * 2 - 1;

            float childX = X + offsetX;
            float childY = Y + offsetY;

            float childEnergy = Energy * 0.25f;

            // Reduce Parent Energy
            Energy *= 0.75f;

            // Reset reproduction cooldown
            ReproductionCooldown = 10;

            return new Predator(childTraits, childX, childY, childEnergy);
        }

        // TODO: calculate a prey score
        // i.e. predators would prefer prey with lower energy of maybe smaller/larger size 
        public Prey? FindNearestPrey(PopulationManager populationManager)
        {
            // Return the nearest prey within relevant sensing range.
            Prey? closest = null;
            float closestDistance = float.MaxValue;

            foreach (var prey in populationManager.PreyPopulation)
            {
                if (!prey.IsAlive)
                    continue;
                
                float distance = DistanceTo(prey);

                if (distance < closestDistance && distance <= Traits.VisionRadius)
                {
                    closest = prey;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        // TODO: This is pretty much identical to our prey function.
        // Should make some more adjustments to differentiate the two
        public float CalculateCatchChance(Prey prey)
        {
            // Compute probability of catching prey based on predator and prey traits.

            float predatorAdvantage = 
                (Traits.Speed * 0.5f) +
                (Traits.Stamina * 0.2f) +
                (Traits.VisionRadius * 0.1f) +
                (Traits.Size * 0.2f);
            
            float preyAdvantage = 
                (prey.Traits.Speed * 0.5f) +
                (prey.Traits.Stamina * 0.3f) +
                (prey.Traits.Size * 0.2f);

            float score = predatorAdvantage - preyAdvantage;

            float probability = 0.5f + (score * 0.1f);

            // Clamp the result
            probability = Math.Clamp(probability, 0.1f, 0.9f);

            return probability;
        }
    }
}
