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
    [SerializeField] private TextMeshProUGUI goldText, gemText;
    [SerializeField] private Transform restartButton;

    [Space]
    [SerializeField] private RewardForFly rewards;
    private float flyLength => GameManager.Instance.FlyLength;
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
        goldReward = rewards.GoldReward(flyLength);
        gemReward = rewards.GemReward(flyLength);
        
        HideMenu();
        menuObject.SetActive(true);

        goldText.text = $"{goldReward.ToString()}";
        gemText.text = $"{gemReward.ToString()}";

        await menuBg.DOColor(new Color(menuBg.color.r, menuBg.color.g, menuBg.color.b, alpha), 0.5f).AsyncWaitForCompletion();
        await menuTitle.DOScale(Vector3.one, 0.5f).AsyncWaitForCompletion();
        
        await goldText.transform.DOScale(Vector3.one, 0.5f).AsyncWaitForCompletion();
        await gemText.transform.DOScale(Vector3.one, 0.5f).AsyncWaitForCompletion();

        restartButton.transform.DOScale(Vector3.one, 0.5f);
    }

    void HideMenu()
    {
        menuBg.color = new Color(menuBg.color.r, menuBg.color.g, menuBg.color.b, 0f);
        menuTitle.localScale = Vector3.zero;
        goldText.transform.localScale = Vector3.zero;
        gemText.transform.localScale = Vector3.zero;
        restartButton.localScale = Vector3.zero;
    }

    void Reward()
    {
        Gold.Plus(goldReward);
        Gem.Plus(gemReward);
    }

    public async void Off()
    {
        Reward();

        goldReward = 0;
        gemReward = 0;
        
        await DarkEclipse.PlayReverse();
        menuObject.SetActive(false);
        
        GameManager.Instance.MergeGame();
    }
}
