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
        // Not sure this will be the best place for this longterm
        public float mutationRate = 0.5f;

        public List<Prey> PreyPopulation { get; private set; }
        public List<Predator> PredatorPopulation { get; private set; }

        public PopulationManager()
        {
            // Initialize organism collections.
            PreyPopulation = [];
            PredatorPopulation = [];
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
                    preyOffspring.Add(prey.CreateOffspring(mutationRate));
            }

            foreach (Predator predator in PredatorPopulation)
            {
                if (predator.IsAlive && predator.CanReproduce())
                    // Add new predator offspring to list
                    predatorOffspring.Add(predator.CreateOffspring(mutationRate));
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
            for (int i = 0; i < preyCount; i++)
            {
                // Prey Population

                // Need random yet reasonable traits here
                // Should store avg value for these traits as a field and new trait
                // will be a random + or - from the avg trait
                // TODO: complete traits constructor. This is a very hack way of doing this
                Traits preyTraits = new Traits
                {
                    // Need to tweak these values later, Not sure what starting balance will be best
                    // Also we need variation they should not all be the same for starting values
                    Speed = 7,
                    Size = 3,
                    Stamina = 5,
                    VisionRadius = 7,
                    Metabolism = 4
                };

                // (This is for intialization, offspring spawn near their parents)
                (float preyRandomX, float preyRandomY) = environmentManager.GetRandomPosition();

                Prey newPrey = new Prey(preyTraits, preyRandomX, preyRandomY, preyStartingEnergy);
                AddPrey(newPrey);
            }

            for (int i = 0;i < predatorCount; i++)
            {
                 // Predator Population

                Traits predatorTraits = new Traits
                {
                    Speed = 0,
                    Size = 0,
                    Stamina = 0,
                    VisionRadius = 0,
                    Metabolism = 0
                };

                (float predatorRandomX, float predatorRandomY) = environmentManager.GetRandomPosition();

                Predator newPredator = new Predator(predatorTraits, predatorRandomX, predatorRandomY, predatorStartingEnergy);
                AddPredator(newPredator);
            }
        }
    }
}