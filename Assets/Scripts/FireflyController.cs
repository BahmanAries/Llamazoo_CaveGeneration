using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FireflyController : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent Agent;

    public LightBulbController[] Lights { get; set; }
    private int currentIndex;

    private void Start()
    {
        foreach (var light in Lights)
        {
            light.SwitchOff();
        }
        Lights[currentIndex].Glow();
    }

    private void FixedUpdate()
    {
        if (!Agent.pathPending && Agent.remainingDistance < 0.5f)
        {
            TurnLightOn();
            GoToNext();
            GlowNext();
        }
    }
    private void TurnLightOn()
    {
        Lights[currentIndex].SwitchOn();
    }
    private void GlowNext()
    {
        if (currentIndex < Lights.Length - 1)
            Lights[currentIndex + 1].Glow();
    }
    private void GoToNext()
    {
        if (Lights.Length > 0)
        {
            currentIndex = (currentIndex + 1) % Lights.Length;
            Agent.destination = Lights[currentIndex].transform.position;
        }
    }
}
