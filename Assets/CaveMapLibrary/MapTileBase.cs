using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.CaveMapLibrary
{
    public abstract class MapTileBase
    {
        /// <summary>
        /// An enum representing the state of this tile
        /// </summary>
        public virtual TileType Type { get; protected set; }
        /// <summary>
        /// Gets an integer between 0 and 15 as the state of current MapTile based on which control nodes are active
        /// </summary>
        public virtual int State { get; protected set; }
    }
}
