using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpeedUI : MonoBehaviour
{
    private Rigidbody rb => PlayerController.Instance.Body;
    [SerializeField] private TextMeshProUGUI text;
    
    void FixedUpdate()
    {
        text.text = $"{Mathf.RoundToInt(rb.velocity.magnitude)} km/h";
    }
}
