using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButtonUI : MonoBehaviour
{
    private TextMeshProUGUI costText;

    [SerializeField] private UpgradeObject upObject;

    [Space] 
    [SerializeField] private GameObject enoughObject;
    [SerializeField] private GameObject noObject;

    private int currentCost => upObject.Cost;

    void Awake()
    {
        GameManager.OnMergeGame += Refresh;
        Gem.OnValueChange += Refresh;
        
        Refresh();
    }

    public void OnButtonClick()
    {
        if (Gem.Value >= currentCost)
        {
            Gem.Minus(currentCost);
            upObject.Action();
            
            Refresh();
        }
    }
    
    public void Refresh()
    {
        if (!costText)
        {
            costText = transform.Find("cost").GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (upObject.Level >= upObject.MaxLevel)
        {
            ChangeObject(false);
            costText.text = "---";
            return; 
        }
        
        if (currentCost >= Gem.Value)
        {
            ChangeObject(false);
        }
        else
        {
            ChangeObject(true);
        }
        
        costText.text = $"{currentCost}";
    }

    void ChangeObject(bool state)
    {
        enoughObject.SetActive(state);
        noObject.SetActive(!state);
    }
}
