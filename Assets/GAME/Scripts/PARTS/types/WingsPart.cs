using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingsPart : Part
{
    [Space]
    [SerializeField] private WingsParameters pars;

    [SerializeField] private TrailRenderer trail1, trial2;

    private void FixedUpdate()
    {
        if (GameManager.GameStarted && PlayerController.Launched)
        {
            trail1.enabled = true;
            trial2.enabled = additionalObject.activeSelf;
        }
        else
        {
            trail1.enabled = false;
            trial2.enabled = false;
        }
    }

    private Vector3 direction => -transform.up;

    public override ParametersModifier GetFlyParameters()
    {
        ParametersModifier modif = new ParametersModifier(
            ModifierType.Wings,
            pars.GetFlyModifier(Level),
            direction
        );

        return modif;
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.magenta;
    //     Gizmos.DrawRay(transform.position, direction * 99999f);
    // }
}
