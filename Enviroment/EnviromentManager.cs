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
        private float foodRegenerationProgress;

        public float Width { get; private set; }
        public float Height { get; private set; }

        public List<Food> FoodSources { get; private set; }

        public float FoodRegenerationRate { get; set; }
        public int MaxFoodCount { get; set; }

        public EnvironmentManager(float width, float height)
        {
            // Initialize world dimensions, food collection, and environment settings.
            Width = width;
            Height = height;
            FoodSources = [];
            FoodRegenerationRate = 0;
            MaxFoodCount = 100;
        }

        public void Update(float deltaTime)
        {
            // Advance environment state for one simulation step.
            // This may include food regeneration and global environmental effects.
            RemoveConsumedFood();

            if (FoodRegenerationRate <= 0 || FoodSources.Count >= MaxFoodCount)
                return;

            foodRegenerationProgress += FoodRegenerationRate * deltaTime;

            while (foodRegenerationProgress >= 1f && FoodSources.Count < MaxFoodCount)
            {
                RegenerateFood();
                foodRegenerationProgress -= 1f;
            }
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
            // Remove or recycle food items that have been consumed.
            FoodSources.RemoveAll(food => food.IsConsumed);
        }

        public Food? GetNearestAvailableFood(float x, float y, float maxRange)
        {
            // Return the closest unconsumed food within a given range.
            Food? nearestFood = null;
            float nearestDistance = maxRange;

            foreach (Food food in FoodSources)
            {
                if (food.IsConsumed)
                    continue;

                float dx = food.X - x;
                float dy = food.Y - y;
                float distance = MathF.Sqrt((dx * dx) + (dy * dy));

                if (distance <= nearestDistance)
                {
                    nearestDistance = distance;
                    nearestFood = food;
                }
            }

            return nearestFood;
        }

        public List<Food> GetFoodInRange(float x, float y, float radius)
        {
            // Return all available food sources within a radius.
            List<Food> foodsInRange = [];

            foreach (Food food in FoodSources)
            {
                if (food.IsConsumed)
                    continue;

                float dx = food.X - x;
                float dy = food.Y - y;
                float distance = MathF.Sqrt((dx * dx) + (dy * dy));

                if (distance <= radius)
                    foodsInRange.Add(food);
            }

            return foodsInRange;
        }

        public bool IsInsideBounds(float x, float y)
        {
            // Return whether the given point is inside the world boundaries.
            return x >= 0 && x <= Width && y >= 0 && y <= Height;
        }

        public (float X, float Y) GetRandomPosition()
        {
            // Return a random valid position inside the environment.
            float randomX = Random.Shared.NextSingle() * Width;
            float randomY = Random.Shared.NextSingle() * Height;

            return (randomX, randomY);
        }

        public void ClearAllFood()
        {
            // Remove all food from the environment.
            FoodSources.Clear();
        }

        public int GetAvailableFoodCount()
        {
            // Return the number of food items currently available.
            return FoodSources.Count(food => !food.IsConsumed);
        }
    }
}
