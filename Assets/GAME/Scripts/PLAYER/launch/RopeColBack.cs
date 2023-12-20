using System;
using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class RopeColBack : MonoBehaviour
{
    private PlayerController player => PlayerController.Instance;
    [SerializeField] private Transform toMove;
    [SerializeField] private Vector3 offset;

    [Space]
    [SerializeField] private bool ByCol = false;
    [SerializeField] private GameObject col;
    
    [Space] 
    [SerializeField] private bool ByDot = false;
    [SerializeField] private float dotSpace = 1.25f;
    [SerializeField] private GameObject rightDot;
    [SerializeField] private GameObject leftDot;
    
    [Space]
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private float particleHeight = -2f;

    private void Awake()
    {
        GameManager.OnGameStart += On;
        Off();
    }

    void On()
    {
        ChangeObjects(1);
        
        toMove.gameObject.SetActive(true);
        particle.gameObject.SetActive(true);
    }
    
    void Off()
    {
        toMove.gameObject.SetActive(false);
        particle.gameObject.SetActive(false);
        
        ChangeObjects(-1);
        toMove.localPosition = Vector3.zero;
    }

    void ChangeObjects(int index)
    {
        rightDot.SetActive(index > 0 && !ByCol && ByDot);
        leftDot.SetActive(index > 0 && !ByCol && ByDot);
            
        col.SetActive(index > 0 && ByCol && !ByDot);
    }
    
    void FixedUpdate()
    {
        if (PlayerController.Launched)
        {
            Off();
        }
        else
        {
            toMove.rotation = LaunchController.Rotate;
            
            if (ByDot)
            {
                rightDot.transform.localPosition = new Vector3(-dotSpace, 0, 0);
                leftDot.transform.localPosition = new Vector3(dotSpace, 0, 0);
            }
            
            if (Input.GetMouseButton(0))
            {
                if (ByCol)
                {
                    col.SetActive(true);
                }
                
                toMove.position = player.transform.position - LaunchController.CorrectPos() + offset;
            }
            else
            {
                if (ByCol)
                {
                    col.SetActive(false);
                }
            }
            
            particle.transform.position = new Vector3(
                player.transform.position.x,
                particleHeight,
                player.transform.position.z);
        }
    }
}
