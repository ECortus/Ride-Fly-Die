using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BarUI : MonoBehaviour
{
    protected virtual float Amount { get; }
    protected virtual float MaxAmount { get; }

    [SerializeField] private Slider slider;

    public void Refresh()
    {
        slider.minValue = 0f;
        slider.maxValue = MaxAmount;

        slider.value = Amount;
    } 
}
