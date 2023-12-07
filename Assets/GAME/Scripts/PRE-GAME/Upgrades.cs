using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Upgrades
{
    public static int LaunchPower
    {
        get => PlayerPrefs.GetInt("LaunchPower", 0);
        private set
        {
            PlayerPrefs.SetInt("LaunchPower", value);
            PlayerPrefs.Save();
        }
    }

    public static void IncreaseLaunchPower() => LaunchPower++;
    public static void ResetLaunchPower() => LaunchPower = 0;
    
    public static int CurrencyAmount
    {
        get => PlayerPrefs.GetInt("CurrencyAmount", 0);
        private set
        {
            PlayerPrefs.SetInt("CurrencyAmount", value);
            PlayerPrefs.Save();
        }
    }
    
    public static void IncreaseCurrencyAmount() => CurrencyAmount++;
    public static void ResetCurrencyAmount() => CurrencyAmount = 0;
    
    public static int PartsBuyLevel
    {
        get => PlayerPrefs.GetInt("PartsBuyLevel", 0);
        private set
        {
            PlayerPrefs.SetInt("PartsBuyLevel", value);
            PlayerPrefs.Save();
        }
    }
    
    public static void IncreasePartsBuyLevel() => PartsBuyLevel++;
    public static void ResetPartsBuyLevel() => PartsBuyLevel = 0;
}
