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
        private const float MinimumSizeValue = 1.5f;
        private const float MaximumSizeValue = 4.5f;
        private const float SizeMutationStep = 0.15f;

        public float Speed { get; set; }
        public float Size { get; set; }
        public float Stamina { get; set; }
        public float VisionDistance { get; set; }
        public float Metabolism { get; set; }

        public Traits()
            : this(1f, 1f, 1f, 1f, 1f)
        {
        }

        public Traits(float speed, float size, float stamina, float visionDistance, float metabolism)
        {
            Speed = speed;
            Size = ClampSize(size);
            Stamina = stamina;
            VisionDistance = visionDistance;
            Metabolism = metabolism;
        }

        public Traits Clone()
        {
            return new Traits(Speed, Size, Stamina, VisionDistance, Metabolism);
        }

        public void Mutate(float mutationRate)
        {
            if (Random.Shared.NextSingle() < mutationRate)
                Speed = MutateValue(Speed);

            if (Random.Shared.NextSingle() < mutationRate)
                Size = MutateSize(Size);

            if (Random.Shared.NextSingle() < mutationRate)
                Stamina = MutateValue(Stamina);
                
            if (Random.Shared.NextSingle() < mutationRate)
                VisionDistance = MutateValue(VisionDistance);

            if (Random.Shared.NextSingle() < mutationRate)
                Metabolism = MutateValue(Metabolism);
        }

        private static float MutateValue(float value)
        {
            float mutatedValue = value + (Random.Shared.NextSingle() * 0.5f) - 0.25f;
            return MathF.Max(MinimumTraitValue, mutatedValue);
        }

        public static float ClampSize(float size)
        {
            return Math.Clamp(size, MinimumSizeValue, MaximumSizeValue);
        }

        private static float MutateSize(float size)
        {
            float mutatedSize = size + (Random.Shared.NextSingle() * 2f * SizeMutationStep) - SizeMutationStep;
            return ClampSize(mutatedSize);
        }
    }
}
