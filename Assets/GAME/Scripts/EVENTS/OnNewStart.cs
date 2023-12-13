using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnNewStart : MonoBehaviour
{
    private bool NewStart
    {
        get => PlayerPrefs.GetInt("IsNewStart", 0) == 0;
        set
        {
            PlayerPrefs.SetInt("IsNewStart", value ? 0 : 1);
            PlayerPrefs.Save();
        }
    }
    
    [SerializeField] private PartType wheels;
    
    void Awake()
    {
        if (NewStart)
        {
            NewStart = false;
            // MergeGrid.Instance.SpawnPart(wheels.GetPart(0));
        }
    }
}
