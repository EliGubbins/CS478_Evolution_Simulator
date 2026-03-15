using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EvolutionSimulator.Core.Environment;

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
        }

        public void Initialize(
            int initialPreyCount,
            int initialPredatorCount,
            float preyStartingEnergy,
            float predatorStartingEnergy)
        {
            // Set up the first simulation state and seed the world.
        }

        public void Step(float deltaTime)
        {
            // Advance the simulation by one step:
            // - update environment
            // - update population
            // - increment counters
        }

        public void Start()
        {
            // Mark the simulation as running.
        }

        public void Stop()
        {
            // Mark the simulation as stopped.
        }

        public void Reset()
        {
            // Reset the simulation to a clean starting state.
        }
    }
}