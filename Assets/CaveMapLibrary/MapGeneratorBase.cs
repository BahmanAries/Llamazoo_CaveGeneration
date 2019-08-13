
namespace Assets.CaveMapLibrary
{
    public abstract class MapGeneratorBase
    {

        protected int _mapWidth;
        protected int _mapHeight;

        /// <summary>
        /// A 2D grid that shows the coordinate and state of each cell
        /// </summary>
        protected TileType[,] _map;

        /// <summary>
        /// Generates a cave by applying the cellular automaton  model to a 2D grid
        /// </summary>
        /// <param name="width">Map width</param>
        /// <param name="height">Map height</param>
        /// <param name="wallDensity">An integer between 0 and 100 determining wall to free-space ratio</param>
        /// <param name="caveMap">Reference to the CaveMap GameObject</param>
        public abstract void GenerateMap(int width, int height, int wallDensity, CaveMapController caveMap);

        /// <summary>
        /// Generates a cave by applying the cellular automaton  model to a 2D grid
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public virtual void GenerateMap(int width, int height)
        {
            _mapWidth = width;
            _mapHeight = height;
        }
    }
}
