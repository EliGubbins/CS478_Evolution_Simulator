using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using EvolutionSimulator.Core.Environment;
using EvolutionSimulator.Core.Models;
using EvolutionSimulator.Core.Entities;

namespace EvolutionSimulator.Core.Entities
{
    public class Prey : Organism
    {
        public float ReproductionCooldown { get; private set; }

        public Prey(Traits traits, float startX, float startY, float startingEnergy)
        {
            // Initialize prey-specific state and inherited organism properties.
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
            // Run one prey update step:
            // - detect predators
            // - search for food
            // - decide movement
            // - consume energy
            // - age
            // - attempt reproduction

            if (!IsAlive)
                return;

            if (ReproductionCooldown > 0)
                ReproductionCooldown -= deltaTime;

            Predator? predator = FindNearestPredator(populationManager);
            Food? food = FindNearestFood(environmentManager);

            DecideMovement(environmentManager, populationManager);

            TryEatFood(environmentManager);

            base.Update(environmentManager, populationManager, deltaTime);
        }

        public void DecideMovement(EnvironmentManager environmentManager, PopulationManager populationManager)
        {
            // Choose whether to flee, forage, or wander based on nearby threats and food.
            Predator? predator = FindNearestPredator(populationManager);

            if (predator != null)
            {
                FleeFromPredator(predator);
                return;
            }

            Food? food = FindNearestFood(environmentManager);

            if (food != null)
            {
                SeekFood(food);
                return;
            }

            Wander();
        }

        public void FleeFromPredator(Predator predator)
        {
            // Set movement direction away from the predator.
            float dx = X - predator.X;
            float dy = predator.Y;

            SetDirection(dx, dy);
        }

        public void SeekFood(Food food)
        {
            // Set movement direction toward a food source.
            float dx = food.X - X;
            float dy = food.Y - Y;

            SetDirection(dx, dy);
        }

        public void Wander()
        {
            // Set a semi-random movement direction when no immediate stimulus exists.
            float dx = Random.Shared.NextSingle() * 2 - 1;
            float dy = Random.Shared.NextSingle() * 2 - 1;

            SetDirection(dx, dy);
        }

        public void TryEatFood(EnvironmentManager environmentManager)
        {
            // Consume nearby food and increase energy if food is reachable.
            Food? food = FindNearestFood(environmentManager);

            if (food == null)
                return;

            float distance = DistanceTo(food.X, food.Y);

            // TODO: determine how distance should be calculated here
            // When should the prey go to the food or not, and what traits should determine that
            // Size is just a place holder comparison for the time being
            if (distance < Traits.Size)
            {
                Energy += food.NutritionValue;

                food.IsConsumed = true;
            }
        }

        public override bool CanReproduce()
        {
            // Return whether this prey has enough energy and meets other reproduction rules.
            if (!IsAlive)
                return false;

            if (Energy < 60)
                return false;
            
            if (ReproductionCooldown > 0)
                return false;
            
            return true;
        }

        public Prey CreateOffspring(float mutationRate)
        {
            // Create a new prey instance with inherited and possibly mutated traits.

            // Copy traits from parent
            Traits childTraits = Traits.Clone();

            // Apply mutation
            childTraits.Mutate(mutationRate);
            
            // Spawn near parent
            float offsetX = Random.Shared.NextSingle() * 2 - 1;
            float offsetY = Random.Shared.NextSingle() * 2 - 1;

            float childX = X + offsetX;
            float childY = Y + offsetY;

            float childEnergy = Energy * 0.25f;

            // Reduce Parent Energy
            Energy *= 0.75f;

            // Reset reproduction cooldown
            ReproductionCooldown = 10;

            return new Prey(childTraits, childX, childY, childEnergy);
        }

        public Predator? FindNearestPredator(PopulationManager populationManager)
        {
            // Return the nearest predator within relevant sensing range.
            Predator? closest = null;
            float closestDistance = float.MaxValue;

            foreach (var predator in populationManager.PredatorPopulation)
            {
                if (!predator.IsAlive)
                    continue;
                
                float distance = DistanceTo(predator);

                if (distance < closestDistance && distance <= Traits.VisionRadius)
                {
                    closest = predator;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        public Food? FindNearestFood(EnvironmentManager environmentManager)
        {
            // Return the nearest available food source within relevant sensing range.
            Food? closest = null;
            float closestDistance = float.MaxValue;

            foreach (var food in environmentManager.FoodSources)
            {
                if (food.IsConsumed)
                    continue;
                
                float distance = DistanceTo(food.X, food.Y);

                if (distance < closestDistance && distance <= Traits.VisionRadius)
                {
                    closest = food;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        public float CalculateEscapeChance(Predator predator)
        {
            // Compute probability of escaping a predator based on prey and predator traits.

            // Compare relative speeds
            float speedDifference = Traits.Speed - predator.Traits.Speed;

            // Stamina makes a slight contribution
            float staminaFactor = Traits.Stamina * 0.1f;

            //Larger prey might be slightly worse at escaping
            float sizePenalty = Traits.Size * 0.05f;

            float score = speedDifference + staminaFactor - sizePenalty;

            // Convert score into probability
            float probability = 0.5f + (score * 0.1f);
            
            // Clamp the result
            probability = Math.Clamp(probability, 0.1f, 0.9f);

            return probability;
        }
    }
}