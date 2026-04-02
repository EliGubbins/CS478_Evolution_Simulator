using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using EvolutionSimulator.Core.Entities;
using EvolutionSimulator.Core.Environment;
using EvolutionSimulator.Core.Models;

namespace EvolutionSimulator.Core
{
    public class PopulationManager
    {
        private const float InitialTraitVariance = 2f;
        private static readonly Traits DefaultPreyTraits = new(6f, 2.5f, 6f, 7f, 4f);
        private static readonly Traits DefaultPredatorTraits = new(8f, 4.5f, 4f, 9f, 4f);

        public float MutationRate { get; set; }

        public List<Prey> PreyPopulation { get; private set; }
        public List<Predator> PredatorPopulation { get; private set; }

        public PopulationManager(float mutationRate = 0.5f)
        {
            // Initialize organism collections.
            PreyPopulation = [];
            PredatorPopulation = [];
            MutationRate = mutationRate;
        }

        public void Update(EnvironmentManager environmentManager, float deltaTime)
        {
            // Advance the full population by one simulation step:
            // - update prey
            // - update predators
            // - resolve deaths
            // - resolve reproduction

            UpdatePrey(environmentManager, deltaTime);

            UpdatePredators(environmentManager, deltaTime);

            RemoveDeadOrganisms();

            HandleReproduction(environmentManager);
        }

        public void UpdatePrey(EnvironmentManager environmentManager, float deltaTime)
        {
            // Update all living prey for this time step.
            foreach (Prey prey in PreyPopulation)
            {
                if (!prey.IsAlive)
                    continue;
                
                prey.Update(environmentManager, this, deltaTime);
            }
        }

        public void UpdatePredators(EnvironmentManager environmentManager, float deltaTime)
        {
            foreach (Predator predator in PredatorPopulation)
            {
                if (!predator.IsAlive)
                    continue; 

                predator.Update(environmentManager, this, deltaTime);
            }
        }

        public void RemoveDeadOrganisms()
        {
            // Remove all dead prey and predators from their collections.
            List<Prey> updatedPrey = [];
            List<Predator> updatedPredators = [];

            foreach (Prey prey in PreyPopulation)
            {
                if (prey.IsAlive)
                    updatedPrey.Add(prey);
            }

            foreach (Predator predator in PredatorPopulation)
            {
                if (predator.IsAlive)
                    updatedPredators.Add(predator);
            }

            // Reset populations after filtering out dead
            PreyPopulation = updatedPrey;
            PredatorPopulation = updatedPredators;
        }

        public void HandleReproduction(EnvironmentManager environmentManager)
        {
            List<Prey> preyOffspring = [];
            List<Predator> predatorOffspring = [];

            // Spawn offspring for prey and predators that meet reproduction conditions.
            foreach (Prey prey in PreyPopulation)
            {
                if (prey.IsAlive && prey.CanReproduce())
                    // Add new prey offspring to list
                    preyOffspring.Add(prey.CreateOffspring(MutationRate));
            }

            foreach (Predator predator in PredatorPopulation)
            {
                if (predator.IsAlive && predator.CanReproduce())
                    // Add new predator offspring to list
                    predatorOffspring.Add(predator.CreateOffspring(MutationRate));
            }

            // Add new offspring to population
            PreyPopulation.AddRange(preyOffspring);
            PredatorPopulation.AddRange(predatorOffspring);
        }

        public void AddPrey(Prey prey)
        {
            // Add a prey organism to the population.
            PreyPopulation.Add(prey);
        }

        public void AddPredator(Predator predator)
        {
            // Add a predator organism to the population.
            PredatorPopulation.Add(predator);
        }

        public int GetLivingPreyCount()
        {
            // Return the number of living prey.
            List<Prey> livingPrey = [];
            foreach (Prey prey in PreyPopulation)
                if (prey.IsAlive)
                    livingPrey.Add(prey);
            
            return livingPrey.Count;
        }

        public int GetLivingPredatorCount()
        {
            // Return the number of living predators.
            List<Predator> livingPredators = [];
            foreach (Predator predator in PredatorPopulation)
                if (predator.IsAlive)
                    livingPredators.Add(predator);
            
            return livingPredators.Count;
        }

        public List<Organism> GetAllLivingOrganisms()
        {
            // Return a combined list of all living organisms.
            List<Organism> organisms = new();
            foreach (Prey prey in PreyPopulation)
                if (prey.IsAlive)
                    organisms.Add(prey);
            
            foreach (Predator predator in PredatorPopulation)
                if (predator.IsAlive)
                    organisms.Add(predator);

            return organisms;
        }

        public void SeedInitialPopulation(
            int preyCount,
            int predatorCount,
            EnvironmentManager environmentManager,
            float preyStartingEnergy,
            float predatorStartingEnergy)
        {
            // Create the initial prey and predator populations at random positions.
            PreyPopulation.Clear();
            PredatorPopulation.Clear();

            for (int i = 0; i < preyCount; i++)
            {
                // Prey Population
                Traits preyTraits = CreateInitialTraits(DefaultPreyTraits);

                // (This is for intialization, offspring spawn near their parents)
                (float preyRandomX, float preyRandomY) = environmentManager.GetRandomPosition();

                Prey newPrey = new Prey(preyTraits, preyRandomX, preyRandomY, preyStartingEnergy);
                AddPrey(newPrey);
            }

            for (int i = 0;i < predatorCount; i++)
            {
                 // Predator Population
                Traits predatorTraits = CreateInitialTraits(DefaultPredatorTraits);

                (float predatorRandomX, float predatorRandomY) = environmentManager.GetRandomPosition();

                Predator newPredator = new Predator(predatorTraits, predatorRandomX, predatorRandomY, predatorStartingEnergy);
                AddPredator(newPredator);
            }
        }

        private static Traits CreateInitialTraits(Traits baseline)
        {
            return new Traits(
                VaryTrait(baseline.Speed),
                VaryTrait(baseline.Size),
                VaryTrait(baseline.Stamina),
                VaryTrait(baseline.VisionDistance),
                VaryTrait(baseline.Metabolism));
        }

        private static float VaryTrait(float baselineValue)
        {
            float offset = (Random.Shared.NextSingle() * 2f * InitialTraitVariance) - InitialTraitVariance;
            return MathF.Max(0.1f, baselineValue + offset);
        }
    }
}
