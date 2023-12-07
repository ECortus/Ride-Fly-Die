using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Records
{
    public static float MaxDistance
    {
        get => PlayerPrefs.GetFloat("MaxDistance_Record", 0);
        private set
        {
            PlayerPrefs.SetFloat("MaxDistance_Record", value);
            PlayerPrefs.Save();
        }
    }

    public static void RecordMaxDistance(float distance)
    {
        MaxDistance = distance;
    }
}
