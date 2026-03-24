using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvolutionSimulator.Core;
using EvolutionSimulator.Core.Analytics;

namespace EcosystemSimulator
{
    internal class Program
    {

        public static void Main(string[] args)
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filePath = Path.Combine("output", $"evolution_report_{timeStamp}.csv");

            {
                if (int.TryParse(args[0], out int number))
                {
                    Console.WriteLine($"Running simulation with {number} steps...");
                    SimulationEngine engine = new SimulationEngine();
                    engine.Start();
                    engine.Step(number);
                    Console.WriteLine("Simulation completed.");
                    MetricsManager metricsManager = new MetricsManager(engine);
                    metricsManager.GetMetrics(engine.PopulationManager, engine.EnvironmentManager);
                    metricsManager.ExportToCsv(filePath);
                    Console.WriteLine("Metrics data exported to " + filePath);

                }
                else
                {
                    Console.WriteLine("Please provide a valid number of steps as an argument.");
                }

            }
        }
    }
}
