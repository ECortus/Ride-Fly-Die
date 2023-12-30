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
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private float particleHeight = -2f;

    [Space] 
    [SerializeField] private ObiRope rope;

    private float defaultY;

    private bool isOn;

    private Vector3 Position
    {
        get
        {
            Vector3 pos = player.transform.TransformPoint(LaunchController.CorrectPos() + offset);
            pos.y = defaultY;
            return pos;
        }
    }

    private Quaternion Rotation
    {
        get
        {
            Quaternion rot = PlayerController.Instance.Body.rotation;
            Vector3 angles = rot.eulerAngles;
            angles.x = angles.z = 0;
            angles.y = (angles.y + 180f) % 360f;
            return Quaternion.Euler(angles);
        }
    }
    
    private void Awake()
    {
        defaultY = transform.position.y;
        
        // GameManager.OnGameStart += On;
        // GameManager.OnGameFinish += Off;
        
        Off();
    }

    public void On()
    {
        ResetPos();
        
        toMove.gameObject.SetActive(true);
        particle.gameObject.SetActive(true);
        
        isOn = true;
    }
    
    public void Off()
    {
        isOn = false;
        
        toMove.gameObject.SetActive(false);
        ResetPos();
        
        particle.gameObject.SetActive(false);
    }

    void ResetPos()
    {
        toMove.localEulerAngles = new Vector3(0, 0, 0);
        toMove.localPosition = Vector3.zero;
    }
    
    void LateUpdate()
    {
        if (!GameManager.GameStarted) return;
        
        if(isOn)
        {
            toMove.rotation = Rotation;
            toMove.position = Position;
            
            particle.transform.position = new Vector3(
                player.transform.position.x,
                particleHeight,
                player.transform.position.z);
            particle.transform.rotation = Quaternion.Euler(0,
                PlayerController.Instance.Body.rotation.eulerAngles.y + 180f,
                90f);
        }
    }
}
