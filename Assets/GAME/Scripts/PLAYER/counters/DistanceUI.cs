using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DistanceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    
    private void FixedUpdate()
    {
        text.text = $"{Mathf.RoundToInt(GameManager.FlyLength)}m";
    }
}
