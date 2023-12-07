using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Zenject;

public class VirtualCameraController : MonoBehaviour
{
    [Inject] public static VirtualCameraController Instance { get; private set; }

    [Inject] private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private CinemachineVirtualCamera[] Virtuals;
    private int index = 0;
    public CinemachineVirtualCamera CurrentVirtual => Virtuals[index];

    public void ChangeVirtualCamera(int ind)
    {
        index = ind;
        
        for (int i = 0; i < Virtuals.Length; i++)
        {
            if (index != i) Virtuals[i].gameObject.SetActive(false);
            else Virtuals[i].gameObject.SetActive(true);
        }
    }
}
