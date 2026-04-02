using EvolutionSimulator.Core.Entities;
using EvolutionSimulator.Core.Environment;

namespace EvolutionSimulator.Core.Analytics
{
    public class MetricsManager
    {
        public PopulationManager PopulationManager { get; private set; }
        public EnvironmentManager EnvironmentManager { get; private set; }
        //metrics storage
        ///////////////////////////////////////////////////////
        public int LivingPreyCount { get; private set; }
        public int LivingPredatorCount { get; private set; }
        public int FoodCount { get; private set; }
        //Speed Averages
        public int PreySpeedAverage { get; private set; }
        public int PredatorSpeedAverage { get; private set; }
        //Size Averages
        public int PreySizeAverage { get; private set; }
        public int PredatorSizeAverage { get; private set; }
        //Stamina Averages
        public int PreyStaminaAverage { get; private set; }
        public int PredatorStaminaAverage { get; private set; }
        //Vision Distance Averages
        public int PreyVisionDistanceAverage { get; private set; }
        public int PredatorVisionDistanceAverage { get; private set; }
        //Metabolism Averages
        public int PreyMetabolismAverage { get; private set; }
        public int PredatorMetabolismAverage { get; private set; }
        ///////////////////////////////////////////////////////
        public MetricsManager(SimulationEngine simulation)
        {
            PopulationManager = simulation.PopulationManager;
            EnvironmentManager = simulation.EnvironmentManager;
            
            
        }

        public void GetMetrics(PopulationManager populationManager, EnvironmentManager environmentManager)
        {
            // Record metrics for the current simulation step, such as population counts
            LivingPreyCount = PopulationManager.GetLivingPreyCount();
            LivingPredatorCount = PopulationManager.GetLivingPredatorCount();
            FoodCount = EnvironmentManager.GetAvailableFoodCount();
            // Calculate average traits for prey and predators.
             PreySpeedAverage = (int)PopulationManager.PreyPopulation.Average(p => p.Traits.Speed);
             PredatorSpeedAverage = (int)PopulationManager.PredatorPopulation.Average(p => p.Traits.Speed);
             PreySizeAverage = (int)PopulationManager.PreyPopulation.Average(p => p.Traits.Size);
             PredatorSizeAverage = (int)PopulationManager.PredatorPopulation.Average(p => p.Traits.Size);
             PreyStaminaAverage = (int)PopulationManager.PreyPopulation.Average(p => p.Traits.Stamina);
             PredatorStaminaAverage = (int)PopulationManager.PredatorPopulation.Average(p => p.Traits.Stamina);
             PreyVisionDistanceAverage = (int)PopulationManager.PreyPopulation.Average(p => p.Traits.VisionDistance);
             PredatorVisionDistanceAverage = (int)PopulationManager.PredatorPopulation.Average(p => p.Traits.VisionDistance);
             PreyMetabolismAverage = (int)PopulationManager.PreyPopulation.Average(p => p.Traits.Metabolism);
             PredatorMetabolismAverage = (int)PopulationManager.PredatorPopulation.Average(p => p.Traits.Metabolism);
        }

        public void ExportToCsv(string filePath)
        {
            var lines = new List<string>
            {
                "Metric,Value",
                $"LivingPreyCount,{LivingPreyCount}",
                $"LivingPredatorCount,{LivingPredatorCount}",
                $"FoodCount,{FoodCount}",
                $"PreySpeedAverage,{PreySpeedAverage}",
                $"PredatorSpeedAverage,{PredatorSpeedAverage}",
                $"PreySizeAverage,{PreySizeAverage}",
                $"PredatorSizeAverage,{PredatorSizeAverage}",
                $"PreyStaminaAverage,{PreyStaminaAverage}",
                $"PredatorStaminaAverage,{PredatorStaminaAverage}",
                $"PreyVisionDistanceAverage,{PreyVisionDistanceAverage}",
                $"PredatorVisionDistanceAverage,{PredatorVisionDistanceAverage}",
                $"PreyMetabolismAverage,{PreyMetabolismAverage}",
                $"PredatorMetabolismAverage,{PredatorMetabolismAverage}"
            };

            File.WriteAllLines(filePath, lines);

        }

    }
}
