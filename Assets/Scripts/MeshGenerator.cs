using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.CaveMapLibrary;
using System.Linq;

/// <summary>
/// Generates 3D mesh for a 2D matrix of integers
/// </summary>
public class MeshGenerator
{
    /// <summary>
    /// A 2D matrix of MapTiles with corresponding TileNodes
    /// </summary>
    public virtual MapTileGrid TileGrid { get; private set; }

    MeshFilter _edgesMesh;
    MeshFilter _wallsMesh;
    MeshFilter _grassMesh;
    Transform _floor;

    List<Vector3> _vertices;
    List<int> _triangles;
    Dictionary<int, List<TileTriangle>> _triangleDictionary = new Dictionary<int, List<TileTriangle>>();
    List<List<int>> _outlines = new List<List<int>>();
    HashSet<int> _checkedVertices = new HashSet<int>();

    public MeshGenerator(CaveMapController caveMap)
    {
        _edgesMesh = caveMap.EdgesMesh;
        _wallsMesh = caveMap.WallsMesh;
        _grassMesh = caveMap.GrassMesh;
        _floor = caveMap.Floor;
    }

    /// <summary>
    /// Generates mesh for a given 2D matrix of integers
    /// </summary>
    /// <param name="map"></param>
    /// <param name="tileSize"></param>
    public virtual void GenerateMesh(TileType[,] map, float tileSize)
    {
        ClearData();

        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        TileGrid = new MapTileGrid(map, tileSize);

        TriangulateGrid();
        CreateGrassMesh();
        CreateWallsMesh(map, tileSize);
        CreateEdgeMesh();
        _floor.localScale = new Vector3(map.GetLength(0) / 10, 1, map.GetLength(1) / 10);
    }

    /// <summary>
    /// Clears triangles, outlines and vertices
    /// </summary>
    void ClearData()
    {
        _triangleDictionary.Clear();
        _outlines.Clear();
        _checkedVertices.Clear();
    }
    /// <summary>
    /// Creates Cave mesh for top view of the walls
    /// </summary>
    /// <param name="map"></param>
    /// <param name="tileSize"></param>
    protected virtual void CreateWallsMesh(TileType[,] map, float tileSize)
    {
        Mesh mesh = new Mesh();
        _wallsMesh.mesh = mesh;

        mesh.vertices = _vertices.ToArray();
        mesh.triangles = _triangles.ToArray();
        mesh.RecalculateNormals();

        int tileAmount = 10;
        Vector2[] uvs = new Vector2[_vertices.Count];
        for (int i = 0; i < _vertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * tileSize, map.GetLength(0) / 2 * tileSize, _vertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-map.GetLength(0) / 2 * tileSize, map.GetLength(0) / 2 * tileSize, _vertices[i].z) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        mesh.uv = uvs;
    }
    /// <summary>
    /// Creates mesh for outline edges of the map
    /// </summary>
    protected virtual void CreateEdgeMesh()
    {
        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        Mesh edgeMesh = new Mesh();
        float wallHeight = 5;

        foreach (List<int> outline in _outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(_vertices[outline[i]]); // left
                wallVertices.Add(_vertices[outline[i + 1]]); // right
                wallVertices.Add(_vertices[outline[i]] - Vector3.up * wallHeight); // bottom left
                wallVertices.Add(_vertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);

                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
            }
        }
        edgeMesh.vertices = wallVertices.ToArray();
        edgeMesh.triangles = wallTriangles.ToArray();
        edgeMesh.uv = uvs.ToArray();

        _edgesMesh.mesh = edgeMesh;

