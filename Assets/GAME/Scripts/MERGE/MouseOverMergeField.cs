using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverMergeField : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    public static bool Is;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Is = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Is = false;
    }
}
