using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.CaveMapLibrary;

public class MapGenerator
{
    private const int randomMin = 0;
    private const int randomMax = 100;
    private const int smoothingIterations = 5;
    private const int borderSize = 1;

    /// <summary>
    /// A 2D grid that shows the coordinate and state of each cell
    /// </summary>
    private TileType[,] _map;
    private int _mapWidth;
    private int _mapHeight;
    private int _wallDensity;
    private int _grassDensity;
    private CaveMapController _caveMap;

    /// <summary>
    /// Generates a cave by applying the cellular automaton  model to a 2D grid
    /// </summary>
    /// <param name="width">Map width</param>
    /// <param name="height">Map height</param>
    /// <param name="wallDensity">An integer between 0 and 100 determining wall to free-space ratio</param>
    /// <param name="caveMap">Reference to the CaveMap GameObject</param>
    public virtual void GenerateMap(int width, int height, int wallDensity, CaveMapController caveMap)
    {
        _caveMap = caveMap;
        _mapWidth = width;
        _mapHeight = height;
        _wallDensity = wallDensity < 0 ? 0 : (wallDensity > 100 ? 100 : wallDensity);
        _grassDensity = wallDensity + ((randomMax - wallDensity) / 3);

        _map = new TileType[_mapWidth, _mapHeight];
        FillMap();
        SmoothMap();
        RemoveIsolations();

        var borderedMap = CreateMapBorder();
        MeshGenerator meshGen = new MeshGenerator(caveMap);
        meshGen.GenerateMesh(borderedMap, 1);
    }
    /// <summary>
    /// randomly fills the map with different tile types
    /// </summary>
    protected virtual void FillMap()
    {
        System.Random random = new System.Random(Guid.NewGuid().GetHashCode());

        for (int x = 0; x < _mapWidth; x++)
        {
            for (int y = 0; y < _mapHeight; y++)
            {
                if (x == 0 || x == _mapWidth - 1 || y == 0 || y == _mapHeight - 1)
                {
                    _map[x, y] = TileType.Wall;
                }
                else
                {
                    var rand = random.Next(randomMin, randomMax);
                    _map[x, y] = rand < _wallDensity ? TileType.Wall : (rand >= _grassDensity ? TileType.Grass : TileType.Floor);
                }
            }
        }
    }
    /// <summary>
    /// Smoothes the map by putting together similar tile types
    /// </summary>
    protected virtual void SmoothMap()
    {
        for (int i = 0; i < smoothingIterations; i++)
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    int neighbourFloorTiles = GetNeighbourTypeCount(TileType.Floor,x, y);
                    int neighbourGrassTiles = GetNeighbourTypeCount(TileType.Grass,x, y);
                    int neighbourWallTiles = GetNeighbourTypeCount(TileType.Wall,x, y);

                    if (neighbourWallTiles > 4)
                        _map[x, y] = TileType.Wall;
                    else if (neighbourGrassTiles > 4)
                        _map[x, y] = TileType.Grass;
                    else if (neighbourFloorTiles > 4)
                        _map[x, y] = TileType.Floor;


                }
            }
        }
    }
    /// <summary>
    /// returns the number of wall tiles around one specific tile
    /// </summary>
    /// <param name="gridX"></param>
    /// <param name="gridY"></param>
    /// <returns></returns>
    int GetNeighbourTypeCount(TileType type, int gridX, int gridY)
    {
        int typeCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < _mapWidth && neighbourY >= 0 && neighbourY < _mapHeight)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        typeCount += _map[neighbourX, neighbourY] == type ? 1 : 0;
                    }
                }
                else
                {
                    typeCount += type == TileType.Wall ? 1 : 0;
                }
            }
        }

        return typeCount;
    }
    /// <summary>
    /// Removes single island rooms from the map
    /// </summary>
    protected virtual void RemoveIsolations()
    {
        List<List<TileCoordinate>> wallRegions = GetRegions(TileType.Wall);
        int wallThresholdSize = 75;

        foreach (List<TileCoordinate> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (TileCoordinate tile in wallRegion)
                {
                    _map[tile.TileX, tile.TileY] = TileType.Grass;
                }
            }
        }

        List<List<TileCoordinate>> roomRegions = GetRegions(TileType.Floor);
        roomRegions.AddRange(GetRegions(TileType.Grass));
        int roomThresholdSize = 25;
        List<Room> survivingRooms = new List<Room>();

        foreach (List<TileCoordinate> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (TileCoordinate tile in roomRegion)
                {
                    _map[tile.TileX, tile.TileY] = TileType.Wall;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, _map));
            }
        }

        AddPins(survivingRooms);

        survivingRooms.Sort();
        survivingRooms[0].SetAsMainRoom();
        ConnectClosestRooms(survivingRooms);
    }

    protected virtual void AddPins(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            if (_map[room.CenterTile.TileX, room.CenterTile.TileY] != TileType.Wall)
            {
                _caveMap.Pins.Add(CoordToWorldPoint(room.CenterTile));
            }
        }
    }
    /// <summary>
    /// Creates a wall border around the map
    /// </summary>
    /// <returns></returns>
    protected virtual TileType[,] CreateMapBorder()
    {
        TileType[,] borderedMap = new TileType[_mapWidth + borderSize * 2, _mapHeight + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < _mapWidth + borderSize && y >= borderSize && y < _mapHeight + borderSize)
                {
                    borderedMap[x, y] = _map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = TileType.Wall;
                }
            }
        }
        return borderedMap;
    }


    /// <summary>
    /// Loops through all rooms and finds and connects the closest rooms 
    /// </summary>
    /// <param name="allRooms"></param>
    /// <param name="forceAccessibilityFromMainRoom"></param>
    protected virtual void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {

        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.IsAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        TileCoordinate bestTileA = new TileCoordinate();
        TileCoordinate bestTileB = new TileCoordinate();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.ConnectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.EdgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.EdgeTiles.Count; tileIndexB++)
                    {
                        TileCoordinate tileA = roomA.EdgeTiles[tileIndexA];
                        TileCoordinate tileB = roomB.EdgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.TileX - tileB.TileX, 2) + Mathf.Pow(tileA.TileY - tileB.TileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }
    /// <summary>
    /// Carves a passage between two rooms
    /// </summary>
    /// <param name="roomA"></param>
    /// <param name="roomB"></param>
    /// <param name="tileA"></param>
    /// <param name="tileB"></param>
    protected virtual void CreatePassage(Room roomA, Room roomB, TileCoordinate tileA, TileCoordinate tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        List<TileCoordinate> line = GetLine(tileA, tileB);
        foreach (TileCoordinate c in line)
        {
            DrawCircle(TileType.Floor, c, 5);
            //DrawCircle(TileType.Grass, c, 1);
        }
    }
    /// <summary>
    /// Carves a circle from the given center
    /// </summary>
    /// <param name="c"></param>
    /// <param name="r"></param>
    void DrawCircle(TileType type, TileCoordinate c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.TileX + x;
                    int drawY = c.TileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        _map[drawX, drawY] = type;
                    }
                }
            }
        }
    }
    /// <summary>
    /// Returns the Tiles intersecting a straight line
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    protected virtual List<TileCoordinate> GetLine(TileCoordinate from, TileCoordinate to)
    {
        List<TileCoordinate> line = new List<TileCoordinate>();

        int x = from.TileX;
        int y = from.TileY;

        int dx = to.TileX - from.TileX;
        int dy = to.TileY - from.TileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new TileCoordinate(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }
    /// <summary>
    /// Gets the real world position of a Tile
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    Vector3 CoordToWorldPoint(TileCoordinate tile)
    {
        return new Vector3(-_mapWidth / 2 + .5f + tile.TileX, 2, -_mapHeight / 2 + .5f + tile.TileY);
    }
    /// <summary>
    /// Get different islolated regions in the map
    /// </summary>
    /// <param name="tileType"></param>
    /// <returns></returns>
    protected virtual List<List<TileCoordinate>> GetRegions(TileType tileType)
    {
        List<List<TileCoordinate>> regions = new List<List<TileCoordinate>>();
        int[,] mapFlags = new int[_mapWidth, _mapHeight];

        for (int x = 0; x < _mapWidth; x++)
        {
            for (int y = 0; y < _mapHeight; y++)
            {
                if (mapFlags[x, y] == 0 && _map[x, y] == tileType)
                {
                    List<TileCoordinate> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (TileCoordinate tile in newRegion)
                    {
                        mapFlags[tile.TileX, tile.TileY] = 1;
                    }
                }
            }
        }

        return regions;
    }
    /// <summary>
    /// Get all the tiles in an specific isolated region
    /// </summary>
    /// <param name="startX">x position to start the search</param>
    /// <param name="startY">y position to start the search</param>
    /// <returns></returns>
    protected virtual List<TileCoordinate> GetRegionTiles(int startX, int startY)
    {
        List<TileCoordinate> tiles = new List<TileCoordinate>();
        int[,] mapFlags = new int[_mapWidth, _mapHeight];
        TileType tileType = _map[startX, startY];

        Queue<TileCoordinate> queue = new Queue<TileCoordinate>();
        queue.Enqueue(new TileCoordinate(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            TileCoordinate tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.TileX - 1; x <= tile.TileX + 1; x++)
            {
                for (int y = tile.TileY - 1; y <= tile.TileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.TileY || x == tile.TileX))
                    {
                        if (mapFlags[x, y] == 0 && _map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new TileCoordinate(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }
    /// <summary>
    /// Determines whether a point is inside the map
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < _mapWidth && y >= 0 && y < _mapHeight;
    }
}
