using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create part parameters/Boost")]
public class BoostParameters : ScriptableObject
{
    [SerializeField] private float defaultMotorForce = 100f;
    [Range(0f, 1f)]
    [SerializeField] private float multiple = 0.2f;

    public float Multiplier(int lvl) => lvl * multiple;
    public float GetMotorForce(int lvl) => defaultMotorForce * (1f + Multiplier(lvl));
}
