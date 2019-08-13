using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.CaveMapLibrary
{
    /// <summary>
    /// Represents a point on the conrners of one MapTile that controls Above and Rigth nodes
    /// </summary>
    public class TileControlNode : TileNode
    {
        public bool IsActive { get; private set; }
        public TileNode Above { get; private set; }
        public TileNode Right { get; private set; }

        public TileControlNode(Vector3 pos, bool active, float tileSize) : base(pos)
        {
            IsActive = active;
            Above = new TileNode(Position + Vector3.forward * tileSize / 2f);
            Right = new TileNode(Position + Vector3.right * tileSize / 2f);
        }
    }
}
