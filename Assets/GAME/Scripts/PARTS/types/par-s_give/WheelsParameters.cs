using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create part parameters/Wheels")]
public class WheelsParameters : ScriptableObject
{
    [Range(0f, 1f)]
    [SerializeField] private float defaultAccelerationModifier = 0.1f;
    [SerializeField] private float multiple = 1.2f;

    public float GetAccelerationModifier(int lvl) => defaultAccelerationModifier * Mathf.Pow(multiple, lvl);
}
