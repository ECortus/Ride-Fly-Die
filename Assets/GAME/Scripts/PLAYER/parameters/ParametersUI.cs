using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ParametersUI : BarUI
{
    [SerializeField] private ParameterObject obj;

    protected override float Amount => obj.Value;
    protected override float MaxAmount => obj.MaxValue;

    private void Awake()
    {
        ConnectedParts.OnUpdate += Refresh;
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDestroy()
    {
        ConnectedParts.OnUpdate -= Refresh;
    }
}
