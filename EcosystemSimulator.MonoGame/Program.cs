namespace EvolutionSimulator.MonoGameHost
{
    internal static class Program
    {
        private static void Main()
        {
            using SimulationGame game = new();
            game.Run();
        }
    }
}
