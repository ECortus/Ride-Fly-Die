using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class DoubleBarUI : MonoBehaviour
{
    private float AmountReal
    {
        get => sliderReal.value;
        set => sliderReal.value = value;
    }
    private float AmountBack
    {
        get => sliderBack.value;
        set => sliderBack.value = value;
    }
    
    protected abstract float Amount { get; }
    protected abstract float MaxAmount { get; }

    public bool isOn => gameObject.activeSelf && _coroutine == null;
    private Coroutine _coroutine;

    [SerializeField] private Slider sliderReal;
    [SerializeField] private float realSpeed = 5f;
        
    [Space]    
    [SerializeField] private Slider sliderBack;
    [SerializeField] private float backSpeed = 3f;

    public void Refresh()
    {
        if (gameObject.activeInHierarchy)
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            _coroutine = StartCoroutine(Refreshing());
        }
        else
        {
            AmountReal = Amount;
            AmountBack = Amount;
        }
    }

    private float backDelay = 0;
    private readonly float backDelayValue = 0.75f;

    IEnumerator Refreshing()
    {
        RefreshMinMax();

        while (!gameObject.activeInHierarchy)
        {
            if (sliderBack.maxValue != MaxAmount)
            {
                RefreshMinMax();
            }
            
            if (Mathf.Abs(AmountBack - Amount) > 0.1f)
            {
                if (Mathf.Abs(AmountBack - Amount) <= 0.8f)
                {
                    backDelay = backDelayValue;
                    
                    AmountReal = Amount;
                    AmountBack = Amount;
                }
                
                if (Amount > AmountBack)
                {
                    if (backDelay <= 0f) AmountReal = Mathf.Lerp(AmountReal, Amount, backSpeed * Time.deltaTime);
                    else backDelay -= Time.deltaTime;

                    AmountBack = Mathf.Lerp(AmountBack, Amount, realSpeed * Time.deltaTime);
                }
                else
                {
                    AmountReal = Mathf.Lerp(AmountReal, Amount, realSpeed * Time.deltaTime);
                    
                    if (backDelay <= 0f) AmountBack = Mathf.Lerp(AmountBack, Amount, backSpeed * Time.deltaTime);
                    else backDelay -= Time.deltaTime;
                }
            }
            else
            {
                backDelay = backDelayValue;

                AmountReal = Amount;
                AmountBack = Amount;
            }
            
            yield return null;
        }
        
        AmountReal = Amount;
        AmountBack = Amount;
        yield return null;
    }

    public void RefreshMinMax()
    {
        sliderReal.minValue = 0f;
        sliderReal.maxValue = MaxAmount;
        
        sliderBack.minValue = 0f;
        sliderBack.maxValue = MaxAmount;
    }
}
