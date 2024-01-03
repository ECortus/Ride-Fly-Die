using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;
using ModestTree;

public class PlayerGrid : MonoBehaviour
{
    public static PlayerGrid Instance { get; private set; }

    [SerializeField] private GridsUploads Uploads;
    
    public readonly int Size = 5;// should be odd numbers
    private int Space = 2;
    
    public int MainIndex => (Size * Size - 1) / 2; // 5 - 12, 3 - 8
    public Vector3 Center => transform.position;
    
    public static Part FindPartByType(PartType partType) =>
        Instance._cells.FirstOrDefault(c => c != null && c.Part != null && c.Part.Type == partType)?.Part;

    public static event Action<Part, bool> OnAddPart;

    public static int FreeCount => Cells.Count(c => c.Part == null);
    public static IReadOnlyCollection<GridCell> Cells => Instance._cells;

    [HideInInspector] public GridCell[] _cells;
    private PartOrientation[] requireOrientations;
    
    [field: SerializeField] public Transform parentForParts { get; private set; }
    
    [SerializeField] private GameObject gridUI;
    public void SetGridUI(bool active) => gridUI.SetActive(active);
    
    [Inject] private void Awake()
    {
        Instance = this;
        _cells = gridUI.GetComponentsInChildren<GridCell>(true);

        GameManager.OnMergeGame += LoadAll;
    }

    void OnEnable()
    {
        
    }

    public void ClearMergeParts()
    {
        Part part;
        
        foreach (var VARIABLE in _cells)
        {
            part = VARIABLE.Part;

            if (part && part.Type.Category != PartCategory.Cabin && part.Type.Category != PartCategory.Grid)
            {
                MergeGrid.Instance.SpawnPart(part.Type.GetPart(part.Level));
                part._currentGridCell.UnRegistry();
                part.DestroyPart();
            }
            else if (VARIABLE.AdditionalPart)
            {
                part = VARIABLE.AdditionalPart;
                if (part.Type.Category != PartCategory.Cabin && part.Type.Category != PartCategory.Grid)
                {
                    MergeGrid.Instance.SpawnPart(part.Type.GetPart(part.Level));
                    part._currentGridCell.UnRegistryAdditional();
                    part.DestroyPart();
                }
            }
        }
    }

    public int HavePartOfType(PartCategory type, int lvl = -1)
    {
        GridCell cell;
        Part part;
        
        int count = 0;
        
        foreach (var VARIABLE in _cells)
        {
            cell = VARIABLE;
            
            if (cell)
            {
                part = cell.Part;
                if (part && part.Type.Category == type && (lvl == -1 || part.Level == lvl))
                {
                    count++;
                }
                
                part = cell.AdditionalPart;
                if (part && part.Type.Category == type && (lvl == -1 || part.Level == lvl))
                {
                    count++;
                }
            }
        }

        return count;
    }

    public Part[] GetPartsByTypes(PartType[] types)
    {
        List<Part> parts = new List<Part>();
        
        foreach (var VARIABLE in _cells)
        {
            if (VARIABLE && VARIABLE.Part)
            {
                if (types.IndexOf(VARIABLE.Part.Type) != -1)
                {
                    parts.Add(VARIABLE.Part);
                }
            }
        }

        return parts.ToArray();
    }
    
    public Part[] GetPartsByType(PartType type)
    {
        List<Part> parts = new List<Part>();
        
        foreach (var VARIABLE in _cells)
        {
            if (VARIABLE && VARIABLE.Part)
            {
                if (VARIABLE.Part.Type == type)
                {
                    parts.Add(VARIABLE.Part);
                }
            }
        }

        return parts.ToArray();
    }

    public GridCell GetByIndex(int index) => _cells[index];

    void LoadAll()
    {
        // PlayerController.Instance.ResetBody();
        Part part;
        foreach (Transform VARIABLE in parentForParts)
        {
            part = VARIABLE.GetComponent<Part>();
            if (part) part.DestroyPart();
            else Destroy(VARIABLE.gameObject);
        }
        
        ConnectedParts.Clear();
        
        for (int i = 0; i < _cells.Length; i++)
        {
            GridCell cell = _cells[i];
            cell.Load(i);
        }
        
        UploadGrids();

        foreach (var VARIABLE in _cells)
        {
            if (VARIABLE.Part)
            {
                SetDefaultPartParsOnGrid(VARIABLE.Part);
            }
        }
    }

