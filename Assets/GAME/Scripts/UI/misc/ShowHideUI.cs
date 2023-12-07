using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShowHideUI : MonoBehaviour
{
    public float showSpeed = 5f;
    [HideInInspector] public bool isShown;

    void Start()
    {
        isShown = transform.localScale.x >= 1f ? true : false;
    }

    public IEnumerator ShowProcess() 
    {
        if(isShown)
        {
            yield return null;
            StopAllCoroutines();
        }

        transform.localScale = Vector3.zero;

        while(transform.localScale.x < 1f)
        {
            transform.localScale += new Vector3(
                showSpeed * Time.unscaledDeltaTime, showSpeed * Time.unscaledDeltaTime, showSpeed * Time.unscaledDeltaTime
            );

            if(!gameObject.activeInHierarchy)
            {
                isShown = true;
                transform.localScale = Vector3.one;
                break;
            }
        
            yield return null;
        }

        isShown = true;
        transform.localScale = Vector3.one;

        yield return null;
    }

    public IEnumerator HideProcess() 
    {
        if(!isShown)
        {
            yield return null;
            StopAllCoroutines();
        }

        while(transform.localScale.x > 0f)
        {
            transform.localScale -= new Vector3(
                showSpeed * Time.unscaledDeltaTime, showSpeed * Time.unscaledDeltaTime, showSpeed * Time.unscaledDeltaTime
            );

            if(!gameObject.activeInHierarchy)
            {
                isShown = false;
                transform.localScale = Vector3.zero;
                break;
            }

            yield return null;
        }

        isShown = false;
        transform.localScale = Vector3.zero;

        yield return null;
    }
}
