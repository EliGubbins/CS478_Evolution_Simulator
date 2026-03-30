using EvolutionSimulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcosystemSimulator.Analytics
{
    internal class Graphs
    {
        public string filePath {  get; set; }
        public SimulationEngine SimulationEngine { get; set;}
        public List<string> data { get; set; }

        public Graphs(SimulationEngine engine) 
        {
            SimulationEngine = engine;
            filePath = SimulationEngine.MetricsManager.path;
        }

        public void CreateAllGraphs()
        {

        }
    }
}
