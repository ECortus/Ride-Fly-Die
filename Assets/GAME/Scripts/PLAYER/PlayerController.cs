using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] private AircraftEngine engine;
    [SerializeField] private Transform partsParent;
    
    public static Action OnRepair { get; set; }
    public void Repair() => OnRepair?.Invoke();
    
    public static Action OnCrash { get; set; }
    public void Crash() => OnCrash?.Invoke();
    
    public static bool Launched { get; set; }
    
    public static Transform Follow { get; private set; }

    public Vector3 Forward => -transform.forward;
    public Vector3 Down => -transform.up;

    public Rigidbody Body => engine.Body;
    public Vector3 Center => engine.Center;
    public void TakeControl() => engine.SetControl(true);
    public void OffControl() => engine.SetControl(false);
    
    public float GetDistanceToGround() => engine.GetDistanceToGround();

    public void AddModifier(ParametersModifier mod) => engine.AddModifier(mod);
    public void RemoveModifier(ParametersModifier mod) => engine.RemoveModifier(mod);

    void Awake()
    {
        Instance = this;

        Follow = transform.Find("follow");
        
        OnCrash += PlayerCrash;
        OnRepair += PlayerRepair;
        
        GameManager.OnGameStart += OnGameStart;
        
        GameManager.OnMergeGame += TakeControl;
        GameManager.OnMergeGame += ResetBody;
        GameManager.OnMergeGame += Repair;
        GameManager.OnMergeGame += engine.ClearModifiers;

        GameManager.OnGameFinish += Crash;

        partsArray = new Part[0];
    }

    public void Launch(float percent = 1f, float angle = 0f)
    {
        Launched = true;
        OffControl();

        Vector3 direction = DirectionFromAngle(180f, angle);
        // Vector3 direction = -Forward;
        direction.y = 0;
        engine.AccelerateBody(direction, percent);
    }

    private Part[] partsArray;

    private Vector3 startMousePosition, currentMousePosition;
    private Vector3 mouseDirection => (currentMousePosition - startMousePosition).normalized;
    private float mouseRotateInput;
    private float mouseLength = 15f;

    void FixedUpdate()
    {
        if (GameManager.GameStarted && Launched)
        {
            if (Input.GetMouseButtonDown(0))
            {
                startMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0) && startMousePosition != Vector3.positiveInfinity)
            {
                currentMousePosition = Input.mousePosition;
                mouseRotateInput = ((currentMousePosition - startMousePosition).magnitude / 100f) / mouseLength;
                
                if (Vector3.Angle(mouseDirection, Vector3.right) > Vector3.Angle(mouseDirection, -Vector3.right))
                {
                    mouseRotateInput *= -1f;
                }
                
                engine.inputPlaneRotate = Mathf.Clamp(engine.inputPlaneRotate, -1f, 1f);
                engine.setMotor(2);
                
                SetPartVisuals(true);
            }
            else
            {
                engine.inputPlaneRotate = 0f;
                startMousePosition = Vector3.positiveInfinity;
                engine.setMotor(0);
                
                SetPartVisuals(false);
            }
        }
        else
        {
            engine.setMotor(0);
            SetPartVisuals(false);
        }
    }

    void SetPartVisuals(bool state)
    {
        foreach (var VARIABLE in partsArray)
        {
            if (VARIABLE)
            {
                VARIABLE.VisualMode = state;
            }
        }
    }

    public void SpawnToPos(Vector3 pos)
    {
        Body.transform.position = pos;
        ResetBody();
    }

    void CorrectSphereColliderCenter()
    {
        Vector3 center = new Vector3(0f, -0.51f, 0f);
        int index = PlayerGrid.Instance.MainIndex;

        GridCell[] cells = PlayerGrid.Instance._cells;
        
        int size = PlayerGrid.Instance.Size;

        int first = -1;
        int last = -1;
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
        
        for (int i = 0; i < size; i++)
        {
            if (cells[lastrow * 5 + i].Part)
            {
                if (first == -1)
                {
                    first = lastrow * 5 + i;
                }
                last = lastrow * 5 + i;
            }
        }

        center.y += (lastrow - 2) * -1.5f;
        
        // int indexMiddle = lastrow * 5 + 2;
        // float indexCenter = (last + first) / 2f;
        // float distanceToMiddle = indexMiddle - indexCenter;
        // center.z = distanceToMiddle * 2f;
        
        engine.Sphere.center = center;
    }
    
    void OnGameStart()
    {
        CorrectSphereColliderCenter();
        
        Body.isKinematic = false;
        engine.enabled = true;
        OffControl();

        partsArray = GetComponentsInChildren<Part>();
        
        // Body.velocity = Vector3.zero;
        // Body.angularVelocity = Vector3.zero;
        
        Launched = false;
    }
    
    void PlayerRepair()
    {
        Body.isKinematic = true;
        engine.enabled = false;
        
        Launched = true;
    }

    void PlayerCrash()
    {
        Body.isKinematic = true;
        engine.enabled = false;

        Launched = true;
    }

    private void ResetBody()
    {
        Body.isKinematic = true;
        
        Body.transform.eulerAngles = new Vector3(0, 180, 0);
        transform.eulerAngles = new Vector3(0, 180, 0);
    }
    
    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
