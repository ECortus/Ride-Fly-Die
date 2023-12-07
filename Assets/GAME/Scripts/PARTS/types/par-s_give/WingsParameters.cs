using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create part parameters/Wings")]
public class WingsParameters : ScriptableObject
{
    [Range(0f, 1f)]
    [SerializeField] private float defaultFlyModifier = 0.1f;
    [SerializeField] private float multiple = 1.2f;

    public float GetFlyModifier(int lvl) => defaultFlyModifier * Mathf.Pow(multiple, lvl);
}
