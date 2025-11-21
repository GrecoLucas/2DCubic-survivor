using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.World.Biomes
{
    public class Biome
    {
        public BiomeType Type { get; }
        public Rectangle Area { get; }
        public Texture2D Texture { get; }
        public bool AllowsEnemySpawns { get; }
        public int TreeDensity { get; }
        public int GoldDensity { get; }

        public Biome(BiomeType type, Rectangle area, Texture2D texture, bool allowsEnemySpawns, int treeDensity, int goldDensity)
        {
            Type = type;
            Area = area;
            Texture = texture;
            AllowsEnemySpawns = allowsEnemySpawns;
            TreeDensity = treeDensity;
            GoldDensity = goldDensity;
        }

        public bool Contains(Vector2 position)
        {
            return Area.Contains((int)position.X, (int)position.Y);
        }
    }
}
