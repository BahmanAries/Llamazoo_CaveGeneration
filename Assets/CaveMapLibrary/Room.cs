using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.CaveMapLibrary
{
    /// <summary>
    /// Represents an Isolated space on the map
    /// </summary>
    public class Room : IComparable<Room>
    {
        public List<TileCoordinate> RoomTiles { get; private set; }
        public List<TileCoordinate> EdgeTiles { get; private set; }
        public TileCoordinate CenterTile { get; private set; }
        public List<Room> ConnectedRooms { get; private set; }
        public int RoomSize { get; private set; }
        public bool IsAccessibleFromMainRoom { get; private set; }
        public bool IsMainRoom { get; private set; }

        public Room()
        {
        }
        public Room(List<TileCoordinate> roomTiles, TileType[,] map)
        {
            RoomTiles = roomTiles;
            RoomSize = RoomTiles.Count;
            ConnectedRooms = new List<Room>();
            EdgeTiles = new List<TileCoordinate>();
            foreach (TileCoordinate tile in RoomTiles)
            {
                for (int x = tile.TileX - 1; x <= tile.TileX + 1; x++)
                {
                    for (int y = tile.TileY - 1; y <= tile.TileY + 1; y++)
                    {
                        if (x == tile.TileX || y == tile.TileY)
                        {
                            if (map[x, y] == TileType.Wall)
                            {
                                EdgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }

            double centerX = EdgeTiles.Average(tile => tile.TileX);
            double centerY = EdgeTiles.Average(tile => tile.TileY);
            CenterTile = new TileCoordinate((int)centerX, (int)centerY);
        }
        public void SetAsMainRoom()
        {
            IsMainRoom = true;
            IsAccessibleFromMainRoom = true;
        }
        /// <summary>
        /// Gives access to the main room from this room
        /// </summary>
        public void SetAccessibleFromMainRoom()
        {
            if (!IsAccessibleFromMainRoom)
            {
                IsAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in ConnectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }
        /// <summary>
        /// Connects two rooms
        /// </summary>
        /// <param name="roomA"></param>
        /// <param name="roomB"></param>
        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.IsAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.IsAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.ConnectedRooms.Add(roomB);
            roomB.ConnectedRooms.Add(roomA);
        }
        /// <summary>
        /// Shows whether this room is commected to the otherRoom
        /// </summary>
        /// <param name="otherRoom"></param>
        /// <returns></returns>
        public bool IsConnected(Room otherRoom)
        {
            return ConnectedRooms.Contains(otherRoom);
        }
        /// <summary>
        /// Compares two rooms based on their size
        /// </summary>
        /// <param name="otherRoom"></param>
        /// <returns></returns>
        public int CompareTo(Room otherRoom)
        {
            return otherRoom.RoomSize.CompareTo(RoomSize);
        }
    }
}
