using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DG.Tweening;
using UnityEngine;

public class RewardAfterFly : MonoBehaviour
{
    [Serializable]
    public struct RouletteSlot
    {
        public int Multiplier;
        [Range(0f, 1f)]
        public float Probability;
    }

    private float AngleOffset
    {
        get
        {
            if (Slots.Length == 0) return 0;
            
            float angle = 0;
            float fullProbability = 0;
            
            foreach (var VARIABLE in Slots)
            {
                fullProbability += VARIABLE.Probability;
            }

            foreach (var VARIABLE in Slots)
            {
                angle += fullAngle * VARIABLE.Probability / fullProbability;
            }

            return -angle / 2;
        }
    }

    [SerializeField] private float fullAngle = 60f;
    [SerializeField] private float speed;
    [Range(0f, 15f)] 
    [SerializeField] private float approx = 0;
    
    [Space]
    [SerializeField] private RouletteSlot[] Slots;

    [Space] 
    [SerializeField] private Transform anglesRef;
    
    [Space]
    [SerializeField] private GameObject defaultObject;
    [SerializeField] private GameObject rewardObject;

    [Space] 
    [SerializeField] private Transform noThanks;

    private WinMenu _winMenu;

    public bool IsOn = false;

    private float Angle { get; set; }
    private int Sign;
    private float Target;

    private int Multiplier;

    private float noThanksTime;
    
    private void Awake()
    {
        _winMenu = GetComponentInParent<WinMenu>();

        // GameManager.OnGameFinish += On;
        // GameManager.OnMergeGame += Off;
    }

    public void On()
    {
        noThanks.DOKill();
        noThanks.localScale = Vector3.zero;
        
        defaultObject.SetActive(!Tutorial.Completed || GameManager.FlyLength <= 15f);
        rewardObject.SetActive(Tutorial.Completed && GameManager.FlyLength > 15f);

        if (rewardObject.activeSelf)
        {
            noThanksTime = 3f;
        }
        else
        {
            noThanksTime = 999999f;
        }

        IsOn = Tutorial.Completed;
        Angle = 0;
        Sign = 1;
        
        Target = fullAngle / 2 * Sign;
        Multiplier = 1;
    }

    public void Off()
    {
        noThanks.DOKill();
        IsOn = false;
    }

    void Update()
    {
        if (IsOn)
        {
            if (noThanks)
            {
                if (noThanksTime <= 0)
                {
                    noThanks.DOScale(Vector3.one, 0.35f);
                    noThanksTime = 999999f;
                }

                noThanksTime -= Time.deltaTime;
            }
            
            if (Mathf.Abs(Angle) > Mathf.Abs(Target) - approx)
            {
                if (Angle >= 0) Sign = -1;
                else Sign = 1;
                
                Target = fullAngle / 2 * Sign;
            }
            
            Angle = Mathf.MoveTowards(Angle, Target, speed * Time.deltaTime);
            SetMultiplier();
            
            anglesRef.localEulerAngles = new Vector3(0, 0, Angle);
        }
    }

    void SetMultiplier()
    {
        float angle = AngleOffset;

        foreach (var VARIABLE in Slots)
        {
            if (Angle >= angle && Angle <= angle + fullAngle * VARIABLE.Probability)
            {
                Multiplier = VARIABLE.Multiplier;
                break;
            }
            
            angle += fullAngle * VARIABLE.Probability;
        }
    }
    
    public void GetReward()
    {
        _winMenu.OffWithReward(Multiplier);
    }

    private void OnDrawGizmos()
    {
        if (anglesRef == null) return;
        
        Gizmos.color = Color.green;

        float angle = AngleOffset;
        
        float fullProbability = 0;
            
        foreach (var VARIABLE in Slots)
        {
            fullProbability += VARIABLE.Probability;
        }
        
        foreach (var VARIABLE in Slots)
        {
            Gizmos.DrawLine(anglesRef.position, anglesRef.position
                + new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad), 0) * 999f);
            
            angle += fullAngle * VARIABLE.Probability / fullProbability; 
        }
        
        Gizmos.DrawLine(anglesRef.position, anglesRef.position
            + new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad), 0) * 999f);
    }
}
