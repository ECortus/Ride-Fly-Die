using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public abstract class Instancer<T> : MonoBehaviour
    where T : Component
{
    [Inject] public static T Instance { get; set; }
    [Inject] void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        
        SetInstance();
    }

    protected abstract void SetInstance();
}
