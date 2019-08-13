using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveMapController : MonoBehaviour
{
    public MeshFilter EdgesMesh;
    public MeshFilter WallsMesh;
    public MeshFilter GrassMesh;
    public Transform Floor;
    public GameObject LampPrefab;
    public List<Vector3> Pins;
}
