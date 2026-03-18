using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvolutionSimulator.Core;

namespace EcosystemSimulator
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // Initialize and run the ecosystem simulation.
            SimulationEngine engine = new SimulationEngine(1000, 1000);
            Console.WriteLine("Ecosystem Simulator starting...");
        }
    }
}
