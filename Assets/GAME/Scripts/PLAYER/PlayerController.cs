using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public AircraftEngine engine;
    [SerializeField] private LaunchPower launchPower;
    [Space] 
    [SerializeField] private GameObject countersUI;
    
    public static Action OnRepair { get; set; }
    public void Repair() => OnRepair?.Invoke();
    
    public static Action OnCrash { get; set; }
    public void Crash() => OnCrash?.Invoke();
    
    public static bool Launched { get; set; }
    public static int Multiplier { get; private set; }
    public static void SetMultiplier(int mult) => Multiplier = mult;
    
    public static Transform Follow { get; private set; }

    public Vector3 Forward => -transform.forward;
    public Vector3 Down => -transform.up;

    public Rigidbody Body => engine.Body;
    public Vector3 Center => engine.Center;
    public void TakeControl() => AircraftEngine.SetControl(true);
    public void OffControl() => AircraftEngine.SetControl(false);
    
    public float GetDistanceToGround() => engine.GetDistanceToGround();
    
    void Awake()
    {
        Instance = this;
        Follow = transform.Find("follow");
        
        OnCrash += PlayerCrash;
        OnRepair += PlayerRepair;

        // GameManager.OnGameStart += ConnectedParts.CalculatePosRot;
        GameManager.OnGameStart += OnGameStart;
        
        GameManager.OnMergeGame += TakeControl;
        GameManager.OnMergeGame += ResetBody;
        GameManager.OnMergeGame += Repair;

        GameManager.OnGameFinish += Crash;
        
        countersUI.SetActive(false);
    }

    public async void PlayBarrelRoll(float time)
    {
        if (GameManager.GameStarted && Launched && !engine.onGround && !AircraftEngine.BlockRotation)
        {
            AircraftEngine.BlockRotation = true;
            ResetMouse();

            await engine.MakeBarrelRoll(time);
            
            ResetMouse();
            AircraftEngine.BlockRotation = false;
        }
    }

    public void AccelerateForward(float speed)
    {
        engine.AccelerateBody(speed);
    }
    
    public void AccelerateForwardForTime(float speed, float time)
    {
        _accelerationCoroutine ??= StartCoroutine(AccelerateForTime(speed, time));
    }

    void StopAccelerateForwardForTime()
    {
        if (_accelerationCoroutine != null)
        {
            StopCoroutine(_accelerationCoroutine);
            _accelerationCoroutine = null;
        }
    }

    private Coroutine _accelerationCoroutine;
    IEnumerator AccelerateForTime(float speed, float limit)
    {
        float time = limit;

        while (time > 0)
        {
            engine.AccelerateBody(speed);
            time -= Time.fixedDeltaTime;
            yield return null;
        }

        _accelerationCoroutine = null;
    }

    public void Launch(float percent = 1f, float angle = 0f)
    {
        Launched = true;
        OffControl();
        
        engine.inputPlaneRotate = 0;
        startMousePosition = Vector3.positiveInfinity;

        Vector3 direction = DirectionFromAngle(180f, angle);
        // Vector3 direction = Forward;
        direction.y = 0;
        engine.AccelerateBody(launchPower.Power, true, percent);
        
        countersUI.SetActive(true);
    }

    private Vector3 startMousePosition, currentMousePosition;
    public float mouseRotateInput { get; private set; }
    
    [SerializeField] private float mouseLength = 6f;
    [SerializeField] private float sphereRadius = 6f;

    [Header("DEBUG")]
    public List<Part> Parts;
    public ConnectedParts.DefaultTransformValue[] DefaultValues;

    void FixedUpdate()
    {
        Parts = ConnectedParts.List;
        DefaultValues = ConnectedParts.DefaultTransforms;
        
        if (AircraftEngine.BlockRotation) return;
        
        if (GameManager.GameStarted && Launched)
        {
            // if (Body.velocity.z < 0.05f && engine.onGround)
            // {
            //     ForceFinish();
            //     return;
            // }
            
            if (Input.GetMouseButtonDown(0))
            {
                startMousePosition = Input.mousePosition;
                return;
            }
            
            if (Input.GetMouseButton(0) && startMousePosition != Vector3.zero)
            {
                if (AircraftEngine.BlockRotation)
                {
                    ResetMouse();
                }
                else
                {
                    currentMousePosition = Input.mousePosition;
                    mouseRotateInput = (currentMousePosition.x - startMousePosition.x) / 100f / mouseLength;

                    engine.inputPlaneRotate = Mathf.Clamp(mouseRotateInput, -1f, 1f);
                }
                
                engine.setMotor(2);
                return;
            }
            
            // if (Input.GetMouseButtonUp(0))
            // {
            //     ResetMouse();
            //     engine.setMotor(0);
            // }
            
            ResetMouse();
            engine.setMotor(0);
        }
        else
        {
            ResetMouse();
            engine.setMotor(0);
        }
    }

    public void ForceFinish()
    {
        GameManager.OnGameFinish -= Crash;
        GameManager.FinishGame();
        GameManager.OnGameFinish += Crash;
    }

    void ResetMouse()
    {
        mouseRotateInput = 0;
        engine.inputPlaneRotate = 0;
        
        startMousePosition = Vector3.zero;
        currentMousePosition = Vector3.zero;
    }
    
    public void SpawnToPos(Vector3 pos)
    {
        Body.transform.position = pos;
        engine.SetVelocity(Vector3.zero);
        ResetBody();
    }

    public void CorrectSphereColliderCenter()
    {
        engine.Sphere.radius = sphereRadius;
        
        Vector3 center = new Vector3(0f, -0.5f, 0f);

        GridCell[] cells = PlayerGrid.Instance._cells;
        
        int size = PlayerGrid.Instance.Size;

        int lastrow = 0;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (cells[i * 5 + j].Part)
                {
                    lastrow = i;
                }
            }
        }

        center.y += (lastrow - 2) * -1.5f + (engine.Sphere.radius - 0.5f);

        float z = (2 - ConnectedParts.BalanceCenter) * ConnectedParts.Balance;
        center.z = z + ConnectedParts.BalanceCenter;
        
        engine.Sphere.center = center;
        
        // Debug.Log("Balance - " + ConnectedParts.Balance);
        // Debug.Log("Center - " + ConnectedParts.BalanceCenter);
    }
    
    void OnGameStart()
    {
        ConnectedParts.CalculatePosRot();
        AircraftEngine.BlockRotation = false;
        
        countersUI.SetActive(false);
        CorrectSphereColliderCenter();
        
        Body.isKinematic = false;
        
        Body.velocity = Vector3.zero;
        Body.angularVelocity = Vector3.zero;
        
        engine.enabled = true;
        OffControl();

        // engine.SetParts(GetComponentsInChildren<Part>());
        
        // Body.velocity = Vector3.zero;
        // Body.angularVelocity = Vector3.zero;
        
        Launched = false;
        
        Multiplier = 1;
    }
    
    void PlayerRepair()
    {
        StopAccelerateForwardForTime();
        
        Body.isKinematic = true;
        engine.enabled = false;
        
        Launched = true;
    }

    void PlayerCrash()
    {
        StopAccelerateForwardForTime();
        
        Body.isKinematic = true;
        engine.enabled = false;

        // engine.Sphere.radius = 0.5f;
        Launched = true;
    }

    private void ResetBody()
    {
        engine.Sphere.radius = 0.5f;
        Body.isKinematic = true;
        
        engine.inputPlaneRotate = 0;
        engine.Clear();
        
        engine.SetRotation(Quaternion.Euler(new Vector3(0, 180f, 0)));
    }
    
    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
