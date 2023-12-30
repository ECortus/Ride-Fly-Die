using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParametersModifier
{
    public ModifierType Type { get; private set; }
    public float Force { get; private set; }
    public Vector3 Direction { get; private set; }
    public Vector3 LocalPosition { get; private set; }
    public float Mass { get; private set; }

    public ParametersModifier(ModifierType type, float force, Vector3 dir, Vector3 local, float mass)
    {
        Type = type;
        Force = force;
        Direction = dir;
        LocalPosition = local;
        Mass = mass;
    }
}

public enum ModifierType
{
    Default, Boost, Wheels, Wings
}
