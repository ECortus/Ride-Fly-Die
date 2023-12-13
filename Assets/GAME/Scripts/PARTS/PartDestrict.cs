using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartDestrict : MonoBehaviour
{
    [field: SerializeField] private Rigidbody[] bodies;
    [field: SerializeField] private Vector3[] poses;
    [field: SerializeField] private Quaternion[] rots;
    
    public void WriteDefault()
    {
        bodies = GetComponentsInChildren<Rigidbody>(true);

        poses = new Vector3[bodies.Length];
        rots = new Quaternion[bodies.Length];

        for(int i = 0; i < bodies.Length; i++)
        {
            poses[i] = bodies[i].transform.position;
            rots[i] = bodies[i].transform.rotation;
        }
        
        SetDefault();
    }
    
    public void SetDefault()
    {
        if(bodies.Length == 0) WriteDefault();
        
        foreach (var VARIABLE in bodies)
        {
            VARIABLE.isKinematic = true;
        }
    }

    public void TurnOn(float force = 0)
    {
        Vector3 dir;
        
        foreach (var VARIABLE in bodies)
        {
            VARIABLE.isKinematic = false;
            VARIABLE.useGravity = true;
            
            if (force > 0)
            {
                dir = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                    );
                
                VARIABLE.AddForce(dir * force, ForceMode.Force);
                VARIABLE.angularVelocity = dir * 8f;
            }
        }
    }
}
