using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PreGameSceneController : MonoBehaviour
{
    [SerializeField] private CinemachineBrain cameraBrain;

    [Space]
    [SerializeField] private GameObject preGameUI;
    [SerializeField] private GameObject mergeUI;
    [SerializeField] private GameObject gameUI;

    [Space] 
    [SerializeField] private GameObject normalRoad;
    [SerializeField] private GameObject mergeRoad;

    void ChangeObjects(int index)
    {
        normalRoad.SetActive(index == 0 || index == 2);
        mergeRoad.SetActive(index == 1);
    }

    private void Awake()
    {
        GameManager.OnMergeGame += MoveToPreGame;
        GameManager.OnGameStart += MoveToFly;
    }

    // void Start()
    // {
    //     MoveToPreGame();
    // }
    
    public async void MoveToPreGame()
    {
        Part.SetBlock(true);
        
        mergeUI.SetActive(false);
        gameUI.SetActive(false);
        ChangeObjects(0);
        
        CameraFollowController.Instance.SetTarget(null);
        VirtualCameraController.Instance.ChangeVirtualCamera(0);

        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !cameraBrain.IsBlending);
        
        preGameUI.SetActive(true);
        // ChangeObjects(0);
    }

    public async void MoveToMerge()
    {
        preGameUI.SetActive(false);
        gameUI.SetActive(false);
        
        CameraFollowController.Instance.SetTarget(null);
        VirtualCameraController.Instance.ChangeVirtualCamera(1);

        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !cameraBrain.IsBlending);
        mergeUI.SetActive(true);
        
        Part.SetBlock(false);
        ChangeObjects(1);
    }
    
    private async void MoveToFly()
    {
        Part.SetBlock(true);
        
        preGameUI.SetActive(false);
        mergeUI.SetActive(false);
        ChangeObjects(2);
        
        // VirtualCameraController.Instance.ChangeVirtualCamera(-1);
        // CameraFollowController.Instance.SetTarget(PlayerController.Follow);
        VirtualCameraController.Instance.ChangeVirtualCamera(2);
        
        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !cameraBrain.IsBlending);
        
        gameUI.SetActive(true);
        LaunchController.Blocked = false;
        // ChangeObjects(2);
    }
}
