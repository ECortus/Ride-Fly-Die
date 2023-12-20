using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

public class LaunchController : MonoBehaviour
{
    public static LaunchController Instance { get; private set; }

    [SerializeField] private CinemachineBrain brain;
    
    [Space]
    [SerializeField] private float maxSpace = 2f;
    [SerializeField] private float maxAngle = 60f;
    [Range(0f, 1f)]
    [SerializeField] private float minPercent = 0.15f;
    
    [Space]
    [SerializeField] private PlayerController toLaunch;
    
    [Space]
    [SerializeField] private GameObject animObject;
    [SerializeField] private Transform pillar1, pillar2, rope;
    [SerializeField] private float animTime;
    
    [Space]
    [SerializeField] private TextMeshProUGUI counterPercent;

    private Vector3 DefaultLaunchZonePosition { get; set; }
    
    [Inject] private void Awake()
    {
        GameManager.OnGameStart += StartLaunch;
        GameManager.OnMergeGame += Off;
        Instance = this;

        DefaultLaunchZonePosition = transform.position;
        
        Off();
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

    public static bool Blocked { get; set; }
    
    void Update()
    {
        if (!PlayerController.Launched && GameManager.GameStarted)
        {
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

                if (dirStartCurrent3 != Vector3.zero && !Blocked)
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
                
                    currentPos = dirStartCurrent3 * distanceStartCurrent + defaultPos;
                
                    SetRot(dirStartCurrent3);
                    SetPos(currentPos);
                
                    SetText(LaunchPercent);
                }
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                if (distanceStartCurrent / maxSpace > minPercent && (defaultPos - toLaunch.transform.position).magnitude > distanceStartCurrent - 0.05f)
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

    private async void On()
    {
        transform.position = DefaultLaunchZonePosition - CorrectPos();
        animObject.SetActive(true);

        rope.gameObject.SetActive(false);
        pillar1.localScale = Vector3.zero;
        pillar2.localScale = Vector3.zero;
        
        SetText(-1);

        pillar1.DOScale(Vector3.one, animTime).SetEase(Ease.OutBounce);
        pillar2.DOScale(Vector3.one, animTime).SetEase(Ease.OutBounce);

        await UniTask.Delay((int)(animTime * 1000));
        rope.gameObject.SetActive(true);
    }

    private void Off()
    {
        SetText(-1);
        animObject.SetActive(false);
        
        rope.gameObject.SetActive(false);
        pillar1.localScale = Vector3.zero;
        pillar2.localScale = Vector3.zero;
    }
    
    public void StartLaunch()
    {
        ResetLaunch();
        
        On();

        defaultPos = toLaunch.transform.position;
        defaultPos.y = -1f;

        PlayerController.Launched = false;
    }

    void Launch()
    {
        // if (Vector3.Angle(dirStartCurrent3, Vector3.right) <
        //     Vector3.Angle(dirStartCurrent3, -Vector3.right))
        // {
        //     angle *= -1;
        // }
        
        PlayerController.Instance.Launch(LaunchPercent, angle);
        
        // Off();
        
        // ResetPos();
        // Debug.Log("laucnhed");
    }
    
    void SetPos(Vector3 pos)
    {
        pos.y = toLaunch.transform.position.y;
        toLaunch.transform.position = pos;
        toLaunch.Body.position = pos;
    }
    
    public static Quaternion Rotate { get; private set; }

    void SetRot(Vector3 dir)
    {
        // toLaunch.transform.rotation = Quaternion.LookRotation(dir);   
        // toLaunch.Body.MoveRotation(Quaternion.Lerp(toLaunch.Body.rotation, Quaternion.LookRotation(dir), rotateSpeed * Time.fixedDeltaTime));
        Rotate = Quaternion.LookRotation(dir);
    }
    
    void ResetPos()
    {
        SetPos(defaultPos);
    }

    void SetText(float percent)
    {
        counterPercent.gameObject.SetActive(percent >= 0);
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
    
    public static Vector3 CorrectPos()
    {
        Vector3 pos = Vector3.zero;
        float offsetZ;
        int index = PlayerGrid.Instance.MainIndex;

        GridCell cell;

        for (int i = 0; i < 3; i++)
        {
            cell = PlayerGrid.Instance.GetByIndex(index - i);
            
            if (cell && cell.Part)
            {
                offsetZ = PlayerGrid.Instance.GetRequireLocalPosition(index - i).z;
                
                offsetZ -=
                    (cell.Part.Type.Category == PartCategory.Boost) ? 1f : 0f;
                // offsetZ +=
                //     (cell.Part.Type.Category == PartCategory.Wings) ? 2f : 0f;
                
                pos.z = offsetZ;
            }
        }
        
        return pos;
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
