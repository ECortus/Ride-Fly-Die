using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SettingsModes
{
    private static readonly string VibrationName = "Vibration";
    public static bool Vibration
    {
        get => PlayerPrefs.GetInt(VibrationName, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(VibrationName, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    
    private static readonly string VolumeName = "Volume";
    public static bool Volume
    {
        get => PlayerPrefs.GetInt(VolumeName, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(VolumeName, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
