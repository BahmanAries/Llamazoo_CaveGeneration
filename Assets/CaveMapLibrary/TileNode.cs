using UnityEngine;

namespace Assets.CaveMapLibrary
{
    /// <summary>
    /// Represents a node on one MapTile
    /// </summary>
    public class TileNode : NodeBase
    {
        /// <summary>
        /// The position of this node in real world
        /// </summary>
        public override Vector3 Position { get; protected set; }

        public TileNode(Vector3 pos)
        {
            Position = pos;
            VertexIndex = -1;
        }
    }
}
