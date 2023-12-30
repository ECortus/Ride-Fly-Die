using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create player parameters/FlyBoost")]
public class FlyBoostParameter : ParameterObject
{
    public override float Value
    {
        get
        {
            Vector3 forceV = -ConnectedParts.BoostDirection * ConnectedParts.BoostModificator;
            
            float force1 = Mathf.Clamp(forceV.z, 0f, 999f);
            float force2 = Mathf.Clamp(-forceV.y, 0f, 999f) / 3f;
            
            // Debug.Log("Boost1 par - " + force1 + ", boost2 par - " + force2);

            float force;
            if (force1 > force2)
            {
                force = Mathf.Pow(force1, 2) - Mathf.Pow(force2, 2);
            }
            else
            {
                force = Mathf.Pow(force2, 2) - Mathf.Pow(force1, 2);
            }

            return Mathf.Pow(force, 1/2f);
        }
    }

    public override float MaxValue => ConnectedParts.MaxBoostModificator;
}
