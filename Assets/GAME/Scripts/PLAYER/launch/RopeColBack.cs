using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class RopeColBack : MonoBehaviour
{
    private PlayerController player => PlayerController.Instance;
    [SerializeField] private Transform toCol;
    [SerializeField] private LaunchController launch;
    
    void FixedUpdate()
    {
        if (!PlayerController.Launched && Input.GetMouseButton(0))
        {
            toCol.gameObject.SetActive(true);
            toCol.position = player.transform.position - CorrectPos();
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
            if (PlayerGrid.Instance.GetByIndex(index - i))
            {
                pos = PlayerGrid.Instance.GetRequireLocalPosition(index - i + 1);
            }
        }
        
        return pos;
    }
}
