using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldUI : FloatingCounter
{
    protected override int resource => Gold.Value;
    
    public static GoldUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        
        Instance = this;
        Gold.OnValueChange += Refresh;
    }
}
