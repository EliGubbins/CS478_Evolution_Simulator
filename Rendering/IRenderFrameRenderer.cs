namespace EvolutionSimulator.Core.Rendering
{
    public interface IRenderFrameRenderer
    {
        void Initialize(WorldRenderViewport viewport, SimulationRenderPalette palette);
        void Render(SimulationRenderFrame frame, WorldRenderViewport viewport);
        void Resize(WorldRenderViewport viewport);
    }
}
