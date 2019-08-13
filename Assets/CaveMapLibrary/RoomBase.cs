using System.Collections.Generic;

namespace Assets.CaveMapLibrary
{
    public abstract class RoomBase
    {
        public virtual List<ICoordinate> RoomTiles { get; protected set; }
    }
}