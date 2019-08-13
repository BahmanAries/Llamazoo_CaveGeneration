using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBulbController : MonoBehaviour
{
    [SerializeField]
    private Light MainLight;
    [SerializeField]
    private Light SmallLight;

    public void SwitchOn()
    {
        MainLight.enabled = true;
    }
    public void SwitchOff()
    {
        MainLight.enabled = false;
        SmallLight.enabled = false;
    }
    public void Glow()
    {
        SmallLight.enabled = true;
    }

}
