using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MergeCell : MonoBehaviour
{
    public static event Action OnUpdateState;
        
    public static MergeCell SelectedCell { get; private set; }
    public Part Part { get; private set; }

    private int _id;
    private bool _showed;

    private void OnMouseOver()
    {
        SelectedCell = this;
    }

    private void OnMouseExit()
    {
        SelectedCell = null;
    }

    public void Registry(Part part)
    {
        Part = part;
        OnUpdateState?.Invoke();
        Save();
    }
    
    public void UnRegistry()
    {
        Part = null;
        OnUpdateState?.Invoke();
        Save();
    }

    private void Save()
    {
        SaveManager.Save($"MergeCell{_id}", Part);
    }
        
    public void Load(int id)
    {
        if (transform.GetChild(0).childCount > 0)
        {
            foreach (Transform VARIABLE in transform.GetChild(0))
            {
                Destroy(VARIABLE.gameObject);
            }
        }
        
        _id = id;
        Part = SaveManager.Load($"MergeCell{_id}");

        if (Part == null) return;
            
        MergeGrid.Instance.SpawnPartToCell(Part, this);
        OnUpdateState?.Invoke();
    }
}
