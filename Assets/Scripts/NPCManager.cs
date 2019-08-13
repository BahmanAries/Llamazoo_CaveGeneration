using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(Instance);
        else
            Instance = this;
    }

    private List<LightBulbController> _lamps;
    private FireflyController _firefly;

    private void Start()
    {
        var fireflyPrefab = Resources.Load("Firefly") as GameObject;
        var lampPregab = Resources.Load("LampPrefab") as GameObject;
        var caveMap = FindObjectOfType<CaveMapController>();

        if (fireflyPrefab != null && lampPregab != null && caveMap != null)
        {
            _lamps = new List<LightBulbController>();
            foreach (var pin in caveMap.Pins)
            {
                _lamps.Add(Instantiate(lampPregab, pin, Quaternion.identity, transform).GetComponent<LightBulbController>());
            }

            _firefly = Instantiate(fireflyPrefab, _lamps[0].transform.position, Quaternion.identity).GetComponent<FireflyController>();
            _firefly.Lights = _lamps.ToArray();

        }
    }

    //private void LateUpdate()
    //{
    //    var pos = _firefly.transform.position;
    //    Camera.main.transform.position = new Vector3(pos.x, 50, pos.z);
    //}
}
