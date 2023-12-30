using System;
using System.Collections;
using System.Collections.Generic;
using ModestTree;
using TMPro;
using TMPro.Examples;
using UnityEngine;

public abstract class Part : MonoBehaviour
{
    private PlayerSettings _settings;
    
    [field: SerializeField] public PartType Type { get; private set; }
    [field: SerializeField] public int Level { get; private set; }
    [field: SerializeField] public float Mass { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    
    [field: SerializeField] public bool PreShowOnPlayer { get; private set; }
    [field: SerializeField] public bool BlockMerge { get; private set; }
    [field: SerializeField] public bool BlockMove { get; private set; }

    [field: SerializeField] public OrientationParameters Orientations { get; private set; }
    
    public PartCategory Category => Type.Category;

    public Rigidbody Body { get; set; }
    private FixedJoint Joint;
    
    private PartOrientation Orientation = PartOrientation.Default;
    public void SetOrientation(PartOrientation ort)
    {
        Orientation = ort;
        ApplyOrientation();
    }

    void ApplyOrientation()
    {
        SwitchMirror(Orientations.GetOrientationParameter(Orientation).ApplyMirror);
        Vector3 angles = Orientations.GetAnglesByOrient(Orientation);

        if (_dragedPart == this)
        {
            if (Type.Category == PartCategory.Wheels)
            {
                if (transform.parent)
                {
                    angles.y = 180;
                }
                else
                {
                    angles.y = 0;
                }
            }
            else if (Type.Category == PartCategory.Wings) angles.y += 180f;
            else if (Type.Category == PartCategory.Boost)
            {
                if (transform.parent)
                {
                    angles.y += 180f;
                }
            }
        }
        else if (_currentGridCell)
        {
            angles.y += 180f;
        }
        else if (_currentMergeCell)
        {
            angles.y += 90f;
        }
        
        if (OverMergeField && _dragedPart && _dragedPart == this && !GameManager.GameStarted)
        {
            switch (Type.Category)
            {
                case PartCategory.Cabin:
                    break;
                case PartCategory.Grid:
                    break;
                case PartCategory.Wheels:
                    angles += new Vector3(0, 0, Cam.transform.eulerAngles.x);
                    break;
                default:
                    angles -= new Vector3(Cam.transform.eulerAngles.x, 0, 0);
                    break;
            }
        }
        
        transform.localEulerAngles = angles;
    }

    private static bool _blockAll;
    private static Part _dragedPart;

    public static Part DragedPart => _dragedPart;
    
    private static void SetDragedPart(Part part)
    {
        _dragedPart = part;
    }

    public static void SetBlock(bool block)
    {
        _blockAll = block;
    }

    protected GameObject Object;
    protected GameObject inGameObject;
    protected GameObject onGridObject;
    protected GameObject additionalObject;
    
    private PartDestrict Destrict;
    private TextMeshProUGUI lvlText;
    
    private Renderer[] _renderers;
    
    private MeshCollider[] _gameColliders;
    private BoxCollider _onGridCollider;

    private readonly float defaultSize = 1f;

    public MergeCell _currentMergeCell { get; set; }
    public GridCell _currentGridCell { get; set; }
    public bool _placedOnGrid { get; set; }
    
    public GridCell GetGridCell() => _currentGridCell;

    private bool HaveRequireNeighbors(out Part neighbor)
    {
        if (_currentGridCell.AdditionalPart == this)
        {
            neighbor = _currentGridCell.Part;
            return _currentGridCell.Part;
        }
            
        return PlayerGrid.Instance.HaveNeighbors(this,  _currentGridCell, out neighbor);
    }

    [Space] 
    [SerializeField] private Material[] standartMaterials;

    [ContextMenu("Set Standart Material")]
    public void SetStandartMaterial()
    {
        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>(true);
        foreach (var VARIABLE in meshes)
        {
            VARIABLE.materials = standartMaterials;
        }
    }

    protected virtual void Awake()
    {
        Init();
    }

    void OnGameStart()
    {
        // if(Body) Destroy(Body);
        Part neighbor;

        if (HaveRequireNeighbors(out neighbor) || Type.Category == PartCategory.Cabin || Type.Category == PartCategory.Grid)
        {
            // if (Type.Category != PartCategory.Cabin) Joint.connectedBody = neighbor.Body;
            // else Joint.connectedBody = PlayerController.Instance.Body;
            
            // Joint.connectedBody = PlayerController.Instance.Body;
            // Body.useGravity = false;
            
            // AddMod();
        }
        else
        {
            Disconnect();
        }
        
        Body.isKinematic = false;

        // Body.interpolation = RigidbodyInterpolation.Extrapolate;
        // Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        SwitchObjects(true);
        SwitchColliders(true);
        SwitchGridCollider(false);
    }

    void AddMod()
    {
        Body.isKinematic = true;
        Body.useGravity = false;
        Body.interpolation = RigidbodyInterpolation.Extrapolate;
        
        Joint.connectedBody = PlayerController.Instance.Body;
        Joint.anchor = Vector3.zero;
        Joint.connectedAnchor = transform.localPosition;
        
        ConnectedParts.Add(this);
    }
    
    void RemoveMod()
    {
        Body.interpolation = RigidbodyInterpolation.None;
        
        Joint.connectedBody = null;
        Joint.anchor = Vector3.zero;
        Joint.connectedAnchor = Vector3.zero;
        
        ConnectedParts.Remove(this);
    }
    
    [ContextMenu("Write default destrict")]
    public void WriteDefaultDestrict()
    {
        if (!Destrict) Destrict = GetComponentInChildren<PartDestrict>(true);
        Destrict.WriteDefault();
    }

    void Init()
    {
        _settings = Resources.Load<PlayerSettings>("SETTINGS/PlayerSettings");
        
        Object = getChildGameObject("obj");

        if (!BlockMerge)
        {
            inGameObject = getChildGameObject("ingame");
            onGridObject = getChildGameObject("ongrid");

            if (inGameObject)
            {
                _gameColliders = inGameObject.GetComponentsInChildren<MeshCollider>();
                _renderers = inGameObject.GetComponentsInChildren<Renderer>(true);

                additionalObject = getChildGameObject("mirror");
            }
            else
            {
                _gameColliders = Object.GetComponentsInChildren<MeshCollider>();
                _renderers = Object.GetComponentsInChildren<Renderer>(true);
            }
        
            SwitchColliders(false);

            GameObject lvl = getChildGameObject("lvl_text");
            if (lvl)
            {
                lvlText = lvl.GetComponent<TextMeshProUGUI>();
                lvlText.text = $"{Level + 1}";
            }
        }
        else
        {
            _gameColliders = Object.GetComponentsInChildren<MeshCollider>();
            _renderers = Object.GetComponentsInChildren<Renderer>(true);
            
            SwitchColliders(false);
        }
        
        // getChildGameObject("gridcol").SetActive(false);
        // _onGridCollider = GetComponent<BoxCollider>();
        // _onGridCollider.size = Vector3.one * 2f;
        
        _onGridCollider = getChildGameObject("gridcol").GetComponent<BoxCollider>();
        _onGridCollider.size = Vector3.one * 1.75f;
        
        SwitchGridCollider(true);

        if(!Destrict) Destrict = GetComponentInChildren<PartDestrict>(true);
        RepairPart();

        Body = GetComponent<Rigidbody>();
        Joint = GetComponent<FixedJoint>();
        
        Body.isKinematic = true;
        Body.useGravity = false;
        Body.mass = Mass;

        Joint.connectedBody = null;
        Joint.autoConfigureConnectedAnchor = false;
        Joint.anchor = Vector3.zero;
        Joint.connectedAnchor = Vector3.zero;
    }

    void Update()
    {
        if (!GameManager.GameStarted)
        {
            ApplyOrientation();
                
            if (_dragedPart)
            {
                SwitchGridCollider(false);
            }
            else
            {
                SwitchGridCollider(true);
            }
        }

        if (_currentGridCell)
        {
            Debug.Log($"----------------{name} {Level}----------------");
            Debug.Log(Joint.anchor);
            Debug.Log(Joint.connectedAnchor);
            Debug.Log(transform.localPosition);
        }
    }

    public void SetLocalPosition(Vector3 local)
    {
        transform.localPosition = local;
        Body.position = transform.position;
    }

    public void SetLocalRotation(Quaternion local)
    {
        transform.localRotation = local;
        Body.rotation = transform.rotation;
    }
    
    public void SetGrid(GridCell cell)
    {
        if (_currentGridCell)
        {
            if (_currentGridCell.AdditionalPart && _currentGridCell.AdditionalPart == this) _currentGridCell.UnRegistryAdditional();
            else if (_currentGridCell.Part == this) _currentGridCell.UnRegistry();

            if (cell == null)
            {
                RemoveMod();
            }
        }
        _currentGridCell = cell;
        
        if(_currentMergeCell) _currentMergeCell.UnRegistry();
        _currentMergeCell = null;
        
        // Debug.Log(gameObject.name + ", registry on grid on SETGRID");
        OnPartPlaceOnGrid();
    }
    
    public void SetAdditionalGrid(GridCell cell)
    {
        if (_currentGridCell)
        {
            if (_currentGridCell.AdditionalPart && _currentGridCell.AdditionalPart == this) _currentGridCell.UnRegistryAdditional();
            
            if (cell == null)
            {
                RemoveMod();
            }
        }
        _currentGridCell = cell;
        
        if(_currentMergeCell) _currentMergeCell.UnRegistry();
        _currentMergeCell = null;
        
        // Debug.Log(gameObject.name + ", registry on grid on SETADDITIONALGRID");
        OnPartPlaceOnGrid();
    }

    public void SetCell(MergeCell cell)
    {
        if(_currentMergeCell) _currentMergeCell.UnRegistry();
        _currentMergeCell = cell;

        if (_currentGridCell)
        {
            if (_currentGridCell.AdditionalPart && _currentGridCell.AdditionalPart == this) _currentGridCell.UnRegistryAdditional();
            else if(_currentGridCell.Part == this) _currentGridCell.UnRegistry();
            
            RemoveMod();
        }
        _currentGridCell = null;
        
        OnPartPlaceOnMergeZone();
        SetOrientation(PartOrientation.Default);
    }
    
    private void ChangeColor(bool canPlace)
    {
        // if (_renderers.Length == 0) return;
        
        MaterialPropertyBlock renderBlock = new MaterialPropertyBlock();

        foreach (Renderer render in _renderers)
        {
            render.GetPropertyBlock(renderBlock);
            renderBlock.SetColor("_Highlight", canPlace ? Color.clear : Color.red);
            render.SetPropertyBlock(renderBlock);
        }
    }
    
    private void OnPartPlaceOnGrid()
    {
        if (_placedOnGrid) return;
        _placedOnGrid = true;
        
        SwitchObjects(true);
        SwitchColliders(false);
        SwitchMirror(true);
        
        // PlayerGrid.Instance.SetDefaultPartParsOnGrid(this);
        
        // SwitchObjects(true);
        // SwitchColliders(true);
        // SwitchGridCollider(false);
        
        // DragState -= OnDragState;
        // Debug.Log(gameObject.name + ", registry on grid");
        
        SetActions(true);
    }

    private void OnPartPlaceOnMergeZone()
    {
        if (!_placedOnGrid) return;
        _placedOnGrid = false;
        
        SwitchObjects(false);
        SwitchColliders(false);
        SwitchMirror(false);
        
        SetOrientation(PartOrientation.Default);
        
        SetActions(false);
    }
    
    public virtual void RepairPart()
    {
        Object.SetActive(true);
        Destrict.gameObject.SetActive(false);
        Destrict.SetDefault();
    }

    public virtual void CrashPart()
    {
        Object.SetActive(false);
        Destrict.gameObject.SetActive(true);
        Destrict.TurnOn(250);
        
        Disconnect();
        Body.isKinematic = true;
        
        PlayerController.OnRepair -= RepairPart;
        PlayerController.OnCrash -= CrashPart;
    }

    void Disconnect()
    {
        RemoveMod();

        Joint.connectedBody = null;
        Body.isKinematic = false;
        Body.useGravity = true;
        
        transform.SetParent(null);
        GameManager.OnMergeGame += DestroyPart;
    }

    private void SetActions(bool state)
    {
        if (state)
        {
            GameManager.OnGameStart += OnGameStart;
        
            PlayerController.OnRepair += RepairPart;
            PlayerController.OnCrash += CrashPart;
        }
        else
        {
            GameManager.OnGameStart -= OnGameStart;
        
            PlayerController.OnRepair -= RepairPart;
            PlayerController.OnCrash -= CrashPart;
        }
    }
    
    public virtual void DestroyPart()
    {
        RemoveMod();
        
        SetActions(false);
        GameManager.OnMergeGame -= DestroyPart;
        
        // if(_currentMergeCell) _currentMergeCell.UnRegistry();
        
        Destroy(gameObject);
    }

    protected virtual void OnMouseDown()
    {
        if (GameManager.GameStarted || _blockAll || BlockMove) return;

        if (Type.Category == PartCategory.Grid)
        {
            if (PlayerGrid.Instance.HaveRequireNeighbors(this, _currentGridCell, PartCategory.Boost, true)
                || PlayerGrid.Instance.HaveRequireNeighbors(this, _currentGridCell, PartCategory.Wheels, true)
                || PlayerGrid.Instance.HaveRequireNeighbors(this, _currentGridCell, PartCategory.Wings, true))
            {
                return;
            }
            
            if (PlayerGrid.Instance.HaveRequireNeighbors(this, _currentGridCell, PartCategory.Cabin, true)
                && PlayerGrid.Instance.HaveRequireNeighbors(this, _currentGridCell, PartCategory.Grid, true))
            {
                return;
            }
        }
        
        if (_dragedPart == null)
        {
            SetDragedPart(this);
            transform.parent = null;
            
            SwitchObjects(true);
            SwitchGridCollider(false);
            
            SwitchMirror(false);
            SetOrientation(PartOrientation.Default);
        }
    }

    private Camera Cam => Camera.main;
    private Vector3 mousePos, worldPos;

    private Quaternion targetRotation;

    private float targetSize;
    private bool OverMergeField => MouseOverMergeField.Is || MergeCell.SelectedCell || DeletePart.Selected || BuyPart.Selected;

    protected virtual void OnMouseDrag()
    {
        if (GameManager.GameStarted || _blockAll || BlockMove) return;
        
        ChangeColor(true);
        if (_dragedPart != null && _dragedPart == this)
        {
            // if(MergeCell.SelectedCell) Debug.Log(MergeCell.SelectedCell + ", " + MergeCell.SelectedCell.Part);
            // else Debug.Log(null);
            
            GridCell selectedGrid = GridCell.SelectedCell;
            // Debug.Log(selectedGrid);
            
            if (selectedGrid)
            {
                PlayerGrid.Instance.UpdateRequireOrientation();
                
                if (PreShowOnPlayer)
                {
                    if (selectedGrid.Part && !selectedGrid.Part.Orientations.Block.Front 
                                          && !selectedGrid.AdditionalPart
                                          && Orientations.HaveOrientation(PartOrientation.Front))
                    {
                        SetOrientation(PartOrientation.Front);
                        PlayerGrid.Instance.UpdatePartParsOnGridOnlyPos(this, selectedGrid);
                    
                        // Debug.Log("first");
                    }
                    else if ((!selectedGrid.Part || selectedGrid.Part == this) && 
                             (!selectedGrid.AdditionalPart || selectedGrid.AdditionalPart == this) &&
                             (PlayerGrid.Instance.HaveNeighbors(this, selectedGrid) || _currentGridCell == selectedGrid) && 
                             PlayerGrid.Instance.HaveOrientation(Orientations))
                    {
                        // Debug.Log($"second, {PlayerGrid.Instance._cells.IndexOf(selectedGrid)}");
                        PlayerGrid.Instance.UpdatePartParsOnGrid(this);
                    }
                    else
                    {
                        // Debug.Log("third");
                    
                        transform.SetParent(null);
                        SetMousePosition();
                        SwitchMirror(false);
                    }
                }
                else
                {
                    transform.SetParent(null);
                    SetMousePosition();
                    SwitchMirror(false);
                }
            }
            else
            {
                transform.SetParent(null);
                SetMousePosition();
            }

            if (!MergeCell.SelectedCell && !DeletePart.Selected && !GridCell.SelectedCell)
            {
                ChangeColor(false);
                targetSize = 1.3f;
            }
            else
            {
                ChangeColor(true);
                targetSize = defaultSize;
            }
            
            targetSize *= OverMergeField ? 0.75f : 1f;
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * targetSize, 6f * Time.deltaTime);
        }
    }

