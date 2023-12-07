using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParametersModifier
{
    public float Force { get; private set; }
    public Vector3 Direction { get; private set; }
    public ModifierType Type { get; private set; }

    public ParametersModifier(ModifierType type, float force, Vector3 dir)
    {
        Type = type;
        Force = force;
        Direction = dir;
    }
}

public enum ModifierType
{
    Default, Boost, Wheels, Wings
}
