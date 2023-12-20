using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create fly-reward")]
public class RewardForFly : ScriptableObject
{
    [SerializeField] private CurrencyAmount currencyUpg;
    
    [Space]
    [field: SerializeField] private float GoldPerMeter = 10f;
    [field: SerializeField] private float GemPerMeter = 10f;

    public int GoldReward(float lenght, int multiplier = 1) => (int)(GoldPerMeter * currencyUpg.GoldMultiple * lenght) * multiplier;
    public int GemReward(float lenght, int multiplier = 1) => (int)(GemPerMeter * currencyUpg.GemMultiple * lenght) * multiplier;
}
