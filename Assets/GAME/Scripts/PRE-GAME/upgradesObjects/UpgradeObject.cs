using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UpgradeObject : ScriptableObject
{
    [field: SerializeField] public int MaxLevel { get; private set; }
    
    public abstract int Level { get; }
    public abstract int Cost { get; }
    public abstract void Action();
}
