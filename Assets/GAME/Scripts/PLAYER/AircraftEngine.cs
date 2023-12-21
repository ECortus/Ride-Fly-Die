using System.Collections;
using System.Collections.Generic;
using ModestTree;
using UnityEngine;
using System;
using System.Linq;
using TMPro.Examples;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]

[ExecuteInEditMode]
public class AircraftEngine : MonoBehaviour
{
    [field: SerializeField] public Rigidbody Body { get; private set; }
    [field: SerializeField] public SphereCollider Sphere { get; private set; }
    
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
    
    [Header("Physics")]
    public float colliderRadius = 2f;
    public float bodyMass = 1f;
    public GRAVITY_MODE gravityMode = GRAVITY_MODE.TowardsGround;
    public float gravityVelocity = 80;
    public float maxGravity = 50;
    public float gravityMultiplierOnMotor = 0.8f;

    [Space] 
    public float maxSlopeAngle = 50f;
    public float rotateVelocity = 45f;
    public float rotateForce = 35f;
    public Vector3 globalRotateDeltaOnGround;
    public Vector3 globalRotateDeltaOffGround;
    public float planeRotateMultiplier = 1.2f;
    public float planeRotateBackMultiplier = 0.75f;
    
    [Space]
    public LayerMask collidableLayers = ~0;
    public bool adjustToScale = false;
    
