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

        public int CurrentStep { get; private set; }
        public float ElapsedTime { get; private set; }
        public bool IsRunning { get; private set; }

        public SimulationEngine(float worldWidth, float worldHeight)
        {
            // Initialize the environment manager, population manager, and engine state.
            EnvironmentManager = new EnvironmentManager(worldWidth, worldHeight);
            // population manager initialization
            Initialize(10, 10, 50, 100);
            CurrentStep = 0;
            ElapsedTime = 0;

        }

        public void Initialize(
            int initialPreyCount,
            int initialPredatorCount,
            float preyStartingEnergy,
            float predatorStartingEnergy)
        {
            //add entities to population manager
            PopulationManager = new PopulationManager();

            PopulationManager.SeedInitialPopulation(
                initialPreyCount,
                initialPredatorCount,
                EnvironmentManager,
                preyStartingEnergy,
                predatorStartingEnergy);
        }

        public void Step(float deltaTime)
        {
            // Advance the simulation by one step:
            // - update environment
            // - update population
            // - increment counters
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
            SimulationEngine newEngine = new SimulationEngine(EnvironmentManager.Width, EnvironmentManager.Height);
        }
    }
}