using EvolutionSimulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScottPlot;

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
            data = SimulationEngine.MetricsManager._rows;
        }

        public void CreateAllGraphs()
        {
            CreatePopulationCountGraph();
            CreateFoodCountGraph();
            CreatePreyTraitsGraph();
            CreatePredatorTraitsGraph();
            //CreateSpeedComparisonGraph();
            //CreateSizeComparisonGraph();
            //CreateStaminaComparisonGraph();
            //CreateVisionRadiusComparisonGraph();
            //CreateMetabolismComparisonGraph();
        }

        private List<(int step, int value)> ParseMetricColumn(int columnIndex)
        {
            var result = new List<(int, int)>();
            foreach (var row in data)
            {
                var values = row.Split(',');
                if (values.Length > columnIndex && int.TryParse(values[0], out int step) && int.TryParse(values[columnIndex], out int value))
                {
                    result.Add((step, value));
                }
            }
            return result;
        }

        public void CreatePopulationCountGraph()
        {
            var plot = new Plot();
            
            var preyData = ParseMetricColumn(1);
            var predatorData = ParseMetricColumn(2);

            var preyLine = plot.Add.ScatterLine(
                preyData.Select(x => (double)x.step).ToArray(),
                preyData.Select(x => (double)x.value).ToArray(),
                color: Color.FromHex("#22AB94"));
            preyLine.LegendText = "Prey";

            var predatorLine = plot.Add.ScatterLine(
                predatorData.Select(x => (double)x.step).ToArray(),
                predatorData.Select(x => (double)x.value).ToArray(),
                color: Color.FromHex("#FF0000"));
            predatorLine.LegendText = "Predators";

            plot.Title("Population Count Over Time");
            plot.XLabel("Step");
            plot.YLabel("Count");
            plot.ShowLegend();

            var outputDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                plot.SavePng(Path.Combine(outputDir, "population_count.png"), 800, 600);
            }
        }

        public void CreateFoodCountGraph()
        {
            var plot = new Plot();
            
            var foodData = ParseMetricColumn(3);

            var foodLine = plot.Add.ScatterLine(
                foodData.Select(x => (double)x.step).ToArray(),
                foodData.Select(x => (double)x.value).ToArray(),
                color: Color.FromHex("#FFA500"));
            foodLine.LegendText = "Food";

            plot.Title("Food Count Over Time");
            plot.XLabel("Step");
            plot.YLabel("Count");
            plot.ShowLegend();

            var outputDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                plot.SavePng(Path.Combine(outputDir, "food_count.png"), 800, 600);
            }
        }

        public void CreatePreyTraitsGraph()
        {
            var plot = new Plot();
            
            var speedData = ParseMetricColumn(4);
            var sizeData = ParseMetricColumn(6);
            var staminaData = ParseMetricColumn(8);
            var visionData = ParseMetricColumn(10);
            var metabolismData = ParseMetricColumn(12);

            var steps = speedData.Select(x => (double)x.step).ToArray();

            var speedLine = plot.Add.ScatterLine(steps, speedData.Select(x => (double)x.value).ToArray(), color: Color.FromHex("#0000FF"));
            speedLine.LegendText = "Speed";

            var sizeLine = plot.Add.ScatterLine(steps, sizeData.Select(x => (double)x.value).ToArray(), color: Color.FromHex("#22AB94"));
            sizeLine.LegendText = "Size";

            var staminaLine = plot.Add.ScatterLine(steps, staminaData.Select(x => (double)x.value).ToArray(), color: Color.FromHex("#800080"));
            staminaLine.LegendText = "Stamina";

            var visionLine = plot.Add.ScatterLine(steps, visionData.Select(x => (double)x.value).ToArray(), color: Color.FromHex("#FFA500"));
            visionLine.LegendText = "Vision Radius";

            var metabolismLine = plot.Add.ScatterLine(steps, metabolismData.Select(x => (double)x.value).ToArray(), color: Color.FromHex("#8B4513"));
            metabolismLine.LegendText = "Metabolism";

            plot.Title("Prey Traits Over Time");
            plot.XLabel("Step");
            plot.YLabel("Average Value");
            plot.ShowLegend();

            var outputDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                plot.SavePng(Path.Combine(outputDir, "prey_traits.png"), 800, 600);
            }
        }

        public void CreatePredatorTraitsGraph()
        {
            var plot = new Plot();
            
            var speedData = ParseMetricColumn(5);
            var sizeData = ParseMetricColumn(7);
            var staminaData = ParseMetricColumn(9);
            var visionData = ParseMetricColumn(11);
            var metabolismData = ParseMetricColumn(13);

            var steps = speedData.Select(x => (double)x.step).ToArray();

            var speedLine = plot.Add.ScatterLine(steps, speedData.Select(x => (double)x.value).ToArray(), color: Color.FromHex("#0000FF"));
            speedLine.LegendText = "Speed";

            var sizeLine = plot.Add.ScatterLine(steps, sizeData.Select(x => (double)x.value).ToArray(), color: Color.FromHex("#22AB94"));
            sizeLine.LegendText = "Size";

            var staminaLine = plot.Add.ScatterLine(steps, staminaData.Select(x => (double)x.value).ToArray(), color: Color.FromHex("#800080"));
            staminaLine.LegendText = "Stamina";

            var visionLine = plot.Add.ScatterLine(steps, visionData.Select(x => (double)x.value).ToArray(), color: Color.FromHex("#FFA500"));
            visionLine.LegendText = "Vision Radius";

            var metabolismLine = plot.Add.ScatterLine(steps, metabolismData.Select(x => (double)x.value).ToArray(), color: Color.FromHex("#8B4513"));
            metabolismLine.LegendText = "Metabolism";

            plot.Title("Predator Traits Over Time");
            plot.XLabel("Step");
            plot.YLabel("Average Value");
            plot.ShowLegend();

            var outputDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                plot.SavePng(Path.Combine(outputDir, "predator_traits.png"), 800, 600);
            }
        }

        public void CreateSpeedComparisonGraph()
        {
            var plot = new Plot();
            
            var preySpeed = ParseMetricColumn(4);
            var predatorSpeed = ParseMetricColumn(5);

            var preyLine = plot.Add.ScatterLine(
                preySpeed.Select(x => (double)x.step).ToArray(),
                preySpeed.Select(x => (double)x.value).ToArray(),
                color: Color.FromHex("#22AB94"));
            preyLine.LegendText = "Prey Speed";

            var predatorLine = plot.Add.ScatterLine(
                predatorSpeed.Select(x => (double)x.step).ToArray(),
                predatorSpeed.Select(x => (double)x.value).ToArray(),
                color: Color.FromHex("#FF0000"));
            predatorLine.LegendText = "Predator Speed";

            plot.Title("Speed Comparison: Prey vs Predators");
            plot.XLabel("Step");
            plot.YLabel("Average Speed");
            plot.ShowLegend();

            var outputDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                plot.SavePng(Path.Combine(outputDir, "speed_comparison.png"), 800, 600);
            }
        }

        public void CreateSizeComparisonGraph()
        {
            var plot = new Plot();
            
            var preySize = ParseMetricColumn(6);
            var predatorSize = ParseMetricColumn(7);

            var preyLine = plot.Add.ScatterLine(
                preySize.Select(x => (double)x.step).ToArray(),
                preySize.Select(x => (double)x.value).ToArray(),
                color: Color.FromHex("#22AB94"));
            preyLine.LegendText = "Prey Size";

            var predatorLine = plot.Add.ScatterLine(
                predatorSize.Select(x => (double)x.step).ToArray(),
                predatorSize.Select(x => (double)x.value).ToArray(),
                color: Color.FromHex("#FF0000"));
            predatorLine.LegendText = "Predator Size";

            plot.Title("Size Comparison: Prey vs Predators");
            plot.XLabel("Step");
            plot.YLabel("Average Size");
            plot.ShowLegend();

            var outputDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                plot.SavePng(Path.Combine(outputDir, "size_comparison.png"), 800, 600);
            }
        }

        public void CreateStaminaComparisonGraph()
        {
            var plot = new Plot();
            
            var preyStamina = ParseMetricColumn(8);
            var predatorStamina = ParseMetricColumn(9);

            var preyLine = plot.Add.ScatterLine(
                preyStamina.Select(x => (double)x.step).ToArray(),
                preyStamina.Select(x => (double)x.value).ToArray(),
                color: Color.FromHex("#22AB94"));
            preyLine.LegendText = "Prey Stamina";

            var predatorLine = plot.Add.ScatterLine(
                predatorStamina.Select(x => (double)x.step).ToArray(),
                predatorStamina.Select(x => (double)x.value).ToArray(),
                color: Color.FromHex("#FF0000"));
            predatorLine.LegendText = "Predator Stamina";

            plot.Title("Stamina Comparison: Prey vs Predators");
            plot.XLabel("Step");
            plot.YLabel("Average Stamina");
            plot.ShowLegend();

            var outputDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                plot.SavePng(Path.Combine(outputDir, "stamina_comparison.png"), 800, 600);
            }
        }

        public void CreateVisionRadiusComparisonGraph()
        {
            var plot = new Plot();
            
            var preyVision = ParseMetricColumn(10);
            var predatorVision = ParseMetricColumn(11);

            var preyLine = plot.Add.ScatterLine(
                preyVision.Select(x => (double)x.step).ToArray(),
                preyVision.Select(x => (double)x.value).ToArray(),
                color: Color.FromHex("#22AB94"));
            preyLine.LegendText = "Prey Vision";

            var predatorLine = plot.Add.ScatterLine(
                predatorVision.Select(x => (double)x.step).ToArray(),
                predatorVision.Select(x => (double)x.value).ToArray(),
                color: Color.FromHex("#FF0000"));
            predatorLine.LegendText = "Predator Vision";

            plot.Title("Vision Radius Comparison: Prey vs Predators");
            plot.XLabel("Step");
            plot.YLabel("Average Vision Radius");
            plot.ShowLegend();

            var outputDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                plot.SavePng(Path.Combine(outputDir, "vision_comparison.png"), 800, 600);
            }
        }

        public void CreateMetabolismComparisonGraph()
        {
            var plot = new Plot();
            
            var preyMetabolism = ParseMetricColumn(12);
            var predatorMetabolism = ParseMetricColumn(13);

            var preyLine = plot.Add.ScatterLine(
                preyMetabolism.Select(x => (double)x.step).ToArray(),
                preyMetabolism.Select(x => (double)x.value).ToArray(),
                color: Color.FromHex("#22AB94"));
            preyLine.LegendText = "Prey Metabolism";

            var predatorLine = plot.Add.ScatterLine(
                predatorMetabolism.Select(x => (double)x.step).ToArray(),
                predatorMetabolism.Select(x => (double)x.value).ToArray(),
                color: Color.FromHex("#FF0000"));
            predatorLine.LegendText = "Predator Metabolism";

            plot.Title("Metabolism Comparison: Prey vs Predators");
            plot.XLabel("Step");
            plot.YLabel("Average Metabolism");
            plot.ShowLegend();

            var outputDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                plot.SavePng(Path.Combine(outputDir, "metabolism_comparison.png"), 800, 600);
            }
        }
    }
}
