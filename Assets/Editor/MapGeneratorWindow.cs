using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AI;
using UnityEngine;

public class MapGeneratorWindow : EditorWindow
{
    private static int _mapWidth = 200;
    private static int _mapHeight = 100;
    private static int _wallsDensity = 5;
    private static bool _addNPC = true;

    [MenuItem("LlamaZoo/Cave Map Window")]
    public static void ShowWindow()
    {
        GetWindow<MapGeneratorWindow>(false, "Cave Map Generator", true);
    }

    void OnGUI()
    {
        _mapWidth =  EditorGUILayout.IntField("Map Width", _mapWidth < 50 ? 50 : _mapWidth > 500 ? 500 : _mapWidth);
        _mapHeight =  EditorGUILayout.IntField("Map Height", _mapHeight < 50 ? 50 : _mapHeight > 500 ? 500 : _mapHeight);
        _wallsDensity = EditorGUILayout.IntSlider("Wall Density",_wallsDensity, 0, 15);
        _addNPC = EditorGUILayout.Toggle("Create NPC", _addNPC);

        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Create New", GUILayout.Width(200), GUILayout.Height(30)))
        {
            CreateNewMap();
        }
        if (GUILayout.Button("Save As Prefab", GUILayout.Width(200), GUILayout.Height(30)))
        {
            SaveMapAsPrefab();
        }
        EditorGUILayout.EndHorizontal();
    }

    #region private Methods
    private static void CreateNewMap()
    {
        ClearMaps();
        GameObject map = Instantiate(Resources.Load("CaveMap")) as GameObject;
        var caveMap = map.GetComponent<CaveMapController>();
        if (caveMap != null)
        {
            MapGenerator mapGen = new MapGenerator();
            mapGen.GenerateMap(_mapWidth, _mapHeight, _wallsDensity + 40 , caveMap);
            NavMeshBuilder.BuildNavMesh();
            if (_addNPC)
            {
                Instantiate(Resources.Load("NPCManager"));
            }
            Selection.activeGameObject = map;
        }
        else
            Debug.LogError("No suitable prefab for CaveMap found.");
    }
    private static void SaveMapAsPrefab()
    {
        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<CaveMapController>() != null)
        {
            string localPath = "Assets/SavedPrefabs/" + Selection.activeGameObject.name + ".prefab";
            // Make sure the file name is unique, in case an existing Prefab has the same name.
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            PrefabUtility.SaveAsPrefabAssetAndConnect(Selection.activeGameObject, localPath, InteractionMode.UserAction);
        }
    }

    private static void ClearMaps()
    {
        var maps = FindObjectsOfType<CaveMapController>();
        foreach (var map in maps)
        {
            DestroyImmediate(map.gameObject);
        }
        var npcs = FindObjectsOfType<NPCManager>();
        foreach (var npc in npcs)
        {
            DestroyImmediate(npc.gameObject);
        }
    }
    #endregion
}
