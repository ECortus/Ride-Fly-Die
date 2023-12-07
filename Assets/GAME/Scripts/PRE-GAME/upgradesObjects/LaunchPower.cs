using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LaunchPowerObject", menuName = "Upgrades/LaucnhPower")]
public class LaunchPower : UpgradeObject
{
    public override int Level => Upgrades.LaunchPower;
    
    [Space]
    [SerializeField] private float powerDefault = 100f;
    [Range(1f, 2f)]
    [SerializeField] private float powerMultiple = 1.2f;

    public float Power => powerDefault * Mathf.Pow(powerMultiple, Upgrades.LaunchPower);

    [Space] 
    [SerializeField] private float defaultCost = 25;
    [SerializeField] private float multipleCostPerLevel = 1.8f;

    public override int Cost => Mathf.RoundToInt(defaultCost * Mathf.Pow(multipleCostPerLevel, Upgrades.LaunchPower));
    public override void Action() => Upgrades.IncreaseLaunchPower();
}
