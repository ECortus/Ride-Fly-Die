using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create part parameters/Boost")]
public class BoostParameters : ScriptableObject
{
    [SerializeField] private float defaultMotorForce = 100f;
    [SerializeField] private float multiple = 1.2f;

    public float GetMotorForce(int lvl) => defaultMotorForce * Mathf.Pow(multiple, lvl);
}
