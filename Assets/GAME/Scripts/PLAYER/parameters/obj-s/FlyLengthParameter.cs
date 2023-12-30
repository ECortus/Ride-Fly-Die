using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create player parameters/FlyLength")]
public class FlyLengthParameter : ParameterObject
{
    public override float Value
    {
        get
        {
            Vector3 forceV1 = -ConnectedParts.AccelerationDirection * ConnectedParts.AccelerationModificator / ConnectedParts.PlaneParameterRelativity;
            Vector3 forceV2 = ConnectedParts.PlaneDirection * ConnectedParts.PlaneModificator * ConnectedParts.PlaneParameterRelativity;

            float force1 = Mathf.Clamp(forceV1.z, 0f, 999f);
            float force2 = Mathf.Clamp(forceV2.z, 0f, 999f);
            
            // Debug.Log("Wheels par - " + force1 + ", wings par - " + force2);
            float force = Mathf.Pow(force1, 2) + Mathf.Pow(force2, 2);

            return Mathf.Pow(force, 1 / 2f);
        }
    }
    public override float MaxValue => ConnectedParts.MaxDistanceModificator;
}
