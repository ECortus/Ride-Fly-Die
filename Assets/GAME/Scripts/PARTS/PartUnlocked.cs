using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PartUnlocked
{
    public static bool Wheels
    {
        get => PlayerPrefs.GetInt("WheelsUnlocked", 0) != 0;
        set
        {
            PlayerPrefs.SetInt("WheelsUnlocked", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    
    public static bool Wings
    {
        get => PlayerPrefs.GetInt("WingsUnlocked", 0) != 0;
        set
        {
            PlayerPrefs.SetInt("WingsUnlocked", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    
    public static bool Grids
    {
        get => PlayerPrefs.GetInt("GridsUnlocked", 0) != 0;
        set
        {
            PlayerPrefs.SetInt("GridsUnlocked", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
