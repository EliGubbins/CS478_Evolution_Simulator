using EvolutionSimulator.Core.Entities;
using EvolutionSimulator.Core.Environment;

namespace EvolutionSimulator.Core.Analytics
{
    public class MetricsManager
    {
        public string path { get; private set; }
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
        //Vision Radius Averages
        public int PreyVisionRadiusAverage { get; private set; }
        public int PredatorVisionRadiusAverage { get; private set; }
        //Metabolism Averages
        public int PreyMetabolismAverage { get; private set; }
        public int PredatorMetabolismAverage { get; private set; }
        ///////////////////////////////////////////////////////
        private int _step;
        public readonly List<string> _rows = new();

        public MetricsManager(SimulationEngine simulation)
        {
            PopulationManager = simulation.PopulationManager;
            EnvironmentManager = simulation.EnvironmentManager;

            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            string outputDir = Path.Combine(projectDir, "output");
            string reportsDir = Path.Combine(outputDir, $"evolution_report_{timeStamp}");
            Directory.CreateDirectory(reportsDir);

            path = Path.Combine(reportsDir, "metrics.csv");

            _step = 0;
        }

        public void GetMetrics(PopulationManager populationManager, EnvironmentManager environmentManager)
        {
            _step++;

            // Record metrics for the current simulation step, such as population counts
            LivingPreyCount = PopulationManager.GetLivingPreyCount();
            LivingPredatorCount = PopulationManager.GetLivingPredatorCount();
            FoodCount = EnvironmentManager.GetAvailableFoodCount();
            // Calculate average traits for prey and predators.
             PreySpeedAverage = SafeAverage(PopulationManager.PreyPopulation, p => p.Traits.Speed);
             PredatorSpeedAverage = SafeAverage(PopulationManager.PredatorPopulation, p => p.Traits.Speed);
             PreySizeAverage = SafeAverage(PopulationManager.PreyPopulation, p => p.Traits.Size);
             PredatorSizeAverage = SafeAverage(PopulationManager.PredatorPopulation, p => p.Traits.Size);
             PreyStaminaAverage = SafeAverage(PopulationManager.PreyPopulation, p => p.Traits.Stamina);
             PredatorStaminaAverage = SafeAverage(PopulationManager.PredatorPopulation, p => p.Traits.Stamina);
             PreyVisionRadiusAverage = SafeAverage(PopulationManager.PreyPopulation, p => p.Traits.VisionRadius);
             PredatorVisionRadiusAverage = SafeAverage(PopulationManager.PredatorPopulation, p => p.Traits.VisionRadius);
             PreyMetabolismAverage = SafeAverage(PopulationManager.PreyPopulation, p => p.Traits.Metabolism);
             PredatorMetabolismAverage = SafeAverage(PopulationManager.PredatorPopulation, p => p.Traits.Metabolism);

            // Buffer this step's data in memory
            _rows.Add(string.Join(",",
                _step,
                LivingPreyCount, LivingPredatorCount, FoodCount,
                PreySpeedAverage, PredatorSpeedAverage,
                PreySizeAverage, PredatorSizeAverage,
                PreyStaminaAverage, PredatorStaminaAverage,
                PreyVisionRadiusAverage, PredatorVisionRadiusAverage,
                PreyMetabolismAverage, PredatorMetabolismAverage));
        }

        public void ExportToCsv()
        {
            string header =
                "Step,LivingPreyCount,LivingPredatorCount,FoodCount," +
                "PreySpeedAvg,PredatorSpeedAvg," +
                "PreySizeAvg,PredatorSizeAvg," +
                "PreyStaminaAvg,PredatorStaminaAvg," +
                "PreyVisionRadiusAvg,PredatorVisionRadiusAvg," +
                "PreyMetabolismAvg,PredatorMetabolismAvg";

            // Write header + all buffered rows in a single I/O operation
            using var writer = new StreamWriter(path, false);
            writer.WriteLine(header);
            foreach (var row in _rows)
            {
                writer.WriteLine(row);
            }

            Console.WriteLine("Metrics data exported to " + path);
        }

        private static int SafeAverage<T>(IEnumerable<T> source, Func<T, float> selector)
            => source.Any() ? (int)source.Average(selector) : 0;
    }
}