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
    [SerializeField] private GameObject adObject;

    private int currentCost => upObject.Cost;

    private bool HaveAdRV = false;

    void Awake()
    {
        GameManager.OnMergeGame += RefreshAdRV;
        GameManager.OnMergeGame += Refresh;
        Gem.OnValueChange += Refresh;
        
        RefreshAdRV();
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

    public void OnButtonClickRV()
    {
        HaveAdRV = false;
            
        upObject.Action();
        Refresh();
    }

    void RefreshAdRV()
    {
        HaveAdRV = Tutorial.Completed;
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
        
        if (currentCost > Gem.Value)
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
        noObject.SetActive(!state && !HaveAdRV);
        adObject.SetActive(!state && HaveAdRV);
        
        enoughObject.SetActive(state);
    }
}