    private Vector3 direction, offset;
    private float offsetValue;
    
    void SetMousePosition()
    {
        mousePos = Input.mousePosition;
        mousePos.z = Cam.transform.position.x;
            
        worldPos = Cam.ScreenToWorldPoint(mousePos);

        direction = (worldPos - Cam.transform.position).normalized;
        offsetValue = OverMergeField ? MergeGrid.ChoiseOffset : 0;
        offset = direction * offsetValue;
        transform.position = worldPos - offset;
        
        // transform.position = worldPos;
                
        SetOrientation(PartOrientation.Default);
    }
    
    protected virtual void OnMouseUp()
    {
        if (GameManager.GameStarted || _blockAll || BlockMove) return;
        
        if (_dragedPart != null && _dragedPart == this)
        {
            if (DeletePart.Selected)
            {
                UnRegistry();
                
                DestroyPart();
                SetDragedPart(null);
                return;
            }
            
            GridCell selectedGrid = GridCell.SelectedCell;
            
            if (selectedGrid)
            {
                if (selectedGrid.Part && !selectedGrid.Part.Orientations.Block.Front 
                        && !selectedGrid.AdditionalPart
                        && Orientations.HaveOrientation(PartOrientation.Front))
                {
                    UnRegistry();
                    
                    selectedGrid.RegistryAdditional(this);
                    SetOrientation(PartOrientation.Front);

                    SetAdditionalGrid(selectedGrid);
                    PlayerGrid.Instance.UpdatePartParsOnGridOnlyPos(this, selectedGrid);
                    SetDragedPart(null);
                    
                    return;
                }
                
                if (!selectedGrid.Part && (!_currentGridCell || selectedGrid != _currentGridCell)
                    && PlayerGrid.Instance.HaveNeighbors(this, selectedGrid) && PlayerGrid.Instance.HaveOrientation(Orientations))
                {
                    selectedGrid.Registry(this);
                    SetGrid(selectedGrid);
                }
            }

            if (!BlockMerge)
            {
                MergeCell selectedCell = MergeCell.SelectedCell;

                if (_currentGridCell && !selectedCell)
                {
                    PlayerGrid.Instance.SetDefaultPartParsOnGrid(this);
                }
                else if (selectedCell)
                {
                    MergeCell currentCell = _currentMergeCell;
                    
                    if (!currentCell || selectedCell != currentCell)
                    {
                        Part mergePart = selectedCell.Part;
                        if (mergePart)
                        {
                            if (mergePart.Type == Type && mergePart.Level == Level && Level != Type.MaxLevel)
                            {
                                SetDragedPart(null);
                                
                                if(currentCell) currentCell.UnRegistry();
                                else
                                {
                                    if(_currentGridCell.AdditionalPart == this) _currentGridCell.UnRegistryAdditional();
                                    else _currentGridCell.UnRegistry();
                                }
                                
                                mergePart._currentMergeCell.UnRegistry();

                                Part part = Type.GetPart(Level + 1);
                                MergeGrid.Instance.SpawnPartToCell(part, mergePart._currentMergeCell);

                                mergePart.DestroyPart();
                                DestroyPart();
                                
                                GetPartUpgrade.ShowUpgrade(Type, Level + 1);
                                
                                return;
                            }

                            if (_currentGridCell)
                            {
                                PlayerGrid.Instance.SetDefaultPartParsOnGrid(this);
                                SetDragedPart(null);
                                return;
                            }
                        }
                        else
                        {
                            selectedCell.Registry(this);
                            SetCell(selectedCell);
                        }
                    }

                    SetDefaultParsOnCell();
                }
                else
                {
                    SetDefaultParsOnCell();
                }
            }
            else
            {
                PlayerGrid.Instance.SetDefaultPartParsOnGrid(this);
            }
        }
        
        SetDragedPart(null);
    }

