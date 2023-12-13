using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class RopeColBack : MonoBehaviour
{
    private PlayerController player => PlayerController.Instance;
    [SerializeField] private Transform toCol;
    
    [Space]
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private float particleHeight = -2f;
    
    [Space]
    [SerializeField] private LaunchController launch;
    
    void FixedUpdate()
    {
        if (!PlayerController.Launched && Input.GetMouseButton(0))
        {
            toCol.gameObject.SetActive(true);
            toCol.position = player.transform.position - CorrectPos();
            toCol.rotation = LaunchController.Rotate;
            
            particle.transform.position = new Vector3(
                player.transform.position.x,
                particleHeight,
                player.transform.position.z);
        }
        else
        {
            toCol.gameObject.SetActive(false);
        }
    }

    Vector3 CorrectPos()
    {
        Vector3 pos = Vector3.zero;
        int index = PlayerGrid.Instance.MainIndex;

        for (int i = 0; i < 3; i++)
        {
            if (PlayerGrid.Instance.GetByIndex(index - i).Part)
            {
                pos.z = PlayerGrid.Instance.GetRequireLocalPosition(index - i).z;
            }
        }
        
        return pos;
    }
}
