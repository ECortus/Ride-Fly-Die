using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public static class ConnectedParts
{
    public static Action OnUpdate;
    
    public static List<Part> List = new List<Part>();
    
    private static PartType _cabins, _grids, _fans, _wheels, _wings;
    private static GridsUploads _gridsUploads;

    private static readonly Vector3 DefaultBoostDirection = new Vector3(0, 0, -1);
    private static readonly Vector3 DefaultAccelerationDirection = new Vector3(0, 0, -1);
    private static readonly Vector3 DefaultPlaneDirection = new Vector3(0, 0, 1);
    
    public static Vector3 BoostDirection = new Vector3(0, 0, -1);
    public static Vector3 AccelerationDirection = new Vector3(0, 0, -1);
    public static Vector3 PlaneDirection = new Vector3(0, 0, 1);

    private static readonly float DefaultBoost = 0.1f;
    private static readonly float DefaultAcceleration = 1f;
    private static readonly float DefaultPlane = 0.05f;

    public static float BoostModificator = 0.1f;
    public static float AccelerationModificator = 1f;
    public static float PlaneModificator = 0.05f;
    public static float BalanceCenter = 0;
    public static float Balance = 0;
    public static float Mass = 0f;

    public static readonly float PlaneParameterRelativity = 3f;

    public static float MaxBoostModificator
    {
        get
        {
            if (!_cabins) _cabins = Resources.Load<PartType>("PARTS/Cabin");
            if (!_grids) _grids = Resources.Load<PartType>("PARTS/Grid");
            if (!_fans) _fans = Resources.Load<PartType>("PARTS/Boost");
            if (!_wheels) _wheels = Resources.Load<PartType>("PARTS/Wheels");
            if (!_wings) _wings = Resources.Load<PartType>("PARTS/Wings");
            
            return _fans.GetPart(4).GetFlyParameters().Force + DefaultBoost;
        }
    }

    public static float MaxDistanceModificator
    {
        get
        {
            if (!_cabins) _cabins = Resources.Load<PartType>("PARTS/Cabin");
            if (!_grids) _grids = Resources.Load<PartType>("PARTS/Grid");
            if (!_fans) _fans = Resources.Load<PartType>("PARTS/Boost");
            if (!_wheels) _wheels = Resources.Load<PartType>("PARTS/Wheels");
            if (!_wings) _wings = Resources.Load<PartType>("PARTS/Wings");
            
            return (_wheels.GetPart(4).GetFlyParameters().Force + DefaultAcceleration) / PlaneParameterRelativity
                   + (_wings.GetPart(4).GetFlyParameters().Force + DefaultPlane) * PlaneParameterRelativity;
        }
    }
    
    public static float MaxMass
    {
        get
        {
            if (!_cabins) _cabins = Resources.Load<PartType>("PARTS/Cabin");
            if (!_grids) _grids = Resources.Load<PartType>("PARTS/Grid");
            if (!_fans) _fans = Resources.Load<PartType>("PARTS/Boost");
            if (!_wheels) _wheels = Resources.Load<PartType>("PARTS/Wheels");
            if (!_wings) _wings = Resources.Load<PartType>("PARTS/Wings");

            int max = 4;
            
            float cabin = _cabins.GetPart(max).Mass;
            float grid = _grids.GetPart(max).Mass;
            float fan = _fans.GetPart(max).Mass;
            float wheels = _wheels.GetPart(max).Mass;
            float wings = _wings.GetPart(max).Mass;
            
            return cabin * 1 + grid * 2 + Mathf.Max(new [] { fan, wheels, wings }) * 4 + wheels * 3 + wings * 3;
        }
    }

    public static float GravityRelativity
    {
        get
        {
            if (!_cabins) _cabins = Resources.Load<PartType>("PARTS/Cabin");
            return Mathf.Pow(Mathf.Clamp(Mass, _cabins.GetPart(0).Mass, 999f) / MaxMass, 1/100f);
        }
    }

    public static void Add(Part part)
    {
        if (!List.Contains(part))
        {
            List.Add(part);
            Debug.Log("Added - " + part);
        }
        else
        {
            Debug.Log("Updated - " + part);
        }
        
        ParametersModifier mod = part.GetFlyParameters();
        CalculateParameters(mod);
    }
    
    public static void Remove(Part part)
    {
        if (List.Contains(part))
        {
            List.Remove(part);
            ParametersModifier mod = part.GetFlyParameters();
            CalculateParameters(mod);
            
            Debug.Log("Removed - " + part);
        }
    }

    public static void Clear()
    {
        List.Clear();

        BoostModificator = DefaultBoost;
        AccelerationModificator = DefaultAcceleration;
        PlaneModificator = DefaultPlane;
        Mass = 0;
        BalanceCenter = 0;
        Balance = 0;
        
        BoostDirection = DefaultBoostDirection;
        AccelerationDirection = DefaultAccelerationDirection;
        PlaneDirection = DefaultPlaneDirection;
        
        OnUpdate?.Invoke();
    }

    public static Vector3[] DefaultPositions;
    public static Quaternion[] DefaultRotations;

    public static void CalculatePosRot()
    {
        DefaultPositions = new Vector3[List.Count];
        DefaultRotations = new Quaternion[List.Count];

        for (int i = 0; i < List.Count; i++)
        {
            DefaultPositions[i] = List[i].transform.localPosition;
            DefaultRotations[i] = List[i].transform.localRotation;
            
            // Debug.Log("Part - " + List[i].name + ", def rot - " + DefaultRotations[i].eulerAngles);
        }
    }
    
    static async void CalculateParameters(ParametersModifier mod)
    {
        if (mod == null) return;
        
        if (mod.Type == ModifierType.Boost)
        {
            CalculateBoost();
        }
        else if (mod.Type == ModifierType.Wheels)
        {
            CalculateAcceleration();
        }
        else if (mod.Type == ModifierType.Wings)
        {
            CalculatePlane();
        }

        // await UniTask.Delay(100);
        
        CalculateMass();
        CalculateBalance();
        
        CalculatePosRot();
        
        OnUpdate?.Invoke();
    }

    static void CalculateBoost()
    {
        Vector3 direction = Vector3.zero;
        float force = 0f;
        int count = 0;

        Part part;
        ParametersModifier mod;

        for (int i = 0; i < List.Count; i++)
        {
            part = List[i];
            if (part)
            {
                mod = part.GetFlyParameters();
                if (mod != null && mod.Type == ModifierType.Boost)
                {
                    direction += mod.Direction;
                    force += mod.Force;
                    count++;
                }
            }
        }

        BoostDirection = (direction + (count > 0 ? Vector3.zero : DefaultBoostDirection)).normalized;
        BoostModificator = (count > 0 ? force / count : 0) + DefaultBoost;
        
        // Debug.Log("BD - " + BoostDirection + ", BM - " + BoostModificator + ", MDM - " + MaxBoostModificator);
    }

    static void CalculateAcceleration()
    {
        Vector3 direction = Vector3.zero;
        float force = 0f;

        Part part;
        ParametersModifier mod;

        for (int i = 0; i < List.Count; i++)
        {
            part = List[i];
            if (part)
            {
                mod = part.GetFlyParameters();
                if (mod != null && mod.Type == ModifierType.Wheels)
                {
                    direction += mod.Direction;
                    force += mod.Force;
                }
            }
        }
        
        AccelerationDirection = (direction + DefaultAccelerationDirection * DefaultAcceleration).normalized;
        AccelerationModificator = force + DefaultAcceleration;
    }

    static void CalculatePlane()
    {
        Vector3 direction = Vector3.zero;
        float force = 0f;
        int count = 0;

        Part part;
        ParametersModifier mod;

        for (int i = 0; i < List.Count; i++)
        {
            part = List[i];
            if (part)
            {
                mod = part.GetFlyParameters();
                if (mod != null && mod.Type == ModifierType.Wings)
                {
                    direction += mod.Direction;
                    force += mod.Force;
                    count++;
                }
            }
        }
        
        PlaneDirection = (direction + DefaultPlaneDirection * DefaultPlane).normalized;
        PlaneModificator = (count > 0 ? force / count : 0f) + DefaultPlane;
    }

    static void CalculateBalance()
    {
        float balance = 0;

        Part part;
        
        int mainIndex = PlayerGrid.Instance.MainIndex;
        BalanceCenter = 0f;
        
        int count = 3;
        for (int i = 0; i < count; i++)
        {
            part = PlayerGrid.Instance.GetByIndex(mainIndex + 1 - i).Part;
            if (part && (part.Type.Category == PartCategory.Grid || part.Type.Category == PartCategory.Cabin))
            {
                if (i == 3 - 1 || i == 0)
                {
                    BalanceCenter += part.GetFlyParameters().LocalPosition.z / 2;
                }
                else
                {
                    BalanceCenter += part.GetFlyParameters().LocalPosition.z;
                }
            }
        }
        
        ParametersModifier mod;
        float z;
        
        for (int i = 0; i < List.Count; i++)
        {
            part = List[i];
            if (part)
            {
                mod = List[i].GetFlyParameters();
                if (mod != null)
                {
                    z = mod.LocalPosition.z - BalanceCenter;
                    z = Mathf.Clamp(z, -1, 1);
                    balance += z * mod.Mass;
                }
            }
        }

        if (Mass != 0)
        {
            balance /= Mass;
            Balance = balance;
        }
        else
        {
            Balance = 0;
        }
    }

    static void CalculateMass()
    {
        float mass = 0;
        
        Part part;
        ParametersModifier mod;
        
        for (int i = 0; i < List.Count; i++)
        {
            part = List[i];
            if (part)
            {
                mod = part.GetFlyParameters();
                if (mod != null 
                    //&& part.Type.Category != PartCategory.Cabin && part.Type.Category != PartCategory.Grid
                    )
                {
                    mass += mod.Mass;
                }
            }
        }

        Mass = mass;
    }
}