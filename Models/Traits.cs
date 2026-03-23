using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulator.Core.Models
{
    public class Traits
    {
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
        {
            // Initialize default trait values.
        }

        public Traits Clone()
        {
            // Return a deep copy of the traits object.
            throw new NotImplementedException();
        }

        public void Mutate(float mutationRate)
        {
            if (Random.Shared.NextSingle() < mutationRate)
                Speed += Random.Shared.NextSingle() * 0.5f -0.25f;

            if (Random.Shared.NextSingle() < mutationRate)
                Size += Random.Shared.NextSingle() * 0.5f -0.25f;

            if (Random.Shared.NextSingle() < mutationRate)
                Stamina += Random.Shared.NextSingle() * 0.5f -0.25f;
                
            if (Random.Shared.NextSingle() < mutationRate)
                VisionRadius += Random.Shared.NextSingle() * 0.5f -0.25f;
        }
    }
}