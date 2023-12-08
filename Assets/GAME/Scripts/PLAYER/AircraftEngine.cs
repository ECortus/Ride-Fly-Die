using System.Collections;
using System.Collections.Generic;
using ModestTree;
using UnityEngine;
using System;
using System.Linq;
using TMPro.Examples;

// [RequireComponent(typeof(Rigidbody))]
// [RequireComponent(typeof(SphereCollider))]

[ExecuteInEditMode]
public class AircraftEngine : MonoBehaviour
{
    [field: SerializeField] public Rigidbody Body { get; private set; }
    [field: SerializeField] public SphereCollider Sphere { get; private set; }
    [SerializeField] private LaunchPower LaunchPower;
    
    private static float GroundCheckDistanceDelta = 0.1f;
    private static float GroundCheckSkinWidthDelta = 0.05f;

    public enum GRAVITY_MODE
    {
        AlwaysDown, TowardsGround
    }
    
    public Vector3 Center => transform.position + Sphere.center;
    
    [SerializeField] private LayerMask GroundMask;
    private RaycastHit rayHitGround;

    private bool _TakeControl { get; set; }
    public void SetControl(bool take) => _TakeControl = take;
    
    private List<ParametersModifier> 
        BoostModifiersList = new List<ParametersModifier>(), 
        AccelerationModifiersList = new List<ParametersModifier>(), 
        FlyModifiersList = new List<ParametersModifier>();

    private Vector3 BoostDirectionModifier;
    private float FlyModifier = 1f;
    private float AccelerationModifier = 1f;

    public void AddModifier(ParametersModifier mod)
    {
        if (mod == null) return;
        
        switch (mod.Type)
        {
            case ModifierType.Boost:
                if(!BoostModifiersList.Contains(mod)) BoostModifiersList.Add(mod);
                break;
            case ModifierType.Wheels:
                if(!AccelerationModifiersList.Contains(mod)) AccelerationModifiersList.Add(mod);
                break;
            case ModifierType.Wings:
                if(!FlyModifiersList.Contains(mod)) FlyModifiersList.Add(mod);
                break;
            default:
                break;
        }
    }

    public void RemoveModifier(ParametersModifier mod)
    {
        if (mod == null) return;
        
        switch (mod.Type)
        {
            case ModifierType.Boost:
                if(BoostModifiersList.Contains(mod)) BoostModifiersList.Remove(mod);
                break;
            case ModifierType.Wheels:
                if(AccelerationModifiersList.Contains(mod)) AccelerationModifiersList.Remove(mod);
                break;
            case ModifierType.Wings:
                if(FlyModifiersList.Contains(mod)) FlyModifiersList.Remove(mod);
                break;
            default:
                break;
        }
    }

    public void ClearModifiers()
    {
        BoostModifiersList.Clear();
        AccelerationModifiersList.Clear();
        FlyModifiersList.Clear();
    }

    private void RefreshMods()
    {
        Vector3 dir = Vector3.zero;
        for(int i = 0; i < BoostModifiersList.Count; i++)
        {
            if (BoostModifiersList[i] != null)
            {
                dir += BoostModifiersList[i].Direction * BoostModifiersList[i].Force;
            }
        }
        BoostDirectionModifier = dir;
        
        float value = 1f;
        for(int i = 0; i < FlyModifiersList.Count; i++)
        {
            if (FlyModifiersList[i] != null)
            {
                value += FlyModifiersList[i].Force * Mathf.Cos(Vector3.Angle(FlyModifiersList[i].Direction, PlayerController.Instance.Forward));
            }
        }
        FlyModifier = value;
        
        value = 1f;
        foreach (var VARIABLE in AccelerationModifiersList)
        {
            if (VARIABLE != null)
            {
                value += VARIABLE.Force * Mathf.Cos(Vector3.Angle(VARIABLE.Direction, PlayerController.Instance.Forward));
            }
        }
        AccelerationModifier = value;
    }

    private List<ParametersModifier> GetParametersByType(List<ParametersModifier> parts, ModifierType type)
    {
        List<ParametersModifier> modifiers = new List<ParametersModifier>();
        
        foreach (ParametersModifier VARIABLE in parts)
        {
            if (VARIABLE.Type == type)
            {
                modifiers.Add(VARIABLE);
            }
        }

        return modifiers;
    }
    