    [Header("Engine")]
    public AnimationCurve accelerationCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0) });
    public float maxAccelerationForward = 100;
    public float maxSpeedForward = 40;
    public float maxAccelerationReverse = 50;
    public float boostMultiplierOnGround = 1.5f;
    
    [Space]
    public float maxSpeedReverse = 20;
    public float brakeStrength = 200;
    
    [Space]
    public float slopeFriction = 1f;
    public float forwardFriction = 40;
    public float lateralFriction = 80;

    [Header("DEBUG: ")]
    public bool onGround = false;
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
    public float inputPlaneRotate;
    private PhysicMaterial customPhysicMaterial;
    private Quaternion bodyVelocityLook;
    private Quaternion groundRotation;
    private float slopeDelta = 0;
    private float boostMultiplier = 1;
    private float averageScale = 1;
    private float realColliderRadius = 0;
    private float scaleAdjustment = 1;
    private float cubicScale = 1;
    private float inverseScaleAdjustment = 1;

    private List<Part> ConnectedParts;

    public void ClearParts()
    {
        if (ConnectedParts != null) ConnectedParts.Clear();

        defaultPosition = null;
        defaultRotation = null;

        boostDirection = accelerationDirection = planeDirection = Vector3.zero;
        boostModificator = accelerationModificator = planeModificator = 0f;
    }
    
    public void AddPart(Part part)
    {
        if (ConnectedParts == null) ConnectedParts = new List<Part>();
        
        if (!ConnectedParts.Contains(part))
        {
            ConnectedParts.Add(part);
            AddParameter(part.GetFlyParameters());
        }
    }
    
    public void RemovePart(Part part)
    {
        if (ConnectedParts == null) ConnectedParts = new List<Part>();
        
        if (ConnectedParts.Contains(part))
        {
            ConnectedParts.Remove(part);
            RemoveParameter(part.GetFlyParameters());
        }
    }
    
    [SerializeField] private Vector3 boostDirection, accelerationDirection, planeDirection;
    [SerializeField] private float boostModificator, accelerationModificator, planeModificator;

    void AddParameter(ParametersModifier mod)
    {
        if (mod == null) return;

        if (accelerationModificator == 0) accelerationModificator = 1f;
        
        if (mod.Type == ModifierType.Boost)
        {
            boostDirection += mod.Direction;
            boostModificator += mod.Force;
        }
        else if (mod.Type == ModifierType.Wheels)
        {
            accelerationDirection += mod.Direction;
            accelerationModificator += mod.Force * -Vector3.Dot(transform.forward, mod.Direction);
        }
        else if (mod.Type == ModifierType.Wings)
        {
            FormWings();
        }
    }

    void RemoveParameter(ParametersModifier mod)
    {
        if (mod == null) return;
        
        if (accelerationModificator == 0) accelerationModificator = 1f;
        
        if (mod.Type == ModifierType.Boost)
        {
            boostDirection -= mod.Direction;
            boostModificator -= mod.Force;
        }
        else if (mod.Type == ModifierType.Wheels)
        {
            accelerationDirection -= mod.Direction;
            accelerationModificator -= mod.Force;
        }
        else if (mod.Type == ModifierType.Wings)
        {
            FormWings();
        }
    }

    void FormWings()
    {
        Vector3 direction = Vector3.zero;
        float force = 0f;
        int count = 0;

        ParametersModifier mod;

        for (int i = 0; i < ConnectedParts.Count; i++)
        {
            if(!ConnectedParts[i]) continue;
            
            mod = ConnectedParts[i].GetFlyParameters();
            if (mod != null && mod.Type == ModifierType.Wings)
            {
                direction += mod.Direction;
                force += mod.Force;
                count++;
            }
        }
        
        planeDirection = direction.normalized;
        planeModificator = force / count;
    }
    
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

    public void AccelerateBody(Vector3 direction, float speed, float percentForce = 1)
    {
        // Body.AddForce(direction * LaunchPower.Power * accelerationModificator * percentForce, ForceMode.Acceleration);
        // Debug.Log(direction * LaunchPower.Power * accelerationModificator * percentForce);
        // Body.velocity += direction * LaunchPower.Power * accelerationModificator * percentForce;
        
        Vector3 dir = Body.transform.forward;
        dir.y = 0f;
        Body.velocity += -dir * speed * (1f + accelerationModificator) * percentForce;
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
        SetVisuals(!_TakeControl && getMotor() > 0);

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
        crossRight = Vector3.Cross(-crossUp, transform.forward);

        forwardVelocity = Vector3.Dot(velocity, crossForward);
        rightVelocity = Vector3.Dot(velocity, crossRight);
    
        groundRotation = Quaternion.LookRotation(crossForward, crossUp);

        float groundXAngle, groundYAngle, groundZAngle, targetX, targetY, targetZ;
        Vector3 globalRotateDelta = onGround ? globalRotateDeltaOnGround : globalRotateDeltaOffGround;

        if (PlayerController.Launched)
        {
            bodyVelocityLook = Quaternion.LookRotation(Body.velocity.normalized);
            
            groundXAngle = groundRotation.eulerAngles.x > 180f
                ? -(360f - groundRotation.eulerAngles.x)
                : groundRotation.eulerAngles.x;
            targetX = bodyVelocityLook.eulerAngles.x > 180f
                ? -(360f - bodyVelocityLook.eulerAngles.x)
                : bodyVelocityLook.eulerAngles.x;
            groundXAngle = Mathf.MoveTowards(groundXAngle,
                onGround ? groundRotation.eulerAngles.x : -targetX,
                globalRotateDelta.x * deltaTime);

            targetY = 180f;
            groundYAngle = Mathf.MoveTowards(Body.rotation.eulerAngles.y,
                targetY,
                globalRotateDelta.y * deltaTime);
            
            groundZAngle = groundRotation.eulerAngles.z > 180f
                ? -(360f - groundRotation.eulerAngles.z)
                : groundRotation.eulerAngles.z;
            targetZ = !onGround ? inputPlaneRotate * maxSlopeAngle : 0f;
            groundZAngle = Mathf.MoveTowards(groundZAngle, 
                targetZ, 
                globalRotateDelta.z * deltaTime);
        }
        else
        {
            groundXAngle = 0;
            groundYAngle = LaunchController.Rotate.eulerAngles.y;
            groundZAngle = 0;
        }

        Quaternion rotation = Quaternion.Euler(groundXAngle, groundYAngle, groundZAngle);
        SetRotation(rotation);
    
        // gravity
        gravityDirection = Vector3.zero;
        float gravityMultiplier = getMotor() > 0 ? gravityMultiplierOnMotor : 1f;
        
        switch (gravityMode)
        {
            case GRAVITY_MODE.AlwaysDown:
                gravityDirection = Vector3.down;
                break;
            case GRAVITY_MODE.TowardsGround:
                gravityDirection = onGround ? -crossUp : Vector3.down;
                break;
        }

        Vector3 motorDirection = onGround
            ? new Vector3(
                -Body.transform.TransformDirection(boostDirection.normalized).x,
                0f,
                -Body.transform.TransformDirection(boostDirection.normalized).z)
            : boostDirection.normalized;
        
        Vector3 accelerationMod = accelerationDirection.normalized * accelerationModificator;
        
        if (onGround)
        {
            if (motor == 0 || Mathf.Sign(motor) != Mathf.Sign(forwardVelocity))
            {
                velocity -= crossForward * Mathf.Sign(forwardVelocity) * 
                            Mathf.Min(Mathf.Abs(forwardVelocity), forwardFriction * scaleAdjustment * deltaTime) * 1.5f;
            }
            velocity -= crossRight * Mathf.Sign(rightVelocity) * Mathf.Min(Mathf.Abs(rightVelocity), lateralFriction * scaleAdjustment * deltaTime);
        }
        
        float slopeMultiplier = Mathf.Max(0, (Mathf.Sign(motor) * slopeDelta * slopeFriction * scaleAdjustment + 1f));
        float accelerationForce = 
            getAcceleration(0.5f * boostMultiplier * slopeMultiplier, 0.25f * boostMultiplier * slopeMultiplier) * boostModificator;
        
        velocity += motorDirection * getMotor() * accelerationForce * accelerationMod.z * (onGround ? boostMultiplierOnGround : 1f) * deltaTime;

        float rotateMultiplier = Mathf.Abs(inputPlaneRotate) > 0.05f ? planeRotateMultiplier : planeRotateBackMultiplier;
        float rotateDelta = rotateForce * rotateMultiplier * deltaTime;
        
        velocity.x = Mathf.Lerp(velocity.x, inputPlaneRotate * rotateVelocity, rotateDelta);
        
        velocity += gravityDirection * Mathf.Min(Mathf.Max(0, maxGravity + velocity.y), gravityVelocity * deltaTime)
            * (PlayerController.Launched ? 1f : 20f) * gravityMultiplier;

        if (planeDirection != Vector3.zero && !onGround)
        {
            velocity += gravityDirection * Vector3.Dot(crossForward, planeDirection) 
                * Mathf.Min(Mathf.Max(0, maxGravity + velocity.y), gravityVelocity * deltaTime) 
                * planeModificator * gravityMultiplier;
        }

        if (!PlayerController.Launched || !GameManager.GameStarted)
        {
            velocity.x = 0;
            if (onGround) velocity.y = 0;
            velocity.z = 0;
        }
        
        SetVelocity(velocity);
    }
    
    private Vector3[] defaultPosition;

    void SetVelocity(Vector3 velocity)
    {
        Body.velocity = velocity;
        
        if (ConnectedParts == null) return;
        
        if (defaultPosition == null)
        {
            defaultPosition = new Vector3[ConnectedParts.Count];
            for (int i = 0; i < ConnectedParts.Count; i++)
            {
                defaultPosition[i] = ConnectedParts[i].transform.localPosition;
            }
        }
        
        for (int i = 0; i < ConnectedParts.Count; i++)
        {
            if (ConnectedParts[i])
            {
                ConnectedParts[i].Body.transform.localPosition = defaultPosition[i];
                ConnectedParts[i].Body.velocity = velocity;
            }
        }
    }

    void SetVisuals(bool state)
    {
        if (ConnectedParts == null) return;
        
        for(int i = 0; i < ConnectedParts.Count; i++)
        {
            if (ConnectedParts[i])
            {
                ConnectedParts[i].VisualMode = state;
            }
        }
    }

    private Quaternion[] defaultRotation;

    void SetRotation(Quaternion rotation)
    {
        Body.MoveRotation(rotation);

        if (ConnectedParts == null) return;
        
        if (defaultRotation == null)
        {
            defaultRotation = new Quaternion[ConnectedParts.Count];
            for (int i = 0; i < ConnectedParts.Count; i++)
            {
                defaultRotation[i] = ConnectedParts[i].transform.localRotation;
            }
        }
        
        for (int i = 0; i < ConnectedParts.Count; i++)
        {
            if (ConnectedParts[i])
            {
                ConnectedParts[i].Body.transform.localRotation = defaultRotation[i];
            }
        }
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
    
    private float getAcceleration(float accelerationMultiplier = 1f, float speedMultiplier = 1f)
    {
        if (getMotor() == 0 || speedMultiplier == 0)
        {
            return brakeStrength;
        }
        else
        {
            float accelerationCurveDelta = Mathf.Abs(forwardVelocity) / (getMaxSpeed() * speedMultiplier);
            if (accelerationCurveDelta > 1) return -getMaxAcceleration() * accelerationMultiplier;
            if (accelerationCurveDelta < 0) return getMaxAcceleration() * accelerationMultiplier;
            return accelerationCurve.Evaluate(accelerationCurveDelta) * getMaxAcceleration() * accelerationMultiplier;
        }
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
