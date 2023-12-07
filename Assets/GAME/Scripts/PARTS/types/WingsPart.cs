using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingsPart : Part
{
    [Space]
    [SerializeField] private WingsParameters pars;

    [SerializeField] private TrailRenderer[] trails;

    void Start()
    {
        trails = GetComponentsInChildren<TrailRenderer>(true);
    }

    private void FixedUpdate()
    {
        if (!GameManager.GameStarted || !VisualMode)
        {
            foreach (var VARIABLE in trails)
            {
                VARIABLE.enabled = false;
            }
        }
        else
        {
            foreach (var VARIABLE in trails)
            {
                VARIABLE.enabled = true;
            }
        }
    }

    public override ParametersModifier GetFlyParameters()
    {
        Vector3 dir = Vector3.zero;
        dir = -transform.up;
        
        ParametersModifier modif = new ParametersModifier(
            ModifierType.Wings,
            pars.GetFlyModifier(Level),
            dir
        );

        return modif;
    }
}