    [Header("Physics")]
    public float colliderRadius = 2f;
    public float bodyMass = 1f;
    public GRAVITY_MODE gravityMode = GRAVITY_MODE.TowardsGround;
    public float gravityVelocity = 80;
    public float maxGravity = 50;
    public float maxSlopeAngle = 50f;
    public float rotateForce = 35f;
    public float planeRotateMultiplier = 1.2f;
    public LayerMask collidableLayers = ~0;
    public bool adjustToScale = false;
    
    [Header("Engine")]
    public float maxAccelerationForward = 100;
    public float maxSpeedForward = 40;
    public float maxAccelerationReverse = 50;
    public float maxSpeedReverse = 20;
    public float brakeStrength = 200;
    public float slopeFriction = 1f;

    private bool onGround = false;
    private Vector3 crossForward = Vector3.zero;
    private Vector3 crossUp = Vector3.zero;
    private Vector3 crossRight = Vector3.zero;
    private bool hitSide = false;
    private float hitSideForce = 0;
    private float hitSideMass = 0;
    private float hitGroundMass = 0;
    private Vector3 hitSidePosition = Vector3.zero;
    private bool hitGround = false;
    private float hitGroundForce = 0;
    private bool hitSideStayStatic = false;
    private bool hitSideStayDynamic = false;
    private float groundVelocity = 0;
    private float forwardVelocity = 0;
    private float rightVelocity = 0;
    private Vector3 gravityDirection = Vector3.zero;
    private float inputSteering = 0;
    private float inputMotor = 0;
    private PhysicMaterial customPhysicMaterial;
    private Quaternion groundRotation;
    private float slopeDelta = 0;
    private float boostMultiplier = 1;
    private float averageScale = 1;
    private float realColliderRadius = 0;
    private float scaleAdjustment = 1;
    private float cubicScale = 1;
    private float inverseScaleAdjustment = 1;
    
    public float inputPlaneRotate { get; set; }
    
    void Start()
    {
        customPhysicMaterial = new PhysicMaterial();
        customPhysicMaterial.bounciness = 0;
        customPhysicMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
        customPhysicMaterial.staticFriction = 0;
        customPhysicMaterial.dynamicFriction = 0;
        customPhysicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
    
        if (!Application.isPlaying) return;
    
        groundRotation = transform.rotation;
    
        crossForward = transform.forward;
        crossRight = transform.right;
        crossUp = transform.up;
    }

    public void AccelerateBody(Vector3 direction, float percentForce = 1)
    {
        Body.AddForce(-direction * LaunchPower.Power * percentForce * AccelerationModifier, ForceMode.Acceleration);
    }
    
    void Update()
    {
        // refresh rigid body and collider parameters
        
        //body.hideFlags = HideFlags.NotEditable;
        //sphereCollider.hideFlags = HideFlags.NotEditable;
    
        // if (_TakeControl) return;
    
        Body.mass = bodyMass * (adjustToScale ? cubicScale : 1);
        Body.drag = 0;
        Body.angularDrag = 1f;
        // Body.constraints = RigidbodyConstraints.FreezeRotation;
        // Body.useGravity = false;
        // Body.isKinematic = false;
        // Body.interpolation = RigidbodyInterpolation.Extrapolate;
        // Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    
        Sphere.radius = colliderRadius;
        Sphere.isTrigger = false;
        Sphere.material = customPhysicMaterial;
    }
    
