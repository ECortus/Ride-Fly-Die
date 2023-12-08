using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeletePart : MonoBehaviour
{
    public static bool Selected { get; private set; }

    [SerializeField] private GameObject selected, notSelected;

    private void OnEnable()
    {
        selected.SetActive(false);
        notSelected.SetActive(true);
    }

    private void OnMouseOver()
    {
        if (Part.DragedPart)
        {
            Selected = true;
            
            selected.SetActive(true);
            notSelected.SetActive(false);
        }
        else
        {
            OnMouseExit();
        }
    }

    private void OnMouseExit()
    {
        Selected = false;
        
        selected.SetActive(false);
        notSelected.SetActive(true);
    }
}
