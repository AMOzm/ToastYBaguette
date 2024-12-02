using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX; // Add this for VisualEffect

public class TurnVXOn : MonoBehaviour
{
    public VisualEffect vfx; // Use VisualEffect for VFX Graph

    void Start()
    {
        if (vfx != null)
        {
            vfx.Stop(); // Ensure the VFX Graph is off at the start
        }
    }

    void OnMouseDown()
    {
        if (vfx != null)
        {
            if (vfx.aliveParticleCount > 0)
            {
                vfx.Stop(); // Turn off the VFX Graph if it's playing
            }
            else
            {
                vfx.Play(); // Turn on the VFX Graph if it's stopped
            }
        }
    }
}
