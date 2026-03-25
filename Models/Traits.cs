using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulator.Core.Models
{
    public class Traits
    {
        private const float MinimumTraitValue = 0.1f;

        public float Speed { get; set; }
        public float Size { get; set; }
        public float Stamina { get; set; }
        // TODO: I think we should split this into two values
        // One value is a float angle which would dermine the degree cone the animal can see in
        // i.e. float angle = 45 degrees
        // Second value is a distance line that would determine the overall distance of their vision vector.
        // i.e float VisionDistance = 10 (meters or just units for x and y)
        // For this to work we will need to essentially calculate a cone (narrower but farther for predators,
        //  and wider but shorter for prey)
        public float VisionRadius { get; set; }
        public float Metabolism { get; set; }

        public Traits()
            : this(1f, 1f, 1f, 1f, 1f)
        {
        }

        public Traits(float speed, float size, float stamina, float visionRadius, float metabolism)
        {
            Speed = speed;
            Size = size;
            Stamina = stamina;
            VisionRadius = visionRadius;
            Metabolism = metabolism;
        }

        public Traits Clone()
        {
            return new Traits(Speed, Size, Stamina, VisionRadius, Metabolism);
        }

        public void Mutate(float mutationRate)
        {
            if (Random.Shared.NextSingle() < mutationRate)
                Speed = MutateValue(Speed);

            if (Random.Shared.NextSingle() < mutationRate)
                Size = MutateValue(Size);

            if (Random.Shared.NextSingle() < mutationRate)
                Stamina = MutateValue(Stamina);
                
            if (Random.Shared.NextSingle() < mutationRate)
                VisionRadius = MutateValue(VisionRadius);

            if (Random.Shared.NextSingle() < mutationRate)
                Metabolism = MutateValue(Metabolism);
        }

        private static float MutateValue(float value)
        {
            float mutatedValue = value + (Random.Shared.NextSingle() * 0.5f) - 0.25f;
            return MathF.Max(MinimumTraitValue, mutatedValue);
        }
    }
}
