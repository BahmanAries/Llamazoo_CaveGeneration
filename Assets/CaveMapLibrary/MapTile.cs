namespace Assets.CaveMapLibrary
{
    /// <summary>
    /// Demonstrates a square with 4 nodes in the corners as control nodes and 4 nodes on the sides as normal nodes
    /// </summary>
    public class MapTile : MapTileBase
    {
        public TileControlNode TopLeft { get; private set; }
        public TileControlNode TopRight { get; private set; }
        public TileControlNode BottomLeft { get; private set; }
        public TileControlNode BottomRight { get; private set; }

        public TileNode CentreTop { get; private set; }
        public TileNode CentreRight { get; private set; }
        public TileNode CentreBottom { get; private set; }
        public TileNode CentreLeft { get; private set; }



        public MapTile(TileType type ,TileControlNode topLeft, TileControlNode topRight, TileControlNode bottomRight, TileControlNode bottomLeft)
        {
            Type = type;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;

            CentreTop = topLeft.Right;
            CentreRight = bottomRight.Above;
            CentreBottom = bottomLeft.Right;
            CentreLeft = bottomLeft.Above;

            if (TopLeft.IsActive)
                State += 8;
            if (TopRight.IsActive)
                State += 4;
            if (BottomRight.IsActive)
                State += 2;
            if (BottomLeft.IsActive)
                State += 1;
        }
    }
}
