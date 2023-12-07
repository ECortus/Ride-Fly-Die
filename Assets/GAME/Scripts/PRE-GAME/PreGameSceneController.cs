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

    [Header("additional shit")] 
    [SerializeField] private GameObject[] planesToOffOnMerge;

    void PlanesOn(bool state)
    {
        // foreach (var VARIABLE in planesToOffOnMerge)
        // {
        //     VARIABLE.SetActive(state);
        // }
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
        
        PlanesOn(true);
        
        CameraFollowController.Instance.SetTarget(null);
        VirtualCameraController.Instance.ChangeVirtualCamera(0);

        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !cameraBrain.IsBlending);
        preGameUI.SetActive(true);
    }

    public async void MoveToMerge()
    {
        preGameUI.SetActive(false);
        gameUI.SetActive(false);
        
        PlanesOn(false);
        
        CameraFollowController.Instance.SetTarget(null);
        VirtualCameraController.Instance.ChangeVirtualCamera(1);

        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !cameraBrain.IsBlending);
        mergeUI.SetActive(true);
        
        Part.SetBlock(false);
    }
    
    public void MoveToFly()
    {
        Part.SetBlock(true);
        preGameUI.SetActive(false);
        mergeUI.SetActive(false);
        
        PlanesOn(true);
        
        VirtualCameraController.Instance.ChangeVirtualCamera(-1);
        // VirtualCameraController.Instance.ChangeVirtualCamera(2);
        CameraFollowController.Instance.SetTarget(PlayerController.Follow);
        
        gameUI.SetActive(true);
    }
}
