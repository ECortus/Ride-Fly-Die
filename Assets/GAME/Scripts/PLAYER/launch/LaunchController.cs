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
    [SerializeField] private float maxRotate = 45f;
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

    [Space] 
    [SerializeField] private RopeColBack _ropeColBack;

    [Space] 
    [SerializeField] private GameObject ropePrefab;
    [SerializeField] private Transform ropeParent;

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
            if (Input.GetMouseButtonDown(0) && (dirStartCurrent3 == Vector3.zero || Vector3.Angle(-Vector3.forward, dirStartCurrent3) <= maxAngle) && isOn)
            {
                startMousePos = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0) && startMousePos != Vector2.zero && isOn)
            {
                currentMousePos = Input.mousePosition;
                
                dirStartCurrent = (currentMousePos - startMousePos).normalized;
                dirStartCurrent3 = new Vector3(dirStartCurrent.x, 0, dirStartCurrent.y);
                
                distanceStartCurrent = (currentMousePos - startMousePos).magnitude / 100f;
                
                if (Vector3.Angle(-Vector3.forward, dirStartCurrent3) <= maxAngle)
                {
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
            }
            else if (Input.GetMouseButtonUp(0) && isOn)
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
            else
            {
                ResetPos();
                SetRot(-Vector3.forward);
                SetText(0f);
            }
        }
    }

    private bool isOn = false;

    private async void On()
    {
        isOn = false;
        
        rope.gameObject.SetActive(false);
        pillar1.localScale = Vector3.zero;
        pillar2.localScale = Vector3.zero;
        
        transform.position = DefaultLaunchZonePosition - CorrectPos();
        animObject.SetActive(true);
        
        SetText(-1);

        pillar1.DOScale(Vector3.one, animTime).SetEase(Ease.OutBounce);
        pillar2.DOScale(Vector3.one, animTime).SetEase(Ease.OutBounce);

        await UniTask.Delay((int)(animTime * 1000));
        rope.gameObject.SetActive(true);

        if (ropeParent.childCount == 0)
        {
            GameObject rp = Instantiate(ropePrefab, ropeParent);
            rp.SetActive(true);
        }
        
        _ropeColBack.On();
        
        counterPercent.gameObject.SetActive(true);
        
        isOn = true;
    }

    private void Off()
    {
        Destroy(ropeParent.GetChild(0).gameObject);
        
        SetText(-1);
        animObject.SetActive(false);
        
        _ropeColBack.Off();
        
        counterPercent.gameObject.SetActive(false);
        
        rope.gameObject.SetActive(false);
        pillar1.localScale = Vector3.zero;
        pillar2.localScale = Vector3.zero;
    }
    
    public void StartLaunch()
    {
        ResetLaunch();
        On();

        defaultPos = toLaunch.transform.position;
        // defaultPos.y = -1f;
        
        PlayerController.Launched = false;
    }

    void Launch()
    {
        // if (Vector3.Angle(dirStartCurrent3, Vector3.right) <
        //     Vector3.Angle(dirStartCurrent3, -Vector3.right))
        // {
        //     angle *= -1;
        // }
        
        _ropeColBack.Off();
        PlayerController.Instance.Launch(LaunchPercent, angle);
        
        // Off();
        
        // ResetPos();
        // Debug.Log("laucnhed");
    }
    
    void SetPos(Vector3 pos)
    {
        pos.y = toLaunch.transform.position.y;
        toLaunch.transform.position = pos;
        toLaunch.Body.position = toLaunch.transform.position;
    }
    
    public static Quaternion Rotate { get; private set; }

    void SetRot(Vector3 dir)
    {
        // toLaunch.transform.rotation = Quaternion.LookRotation(dir);   
        // toLaunch.Body.MoveRotation(Quaternion.Lerp(toLaunch.Body.rotation, Quaternion.LookRotation(dir), rotateSpeed * Time.fixedDeltaTime));
        // dir = toLaunch.transform.TransformDirection(dir);
        dir.y = 0;
        Quaternion rot = Quaternion.LookRotation(dir);

        if (angle > maxRotate)
        {
            rot = Quaternion.LookRotation(DirectionFromAngle(180f, maxRotate * Mathf.Clamp(angle, -1f, 1f)));
        }

        Rotate = rot;

        // Debug.DrawRay(toLaunch.transform.position, -dir * 999f, Color.cyan);
    }
    
    void ResetPos()
    {
        SetPos(defaultPos);
    }

    void SetText(float percent)
    {
        // counterPercent.gameObject.SetActive(percent >= 0);
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
        float offsetZ = 0;
        int index = PlayerGrid.Instance.MainIndex;

        GridCell cell;

        for (int i = 0; i < 3; i++)
        {
            cell = PlayerGrid.Instance.GetByIndex(index - i);
            
            if (cell && cell.Part)
            {
                if (cell.Part.Type.Category == PartCategory.Boost 
                    || cell.Part.Type.Category == PartCategory.Wheels
                    || cell.Part.Type.Category == PartCategory.Wings)
                {
                    offsetZ += 0.5f;
                }
                else
                {
                    offsetZ = PlayerGrid.Instance.GetRequireLocalPosition(index - i).z - 0.75f;
                }
                
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

    // private void OnDrawGizmos()
    // {
    //     if (debugToLaunch)
    //     {
    //         Gizmos.color = Color.blue;
    //
    //         Vector3 pos = debugToLaunch.position;
    //         Vector3 dir = DirectionFromAngle(0, maxAngle / 2);
    //         
    //         Gizmos.DrawLine(pos, pos + (dir * 15f));
    //         dir = DirectionFromAngle(0, -maxAngle / 2);
    //         Gizmos.DrawLine(pos, pos + (dir * 15f));
    //     }
    // }
}
