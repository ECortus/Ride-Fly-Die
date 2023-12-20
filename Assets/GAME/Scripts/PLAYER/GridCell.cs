using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridCell : MonoBehaviour
{
    public static event Action OnUpdateState;
        
    public static GridCell SelectedCell { get; private set; }
    public Part Part { get; private set; }
    public Part AdditionalPart { get; private set; }

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
    
    public void RegistryAdditional(Part part)
    {
        AdditionalPart = part;
        OnUpdateState?.Invoke();
        Save();
    }
    
    public void UnRegistryAdditional()
    {
        AdditionalPart = null;
        OnUpdateState?.Invoke();
        Save();
    }
    
    public void UnRegistry()
    {
        Part = null;

        if (AdditionalPart)
        {
            MergeGrid.Instance.SpawnPart(Part.Type.GetPart(Part.Level));
            AdditionalPart._currentGridCell.UnRegistryAdditional();
            AdditionalPart.DestroyPart();
        }
        AdditionalPart = null;
        
        OnUpdateState?.Invoke();
        Save();
    }

    private void Save()
    {
        SaveManager.Save($"GridCell{_id}", Part);
        SaveManager.Save($"AdditionalGridCell{_id}", AdditionalPart);
    }
        
    public void Load(int id)
    {
        _id = id;
        Part = SaveManager.Load($"GridCell{_id}");
        AdditionalPart = SaveManager.Load($"AdditionalGridCell{_id}");
            
        if (Part) PlayerGrid.Instance.SpawnPartToCell(Part, this);
        if (AdditionalPart) PlayerGrid.Instance.SpawnAdditionalPartToCell(AdditionalPart, this);
        OnUpdateState?.Invoke();
    }
}
