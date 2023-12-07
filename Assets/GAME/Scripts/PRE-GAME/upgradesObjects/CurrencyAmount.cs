using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CurrencyAmountObject", menuName = "Upgrades/CurrencyAmount")]
public class CurrencyAmount : UpgradeObject
{
    public override int Level => Upgrades.CurrencyAmount;

    [Space]
    [Range(1f, 2f)]
    [SerializeField] private float goldMultiple = 1.1f;
    [Range(1f, 2f)]
    [SerializeField] private float gemMultiple = 1.05f;

    public float GoldMultiple => Mathf.Pow(goldMultiple,Upgrades.CurrencyAmount);
    public float GemMultiple => Mathf.Pow(gemMultiple,Upgrades.CurrencyAmount);

    [Space] 
    [SerializeField] private float defaultCost = 18;
    [SerializeField] private float multipleCostPerLevel = 1.9f;

    public override int Cost => Mathf.RoundToInt(defaultCost * Mathf.Pow(multipleCostPerLevel, Upgrades.CurrencyAmount));
    public override void Action() => Upgrades.IncreaseCurrencyAmount();
}
