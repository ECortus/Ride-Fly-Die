using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    private bool Completed
    {
        get => PlayerPrefs.GetInt("TutorialComplete", 0) > 0;
        set
        {
            PlayerPrefs.SetInt("TutorialComplete", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    [SerializeField] private Transform hand;
    [SerializeField] private float doTime = 0.75f;

    [Space] 
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private PartType wheels;
    
    [Space]
    [SerializeField] private Button play1Button;
    [SerializeField] private Button play2Button;
    [SerializeField] private Button backButton;

    [Space]
    [SerializeField] private Transform mergePos;
    [SerializeField] private Transform playPos;
    [SerializeField] private Transform wheel1Pos;
    [SerializeField] private Transform wheel2Pos;

    void Awake()
    {
        if(!Completed) Init();
    }
    
    private async void Init()
    {
        play1Button.interactable = false;
        play2Button.interactable = false;
        backButton.interactable = false;

        hand.gameObject.SetActive(true);
        
        hand.position = mergePos.position;
        hand.localScale = Vector3.one;
        hand.DOScale(Vector3.one * 0.75f, doTime)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        
        await UniTask.WaitUntil(() => brain.IsBlending);

        hand.DOKill();
        hand.gameObject.SetActive(false);
        
        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !brain.IsBlending);
        
        if(MergeGrid.FreeCount == 9 
           && !PlayerGrid.Instance._cells[PlayerGrid.Instance.MainIndex + PlayerGrid.Instance.Size].Part) 
            MergeGrid.Instance.SpawnPart(wheels.GetPart(0));
        
        hand.gameObject.SetActive(true);
        
        hand.position = wheel1Pos.position;
        hand.DOMove(wheel2Pos.position, doTime)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        await UniTask.WaitUntil(() => 
            PlayerGrid.Instance._cells[PlayerGrid.Instance.MainIndex + PlayerGrid.Instance.Size].Part
        );

        hand.DOKill();
        
        hand.position = playPos.position;
        hand.localScale = Vector3.one;
        hand.DOScale(Vector3.one * 0.75f, doTime)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        
        play2Button.interactable = true;

        await UniTask.WaitUntil(() => GameManager.GameStarted);

        play1Button.interactable = true;
        backButton.interactable = true;
        
        gameObject.SetActive(false);
        Completed = true;
    }
}
