namespace EvolutionSimulator.Core.Models
{
    public enum TerrainType
    {
        OpenGround,
        Forest,
        Water,
        Rocky
    }

    public class TerrainRegion
    {
        public Guid Id { get; set; }
        public TerrainType TerrainType { get; set; }
        public float CenterX { get; set; }
        public float CenterY { get; set; }
        public float Radius { get; set; }

        public TerrainRegion(TerrainType terrainType, float centerX, float centerY, float radius)
        {
            Id = Guid.NewGuid();
            TerrainType = terrainType;
            CenterX = centerX;
            CenterY = centerY;
            Radius = radius;
        }
    }
}
