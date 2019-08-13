using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.CaveMapLibrary
{
    /// <summary>
    /// Represents a node on one MapTile
    /// </summary>
    public class TileNode
    {
        /// <summary>
        /// The position of this node in real world
        /// </summary>
        public Vector3 Position { get; private set; }
        /// <summary>
        /// The Index of this node inside the mesh
        /// </summary>
        public int VertexIndex { get; set; }

        public TileNode(Vector3 pos)
        {
            Position = pos;
            VertexIndex = -1;
        }
    }
}
