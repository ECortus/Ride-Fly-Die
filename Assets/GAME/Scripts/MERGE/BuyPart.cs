using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using Zenject;

public class BuyPart : MonoBehaviour
{
    public static BuyPart Instance { get; private set; }
    
    [SerializeField] private BuyPartParameters BPT;

    private PartType[] TypesToBuy => BPT.Types;
    private float defaultPrice => BPT.DefaultPrice;
    private float multiple => BPT.Multiple;

    [Space]
    [SerializeField] private GameObject availableObject;
    [SerializeField] private GameObject disableObject;
    [SerializeField] private TextMeshProUGUI costText;

    private static PartType RequirePartToBuy { get; set; }
    
    public static void SetPartToBuy(PartType type)
    {
        RequirePartToBuy = type;
    }

    public static void NullPartToBuy() => RequirePartToBuy = null;
    
    public static void SetEnable(bool state)
    {
        
    }

    private int BuyCount
    {
        get => PlayerPrefs.GetInt("BuyCount", 0);
        set
        {
            PlayerPrefs.SetInt("BuyCount", value);
            PlayerPrefs.Save();
        }
    }

    public static int Cost => Instance._cost;
    private int _cost => Mathf.RoundToInt(defaultPrice * Mathf.Pow(multiple, BuyCount));

    private void Awake()
    {
        Instance = this;
        
        Gold.OnValueChange += Refresh;
        MergeCell.OnUpdateState += Refresh;

        MergeGrid.OnAddPart += Refresh;
        
        Refresh();
    }

    private void Refresh()
    {
        costText.text = $"{Cost}";
        if (Cost <= Gold.Value && MergeGrid.FreeCount != 0)
        {
            ChangeObject(true);
        }
        else
        {
            ChangeObject(false);
        }
    }
    
    private void Buy()
    {
        BuyCount++;

        PartType type;
        Part part;
        
        if (RequirePartToBuy)
        {
            type = RequirePartToBuy;
        }
        else
        {
            type = TypesToBuy[Random.Range(0, TypesToBuy.Length)];
        }
        
        int lvl = Upgrades.PartsBuyLevel;
        part = type.PartLevels[lvl];
        
        MergeGrid.Instance.SpawnPart(part);
        Refresh();
    }

    public void OnButtonClick_Buy()
    {
        if (MergeGrid.FreeCount == 0) return;
        
        int freeCount = PlayerPrefs.GetInt("FreeParts");
        if (freeCount > 0)
        {
            PlayerPrefs.SetInt("FreeParts", freeCount - 1);
            Buy();
            return;
        }

        if (Gold.Value >= Cost)
        {
            Gold.Minus(Cost);
            Buy();
        }
    }

    public void OnButtonClick_RewardBuy()
    {
        Buy();
    }

    void ChangeObject(bool state)
    {
        availableObject.SetActive(state);
        disableObject.SetActive(!state);
    }
}
