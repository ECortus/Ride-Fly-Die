using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeightUI : MonoBehaviour
{
    private PlayerController player => PlayerController.Instance;
    [SerializeField] private TextMeshProUGUI text;
    
    private void FixedUpdate()
    {
        text.text = $"{Mathf.RoundToInt(player.GetDistanceToGround())}m";
    }
}
