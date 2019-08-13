using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.CaveMapLibrary
{
    public abstract class MapTileGridBase
    {
        /// <summary>
        /// Gets the MapTiles of this grid
        /// </summary>
        public virtual MapTileBase[,] Tiles { get; protected set; }
    }
}
