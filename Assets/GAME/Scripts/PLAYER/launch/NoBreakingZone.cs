using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoBreakingZone : MonoBehaviour
{
    private bool Condition(GameObject obj) => obj.layer == LayerMask.NameToLayer("Player") && GameManager.GameStarted;
    
    private void OnTriggerStay(Collider other)
    {
        if (Condition(other.gameObject))
        {
            AircraftEngine.NoBreaking = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (Condition(other.gameObject))
        {
            AircraftEngine.NoBreaking = false;
        }
    }
}