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
    void GameMerge() => OnMergeGame?.Invoke();
    
    public static Action OnGameStart { get; set; }
    void GameStart() => OnGameStart?.Invoke();
    
    public static Action OnGameFinish { get; set; }
    void GameFinish() => OnGameFinish?.Invoke();

    public float FlyLength
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
    
    [field: SerializeField] public Vector3 LaunchPos { get; private set; }

    private void Awake()
    {
        Instance = this;
        OnGameFinish += RecordMaxFlyLength;
    }

    void Start()
    {
        MergeGame();
    }
    
    public async void MergeGame()
    {
        PlayerController.Instance.SpawnToPos(LaunchPos);
        
        GameStarted = false;
        GameMerge();
        
        await DarkEclipse.Play();
    }
    
    public void StartGame()
    {
        if (GameStarted) return;
        
        GameStarted = true;
        GameStart();
    }

    void RecordMaxFlyLength()
    {
        if (FlyLength >= Records.MaxDistance)
        {
            Records.RecordMaxDistance(FlyLength);
        }
    }
    
    public void FinishGame()
    {
        if (!GameStarted) return;
        
        GameStarted = false;
        GameFinish();
    }
}
