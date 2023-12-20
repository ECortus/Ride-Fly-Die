using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationZone : MonoBehaviour
{
    [SerializeField] private float speed;
    
    private bool Condition(GameObject obj) => obj.layer == LayerMask.NameToLayer("Player") && PlayerController.Launched && GameManager.GameStarted;
    
    private void OnTriggerStay(Collider other)
    {
        if (Condition(other.gameObject))
        {
            PlayerController.Instance.AccelerateForward(speed);
        }
    }
}
