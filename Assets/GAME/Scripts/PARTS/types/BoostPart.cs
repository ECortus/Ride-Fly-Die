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
    [SerializeField] private ParticleSystem trail;

    private float rotateTarget;
    private float rotateMotor;

    private Vector3 rotate;
    
    private void FixedUpdate()
    {
        if (GameManager.GameStarted && VisualMode)
        {
            if (!trail.isPlaying) trail.Play();
            rotateTarget = 1;
        }
        else
        {
            trail.Stop();
            trail.Clear();
            
            rotateTarget = 0;
        }

        rotateMotor = Mathf.Lerp(rotateMotor, rotateTarget, rotateAcceleration * Time.fixedDeltaTime);
        rotate = new Vector3(0, 0, rotateMotor * rotateSpeed * Time.fixedDeltaTime);

        if (rotate != Vector3.zero)
        {
            fan.Rotate(rotate, Space.Self);
        }
    }
    
    private Vector3 direction => new Vector3(transform.forward.x, transform.forward.y, transform.forward.z);
    
    public override ParametersModifier GetFlyParameters()
    {
        ParametersModifier modif = new ParametersModifier(
            ModifierType.Boost,
            pars.GetMotorForce(Level),
            direction,
            transform.localPosition,
            Mass
        );

        return modif;
    }
    
    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.magenta;
    //     Gizmos.DrawRay(transform.position, direction * 999f);
    // }
}
