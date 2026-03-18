using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulator.Core.Models
{
    public class Food
    {
        public Guid Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float NutritionValue { get; set; }
        public bool IsConsumed { get; set; }

        public Food(float x, float y, float nutritionalValue = 10)
        {
            Id = Guid.NewGuid();
            X = x;
            Y = y;
            NutritionValue = nutritionalValue;
            IsConsumed = false;
        }
    }
}