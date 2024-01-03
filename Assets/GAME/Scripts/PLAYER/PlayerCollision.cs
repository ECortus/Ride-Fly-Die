using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private bool condition(GameObject go) =>
        go.layer == LayerMask.NameToLayer("Hit") 
        //|| go.layer == LayerMask.NameToLayer("Ground")
        ;
    
    // private void OnCollisionEnter(Collision other)
    // {
    //     GameObject go = other.gameObject;
    //
    //     if (condition(go))
    //     {
    //         // PlayerController.Instance.Crash();
    //         GameManager.FinishGame();
    //     }
    // }
    //
    // private void OnTriggerEnter(Collider other)
    // {
    //     GameObject go = other.gameObject;
    //
    //     if (condition(go))
    //     {
    //         // PlayerController.Instance.Crash();
    //         GameManager.FinishGame();
    //     }
    // }
}
