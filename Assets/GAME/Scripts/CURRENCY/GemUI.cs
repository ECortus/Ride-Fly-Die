using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemUI : FloatingCounter
{
    protected override int resource => Gem.Value;
    
    public static GemUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        
        Instance = this;
        Gem.OnValueChange += Refresh;
    }
}
