using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LaunchController : MonoBehaviour
{
    [SerializeField] private float maxSpace = 2f;
    [SerializeField] private float maxAngle = 60f;
    [Range(0f, 1f)]
    [SerializeField] private float minPercent = 0.15f;
    
    [Space]
    [SerializeField] private PlayerController toLaunch;
    [SerializeField] private Transform fromLaunchCoord;
    
    [Space]
    [SerializeField] private TextMeshProUGUI counterPercent;
    
    private void Awake()
    {
        GameManager.OnGameStart += StartLaunch;
        counterPercent.gameObject.SetActive(false);
    }
    
    private float LaunchPercent => distanceStartCurrent / maxSpace;
    
    private Vector3 launchFrom;
    private Vector3 launchCurrent;

    private Vector3 defaultPos, currentPos;

    private Vector2 startMousePos, currentMousePos, dirStartCurrent;
    private Vector3 dirStartCurrent3;

    private float angle;
    private float distanceStartCurrent;
    private float angleToDir;

    private Vector3 velocity;
    
    void Update()
    {
        if (!PlayerController.Launched && GameManager.GameStarted)
        {
            velocity = toLaunch.Body.velocity;
            velocity.x = 0;
            velocity.z = 0;
            toLaunch.Body.velocity = velocity;
            toLaunch.Body.angularVelocity = Vector3.zero;
            
            if (Input.GetMouseButtonDown(0) && (dirStartCurrent3 == Vector3.zero || Vector3.Angle(-Vector3.forward, dirStartCurrent3) <= maxAngle))
            {
                startMousePos = Input.mousePosition;
            }
            
            if (Input.GetMouseButton(0) && startMousePos != Vector2.zero)
            {
                currentMousePos = Input.mousePosition;
                
                dirStartCurrent = (currentMousePos - startMousePos).normalized;
                dirStartCurrent3 = new Vector3(dirStartCurrent.x, 0, dirStartCurrent.y);
                
                distanceStartCurrent = (currentMousePos - startMousePos).magnitude / 100f;
                
                if (Vector3.Angle(-Vector3.forward, dirStartCurrent3) > maxAngle)
                {
                    dirStartCurrent3 = Vector3.zero;
                }
                
                if (distanceStartCurrent > maxSpace)
                {
                    distanceStartCurrent = maxSpace;
                }

                if (dirStartCurrent3 != Vector3.zero)
                {
                    angle = Vector3.Angle(-Vector3.forward, dirStartCurrent3);
                    if (angle > maxAngle / 2)
                    {
                        angleToDir = maxAngle / 2;

                        if (Vector3.Angle(dirStartCurrent3, Vector3.right) <
                            Vector3.Angle(dirStartCurrent3, -Vector3.right))
                        {
                            angleToDir *= -1;
                        }
                    
                        dirStartCurrent3 = DirectionFromAngle(180f, angleToDir);
                        angle = angleToDir;
                    }
                
                    currentPos = dirStartCurrent3 * distanceStartCurrent + fromLaunchCoord.position;
                
                    SetRot(dirStartCurrent3);
                    SetPos(currentPos);
                
                    SetText(LaunchPercent);
                }
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                if (distanceStartCurrent / maxSpace > minPercent)
                {
                    Launch();
                }
                else
                {
                    ResetPos();
                    SetRot(-Vector3.forward);
                    SetText(0f);
                }
            }
        }
    }
    
    void StartLaunch()
    {
        ResetLaunch();
        
        defaultPos = fromLaunchCoord.position;
        defaultPos.y = toLaunch.transform.position.y;
        counterPercent.gameObject.SetActive(true);
        SetText(0);

        PlayerController.Launched = false;
    }

    void Launch()
    {
        counterPercent.gameObject.SetActive(false);
        
        if (Vector3.Angle(dirStartCurrent3, Vector3.right) <
            Vector3.Angle(dirStartCurrent3, -Vector3.right))
        {
            angle *= -1;
        }
        
        PlayerController.Instance.Launch(LaunchPercent, angle);
        
        ResetLaunch();
        
        // ResetPos();
        // Debug.Log("laucnhed");
    }
    
    void SetPos(Vector3 pos)
    {
        pos.y = toLaunch.transform.position.y;
        toLaunch.transform.position = pos;
        toLaunch.Body.position = pos;
    }

    void SetRot(Vector3 dir)
    {
        toLaunch.transform.rotation = Quaternion.LookRotation(dir);   
    }
    
    void ResetPos()
    {
        SetPos(defaultPos);
    }

    void SetText(float percent)
    {
        counterPercent.text = $"{Mathf.RoundToInt(percent * 100f)}%";
    }

    void ResetLaunch()
    {
        startMousePos = Vector2.zero;
        currentMousePos = Vector2.zero;
        
        dirStartCurrent3 = Vector3.zero;
        angle = 0;
        currentPos = Vector3.zero;
        distanceStartCurrent = 0;
    }
    
    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void OnDrawGizmos()
    {
        if (toLaunch)
        {
            Gizmos.color = Color.blue;

            Vector3 dir = DirectionFromAngle(180f, angle / 2);
            Gizmos.DrawLine(toLaunch.transform.position, toLaunch.transform.position + (dir * 15f));
            dir = DirectionFromAngle(180f, -angle / 2);
            Gizmos.DrawLine(toLaunch.transform.position, toLaunch.transform.position + (dir * 15f));
        }
    }
}
