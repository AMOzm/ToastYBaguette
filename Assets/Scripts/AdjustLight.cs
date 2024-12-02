using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustLight : MonoBehaviour
{
    public Light myLight;

void Start()
{
    myLight = GetComponent<Light>();
    myLight.intensity = 100000000f;  // Set intensity manually, ignoring Volume Profile settings
}
}
