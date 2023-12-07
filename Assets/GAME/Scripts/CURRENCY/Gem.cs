using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Gem
{
    public static Action OnValueChange { get; set; }
    
    public static int Value
    {
        get => PlayerPrefs.GetInt("Gem", 0);
        private set
        {
            PlayerPrefs.SetInt("Gem", value);
            PlayerPrefs.Save();
        }
    }

    public static void Plus(int amount)
    {
        Value += amount;
        OnValueChange?.Invoke();
    }

    public static void Minus(int amount)
    {
        Value -= amount;
        OnValueChange?.Invoke();
    }
}
