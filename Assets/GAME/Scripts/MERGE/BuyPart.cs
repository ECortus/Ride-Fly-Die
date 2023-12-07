using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using Zenject;

public class BuyPart : MonoBehaviour
{
    [SerializeField] private BuyPartParameters BPT;

    private PartType[] TypesToBuy => BPT.Types;
    private float defaultPrice => BPT.DefaultPrice;
    private float multiple => BPT.Multiple;

    [Space]
    [SerializeField] private GameObject availableObject;
    [SerializeField] private GameObject disableObject;
    [SerializeField] private TextMeshProUGUI costText;

    private int BuyCount
    {
        get => PlayerPrefs.GetInt("BuyCount", 0);
        set
        {
            PlayerPrefs.SetInt("BuyCount", value);
            PlayerPrefs.Save();
        }
    }

    private int Cost => Mathf.RoundToInt(defaultPrice * Mathf.Pow(multiple, BuyCount));

    private void Awake()
    {
        Gold.OnValueChange += Refresh;
        MergeCell.OnUpdateState += Refresh;

        MergeGrid.OnAddPart += Refresh;
        
        Refresh();
    }

    public void Refresh()
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

        PartType type = TypesToBuy[Random.Range(0, TypesToBuy.Length)];
        int lvl = Upgrades.PartsBuyLevel;
        Part part = type.PartLevels[lvl];
        
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
