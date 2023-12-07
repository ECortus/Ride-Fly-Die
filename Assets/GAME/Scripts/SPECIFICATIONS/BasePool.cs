using System;
using System.Collections;
using System.Collections.Generic;
using ModestTree;
using UnityEngine;

public abstract class BasePool<TC, T, TP> : Instancer<TC>
    where TC : Component
    where T : Component, new()
    where TP : Enum
{
    [Serializable]
    public class Pool
    {
        public List<T> List = new List<T>();
    }
    
    private int _typeCount = 0;
    public int TypeCount
    {
        get
        {
            if(_typeCount == 0)
            {
                _typeCount = Enum.GetNames(typeof(TP)).Length;
            }
            return _typeCount;
        }
    }
    
    protected Pool[] Pools = Array.Empty<Pool>();
    
    public void CreatePools()
    {
        Pools = new Pool[TypeCount];
        for(int i = 0; i < TypeCount; i++)
        {
            Pools[i] = new Pool();
        }
    }
    
    public List<T> GetArray(TP type)
    {
        if(Pools.Length < TypeCount)
        {
            CreatePools();
        }
        
        return Pools[GetIndexByType(type)].List;
    }
    
    public GameObject Insert(TP type, T obj, Vector3 pos, Quaternion rot = new Quaternion())
    {
        List<T> list = GetArray(type);
        
        foreach(T item in list)
        {
            if(!item) continue;

            if(Condition(item))
            {
                InsertAction(item, pos, rot);
                return item.gameObject;
            }
        }

        T scr = Instantiate(obj);
        InsertAction(scr, pos, rot);
        
        list.Add(scr);
        
        return scr.gameObject;
    }
    protected abstract void InsertAction(T obj, Vector3 pos, Quaternion rot);
    protected abstract bool Condition(T obj);

    int GetIndexByType(TP type)
    {
        int i = Convert.ToInt32(type);
        if(i == 0) return -1;

        i = (int)(Mathf.Log(i, 2));
        return i;
    }
}
