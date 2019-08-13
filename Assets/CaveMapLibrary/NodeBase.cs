using UnityEngine;

namespace Assets.CaveMapLibrary
{
    public abstract class NodeBase
    {
        /// <summary>
        /// position of this node in real world
        /// </summary>
        public abstract Vector3 Position { get; protected set; }
        /// <summary>
        /// The Index of this node inside the mesh
        /// </summary>
        public virtual int VertexIndex { get; set; }
    }
}