        CreateWallCollider(edgeMesh);
    }
    protected virtual void CreateFloorMesh(int width, int height)
    {
        _floor.localScale = new Vector3(width / 10, 1, height / 10);
        Mesh floorMesh = new Mesh();
        var vertices = new Vector3[4];

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(width, 0, 0);
        vertices[2] = new Vector3(0, height, 0);
        vertices[3] = new Vector3(width, height, 0);

        floorMesh.vertices = vertices;

        var tri = new int[6];

        tri[0] = 0;
        tri[1] = 2;
        tri[2] = 1;

        tri[3] = 2;
        tri[4] = 3;
        tri[5] = 1;

        floorMesh.triangles = tri;

        var normals = new Vector3[4];

        normals[0] = -Vector3.forward;
        normals[1] = -Vector3.forward;
        normals[2] = -Vector3.forward;
        normals[3] = -Vector3.forward;

        floorMesh.normals = normals;

        var uv = new Vector2[4];

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        floorMesh.uv = uv;
    }
    protected virtual void CreateGrassMesh()
    {
        List<Vector3> grassVertices = new List<Vector3>();
        List<int> grassTriangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        Mesh grassMesh = new Mesh();
        foreach (var grassTile in TileGrid.Tiles)
        {
            if (grassTile.Type == TileType.Grass)
            {
                int startIndex = grassVertices.Count;
                grassVertices.Add(grassTile.BottomLeft.Position - Vector3.up * 5);
                grassVertices.Add(grassTile.BottomRight.Position - Vector3.up * 5);
                grassVertices.Add(grassTile.TopLeft.Position - Vector3.up * 5);
                grassVertices.Add(grassTile.TopRight.Position - Vector3.up * 5);

                grassTriangles.Add(startIndex + 0);
                grassTriangles.Add(startIndex + 2);
                grassTriangles.Add(startIndex + 1);

                grassTriangles.Add(startIndex + 2);
                grassTriangles.Add(startIndex + 3);
                grassTriangles.Add(startIndex + 1);

                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));
            }
        }
        grassMesh.vertices = grassVertices.ToArray();
        grassMesh.triangles = grassTriangles.ToArray();
        grassMesh.uv = uvs.ToArray();
        _grassMesh.mesh = grassMesh;
    }
    /// <summary>
    /// Adds Collider to the walls
    /// </summary>
    /// <param name="wallMesh"></param>
    protected virtual void CreateWallCollider(Mesh wallMesh)
    {
        MeshCollider wallCollider = _edgesMesh.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }
    /// <summary>
    /// Creates Triangles for all the walls on the map
    /// </summary>
    protected virtual void TriangulateGrid()
    {
        for (int x = 0; x < TileGrid.Tiles.GetLength(0); x++)
        {
            for (int y = 0; y < TileGrid.Tiles.GetLength(1); y++)
            {
                TriangulateSquare(TileGrid.Tiles[x, y]);
            }
        }
    }
    /// <summary>
    /// Creates Triangles between nodes inside a Tile based on the state of that tile
    /// </summary>
    /// <param name="tile"></param>
    protected virtual void TriangulateSquare(MapTile tile)
    {
        switch (tile.State)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(tile.CentreLeft, tile.CentreBottom, tile.BottomLeft);
                break;
            case 2:
                MeshFromPoints(tile.BottomRight, tile.CentreBottom, tile.CentreRight);
                break;
            case 4:
                MeshFromPoints(tile.TopRight, tile.CentreRight, tile.CentreTop);
                break;
            case 8:
                MeshFromPoints(tile.TopLeft, tile.CentreTop, tile.CentreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(tile.CentreRight, tile.BottomRight, tile.BottomLeft, tile.CentreLeft);
                break;
            case 6:
                MeshFromPoints(tile.CentreTop, tile.TopRight, tile.BottomRight, tile.CentreBottom);
                break;
            case 9:
                MeshFromPoints(tile.TopLeft, tile.CentreTop, tile.CentreBottom, tile.BottomLeft);
                break;
            case 12:
                MeshFromPoints(tile.TopLeft, tile.TopRight, tile.CentreRight, tile.CentreLeft);
                break;
            case 5:
                MeshFromPoints(tile.CentreTop, tile.TopRight, tile.CentreRight, tile.CentreBottom, tile.BottomLeft, tile.CentreLeft);
                break;
            case 10:
                MeshFromPoints(tile.TopLeft, tile.CentreTop, tile.CentreRight, tile.BottomRight, tile.CentreBottom, tile.CentreLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(tile.CentreTop, tile.TopRight, tile.BottomRight, tile.BottomLeft, tile.CentreLeft);
                break;
            case 11:
                MeshFromPoints(tile.TopLeft, tile.CentreTop, tile.CentreRight, tile.BottomRight, tile.BottomLeft);
                break;
            case 13:
                MeshFromPoints(tile.TopLeft, tile.TopRight, tile.CentreRight, tile.CentreBottom, tile.BottomLeft);
                break;
            case 14:
                MeshFromPoints(tile.TopLeft, tile.TopRight, tile.BottomRight, tile.CentreBottom, tile.CentreLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(tile.TopLeft, tile.TopRight, tile.BottomRight, tile.BottomLeft);
                _checkedVertices.Add(tile.TopLeft.VertexIndex);
                _checkedVertices.Add(tile.TopRight.VertexIndex);
                _checkedVertices.Add(tile.BottomRight.VertexIndex);
                _checkedVertices.Add(tile.BottomLeft.VertexIndex);
                break;
        }

    }
    /// <summary>
    /// Creates Triangles based on all the given nodes
    /// </summary>
    /// <param name="points"></param>
    protected virtual void MeshFromPoints(params TileNode[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);

    }
    /// <summary>
    /// Assigns Vertex Indices for all the given nodes
    /// </summary>
    /// <param name="points"></param>
    protected virtual void AssignVertices(TileNode[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].VertexIndex == -1)
            {
                points[i].VertexIndex = _vertices.Count;
                _vertices.Add(points[i].Position);
            }
        }
    }

    void CreateTriangle(TileNode a, TileNode b, TileNode c)
    {
        _triangles.Add(a.VertexIndex);
        _triangles.Add(b.VertexIndex);
        _triangles.Add(c.VertexIndex);

        TileTriangle triangle = new TileTriangle(a.VertexIndex, b.VertexIndex, c.VertexIndex);
        AddTriangleToDictionary(triangle.VertexIndexA, triangle);
        AddTriangleToDictionary(triangle.VertexIndexB, triangle);
        AddTriangleToDictionary(triangle.VertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, TileTriangle triangle)
    {
        if (_triangleDictionary.ContainsKey(vertexIndexKey))
        {
            _triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<TileTriangle> triangleList = new List<TileTriangle>();
            triangleList.Add(triangle);
            _triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines()
    {

        for (int vertexIndex = 0; vertexIndex < _vertices.Count; vertexIndex++)
        {
            if (!_checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    _checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    _outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, _outlines.Count - 1);
                    _outlines[_outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        _outlines[outlineIndex].Add(vertexIndex);
        _checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<TileTriangle> trianglesContainingVertex = _triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            TileTriangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !_checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<TileTriangle> trianglesContainingVertexA = _triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }


}
