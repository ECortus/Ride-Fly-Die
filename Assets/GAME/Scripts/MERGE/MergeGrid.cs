using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Zenject;

public class MergeGrid : MonoBehaviour
{
    public static MergeGrid Instance { get; private set; }

    public static Part FindPartByType(PartType partType) =>
        Instance._cells.FirstOrDefault(c => c != null && c.Part != null && c.Part.Type == partType)?.Part;

    public static event Action OnAddPart;

    public static int FreeCount => Cells.Count(c => c.Part == null);
    public static IReadOnlyCollection<MergeCell> Cells => Instance._cells;

    [HideInInspector] public MergeCell[] _cells;

    public static float ChoiseOffset => Instance._choiseOffset;
    [SerializeField] private float _choiseOffset;
    
    [Inject] private void Awake()
    {
        Instance = this;
        _cells = GetComponentsInChildren<MergeCell>();

        GameManager.OnMergeGame += LoadAll;
    }

    void Start()
    {
        // LoadAll();
    }

    void LoadAll()
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            MergeCell cell = _cells[i];
            cell.Load(i);
        }
    }

    public void ClearAll()
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            MergeCell cell = _cells[i];
            
            foreach (Transform VARIABLE in cell.transform.GetChild(0))
            {
                Destroy(VARIABLE.gameObject);
            }
            
            cell.UnRegistry();
        }
    }
    
    public int HavePartOfType(PartCategory type, int lvl = -1)
    {
        int count = 0;
        
        foreach (var VARIABLE in _cells)
        {
            if (VARIABLE && VARIABLE.Part && VARIABLE.Part.Type.Category == type
                && (lvl == -1 || VARIABLE.Part.Level == lvl))
            {
                count++;
            }
        }

        return count;
    }

    public MergeCell GetFreeCell() => _cells.FirstOrDefault(c => c.Part == null);
    
    public void SpawnPart(Part partPref)
    {
        if (FreeCount == 0) return;
        
        MergeCell cell = GetFreeCell();
        SpawnPartToCell(partPref, cell);
    }

    public void SpawnPartToCell(Part partPref, MergeCell cell, bool merged = false)
    {
        Part part = Instantiate(partPref);
        
        cell.Registry(part);
        part._placedOnGrid = true;
        part.SetCell(cell);
        
        part.SetDefaultParsOnCell();
        
        OnAddPart?.Invoke();
    }
}
