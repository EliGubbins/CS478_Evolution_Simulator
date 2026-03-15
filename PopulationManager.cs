using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EvolutionSimulator.Core.Entities;
using EvolutionSimulator.Core.Environment;

namespace EvolutionSimulator.Core
{
    public class PopulationManager
    {
        public List<Prey> PreyPopulation { get; private set; }
        public List<Predator> PredatorPopulation { get; private set; }

        public PopulationManager()
        {
            // Initialize organism collections.
        }

        public void Update(EnvironmentManager environmentManager, float deltaTime)
        {
            // Advance the full population by one simulation step:
            // - update prey
            // - update predators
            // - resolve deaths
            // - resolve reproduction
        }

        public void UpdatePrey(EnvironmentManager environmentManager, float deltaTime)
        {
            // Update all living prey for this time step.
        }

        public void UpdatePredators(EnvironmentManager environmentManager, float deltaTime)
        {
            // Update all living predators for this time step.
        }

        public void RemoveDeadOrganisms()
        {
            // Remove all dead prey and predators from their collections.
        }

        public void HandleReproduction(EnvironmentManager environmentManager)
        {
            // Spawn offspring for prey and predators that meet reproduction conditions.
        }

        public void AddPrey(Prey prey)
        {
            // Add a prey organism to the population.
        }

        public void AddPredator(Predator predator)
        {
            // Add a predator organism to the population.
        }

        public int GetLivingPreyCount()
        {
            // Return the number of living prey.
            throw new NotImplementedException();
        }

        public int GetLivingPredatorCount()
        {
            // Return the number of living predators.
            throw new NotImplementedException();
        }

        public List<Organism> GetAllLivingOrganisms()
        {
            // Return a combined list of all living organisms.
            throw new NotImplementedException();
        }

        public void SeedInitialPopulation(
            int preyCount,
            int predatorCount,
            EnvironmentManager environmentManager,
            float preyStartingEnergy,
            float predatorStartingEnergy)
        {
            // Create the initial prey and predator populations at random positions.
        }
    }
}