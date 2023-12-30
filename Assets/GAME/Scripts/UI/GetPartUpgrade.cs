using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using DG.Tweening;

public class GetPartUpgrade : MonoBehaviour
{
    private static GetPartUpgrade Instance { get; set; }
    
    [SerializeField] private Image menuBg;
    [SerializeField] private GameObject menuObject;
    
    [Space]
    [SerializeField] private Image sprite;
    [SerializeField] private Transform detailParent, claim;
    [SerializeField] private TextMeshProUGUI titleText, levelText, description;

    [Space] 
    [SerializeField] private BoostParameters boost;
    [SerializeField] private WheelsParameters wheels;
    [SerializeField] private WingsParameters wings;

    private float alpha;

    private static int WingsLevelUnlocked
    {
        get => PlayerPrefs.GetInt("WingsLevelUnlocked", 0);
        set
        {
            PlayerPrefs.SetInt("WingsLevelUnlocked", value);
            PlayerPrefs.Save();
        }
    }
    private static int FanLevelUnlocked
    {
        get => PlayerPrefs.GetInt("FanLevelUnlocked", 0);
        set
        {
            PlayerPrefs.SetInt("FanLevelUnlocked", value);
            PlayerPrefs.Save();
        }
    }
    
    private static int WheelsLevelUnlocked
    {
        get => PlayerPrefs.GetInt("WheelsLevelUnlocked", 0);
        set
        {
            PlayerPrefs.SetInt("WheelsLevelUnlocked", value);
            PlayerPrefs.Save();
        }
    }

    private static bool Condition(PartType type, int level)
    {
        PartCategory category = type.Category;

        if (category == PartCategory.Boost)
        {
            return FanLevelUnlocked < level;
        }
        
        if (category == PartCategory.Wheels)
        {
            return WheelsLevelUnlocked < level;
        }
        
        if (category == PartCategory.Wings)
        {
            return WingsLevelUnlocked < level;
        }

        return false;
    }
    
    void Awake()
    {
        alpha = menuBg.color.a;
        Instance = this;
    }

    public static void ShowUpgrade(PartType type, int level)
    {
        if (Condition(type, level))
        {
            Instance._ShowUpgrade(type, level);
        }
    }
    
    private async void _ShowUpgrade(PartType type, int level)
    {
        HideMenu();
        menuObject.SetActive(true);
        
        levelText.text = $"Level 0{(level + 1).ToString()}";
        
        if (type.Category == PartCategory.Boost)
        {
            titleText.text = $"NEW ENGINE UNLOCKED";
            description.text = $"Takeoff speed increased by {Mathf.RoundToInt(boost.Multiplier(level) * 100).ToString()}%";

            FanLevelUnlocked = level;
        }
        else if (type.Category == PartCategory.Wings)
        {
            titleText.text = $"NEW WINGS UNLOCKED";
            description.text = $"Flight distance increased by {Mathf.RoundToInt(wings.Multiplier(level) * 100).ToString()}%";
            
            WingsLevelUnlocked = level;
        }
        else if (type.Category == PartCategory.Wheels)
        {
            titleText.text = $"NEW WHEELS UNLOCKED";
            description.text = $"Flight speed increased by {Mathf.RoundToInt(wheels.Multiplier(level) * 100).ToString()}%";
            
            WheelsLevelUnlocked = level;
        }
        
        sprite.sprite = type.GetPart(level).Sprite;
        
        await menuBg.DOColor(new Color(menuBg.color.r, menuBg.color.g, menuBg.color.b, alpha), 0.5f).AsyncWaitForCompletion();
        await titleText.transform.DOScale(Vector3.one, 0.25f).AsyncWaitForCompletion();
        
        await detailParent.DOScale(Vector3.one, 0.3f).AsyncWaitForCompletion();
        
        claim.DOScale(Vector3.one, 0.35f);
    }

    public void Off()
    {
        HideMenu();
        menuObject.SetActive(false);
    }
    
    void HideMenu()
    {
        menuBg.color = new Color(menuBg.color.r, menuBg.color.g, menuBg.color.b, 0f);
        titleText.transform.localScale = Vector3.zero;
        detailParent.localScale = Vector3.zero;
        claim.localScale = Vector3.zero;
    }
}
