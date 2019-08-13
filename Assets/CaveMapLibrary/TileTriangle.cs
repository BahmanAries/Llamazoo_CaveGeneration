using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.CaveMapLibrary
{
    /// <summary>
    /// Represents a triangle connection three vertices
    /// </summary>
    struct TileTriangle
    {
        public int VertexIndexA { get; private set; }
        public int VertexIndexB { get; private set; }
        public int VertexIndexC { get; private set; }
        int[] vertices;

        public TileTriangle(int a, int b, int c)
        {
            VertexIndexA = a;
            VertexIndexB = b;
            VertexIndexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this[int i]
        {
            get
            {
                return vertices[i];
            }
        }


        public bool Contains(int vertexIndex)
        {
            return vertexIndex == VertexIndexA || vertexIndex == VertexIndexB || vertexIndex == VertexIndexC;
        }
    }
}