    void UnRegistry()
    {
        if (_currentGridCell)
        {
            if(_currentGridCell.AdditionalPart && _currentGridCell.AdditionalPart == this) _currentGridCell.UnRegistryAdditional();
            if(_currentGridCell.Part && _currentGridCell.Part == this) _currentGridCell.UnRegistry();
            
            RemoveMod();
        }
                    
        if(_currentMergeCell && _currentMergeCell.Part == this) _currentMergeCell.UnRegistry();
    }

    public void SetDefaultParsOnGrid(Vector3 pos, Transform parent)
    {
        SwitchObjects(true);
        SwitchGridCollider(false);
        
        transform.SetParent(parent);
        
        transform.localPosition = pos + Orientations.GetOrientationParameter(Orientation).FixedPosition;
        transform.localScale = Vector3.one * defaultSize;
        
        ChangeColor(true);
        
        ApplyOrientation();
        
        AddMod();
        // OnPartPlaceOnGrid();
    }

    public void SetDefaultParsOnCell()
    {
        SwitchObjects(false);
        SwitchGridCollider(true);
        
        transform.SetParent(_currentMergeCell.transform.GetChild(0));
        
        transform.localPosition = new Vector3(0f, 0f, -1f);
        transform.localScale = Vector3.one * defaultSize;
        
        SetOrientation(PartOrientation.Default);
        ChangeColor(true);
        
        ApplyOrientation();
        // OnPartPlaceOnMergeZone();
    }

