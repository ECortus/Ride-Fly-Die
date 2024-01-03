using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class WinMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuObject;
    [SerializeField] private Image menuBg;
    [SerializeField] private Transform menuTitle;

    [Space] 
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI gemText, flyText;

    [Space]
    [SerializeField] private RewardForFly rewards;
    [SerializeField] private RewardAfterFly getRewardUI;
    [SerializeField] private PartUnlockController partUnlock;
    
    private float flyLength => GameManager.FlyLength;
    private int goldReward, gemReward;

    private float alpha;

    void Awake()
    {
        //GameManager.OnMergeGame += Off;
        GameManager.OnGameFinish += On;

        alpha = menuBg.color.a;
    }
    
    private async void On()
    {
        int multiplier = PlayerController.Multiplier;
        // Debug.Log(multiplier + " - reward multiplier");
        
        goldReward = rewards.GoldReward(flyLength, multiplier);
        gemReward = rewards.GemReward(flyLength, multiplier);
        
        HideMenu();
        menuObject.SetActive(true);

        goldText.text = $"{goldReward.ToString()}";
        gemText.text = $"{gemReward.ToString()}";
        flyText.text = $"{Mathf.RoundToInt(flyLength).ToString()}m";

        await menuBg.DOColor(new Color(menuBg.color.r, menuBg.color.g, menuBg.color.b, alpha), 0.5f).AsyncWaitForCompletion();
        await menuTitle.DOScale(Vector3.one, 0.25f).AsyncWaitForCompletion();
        
        partUnlock.Refresh();
        await partUnlock.transform.DOScale(Vector3.one, 0.3f).AsyncWaitForCompletion();
        
        await flyText.transform.DOScale(Vector3.one, 0.35f).AsyncWaitForCompletion();
        await goldText.transform.DOScale(Vector3.one, 0.35f).AsyncWaitForCompletion();
        await gemText.transform.DOScale(Vector3.one, 0.35f).AsyncWaitForCompletion();

        getRewardUI.On();
        getRewardUI.transform.DOScale(Vector3.one, 0.35f);

        Reward();
    }

    void HideMenu()
    {
        menuBg.DOKill();
        menuTitle.DOKill();
        partUnlock.transform.DOKill();
        flyText.transform.DOKill();
        goldText.transform.DOKill();
        gemText.transform.DOKill();
        getRewardUI.transform.DOKill();
        
        menuBg.color = new Color(menuBg.color.r, menuBg.color.g, menuBg.color.b, 0f);
        menuTitle.localScale = Vector3.zero;
        
        flyText.transform.localScale = Vector3.zero;
        goldText.transform.localScale = Vector3.zero;
        gemText.transform.localScale = Vector3.zero;
        
        getRewardUI.transform.localScale = Vector3.zero;
        partUnlock.transform.localScale = Vector3.zero;
    }

    void Reward()
    {
        Gold.Plus(goldReward);
        Gem.Plus(gemReward);
    }
    
    public void OffWithReward(int multiple)
    {
        for (int i = 0; i < multiple - 1; i++)
        {
            Reward();
        }
        
        Off();
    }
    
    public void OffWithoutReward()
    {
        Off();
    }

    private async void Off()
    {
        goldReward = 0;
        gemReward = 0;
        getRewardUI.Off();
        
        await DarkEclipse.PlayReverse();
        menuObject.SetActive(false);
        
        GameManager.MergeGame();
    }
}
