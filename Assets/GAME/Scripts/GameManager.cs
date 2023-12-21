using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static bool GameStarted;
    
    public static Action OnMergeGame { get; set; }
    static void GameMerge() => OnMergeGame?.Invoke();
    
    public static Action OnGameStart { get; set; }
    static void GameStart() => OnGameStart?.Invoke();
    
    public static Action OnGameFinish { get; set; }
    static void GameFinish() => OnGameFinish?.Invoke();

    public static float FlyLength
    {
        get
        {
            Vector3 from = LaunchPos;
            Vector3 to = PlayerController.Instance.Center;

            from.y = 0;
            to.y = 0;

            return (from - to).magnitude;
        }
    }
    
    public static float FlyHeight => PlayerController.Instance.GetDistanceToGround();

    [SerializeField] private PlayerController player;
    public static Vector3 LaunchPos { get; private set; }

    private void Awake()
    {
        Instance = this;
        
        LaunchPos = player.transform.position;
        
        OnGameFinish += RecordMaxFlyLength;
        if (!Tutorial.Completed) OnMergeGame += CheckTutorial;
    }

    void Start()
    {
        MergeGame();
    }

    void CheckTutorial()
    {
        if (!Tutorial.Completed)
        {
            Tutorial.StartTutorial();
        }
        else
        {
            OnMergeGame -= CheckTutorial;
        }
    }
    
    public static async void MergeGame()
    {
        LaunchController.Blocked = true;
        PlayerController.Instance.SpawnToPos(LaunchPos);
        
        GameStarted = false;
        GameMerge();
        
        await DarkEclipse.Play();
    }
    
    public static async void StartGame()
    {
        if (GameStarted) return;
        
        GameStarted = true;
        GameStart();
    }

    public static void RecordMaxFlyLength()
    {
        if (FlyLength >= Records.MaxDistance)
        {
            Records.RecordMaxDistance(FlyLength);
        }
    }
    
    public static void FinishGame()
    {
        if (!GameStarted) return;
        
        GameStarted = false;
        GameFinish();
    }
}
