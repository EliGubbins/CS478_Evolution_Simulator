using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EvolutionSimulator.Core.Models;

namespace EvolutionSimulator.Core.Environment
{
    public class EnvironmentManager
    {
        public float Width { get; private set; }
        public float Height { get; private set; }

        public List<Food> FoodSources { get; private set; }

        public float FoodRegenerationRate { get; set; }
        public int MaxFoodCount { get; set; }
        public float SpawnCounter { get; set; } 


        public EnvironmentManager(float width = 1000, float height = 1000)
        {
            Width = width;
            Height = height;
            FoodSources = new List<Food>();
            FoodRegenerationRate = 0.5f; // Example: .5 per second
            MaxFoodCount = 100;
            SpawnCounter = 0;

        }

        public void Update(float deltaTime)
        {
            // Advance environment state for one simulation step.
            // This may include food regeneration and global environmental effects.

            SpawnCounter =+ deltaTime * FoodRegenerationRate;
            for (float i = .49f; i<SpawnCounter; i++)
                if (GetAvailableFoodCount() < MaxFoodCount) { RegenerateFood(); }
                SpawnCounter--;   
        }

        public void RegenerateFood()
        {
            // Add new food to the environment according to regeneration rules.
            float x;
            float y;
            (x, y) = GetRandomPosition();
            FoodSources.Add(new Food(x, y));
        }

        public void RemoveConsumedFood()
        {
            FoodSources.RemoveAll(F => F.IsConsumed);
        }

        public Food? GetNearestAvailableFood(float x, float y, float maxRange)
        {
            // Return the closest unconsumed food within a given range.
            throw new NotImplementedException();
        }

        public List<Food> GetFoodInRange(float x, float y, float radius)
        {
            // Return all available food sources within a radius.
            throw new NotImplementedException();
        }

        public bool IsInsideBounds(float x, float y)
        {
            if (x < 0 || x > Width || y < 0 || y > Height)
                return false;
            return true;
        }

        public (float X, float Y) GetRandomPosition()
        {
            Random random = new Random();
            float x = (float)(random.NextDouble() * Width);
            float y = (float)(random.NextDouble() * Height);
            return (x, y);
        }

        public void ClearAllFood()
        {
            FoodSources.Clear();
        }

        public int GetAvailableFoodCount()
        {
            FoodSources.Count();
            return FoodSources.Count;
        }
    }
}