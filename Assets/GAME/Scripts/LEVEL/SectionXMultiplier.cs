using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionXMultiplier : MonoBehaviour
{
    public int Multiplier;
    public bool Entered { get; private set; }

    private PlatformX5 platform;

    private bool Condition(GameObject go) => go.layer == LayerMask.NameToLayer("Player") && GameManager.GameStarted;

    void Awake()
    {
        Entered = false;
        platform = GetComponentInParent<PlatformX5>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (Condition(other.gameObject))
        {
            Entered = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (Condition(other.gameObject))
        {
            Entered = false;

            if (platform.CurrentMultiplier < 1)
            {
                PlayerController.SetMultiplier(1);
            }
        }
    }
}
