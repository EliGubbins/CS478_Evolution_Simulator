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
    }
}