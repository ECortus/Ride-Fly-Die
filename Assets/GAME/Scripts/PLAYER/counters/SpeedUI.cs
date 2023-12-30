using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpeedUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    
    void FixedUpdate()
    {
        text.text = $"{Mathf.RoundToInt(GameManager.FlySpeed).ToString()} km/h";
    }
}
