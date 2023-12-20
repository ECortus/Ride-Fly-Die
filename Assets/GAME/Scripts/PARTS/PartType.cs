using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PartType", menuName = "Create Part Type", order = 0)]
public class PartType : ScriptableObject
{
    [field: SerializeField] public PartCategory Category { get; private set; }
    [field: SerializeField] public Part[] PartLevels { get; private set; }
    public Part GetPart(int level) => PartLevels[Mathf.Clamp(level, 0, PartLevels.Length - 1)];

    public int MaxLevel => PartLevels.Length - 1;
}

public enum PartCategory
{
    Default, Cabin, Grid, Wings, Boost, Wheels
}
