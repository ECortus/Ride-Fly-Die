using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelsPart : Part
{
    [Space]
    [SerializeField] private WheelsParameters pars;

    [Space] 
    [SerializeField] private float rotateSpeed = 50f;
    [SerializeField] private float rotateAcceleration = 50f;
    [SerializeField] private Transform wheel1, wheel2;

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
        rotate = new Vector3(-rotateMotor * rotateSpeed * Time.fixedDeltaTime, 0, 0);

        if (rotate != Vector3.zero)
        {
            wheel1.Rotate(rotate, Space.Self);
            wheel2.Rotate(rotate, Space.Self);
        }
    }
    
    public override ParametersModifier GetFlyParameters()
    {
        Vector3 dir = Vector3.zero;
        dir = transform.forward;
        
        ParametersModifier modif = new ParametersModifier(
            ModifierType.Wheels,
            pars.GetAccelerationModifier(Level),
            dir
        );

        return modif;
    }
}
