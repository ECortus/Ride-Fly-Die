using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformX5 : MonoBehaviour
{
    [Header("SORT BY VALUE")]
    [SerializeField] private SectionXMultiplier[] Sections;
    
    public int CurrentMultiplier
    {
        get
        {
            foreach (var VARIABLE in Sections)
            {
                if (VARIABLE && VARIABLE.Entered)
                {
                    return VARIABLE.Multiplier;
                }
            }
            
            return -1;
        }
    }

    private int multiplier;

    private void FixedUpdate()
    {
        if (GameManager.GameStarted && PlayerController.Launched)
        {
            multiplier = CurrentMultiplier;
            if (multiplier > 0)
            {
                if (PlayerController.Instance.Body.velocity.z < 0.05f && PlayerController.Instance.engine.onGround)
                {
                    PlayerController.Instance.ForceFinish();
                    return;
                }
                
                PlayerController.SetMultiplier(multiplier);
            }
        }
    }
}
