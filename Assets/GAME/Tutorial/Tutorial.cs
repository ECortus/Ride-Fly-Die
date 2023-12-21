using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    private static Tutorial Instance { get; set; }
    
    public static bool Completed => IterationsCompleted >= 4;
    
    public static int IterationsCompleted
    {
        get => PlayerPrefs.GetInt("IterationsCompleted", 0);
        set
        {
            PlayerPrefs.SetInt("IterationsCompleted", value);
            PlayerPrefs.Save();
        }
    }
    
    public static void StartTutorial()
    {
        if (IterationsCompleted == 0)
        {
            Instance.FirstIteration();
        }
        else if (IterationsCompleted == 1 && PartUnlocked.Wheels)
        {
            Instance.SecondIteration();
        }
        else if (IterationsCompleted == 2 && PartUnlocked.Wings)
        {
            Instance.ThirdIteration();
        }
        else if (IterationsCompleted == 3 && PartUnlocked.Grids)
        {
            Instance.FourthIteration();
        }
        else if (Completed)
        {
            Instance.gameObject.SetActive(false);
            Instance = null;
        }
    }
    
    [Space]
    [SerializeField] private Transform hand;
    [SerializeField] private float doTime = 0.75f;

    [Space] 
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private PartType wheels, fans, wings;
    [SerializeField] private UpgradeObject launch, currency, level;
    
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
    [SerializeField] private Button play2Button, buyButton, backButton, restartButton, settingsButton;

    [Space]
    [SerializeField] private Transform mergePos;
    [SerializeField] private Transform playPos;
    [SerializeField] private Transform buyPos;
    [SerializeField] private Transform launchPos, currencyPos, levelPos;
    [SerializeField] private Transform grid1Pos, grid2Pos;

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

    [Space] 
    [SerializeField] private GameObject rotateObj;
    [SerializeField] private Transform rotatePos1, rotatePos2;

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

    Part GetPartOnGrid(int index)
    {
        return PlayerGrid.Instance._cells[index].Part;
    }

    void Awake()
    {
        if (Completed)
        {
            gameObject.SetActive(false);
            return;
        }
        
        Instance = this;
    }

    private bool MergeFieldHaveType(PartType type, int count, int lvl = 0) => MergeGrid.Instance.HavePartOfType(type, lvl) >= count;
    private bool PlayerGridHaveType(PartType type, int count, int lvl = 0) => PlayerGrid.Instance.HavePartOfType(type, lvl) >= count;
    
    private async void FirstIteration()
    {
        PlayerGrid.Instance.ClearMergeParts();
        MergeGrid.Instance.ClearAll();
        
        Part.SetBlock(true);
        
        SetAllButtons(false);

        int index;
        Transform target;

        HandScale(mergePos.position);
        SetLayer(0);

        await UniTask.WaitUntil(() => brain.IsBlending);

        HandOff();
        SetLayer(-1);

        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !brain.IsBlending);

        if (!PlayerGridHaveType(fans, 1))
        {
            if (!MergeFieldHaveType(fans, 1))
            {
                BuyPart.SetPartToBuy(fans);
                buyButton.interactable = true;

                HandScale(buyPos.position);
                SetLayer(1);
                
                Gold.Plus(Mathf.Clamp(BuyPart.Cost - Gold.Value, 0, 999));
                await UniTask.WaitUntil(() => MergeFieldHaveType(fans, 1));
            }

            buyButton.interactable = false;

            index = GetIndexOfPartOnMerge(fans);
            target = cellsPoses[index];
            
            HandMove(target.position, detailPosFan.position);
            SetCellLayer(index);
            
            Part.SetBlock(false);

            await UniTask.WaitUntil(() => PlayerGridHaveType(fans, 1));
        }
        
        BuyPart.NullPartToBuy();

        HandScale(playPos.position);
        SetLayer(5);

        play2Button.interactable = true;
        
        Part.SetBlock(true);

        await UniTask.WaitUntil(() => GameManager.GameStarted);
        
        Part.SetBlock(false);

        HandOff();
        SetLayer(-1);

        SetAllButtons(true);

        IterationsCompleted = 1;
    }
    
    private async void SecondIteration()
    {
        MergeGrid.Instance.ClearAll();
        Part.SetBlock(true);
        
        SetAllButtons(false);

        int index;
        Transform target;

        if (launch.Level == 0)
        {
            Gem.Plus(Mathf.Clamp(launch.Cost - Gem.Value, 0, 999));
            
            HandScale(launchPos.position);
            SetLayer(2);

            await UniTask.WaitUntil(() => launch.Level > 0);
        }
        else if (currency.Level == 0)
        {
            Gem.Plus(Mathf.Clamp(currency.Cost - Gem.Value, 0, 999));
            
            HandScale(currencyPos.position);
            SetLayer(3);
            
            await UniTask.WaitUntil(() => currency.Level > 0);
        }
        else if (level.Level == 0)
        {
            Gem.Plus(Mathf.Clamp(level.Cost - Gem.Value, 0, 999));
            
            HandScale(levelPos.position);
            SetLayer(4);
            
            await UniTask.WaitUntil(() => level.Level > 0);
        }

        HandScale(mergePos.position);
        SetLayer(0);

        await UniTask.WaitUntil(() => brain.IsBlending);

        HandOff();
        SetLayer(-1);

        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !brain.IsBlending);

        if (!PlayerGridHaveType(wheels, 1, 1))
        {
            MergeGrid.Instance.SpawnPart(wheels.GetPart(0));
            
            if (!MergeFieldHaveType(wheels, 2))
            {
                BuyPart.SetPartToBuy(wheels);
                buyButton.interactable = true;

                HandScale(buyPos.position);
                SetLayer(1);
                
                Gold.Plus(Mathf.Clamp(BuyPart.Cost - Gold.Value, 0, 999));
                await UniTask.WaitUntil(() => MergeFieldHaveType(wheels, 2));
            }
            
            Part.SetBlock(false);

            if (!MergeFieldHaveType(wheels, 1, 1))
            {
                buyButton.interactable = false;

                Transform trg1, trg2;
                
                index = GetIndexOfPartOnMerge(wheels, 0);
                trg1 = cellsPoses[index];
                SetCellLayer(index);

                index = GetIndexOfPartOnMerge(wheels, 0, index);
                trg2 = cellsPoses[index];
                SetCellLayer(index);
                
                HandMove(trg1.position, trg2.position);
                await UniTask.WaitUntil(() => MergeFieldHaveType(wheels, 1, 1));
            }

            index = GetIndexOfPartOnMerge(wheels, 1);
            target = cellsPoses[index];
            
            HandMove(target.position, detailPosWheel.position);
            SetCellLayer(index);

            await UniTask.WaitUntil(() => PlayerGridHaveType(wheels, 1, 1));
        }
        
        BuyPart.NullPartToBuy();

        HandScale(playPos.position);
        SetLayer(5);

        play2Button.interactable = true;
        
        Part.SetBlock(true);

        await UniTask.WaitUntil(() => GameManager.GameStarted);
        
        Part.SetBlock(false);

        HandOff();
        SetLayer(-1);

        SetAllButtons(true);

        IterationsCompleted = 2;
    }
    
    private async void ThirdIteration()
    {
        MergeGrid.Instance.ClearAll();
        Part.SetBlock(true);
        
        SetAllButtons(false);

        int index;
        Transform target;

        HandScale(mergePos.position);
        SetLayer(0);

        await UniTask.WaitUntil(() => brain.IsBlending);

        HandOff();
        SetLayer(-1);

        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !brain.IsBlending);

        if (!PlayerGridHaveType(wings, 1))
        {
            if (!MergeFieldHaveType(wings, 1))
            {
                BuyPart.SetPartToBuy(wings);
                buyButton.interactable = true;

                HandScale(buyPos.position);
                SetLayer(1);
                
                Gold.Plus(Mathf.Clamp(BuyPart.Cost - Gold.Value, 0, 999));
                await UniTask.WaitUntil(() => MergeFieldHaveType(wings, 1));
            }

            buyButton.interactable = false;

            index = GetIndexOfPartOnMerge(wings);
            target = cellsPoses[index];
            
            HandMove(target.position, detailPosWings.position);
            SetCellLayer(index);
            
            Part.SetBlock(false);

            await UniTask.WaitUntil(() => PlayerGridHaveType(wings, 1));
        }
        
        BuyPart.NullPartToBuy();

        HandScale(playPos.position);
        SetLayer(5);

        play2Button.interactable = true;
        
        Part.SetBlock(true);

        await UniTask.WaitUntil(() => GameManager.GameStarted);
        
        Part.SetBlock(false);

        HandOff();
        SetLayer(-1);

        SetAllButtons(true);

        IterationsCompleted = 3;
    }
    
    private async void FourthIteration()
    {
        PlayerGrid.Instance.ClearMergeParts();
        Part.SetBlock(true);
        
        SetAllButtons(false);

        int index;
        Transform target;

        HandScale(mergePos.position);
        SetLayer(0);

        await UniTask.WaitUntil(() => brain.IsBlending);

        HandOff();
        SetLayer(-1);

        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !brain.IsBlending);

        index = PlayerGrid.Instance.MainIndex;
        int index1 = index + 1;
        int index2 = index - 1;

        if (GetPartOnGrid(index1))
        {
            HandMove(grid1Pos.position, grid2Pos.position);
            // await UniTask.WaitUntil(() => GetPartOnGrid(index2));
        }
        else if (GetPartOnGrid(index2))
        {
            HandMove(grid2Pos.position, grid1Pos.position);
            // await UniTask.WaitUntil(() => GetPartOnGrid(index1));
        }
        
        await UniTask.WaitUntil(() => Input.GetMouseButtonUp(0));

        HandOff();
        SetLayer(-1);

        SetAllButtons(true);

        IterationsCompleted = 4;
    }

    public static void PlaneIteration()
    {
        // Instance._PlaneIteration();
    }

    private async void _PlaneIteration()
    {
        if (Mathf.Abs(PlayerController.Instance.mouseRotateInput) <= 0)
        {
            SetAllButtons(false);
            SetLayer(-1);

            Time.timeScale = 0;
        
            rotateObj.SetActive(true);
            HandMove(rotatePos1.position, rotatePos2.position);

            await UniTask.WaitUntil(() => Mathf.Abs(PlayerController.Instance.mouseRotateInput) > 0.1f);
        
            rotateObj.SetActive(false);
        
            Time.timeScale = 1;
        
            HandOff();
            SetAllButtons(true);
        }
    }

    void SetAllButtons(bool state)
    {
        play1Button.interactable = state;
        play2Button.interactable = state;
        backButton.interactable = state;
        buyButton.interactable = state;
    }

    void HandMove(Vector3 first, Vector3 second)
    {
        hand.DOKill();
        hand.gameObject.SetActive(true);
        
        hand.position = first;
        hand.localScale = Vector3.one;
        hand.DOMove(second, doTime)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);
    }

    void HandScale(Vector3 pos)
    {
        hand.DOKill();
        hand.gameObject.SetActive(true);
        
        hand.position = pos;
        hand.localScale = Vector3.one;
        hand.DOScale(Vector3.one * 0.75f, doTime)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);
    }

    void HandOff()
    {
        hand.DOKill();
        hand.gameObject.SetActive(false);
    }
}
