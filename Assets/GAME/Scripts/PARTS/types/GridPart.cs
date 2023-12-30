using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPart : Part
{
    public override ParametersModifier GetFlyParameters()
    {
        ParametersModifier modif = new ParametersModifier(
            ModifierType.Default,
            0,
            Vector3.zero, 
            transform.localPosition,
            Mass
        );

        return modif;
    }
}