    void UploadGrids()
    {
        GridsUploads.UploadStat stat = Uploads.CurrentStat;

        int index;
        GridCell cell;
        Part part;
        
        for(int i = 0; i < stat.Parts.Length; i++)
        {
            if (stat.Parts[i].Type.Category == PartCategory.Cabin)
            {
                if (HavePartOfType(stat.Parts[i].Type.Category, stat.LevelsOf) > 0)
                {
                    continue;
                }
            }
            else if (stat.Parts[i].Type.Category == PartCategory.Grid)
            {
                if (HavePartOfType(stat.Parts[i].Type.Category, stat.LevelsOf) >= stat.Parts.Length - 1 || !PartUnlocked.Grids)
                {
                    break;
                }
                
                ClearMergeParts();
            }
            
            index = MainIndex + stat.Parts[i].IndexOffset.x + stat.Parts[i].IndexOffset.y * 5;
            cell = _cells[index];

            part = cell.Part;
            if (part && part.Type.Category != stat.Parts[i].Type.Category)
            {
                cell.UnRegistry();
                part.DestroyPart();
            }
            
            SpawnPartToCell(stat.Parts[i].Type.GetPart(stat.LevelsOf), cell);
        }
    }

    public void SpawnPartToCell(Part partPref, GridCell cell, bool merged = false)
    {
        Part part = Instantiate(partPref);
        
        cell.Registry(part);
        part._placedOnGrid = false;
        part.SetGrid(cell);
        
        SetDefaultPartParsOnGrid(part);
        
        // OnAddPart?.Invoke(part, merged);
    }
    
    public void SpawnAdditionalPartToCell(Part partPref, GridCell cell, bool merged = false)
    {
        Part part = Instantiate(partPref);
        
        cell.RegistryAdditional(part);
        part._placedOnGrid = false;
        part.SetAdditionalGrid(cell);
        
        int index = _cells.IndexOf(part.GetGridCell());
        Vector3 pos = GetRequireLocalPosition(index);
        
        part.SetOrientation(PartOrientation.Front);
        part.SetDefaultParsOnGrid(pos, parentForParts);

        // OnAddPart?.Invoke(part, merged);
    }

    public void SetDefaultPartParsOnGrid(Part part)
    {
        int index = _cells.IndexOf(part.GetGridCell());
        Vector3 pos = GetRequireLocalPosition(index);
        
        if(UpdateRequireOrientationByPart(part)) part.SetOrientation(requireOrientations[0]);
        part.SetDefaultParsOnGrid(pos, parentForParts);
    }

    public void UpdatePartParsOnGridOnlyPos(Part part, GridCell cell)
    {
        int index = _cells.IndexOf(cell);
        Vector3 pos = GetRequireLocalPosition(index);
        
        part.SetDefaultParsOnGrid(pos, parentForParts);
    }
    
    public void UpdatePartParsOnGrid(Part part)
    {
        int index = _cells.IndexOf(GridCell.SelectedCell);
        Vector3 pos = GetRequireLocalPosition(index);

        if(UpdateRequireOrientation()) part.SetOrientation(requireOrientations[0]);
        part.SetDefaultParsOnGrid(pos, parentForParts);
    }
    
    public bool UpdateRequireOrientationByPart(Part part)
    {
        requireOrientations = GetAvailableOrientations(_cells.IndexOf(part.GetGridCell()));
        // Debug.Log(part.name + ": " + requireOrientations.Length);
        return requireOrientations.Length > 0;
    }
    
    public bool UpdateRequireOrientation()
    {
        requireOrientations = GetAvailableOrientations(_cells.IndexOf(GridCell.SelectedCell));
        return requireOrientations.Length > 0;
    }
    
    public bool HaveNeighbors(Part part, GridCell cell, List<PartCategory> typeExceptation = null, bool ignoreBlock = false)
    {
        bool value = false;
        GridCell[] neighbors = GetNeighborCellsIndex(_cells.IndexOf(cell), ignoreBlock);
        
        foreach (var VARIABLE in neighbors)
        {
            // Debug.Log(part.Type.Category + ", " + _cells.IndexOf(cell) + " - " + VARIABLE);
            if (VARIABLE && VARIABLE.Part && (VARIABLE != part.GetGridCell() || VARIABLE.AdditionalPart == part) 
                && VARIABLE.Part != part && (typeExceptation == null || !typeExceptation.Contains(VARIABLE.Part.Type.Category)
                && VARIABLE.Part.Type.Category != PartCategory.Cabin && VARIABLE.Part.Type.Category != PartCategory.Grid))
            {
                value = true;
                break;
            }
        }

        return value;
    }
    
