using EvolutionSimulator.Core.Environment;
using EvolutionSimulator.Core.Models;

namespace EvolutionSimulator.Core.Entities
{
    public class Predator : Organism
    {
        public float ReproductionCooldown { get; private set; }

        public Predator(Traits traits, float startX, float startY, float startingEnergy)
        {
            // Initialize predator-specific state and inherited organism properties.
            Id = Guid.NewGuid();

            Traits = traits;

            X = startX;
            Y = startY;

            Energy = startingEnergy;

            Age = 0;
            IsAlive = true;

            // TODO: May want to experiment with a random initial direction on creation
            DirectionX = 0;
            DirectionY = 0;

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

            Prey? prey = FindNearestPrey(populationManager);

            // Dont't think this is needed for pred since they just eat prey
            //Food? food = FindNearestFood(environmentManager);

            // Does not need env manager since pred does not look for food
            DecideMovement(populationManager);

            // We should try and catch prey instead
            //TryEatFood(environmentManager);
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
            // Should be good, might need more than just setting direction (escape score), but
            // That might be able to be handled just exclusively in prey
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
            throw new NotImplementedException();
        }

        public Prey? FindNearestPrey(PopulationManager populationManager)
        {
            // Return the nearest prey within relevant sensing range.
            throw new NotImplementedException();
        }

        public float CalculateCatchChance(Prey prey)
        {
            // Compute probability of catching prey based on predator and prey traits.
            throw new NotImplementedException();
        }
    }
}