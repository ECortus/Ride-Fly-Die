using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Gold
{
    public static Action OnValueChange { get; set; }
    
    public static int Value
    {
        get => PlayerPrefs.GetInt("Gold", 0);
        private set
        {
            PlayerPrefs.SetInt("Gold", value);
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
