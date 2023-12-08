using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerChelDoll : MonoBehaviour
{
    static readonly int _SomethingTopHash = Animator.StringToHash("SomethingTop");
    
    [SerializeField] private Animator animator;
    [SerializeField] private Transform rayDot;
    [SerializeField] private LayerMask partMask;

    [Space] 
    [field: SerializeField] private Rigidbody[] bodies;
    [field: SerializeField] private Collider[] cols;

    private void Awake()
    {
        SetDefault();
    }

    private Ray ray;
    private RaycastHit hit;
    private Part part;

    bool SomethingOverPlayer
    {
        get
        {
            bool value = false;

            ray = new Ray(rayDot.position, Vector3.up * 0.45f);
            if (Physics.Raycast(ray, out hit, 2f, partMask))
            {
                part = hit.collider.gameObject.GetComponentInParent<Part>();
                if (part && part.Type.Category == PartCategory.Grid)
                {
                    value = true;
                }
            }
            
            return value;
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.GameStarted && !Part.DragedPart)
        {
            animator.SetBool(_SomethingTopHash, SomethingOverPlayer);
        }
    }

    [ContextMenu("Write default bodies par-s")]
    public void WriteDefault()
    {
        bodies = GetComponentsInChildren<Rigidbody>(true);
        cols = GetComponentsInChildren<Collider>(true);
        
        SetDefault();
    }
    
    public void SetDefault()
    {
        if(bodies.Length == 0) WriteDefault();
        
        foreach (var VARIABLE in bodies)
        {
            VARIABLE.isKinematic = true;
        }

        foreach (var VARIABLE in cols)
        {
            VARIABLE.enabled = false;
        }
        
        animator.enabled = true;
    }

    public void TurnRag(float force = 0)
    {
        animator.enabled = false;
        Vector3 dir = Vector3.up;
        
        foreach (var VARIABLE in cols)
        {
            VARIABLE.enabled = true;
        }
        
        foreach (var VARIABLE in bodies)
        {
            VARIABLE.isKinematic = false;
            VARIABLE.useGravity = true;

            if (force > 0)
            {
                VARIABLE.AddForce(dir * force, ForceMode.Force);
            }
        }
    }

    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawLine(transform.position + raycastOffset, transform.position + raycastOffset + Vector3.up);
    // }
}
