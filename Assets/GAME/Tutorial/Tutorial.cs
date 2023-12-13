using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    private static Tutorial Instance;
    
    public static bool MainCompleted
    {
        get => PlayerPrefs.GetInt("MainTutorialComplete", 0) != 0;
        set
        {
            PlayerPrefs.SetInt("MainTutorialComplete", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    
    public static bool MergeCompleted
    {
        get => PlayerPrefs.GetInt("MergeTutorialComplete", 0) != 0;
        set
        {
            PlayerPrefs.SetInt("MergeTutorialComplete", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    
    public static void StartMainTutorial()
    {
        if(!MainCompleted) Instance.MainInit();
    }

    public static void StartMergeTutorial()
    {
        if(!MergeCompleted) Instance.MergeInit();
    }

    [SerializeField] private Transform hand;
    [SerializeField] private float doTime = 0.75f;

    [Space] 
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private PartType wheels, fans, wings;
    
    [Space] 
    [SerializeField] private GameObject[] layers;

    void SetLayer(int index)
    {
        foreach (var VARIABLE in layers)
        {
            VARIABLE.SetActive(false);
        }

        if (index >= 0 && index < layers.Length)
        {
            SetCellLayer(-1);
            layers[index].SetActive(true);
        }
    }
    
    [Space]
    [SerializeField] private Button play1Button;
    [SerializeField] private Button play2Button;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button backButton;

    [Space]
    [SerializeField] private Transform mergePos;
    [SerializeField] private Transform playPos;
    [SerializeField] private Transform buyPos;

    [Space] 
    [SerializeField] private GameObject layersBG;
    [SerializeField] private GameObject[] layersCells;

    void SetCellLayer(int index)
    {
        if (index < 0 || index >= layersCells.Length)
        {
            layersBG.SetActive(false);
            foreach (var VARIABLE in layersCells)
            {
                VARIABLE.SetActive(false);
            }

            return;
        }
        
        SetLayer(-1);
        
        layersBG.SetActive(true);
        layersCells[index].SetActive(true);
    }
    
    [SerializeField] private Transform[] cellsPoses;
    [SerializeField] private Transform detailPosWheel, detailPosFan, detailPosWings;

    int GetIndexOfPartOnMerge(PartType type, int lvl = -1, int indexIgnored = -1)
    {
        int pos = -1;
        MergeCell cell;

        for (int i = 0; i < MergeGrid.Instance._cells.Length; i++)
        {
            cell = MergeGrid.Instance._cells[i];
            if (cell && cell.Part && cell.Part.Type == type
                && (lvl == -1 || cell.Part.Level == lvl)
                && (indexIgnored == -1 || i != indexIgnored))
            {
                pos = i;
                break;
            }
        }
        
        return pos;
    }

    void Awake()
    {
        if (MainCompleted && MergeCompleted)
        {
            gameObject.SetActive(false);
            return;
        }

        Instance = this;

        // if (!MainCompleted) MainInit();
        // else if (!MergeCompleted) MergeInit();
    }

    private bool MainCompleteConditionWheelFirst => MergeGrid.Instance.HavePartOfType(wheels) > 0;
    private bool MainCompleteConditionWheelSecond => PlayerGrid.Instance.HavePartOfType(wheels) > 0;
    private bool MainCompleteConditionFanFirst => MergeGrid.Instance.HavePartOfType(fans) > 0;
    private bool MainCompleteConditionFanSecond => PlayerGrid.Instance.HavePartOfType(fans) > 0;
    
    private bool MergeCompleteConditionFirst => MergeGrid.Instance.HavePartOfType(wings, 1) > 0;
    private bool MergeCompleteConditionSecond => PlayerGrid.Instance.HavePartOfType(wings, 1) > 0;
    
    private async void MainInit()
    {
        play1Button.interactable = false;
        play2Button.interactable = false;
        backButton.interactable = false;
        buyButton.interactable = false;

        int index;
        Transform target;

        hand.gameObject.SetActive(true);
        
        hand.position = mergePos.position;
        hand.localScale = Vector3.one;
        hand.DOScale(Vector3.one * 0.75f, doTime)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        
        SetLayer(0);
        
        await UniTask.WaitUntil(() => brain.IsBlending);

        hand.DOKill();
        hand.gameObject.SetActive(false);
        
        SetLayer(-1);
        
        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !brain.IsBlending);

        if (!MainCompleteConditionWheelSecond)
        {
            BuyPart.SetPartToBuy(wheels);
            hand.gameObject.SetActive(true);
            buyButton.interactable = true;
        
            hand.position = buyPos.position;
            hand.localScale = Vector3.one;
            hand.DOScale(Vector3.one * 0.75f, doTime)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        
            SetLayer(1);

            if (!MainCompleteConditionWheelFirst)
            {
                Gold.Plus(Mathf.Clamp(BuyPart.Cost - Gold.Value, 0, 999));
                await UniTask.WaitUntil(() => MainCompleteConditionWheelFirst);
            }
        
            hand.DOKill();
            hand.localScale = Vector3.one;
            buyButton.interactable = false;

            index = GetIndexOfPartOnMerge(wheels);
            target = cellsPoses[index];
            SetCellLayer(index);
        
            hand.position = target.position;
            hand.DOMove(detailPosWheel.position, doTime)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            await UniTask.WaitUntil(() => MainCompleteConditionWheelSecond);
        }

        if (!MainCompleteConditionFanSecond)
        {
            hand.DOKill();
            BuyPart.SetPartToBuy(fans);
            buyButton.interactable = true;
        
            hand.position = buyPos.position;
            hand.localScale = Vector3.one;
            hand.DOScale(Vector3.one * 0.75f, doTime)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        
            SetLayer(1);

            if (!MainCompleteConditionFanFirst)
            {
                Gold.Plus(Mathf.Clamp(BuyPart.Cost - Gold.Value, 0, 999));
                await UniTask.WaitUntil(() => MainCompleteConditionFanFirst);
            }
        
            hand.DOKill();
            hand.localScale = Vector3.one;
            buyButton.interactable = false;
        
            index = GetIndexOfPartOnMerge(fans);
            target = cellsPoses[index];
            SetCellLayer(index);
        
            hand.position = target.position;
            hand.DOMove(detailPosFan.position, doTime)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            await UniTask.WaitUntil(() => MainCompleteConditionFanSecond);
        }
        
        hand.DOKill();
        hand.gameObject.SetActive(true);
        BuyPart.NullPartToBuy();
        
        hand.position = playPos.position;
        hand.localScale = Vector3.one;
        hand.DOScale(Vector3.one * 0.75f, doTime)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        
        SetLayer(2);
        
        play2Button.interactable = true;

        await UniTask.WaitUntil(() => GameManager.GameStarted);

        hand.DOKill();
        hand.gameObject.SetActive(false);
        SetLayer(-1);
        
        play1Button.interactable = true;
        backButton.interactable = true;
        buyButton.interactable = true;
        
        MainCompleted = true;
    }
    
    private async void MergeInit()
    {
        play1Button.interactable = false;
        play2Button.interactable = false;
        backButton.interactable = false;
        buyButton.interactable = false;
        
        int index;
        Transform targetFirst, targetTwo;
        
        MergeGrid.Instance.ClearAll();
        
        hand.gameObject.SetActive(true);
        
        hand.position = mergePos.position;
        hand.localScale = Vector3.one;
        hand.DOScale(Vector3.one * 0.75f, doTime)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        
        SetLayer(0);
        
        await UniTask.WaitUntil(() => brain.IsBlending);
        
        hand.DOKill();
        hand.gameObject.SetActive(false);
        
        SetLayer(-1);
        
        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !brain.IsBlending);

        if (!MergeCompleteConditionSecond)
        {
            if (!MergeCompleteConditionFirst)
            {
                BuyPart.SetPartToBuy(wings);
                hand.gameObject.SetActive(true);
                buyButton.interactable = true;
        
                hand.position = buyPos.position;
                hand.localScale = Vector3.one;
                hand.DOScale(Vector3.one * 0.75f, doTime)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
        
                SetLayer(1);

                if (MergeGrid.Instance.HavePartOfType(wings) <= 0)
                {
                    Gold.Plus(Mathf.Clamp(BuyPart.Cost - Gold.Value, 0, 999));
                    await UniTask.WaitUntil(() => MergeGrid.Instance.HavePartOfType(wings) > 0);
                }

                if (MergeGrid.Instance.HavePartOfType(wings) <= 1)
                {
                    Gold.Plus(Mathf.Clamp(BuyPart.Cost - Gold.Value, 0, 999));
                    await UniTask.WaitUntil(() => MergeGrid.Instance.HavePartOfType(wings) > 1);
                }
        
                hand.DOKill();
                buyButton.interactable = false;
        
                index = GetIndexOfPartOnMerge(wings);
                targetFirst = cellsPoses[index];
                SetCellLayer(index);
        
                index = GetIndexOfPartOnMerge(wings, -1, index);
                targetTwo = cellsPoses[index];
                SetCellLayer(index);
        
                hand.position = targetFirst.position;
                hand.DOMove(targetTwo.position, doTime)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
        
                await UniTask.WaitUntil(() => MergeCompleteConditionFirst);
            }
        
            hand.DOKill();
            hand.localScale = Vector3.one;
        
            SetCellLayer(-1);
        
            index = GetIndexOfPartOnMerge(wings, 1);
            targetFirst = cellsPoses[index];
            SetCellLayer(index);
        
            hand.position = targetFirst.position;
            hand.DOMove(detailPosWings.position, doTime)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        
            buyButton.interactable = false;
        
            await UniTask.WaitUntil(() => MergeCompleteConditionSecond);
        }
        
        hand.DOKill();
        hand.gameObject.SetActive(true);
        BuyPart.NullPartToBuy();
        
        hand.position = playPos.position;
        hand.localScale = Vector3.one;
        hand.DOScale(Vector3.one * 0.75f, doTime)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        
        SetLayer(2);
        
        play2Button.interactable = true;
        
        await UniTask.WaitUntil(() => GameManager.GameStarted);
        
        hand.DOKill();
        hand.gameObject.SetActive(false);
        SetLayer(-1);
        
        play1Button.interactable = true;
        backButton.interactable = true;
        buyButton.interactable = true;
        
        gameObject.SetActive(false);
        MergeCompleted = true;
    }
}
