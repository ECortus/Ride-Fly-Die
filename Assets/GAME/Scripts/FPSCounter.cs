using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private float timer, refresh, avgFramerate;
    private TextMeshProUGUI m_Text;
 
    private void Start()
    {
        m_Text = GetComponent<TextMeshProUGUI>();
    }
    
    private void Update()
    {
        //Change smoothDeltaTime to deltaTime or fixedDeltaTime to see the difference
        float timelapse = Time.smoothDeltaTime;
        timer = timer <= 0 ? refresh : timer -= timelapse;
 
        if(timer <= 0) avgFramerate = (int) (1f / timelapse);
        m_Text.text = avgFramerate.ToString();
    }
}
