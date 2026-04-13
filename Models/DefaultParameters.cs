using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace EcosystemSimulator.Models
{
    public class DefaultParameters
    {
        public float WorldWidth { get; set; } = 100f;
        public float WorldHeight { get; set; } = 100f;
        public int InitialPreyCount { get; set; } = 25;
        public int InitialPredatorCount { get; set; } = 8;
        public float PreyStartingEnergy { get; set; } = 50f;
        public float PredatorStartingEnergy { get; set; } = 60f;
        public float MutationRate { get; set; } = 0.1f;
        public int InitialFoodCount { get; set; } = 60;
        public float FoodRegenerationRate { get; set; } = 3f;
        public float FoodNutritionValue { get; set; } = 10f;
        public int MaxFoodCount { get; set; } = 150;
        public float DeltaTime { get; set; } = 1f;
        public int MaxSteps { get; set; } = 100;
        public int OutputInterval { get; set; } = 5;

        // Initial trait variance applied when seeding populations
        public float InitialTraitVariance { get; set; } = 2f;

        // Default prey traits
        public float PreySpeed { get; set; } = 6f;
        public float PreySize { get; set; } = 2.5f;
        public float PreyStamina { get; set; } = 6f;
        public float PreyVisionDistance { get; set; } = 7f;
        public float PreyMetabolism { get; set; } = 4f;

        // Default predator traits
        public float PredatorSpeed { get; set; } = 8f;
        public float PredatorSize { get; set; } = 4.5f;
        public float PredatorStamina { get; set; } = 4f;
        public float PredatorVisionDistance { get; set; } = 9f;
        public float PredatorMetabolism { get; set; } = 4f;

        public static DefaultParameters LoadFromFile(string filePath)
        {
            var parameters = new DefaultParameters();

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[DEBUG] File not found: {filePath}");
                Console.WriteLine($"[DEBUG] Current directory: {Directory.GetCurrentDirectory()}");
                return parameters;
            }

            Console.WriteLine($"[DEBUG] Loading from: {filePath}");

            try
            {
                var lines = File.ReadAllLines(filePath);
                Console.WriteLine($"[DEBUG] Read {lines.Length} lines from file");

                foreach (var line in lines)
                {
                    // Skip empty lines, section headers, and comments
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("[") || line.TrimStart().StartsWith(";"))
                        continue;

                    var parts = line.Split('=');
                    if (parts.Length != 2)
                    {
                        Console.WriteLine($"[DEBUG] Skipping malformed line: {line}");
                        continue;
                    }

                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    Console.WriteLine($"[DEBUG] Parsing: {key} = {value}");

                    switch (key)
                    {
                        case "WorldWidth":
                            if (float.TryParse(value, out var worldWidth))
                                parameters.WorldWidth = worldWidth;
                            break;
                        case "WorldHeight":
                            if (float.TryParse(value, out var worldHeight))
                                parameters.WorldHeight = worldHeight;
                            break;
                        case "InitialPreyCount":
                            if (int.TryParse(value, out var initialPreyCount))
                                parameters.InitialPreyCount = initialPreyCount;
                            break;
                        case "InitialPredatorCount":
                            if (int.TryParse(value, out var initialPredatorCount))
                                parameters.InitialPredatorCount = initialPredatorCount;
                            break;
                        case "PreyStartingEnergy":
                            if (float.TryParse(value, out var preyStartingEnergy))
                                parameters.PreyStartingEnergy = preyStartingEnergy;
                            break;
                        case "PredatorStartingEnergy":
                            if (float.TryParse(value, out var predatorStartingEnergy))
                                parameters.PredatorStartingEnergy = predatorStartingEnergy;
                            break;
                        case "MutationRate":
                            if (float.TryParse(value, out var mutationRate))
                                parameters.MutationRate = mutationRate;
                            break;
                        case "InitialFoodCount":
                            if (int.TryParse(value, out var initialFoodCount))
                                parameters.InitialFoodCount = initialFoodCount;
                            break;
                        case "FoodRegenerationRate":
                            if (float.TryParse(value, out var foodRegenerationRate))
                                parameters.FoodRegenerationRate = foodRegenerationRate;
                            break;
                        case "FoodNutritionValue":
                            if (float.TryParse(value, out var foodNutritionValue))
                                parameters.FoodNutritionValue = foodNutritionValue;
                            break;
                        case "MaxFoodCount":
                            if (int.TryParse(value, out var maxFoodCount))
                                parameters.MaxFoodCount = maxFoodCount;
                            break;
                        case "DeltaTime":
                            if (float.TryParse(value, out var deltaTime))
                                parameters.DeltaTime = deltaTime;
                            break;
                        case "MaxSteps":
                            if (int.TryParse(value, out var maxSteps))
                                parameters.MaxSteps = maxSteps;
                            break;
                        case "OutputInterval":
                            if (int.TryParse(value, out var outputInterval))
                                parameters.OutputInterval = outputInterval;
                            break;
                        case "InitialTraitVariance":
                            if (float.TryParse(value, out var traitVariance))
                                parameters.InitialTraitVariance = traitVariance;
                            break;
                        case "PreySpeed":
                            if (float.TryParse(value, out var preySpeed))
                                parameters.PreySpeed = preySpeed;
                            break;
                        case "PreySize":
                            if (float.TryParse(value, out var preySize))
                                parameters.PreySize = preySize;
                            break;
                        case "PreyStamina":
                            if (float.TryParse(value, out var preyStamina))
                                parameters.PreyStamina = preyStamina;
                            break;
                        case "PreyVisionDistance":
                            if (float.TryParse(value, out var preyVision))
                                parameters.PreyVisionDistance = preyVision;
                            break;
                        case "PreyMetabolism":
                            if (float.TryParse(value, out var preyMetabolism))
                                parameters.PreyMetabolism = preyMetabolism;
                            break;
                        case "PredatorSpeed":
                            if (float.TryParse(value, out var predatorSpeed))
                                parameters.PredatorSpeed = predatorSpeed;
                            break;
                        case "PredatorSize":
                            if (float.TryParse(value, out var predatorSize))
                                parameters.PredatorSize = predatorSize;
                            break;
                        case "PredatorStamina":
                            if (float.TryParse(value, out var predatorStamina))
                                parameters.PredatorStamina = predatorStamina;
                            break;
                        case "PredatorVisionDistance":
                            if (float.TryParse(value, out var predatorVision))
                                parameters.PredatorVisionDistance = predatorVision;
                            break;
                        case "PredatorMetabolism":
                            if (float.TryParse(value, out var predatorMetabolism))
                                parameters.PredatorMetabolism = predatorMetabolism;
                            break;
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[DEBUG] IO Error reading file: {ex.Message}");

            }

            Console.WriteLine($"[DEBUG] Loading complete. WorldWidth={parameters.WorldWidth}");
            return parameters;
        }
    }
}