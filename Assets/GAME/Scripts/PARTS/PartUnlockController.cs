using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartUnlockController : MonoBehaviour
{
    [SerializeField] private float minFlyToTutorialUnlock = 50f;
    [SerializeField] private int minLevel = 1;
    
    [Space] 
    [SerializeField] private GridsUploads grids;
    [SerializeField] private GameObject gridsView;
    [SerializeField] private Slider flySlider;
    [SerializeField] private GridLayoutGroup icons;
    [SerializeField] private GameObject iconPrefab;
    
    [Space]
    [SerializeField] private PartType wheels;
    [SerializeField] private PartType wings;
    [SerializeField] private GameObject detailsView;
    [SerializeField] private Image detailSprite;
    [SerializeField] private TextMeshProUGUI detailTitle, detailTitle1;

    public void Refresh()
    {
        detailsView.SetActive(false);
        gridsView.SetActive(false);
        
        if (!PartUnlocked.Wheels && GameManager.FlyLength > minFlyToTutorialUnlock)
        {
            PartUnlocked.Wheels = true;
            SetDetails(wheels);
        }
        else if (!PartUnlocked.Wings && GameManager.FlyLength > minFlyToTutorialUnlock)
        {
            PartUnlocked.Wings = true;
            SetDetails(wings);
        }
        else if (PartUnlocked.Wheels && PartUnlocked.Wings)
        {
            if (grids.Stats[1].RequireDistance <= Records.MaxDistance)
            {
                PartUnlocked.Grids = true;
            }
            
            SetGrids();
        }
    }

    void SetGrids()
    {
        foreach (Transform VARIABLE in icons.transform)
        {
            Destroy(VARIABLE.gameObject);
        }

        GridsUploads.UploadStat[] Stats = grids.Stats;
        int count = Stats.Length;

        if (count == 0) return;
        
        detailsView.SetActive(false);
        gridsView.SetActive(true);

        GameObject icon;
        GridsUploads.UploadStat stat;
        
        for (int i = minLevel; i < count; i++)
        {
            icon = Instantiate(iconPrefab, icons.transform);
            stat = Stats[i];
            
            SetIcon(icon, stat.Sprite, stat.RequireDistance, stat.RequireDistance <= Records.MaxDistance);
        }

        RectTransform flySliderTransform = flySlider.GetComponent<RectTransform>();
        flySliderTransform.sizeDelta = new Vector2(icons.cellSize.x * (count - 1 - minLevel) + (icons.spacing.x + 5) * (count - 2 - minLevel), flySliderTransform.sizeDelta.y);

        flySlider.minValue = 0;
        flySlider.maxValue = 1;

        float space = 1f / count;
        
        int completed = 0;
        float requireDistance = 0;
        float distance = Records.MaxDistance;

        for(int i = minLevel; i < count; i++)
        {
            requireDistance = Stats[i].RequireDistance;
            
            if (distance - requireDistance <= 0)
            {
                requireDistance -= Stats[i - 1].RequireDistance;
                distance -= requireDistance;
                break;
            }

            completed++;
        }

        flySlider.value = space * completed + space * (distance / requireDistance);
    }

    void SetIcon(GameObject icon, Sprite sprite, float length, bool state)
    {
        Image image = icon.transform.GetChild(0).GetComponent<Image>();
        TextMeshProUGUI text = icon.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        Transform toggle = icon.transform.GetChild(2);

        image.sprite = sprite;
        text.text = $"{Mathf.RoundToInt(length).ToString()}m";
        
        toggle.GetChild(0).gameObject.SetActive(!state);
        toggle.GetChild(1).gameObject.SetActive(state);
    }

    void SetDetails(PartType type = null)
    {
        if (type == null)
        {
            detailsView.SetActive(false);
            return;
        }
        
        gridsView.SetActive(false);
        detailsView.SetActive(true);

        Sprite spr = type.GetPart(0).Sprite;
        string nm = type.name;

        detailSprite.sprite = spr;
        detailTitle.text = $"{nm} Available";
        detailTitle1.text = nm;
    }
}
