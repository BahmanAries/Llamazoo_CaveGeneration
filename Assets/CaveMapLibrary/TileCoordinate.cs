namespace Assets.CaveMapLibrary
{
    /// <summary>
    /// Represents Tile coordinates inside the map
    /// </summary>
    public struct TileCoordinate
    {
        public int TileX { get; private set; }
        public int TileY { get; private set; }

        public TileCoordinate(int x, int y)
        {
            TileX = x;
            TileY = y;
        }
    }
}
