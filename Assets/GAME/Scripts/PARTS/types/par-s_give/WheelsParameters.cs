using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create part parameters/Wheels")]
public class WheelsParameters : ScriptableObject
{
    [Range(0f, 1f)]
    [SerializeField] private float defaultAccelerationModifier = 0.1f;
    [Range(0f, 1f)]
    [SerializeField] private float multiple = 0.2f;

    public float Multiplier(int lvl) => lvl * multiple;
    public float GetAccelerationModifier(int lvl) => defaultAccelerationModifier * (1f + Multiplier(lvl));
}
