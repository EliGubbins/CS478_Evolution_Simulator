using EvolutionSimulator.Core.Environment;
using EvolutionSimulator.Core.Models;

namespace EvolutionSimulator.Core.Entities
{
    public abstract class Organism
    {
        public Guid Id { get; protected set; }
        public float X { get; protected set; }
        public float Y { get; protected set; }

        public float DirectionX { get; protected set; }
        public float DirectionY { get; protected set; }

        public float Energy { get; protected set; }
        public int Age { get; protected set; }
        public bool IsAlive { get; protected set; }

        public Traits Traits { get; protected set; }

        protected Organism()
        {
            // Initialize shared organism fields.
        }

        public virtual void Update(EnvironmentManager environmentManager, PopulationManager populationManager, float deltaTime)
        {
            // Perform one simulation update for this organism.
            // This may include movement, energy use, sensing, and state updates.
        }

        public virtual void Move(float deltaTime)
        {
            // Update organism position based on direction, speed, and time step.
        }

        public virtual void ConsumeEnergy(float deltaTime)
        {
            // Reduce energy based on metabolism and activity.
        }

        public virtual void AgeOneStep()
        {
            // Increment age or age-related counters.
        }

        public virtual void Die()
        {
            // Mark the organism as dead and perform cleanup-related state changes.
        }

        public virtual bool CanReproduce()
        {
            // Return whether this organism currently meets reproduction requirements.
            throw new NotImplementedException();
        }

        public virtual void SetDirection(float x, float y)
        {
            // Normalize and store movement direction.
        }

        public virtual float DistanceTo(float x, float y)
        {
            // Return distance from this organism to a point.
            throw new NotImplementedException();
        }

        public virtual float DistanceTo(Organism other)
        {
            // Return distance from this organism to another organism.
            throw new NotImplementedException();
        }

        protected virtual void ClampToWorldBounds(EnvironmentManager environmentManager)
        {
            // Keep this organism inside the environment bounds.
        }
    }
}