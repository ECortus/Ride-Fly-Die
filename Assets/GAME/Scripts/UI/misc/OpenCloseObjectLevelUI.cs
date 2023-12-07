using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCloseObjectLevelUI : ShowHideUI
{
    public virtual void Open()
    {
        if(isShown) return;
        StartCoroutine(ShowProcess());
    }

    public virtual void Close()
    {
        if(!isShown) return;
        StartCoroutine(HideProcess());
    }
}
