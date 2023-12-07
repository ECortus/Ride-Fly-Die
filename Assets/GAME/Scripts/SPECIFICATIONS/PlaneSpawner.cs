using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct PlaneAndCount
    {
        public GameObject prefab;
        public int count;
        public float angle;
    }

    [SerializeField] private PlaneAndCount[] planes;
    [SerializeField] private int repeat;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Transform parent;

    [ContextMenu("Spawn")]
    public void Spawn()
    {
        Destroy();
        
        GameObject plane;
        int count = 0;
        int repeatCount = repeat;

        while (repeatCount > 0)
        {
            for (int i = 0; i < planes.Length; i++)
            {
                for (int j = 0; j < planes[i].count; j++)
                {
                    plane = Instantiate(planes[i].prefab);
                
                    plane.transform.SetParent(parent);
                    plane.transform.localScale = Vector3.one * 100;
                    plane.transform.localEulerAngles = new Vector3(-90f, planes[i].angle, 0);
                    plane.transform.localPosition = new Vector3(offset.x, 0, offset.y) * (count);
                
                    count++;
                }
            }

            repeatCount--;
        }
    }

    [ContextMenu("Destroy")]
    public void Destroy()
    {
        for(int i = 0; i < parent.childCount;)
        {
            DestroyImmediate(parent.GetChild(0).gameObject);
        }
    }
}
