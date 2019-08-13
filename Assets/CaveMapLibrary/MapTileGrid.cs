using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.CaveMapLibrary
{
    /// <summary>
    /// A 2D matrix of MapTiles that covers the entire area of a cave map
    /// </summary>
    public class MapTileGrid :MapTileGridBase
    {
        /// <summary>
        /// Creates A 2D matrix of MapTiles that covers the entire area of the cave map
        /// </summary>
        /// <param name="map"></param>
        /// <param name="tileSize"></param>
        public MapTileGrid(TileType[,] map, float tileSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * tileSize;
            float mapHeight = nodeCountY * tileSize;

            TileControlNode[,] controlNodes = new TileControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * tileSize + tileSize / 2, 0, -mapHeight / 2 + y * tileSize + tileSize / 2);
                    controlNodes[x, y] = new TileControlNode(pos, map[x, y] == TileType.Wall, tileSize);
                }
            }

            Tiles = new MapTile[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    Tiles[x, y] = new MapTile(map[x,y], controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }

        }
    }
}
