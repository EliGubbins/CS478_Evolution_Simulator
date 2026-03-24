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
            Id = Guid.NewGuid();
            IsAlive = true;
            Traits = new Traits();
        }

        protected Organism(Traits traits, float startX, float startY, float startingEnergy)
            : this()
        {
            Traits = traits;
            X = startX;
            Y = startY;
            Energy = startingEnergy;
            Age = 0;

            // TODO: May want to experiment with a random initial direction on creation
            DirectionX = 0;
            DirectionY = 0;
        }

        public virtual void Update(EnvironmentManager environmentManager, PopulationManager populationManager, float deltaTime)
        {
            // Perform one simulation update for this organism.
            // This may include movement, energy use, sensing, and state updates.
            if (!IsAlive)
                return;
            
            Move(deltaTime);

            ClampToWorldBounds(environmentManager);

            ConsumeEnergy(deltaTime);

            AgeOneStep();
        }

        public virtual void Move(float deltaTime)
        {
            // Update organism position based on direction, speed, and time step.
            if (!IsAlive)
                return;

            float speed = Traits.Speed;

            this.X += DirectionX * speed * deltaTime;
            this.Y += DirectionY * speed * deltaTime;
        }

        public virtual void ConsumeEnergy(float deltaTime)
        {
            // Reduce energy based on metabolism and activity.
            if (!IsAlive)
                return;

            float metabolism = Traits.Metabolism;

            Energy -= metabolism * deltaTime;

            if (Energy <= 0 )
                Die();
        }

        public virtual void AgeOneStep()
        {
            // Increment age or age-related counters.
            // Age will eventually factor in to speed, metabolism, and effect traits
            if (!IsAlive)
                return;
            
            Age += 1;
        }

        public virtual void Die()
        {
            // Mark the organism as dead and perform cleanup-related state changes.
            // TODO: for the analytics we may eventually want to store 
            IsAlive = false;
            Energy = 0;
        }

        public virtual bool CanReproduce()
        {
            // Return whether this organism currently meets reproduction requirements.
            // TODO: refine more detailed standards for when reproduction occurs, this is just a placeholder
            return IsAlive && Energy >= 50;
        }

        public virtual void SetDirection(float x, float y)
        {
            // Normalize and store movement direction so that animals do not move faster in the diagonal.
            float length = (float)Math.Sqrt(x * x + y *y);

            if (length == 0)
            {
                DirectionX = 0; 
                DirectionY = 0;
                return;
            }

            DirectionX = x / length;
            DirectionY = y /length;
        }

        public virtual float DistanceTo(float x, float y)
        {
            // Return distance from this organism to a point using pythagorean theorem.
            float x1 = this.X;
            float y1 = this.Y;
            float dx = x - x1;
            float dy = y - y1;
            return (float)Math.Sqrt(dx *dx + dy* dy);
        }

        public virtual float DistanceTo(Organism other)
        {
            // Return distance from this organism to another organism.
            float x1 = this.X;
            float y1 = this.Y;
            float x2 = other.X;
            float y2 = other.Y;
            float dx = x2 - x1;
            float dy = y2 - y1;
            return (float)Math.Sqrt(dx * dx + dy *dy );
        }

        protected virtual void ClampToWorldBounds(EnvironmentManager environmentManager)
        {
            // Keep this organism inside the environment bounds.
            if (X < 0) X = 0;
            if (Y < 0) Y = 0;

            if (X > environmentManager.Width)
                X = environmentManager.Width;

            if (Y > environmentManager.Height)
                Y = environmentManager.Height;
        }
    }
}