    public bool HaveRequireNeighbors(Part part, GridCell cell, PartCategory requireCategory, bool ignoreBlock = false)
    {
        bool value = false;
        GridCell[] neighbors = GetNeighborCellsIndex(_cells.IndexOf(cell), ignoreBlock);
        
        foreach (var VARIABLE in neighbors)
        {
            if (VARIABLE && VARIABLE.Part && VARIABLE.Part != part && requireCategory == VARIABLE.Part.Type.Category)
            {
                value = true;
                break;
            }
        }

        return value;
    }
    
    public bool HaveNeighbors(Part part, GridCell cell, out Part heighbor, bool ignoreblock = false)
    {
        bool value = false;
        heighbor = null;
        
        GridCell[] neighbors = GetNeighborCellsIndex(_cells.IndexOf(cell), ignoreblock);
        
        foreach (var VARIABLE in neighbors)
        {
            // Debug.Log(part.Type.Category + ", " + _cells.IndexOf(cell) + " - " + VARIABLE);
            if (VARIABLE && VARIABLE.Part && (VARIABLE != part.GetGridCell() || VARIABLE.AdditionalPart == part) && VARIABLE.Part != part)
            {
                if(VARIABLE.Part.Type.Category != PartCategory.Grid && VARIABLE.Part.Type.Category != PartCategory.Cabin) continue;

                heighbor = VARIABLE.Part;
                value = true;
                break;
            }
        }
        
        return value;
    }

    public bool HaveOrientation(OrientationParameters orients)
    {
        if (requireOrientations.Length == 0) return false;
        
        bool value = false;

        foreach (var VARIABLE in requireOrientations)
        {
            if (orients.HaveOrientation(VARIABLE))
            {
                value = true;
                break;
            }
        }
        
        return value;
    }

    private PartOrientation[] GetAvailableOrientations(int index, bool ignoreblock = false)
    {
        List<PartOrientation> list = new List<PartOrientation>();
        GridCell[] cells = GetNeighborCellsIndex(index, ignoreblock);

        if(cells[4]) list.Add(PartOrientation.Front);
        
        if(cells[1]) list.Add(PartOrientation.Right);
        if(cells[0]) list.Add(PartOrientation.Left);
        if(cells[3]) list.Add(PartOrientation.Bottom);
        if(cells[2]) list.Add(PartOrientation.Top);

        return list.ToArray();
    }

    private GridCell[] GetNeighborCellsIndex(int index, bool ignoreblock)
    {
        GridCell[] cells = new GridCell[5];

        if (CheckIndex(index + 1))
        {
            if ((index + 1) % Size > 0 && _cells[index + 1].Part
                && (!_cells[index + 1].Part.Orientations.Block.Left || ignoreblock)) cells[0] = _cells[index + 1]; //left
        }

        if (CheckIndex(index - 1))
        {
            if((index - 1) % Size < Size - 1 && _cells[index - 1].Part
                && (!_cells[index - 1].Part.Orientations.Block.Right || ignoreblock)) cells[1] = _cells[index - 1]; //right
        }

        if (CheckIndex(index + Size))
        {
            if(_cells[index + Size].Part
               && (!_cells[index + Size].Part.Orientations.Block.Top || ignoreblock)) cells[2] = _cells[index + Size]; //top
        }

        if (CheckIndex(index - Size))
        {
            if(_cells[index - Size].Part
               && (!_cells[index - Size].Part.Orientations.Block.Bottom || ignoreblock)) cells[3] = _cells[index - Size]; //bottom
        }

        Part frontPart = _cells[index].Part;
        
        if (frontPart)
        {
            if(!frontPart.Orientations.Block.Front)
            {
                cells[4] = _cells[index];
            }
        }

        return cells;
    }
    
    bool CheckIndex(int index) => index == Mathf.Clamp(index, 0, _cells.Length - 1);

    public Vector3 GetRequireLocalPositionByGridCell(GridCell cell)
    {
        int index = _cells.IndexOf(cell);
        Vector2Int pos = Vector2Int.zero;

        pos.x = (MainIndex % Size - index % Size) * Space;
        pos.y = (MainIndex / Size - index / Size) * Space;

        return new Vector3(0, pos.y, pos.x);
    }
    
    public Vector3 GetRequireLocalPosition(int index)
    {
        Vector2Int pos = Vector2Int.zero;

        pos.x = (MainIndex % Size - index % Size) * Space;
        pos.y = (MainIndex / Size - index / Size) * Space;

        return new Vector3(0, pos.y, pos.x);
    }
}