    void SwitchObjects(bool state)
    {
        if (inGameObject)
        {
            inGameObject.gameObject.SetActive(state);
        }

        if (_settings.Mod == GridMode.Model)
        {
            if (onGridObject) onGridObject.gameObject.SetActive(!state);
            if (_currentMergeCell) _currentMergeCell.SetUI(false);
        }
        else if (_settings.Mod == GridMode.Sprite)
        {
            if (onGridObject) onGridObject.gameObject.SetActive(false);
            if (_currentMergeCell) _currentMergeCell.SetUI(!state);
        }
    }

    void SwitchMirror(bool state)
    {
        if(additionalObject) additionalObject.SetActive(state);
    }

    void SwitchColliders(bool state)
    {
        foreach (var VARIABLE in _gameColliders)
        {
            if(VARIABLE) VARIABLE.enabled = state;
        }
    }

    void SwitchGridCollider(bool state)
    {
        if(_onGridCollider) _onGridCollider.enabled = state;
    }

    public virtual ParametersModifier GetFlyParameters() => null;
    
    public virtual bool VisualMode { protected get; set; }

    private bool Condition(GameObject go) =>
        go.layer == LayerMask.NameToLayer("Hit") && GameManager.GameStarted && PlayerController.Launched;
    
    public virtual void OnCollisionEnter(Collision other)
    {
        // Debug.Log("COLLISING WITH " + other.gameObject);
        if (Condition(other.gameObject))
        {
            CrashPart();
        }
    }
    
    private void OnCollisionExit(Collision other)
    {
        
    }
    
    GameObject getChildGameObject(string withName) 
    {
        Transform[] ts = transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }
}
