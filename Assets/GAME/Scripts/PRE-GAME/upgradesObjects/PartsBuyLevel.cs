using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PartsBuyLevelObject", menuName = "Upgrades/PartsBuyLevel")]
public class PartsBuyLevel : UpgradeObject
{
    public override int Level => Upgrades.PartsBuyLevel;
    public int LevelUp => Upgrades.PartsBuyLevel;

    [Space] 
    [SerializeField] private float defaultCost = 18;
    [SerializeField] private float multipleCostPerLevel = 1.9f;

    public override int Cost => Mathf.RoundToInt(defaultCost * Mathf.Pow(multipleCostPerLevel, Upgrades.PartsBuyLevel));
    public override void Action() => Upgrades.IncreasePartsBuyLevel();
}