    void FixedUpdate()
    {
        if (_TakeControl) return;
        
        averageScale = (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3f;
        realColliderRadius = Sphere.radius * averageScale;
    
        float deltaTime = Time.fixedDeltaTime;
        float motor = inputMotor;
        float groundCheckSkinWidth = realColliderRadius * GroundCheckSkinWidthDelta;
        float groundCheckDistance = realColliderRadius * GroundCheckDistanceDelta;
    
        //// basic ground check
        onGround = false;
        crossUp = transform.up;
        
        RaycastHit hitSphere;
        if (Physics.SphereCast(
                Sphere.bounds.center, realColliderRadius - groundCheckSkinWidth, Vector3.down, 
                out hitSphere, groundCheckDistance + groundCheckSkinWidth, collidableLayers, QueryTriggerInteraction.Ignore))
        {
            crossUp = hitSphere.normal;
            if (Vector3.Angle(crossUp, Vector3.up) <= maxSlopeAngle)
            {
                onGround = true;
            }
        }
    
        //// get average ground direction from corners (forward-left, forward-right, back-left, back-right)
        Vector3[] groundCheckSource = new Vector3[]{
            Sphere.bounds.center + Vector3.forward * realColliderRadius + Vector3.left * realColliderRadius,
            Sphere.bounds.center + Vector3.forward * realColliderRadius + Vector3.right * realColliderRadius,
            Sphere.bounds.center + Vector3.back * realColliderRadius + Vector3.left * realColliderRadius,
            Sphere.bounds.center + Vector3.back * realColliderRadius + Vector3.right * realColliderRadius
        };
    
        Vector3[] groundCheckHits = new Vector3[groundCheckSource.Length];
        bool[] groundCheckFound = new bool[groundCheckSource.Length];
        
        RaycastHit rayHit;
        for (int i = 0; i < groundCheckSource.Length; i++)
        {
            groundCheckFound[i] = Physics.Raycast(
                groundCheckSource[i], Vector3.down, out rayHit, realColliderRadius * 2, 
                collidableLayers, QueryTriggerInteraction.Ignore);
            if (groundCheckFound[i])
            {
                groundCheckHits[i] = rayHit.point;
            }
        }
    
        //// append the calculated corner normals to the center normal
        Vector3 triFRNormal = Vector3.zero;
        if (groundCheckFound[0] && groundCheckFound[1] && groundCheckFound[2])
        {
            triFRNormal = getTriangleNormal(groundCheckHits[0], groundCheckHits[1], groundCheckHits[2]);
        }
    
        Vector3 triBLNormal = Vector3.zero;
        if (groundCheckFound[1] && groundCheckFound[3] && groundCheckFound[2])
        {
            triBLNormal = getTriangleNormal(groundCheckHits[1], groundCheckHits[3], groundCheckHits[2]);
        }
    
        crossUp = (crossUp + triFRNormal + triBLNormal).normalized;
    
        //// calculate ground rotation
        Vector3 velocity = Body.velocity;
        groundVelocity = (velocity - Vector3.up * velocity.y).magnitude;
        crossForward = Vector3.Cross(-crossUp, transform.right);
    
        groundRotation = Quaternion.LookRotation(crossForward, crossUp);
        
        float groundXAngle = groundRotation.eulerAngles.x;
        float groundYAngle = Body.rotation.eulerAngles.y;
        float groundZAngle = groundRotation.eulerAngles.z;

        Quaternion rotation;
        rotation = Quaternion.Euler(groundXAngle, groundYAngle, groundZAngle);
        
        // Body.MoveRotation(rotation);
    
        // gravity
        gravityDirection = Vector3.zero;
        switch (gravityMode)
        {
            case GRAVITY_MODE.AlwaysDown:
                gravityDirection = Vector3.down;
                break;
            case GRAVITY_MODE.TowardsGround:
                gravityDirection = onGround ? -crossUp : Vector3.down;
                break;
        }

        velocity += -transform.forward * 12f * deltaTime;

        velocity += gravityDirection * Mathf.Min(Mathf.Max(0, maxGravity + velocity.y), gravityVelocity * deltaTime);
        
        Body.velocity = velocity;
    }
    
    void OnDestroy()
    {
        if (Body != null) Body.hideFlags = HideFlags.None;
        if (Sphere != null) Sphere.hideFlags = HideFlags.None;
    }
    
    public float GetDistanceToGround()
    {
        Physics.Raycast(Center, Vector3.down * 999f, out rayHitGround, 999f, GroundMask);

        if (rayHitGround.transform)
        {
            return (Center - rayHitGround.point).magnitude;
        }
        
        return 0f;
    }
    
    private int getZeroSign(float value)
    {
        if (value == 0) return 0;
        else return (int)Mathf.Sign(value);
    }
    
    public int getVelocityDirection()
    {
        return getZeroSign(forwardVelocity);
    }

    public float getMaxSpeed()
    {
        return getVelocityDirection() >= 0 ? maxSpeedForward * scaleAdjustment : maxSpeedReverse * scaleAdjustment;
    }

    public float getMaxAcceleration()
    {
        return getVelocityDirection() >= 0 ? maxAccelerationForward * scaleAdjustment : maxAccelerationReverse * scaleAdjustment;
    }

    public void setMotor(int mtr) => inputMotor = mtr;
    public float getMotor() => inputMotor;
    
    private Vector3 getTriangleNormal(Vector3 pa, Vector3 pb, Vector3 pc)
    {
        Vector3 u = pb - pa;
        Vector3 v = pc - pa;

        return new Vector3(u.y * v.z - u.z * v.y, u.z * v.x - u.x * v.z, u.x * v.y - u.y * v.x).normalized;
    }
}
