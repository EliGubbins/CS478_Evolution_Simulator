using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EvolutionSimulator.Core.Environment;
using EvolutionSimulator.Core.Entities;

namespace EvolutionSimulator.Core
{
    public class SimulationEngine
    {
        public EnvironmentManager EnvironmentManager { get; private set; }
        public PopulationManager PopulationManager { get; private set; }

        public int InitialPreyCount { get; private set; }
        public int InitialPredatorCount { get; private set; }
        public float PreyStartingEnergy { get; private set; }
        public float PredatorStartingEnergy { get; private set; }
        public float MutationRate { get; private set; }

        public int CurrentStep { get; private set; }
        public float ElapsedTime { get; private set; }
        public bool IsRunning { get; private set; }

        public SimulationEngine(
            float worldWidth,
            float worldHeight,
            int initialPreyCount = 10,
            int initialPredatorCount = 10,
            float preyStartingEnergy = 50f,
            float predatorStartingEnergy = 60f,
            float mutationRate = 0.5f)
        {
            // Initialize the environment manager, population manager, and engine state.
            EnvironmentManager = new EnvironmentManager(worldWidth, worldHeight);
            PopulationManager = new PopulationManager(mutationRate);

            Initialize(
                initialPreyCount,
                initialPredatorCount,
                preyStartingEnergy,
                predatorStartingEnergy,
                mutationRate);
        }

        public void Initialize(
            int initialPreyCount,
            int initialPredatorCount,
            float preyStartingEnergy,
            float predatorStartingEnergy,
            float mutationRate = 0.5f)
        {
            InitialPreyCount = initialPreyCount;
            InitialPredatorCount = initialPredatorCount;
            PreyStartingEnergy = preyStartingEnergy;
            PredatorStartingEnergy = predatorStartingEnergy;
            MutationRate = mutationRate;

            ResetSimulationState();

            PopulationManager.MutationRate = MutationRate;

            PopulationManager.SeedInitialPopulation(
                InitialPreyCount,
                InitialPredatorCount,
                EnvironmentManager,
                PreyStartingEnergy,
                PredatorStartingEnergy);
        }

        public void Step(float deltaTime)
        {
            // Advance the simulation by one step:
            // - update environment
            // - update population
            // - increment counters
            if (!IsRunning)
                return;

            EnvironmentManager.Update(deltaTime);
            PopulationManager.Update(EnvironmentManager, deltaTime);
            // Increment counters here
            CurrentStep += 1;
            ElapsedTime += deltaTime;
        }

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void Reset()
        {
            Initialize(
                InitialPreyCount,
                InitialPredatorCount,
                PreyStartingEnergy,
                PredatorStartingEnergy,
                MutationRate);
        }

        private void ResetSimulationState()
        {
            CurrentStep = 0;
            ElapsedTime = 0;
            IsRunning = false;
            EnvironmentManager.ClearAllFood();
        }
    }
}
