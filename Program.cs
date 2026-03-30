using EcosystemSimulator.Analytics;
using EvolutionSimulator.Core.Analytics;
using System.Net.NetworkInformation;

namespace EvolutionSimulator.Core
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Evolution Simulator");
            Console.WriteLine("-------------------");

            SimulationSettings settings = PromptForSettings();

            SimulationEngine engine = new(
                settings.WorldWidth,
                settings.WorldHeight,
                settings.InitialPreyCount,
                settings.InitialPredatorCount,
                settings.PreyStartingEnergy,
                settings.PredatorStartingEnergy,
                settings.MutationRate);

            engine.EnvironmentManager.MaxFoodCount = settings.MaxFoodCount;
            engine.EnvironmentManager.DefaultFoodNutritionValue = settings.FoodNutritionValue;
            engine.EnvironmentManager.FoodRegenerationRate = settings.FoodRegenerationRate;
            engine.EnvironmentManager.SeedInitialFood(settings.InitialFoodCount);

            Console.WriteLine();
            Console.WriteLine("Starting simulation...");
            PrintSummary(engine);

            engine.Start();

            while (engine.IsRunning && engine.CurrentStep < settings.MaxSteps)
            {
                engine.Step(settings.DeltaTime);

                if (ShouldPrintStep(engine.CurrentStep, settings.OutputInterval))
                    PrintSummary(engine);

                if (engine.PopulationManager.GetLivingPreyCount() == 0 || engine.PopulationManager.GetLivingPredatorCount() == 0)
                {
                    engine.Stop();
                }
            }

            Console.WriteLine();
            Console.WriteLine("Simulation complete.");

            if (engine.PopulationManager.GetLivingPreyCount() == 0)
                Console.WriteLine("Reason: prey population reached zero.");
            else if (engine.PopulationManager.GetLivingPredatorCount() == 0)
                Console.WriteLine("Reason: predator population reached zero.");
            else if (engine.CurrentStep >= settings.MaxSteps)
                Console.WriteLine("Reason: reached configured step limit.");

            //results 
            PrintSummary(engine);
            engine.MetricsManager.ExportToCsv();
            var graphs = new Graphs(engine);
            graphs.CreateAllGraphs();
        }

        private static SimulationSettings PromptForSettings()
        {
            Console.WriteLine("Press Enter to accept the default shown in brackets.");
            Console.WriteLine();

            return new SimulationSettings(
                WorldWidth: ReadFloat("World width", 100f),
                WorldHeight: ReadFloat("World height", 100f),
                InitialPreyCount: ReadInt("Initial prey count", 25),
                InitialPredatorCount: ReadInt("Initial predator count", 8),
                PreyStartingEnergy: ReadFloat("Prey starting energy", 50f),
                PredatorStartingEnergy: ReadFloat("Predator starting energy", 60f),
                MutationRate: ReadFloat("Mutation rate", 0.1f),
                InitialFoodCount: ReadInt("Initial food count", 60),
                FoodRegenerationRate: ReadFloat("Food regeneration rate", 3f),
                FoodNutritionValue: ReadFloat("Food nutrition value", 10f),
                MaxFoodCount: ReadInt("Maximum food count", 150),
                DeltaTime: ReadFloat("Delta time per step", 1f),
                MaxSteps: ReadInt("Maximum steps", 100),
                OutputInterval: ReadInt("Output every N steps", 5));
        }

        private static int ReadInt(string label, int defaultValue)
        {
            while (true)
            {
                Console.Write($"{label} [{defaultValue}]: ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    return defaultValue;

                if (int.TryParse(input, out int value) && value >= 0)
                    return value;

                Console.WriteLine("Please enter a non-negative whole number.");
            }
        }

        private static float ReadFloat(string label, float defaultValue)
        {
            while (true)
            {
                Console.Write($"{label} [{defaultValue}]: ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    return defaultValue;

                if (float.TryParse(input, out float value) && value >= 0)
                    return value;

                Console.WriteLine("Please enter a non-negative number.");
            }
        }

        private static bool ShouldPrintStep(int currentStep, int outputInterval)
        {
            if (currentStep <= 0)
                return false;

            if (outputInterval <= 0)
                return true;

            return currentStep % outputInterval == 0;
        }

        private static void PrintSummary(SimulationEngine engine)
        {
            int livingPrey = engine.PopulationManager.GetLivingPreyCount();
            int livingPredators = engine.PopulationManager.GetLivingPredatorCount();
            int availableFood = engine.EnvironmentManager.GetAvailableFoodCount();

            Console.WriteLine(
                $"Step {engine.CurrentStep,4} | " +
                $"Time {engine.ElapsedTime,6:0.0} | " +
                $"Prey {livingPrey,4} | " +
                $"Predators {livingPredators,4} | " +
                $"Food {availableFood,4}");
        }

        private sealed record SimulationSettings(
            float WorldWidth,
            float WorldHeight,
            int InitialPreyCount,
            int InitialPredatorCount,
            float PreyStartingEnergy,
            float PredatorStartingEnergy,
            float MutationRate,
            int InitialFoodCount,
            float FoodRegenerationRate,
            float FoodNutritionValue,
            int MaxFoodCount,
            float DeltaTime,
            int MaxSteps,
            int OutputInterval);


    }
}
