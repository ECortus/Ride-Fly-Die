using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeightUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    
    private void FixedUpdate()
    {
        text.text = $"{Mathf.RoundToInt(GameManager.FlyHeight)}m";
    }
}
