using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create part parameters/Wings")]
public class WingsParameters : ScriptableObject
{
    [Range(0f, 1f)]
    [SerializeField] private float defaultFlyModifier = 0.1f;
    [Range(0f, 1f)]
    [SerializeField] private float multiple = 0.2f;

    public float Multiplier(int lvl) => lvl * multiple;
    public float GetFlyModifier(int lvl) => defaultFlyModifier * (1f + Multiplier(lvl));
}
