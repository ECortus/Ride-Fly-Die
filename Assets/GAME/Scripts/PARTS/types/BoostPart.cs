using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPart : Part
{
    [Space]
    [SerializeField] private BoostParameters pars;

    [Space] 
    [SerializeField] private float rotateSpeed = 50f;
    [SerializeField] private float rotateAcceleration = 50f;
    [SerializeField] private Transform fan;

    private float rotateTarget;
    private float rotateMotor;

    private Vector3 rotate;
    
    private void FixedUpdate()
    {
        if (GameManager.GameStarted && VisualMode)
        {
            rotateTarget = 1;
        }
        else
        {
            rotateTarget = 0;
        }

        rotateMotor = Mathf.Lerp(rotateMotor, rotateTarget, rotateAcceleration * Time.fixedDeltaTime);
        rotate = new Vector3(0, 0, rotateMotor * rotateSpeed * Time.fixedDeltaTime);

        if (rotate != Vector3.zero)
        {
            fan.Rotate(rotate, Space.Self);
        }
    }
    
    private Vector3 direction => -Vector3.forward * Mathf.Sign(Vector3.Dot(transform.forward, Vector3.forward));
    
    public override ParametersModifier GetFlyParameters()
    {
        ParametersModifier modif = new ParametersModifier(
            ModifierType.Boost,
            pars.GetMotorForce(Level),
            direction
        );

        return modif;
    }
    
    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.magenta;
    //     Gizmos.DrawRay(transform.position, direction * 999f);
    // }
}
