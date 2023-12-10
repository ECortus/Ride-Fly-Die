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

    [SerializeField] MergeCell _currentMergeCell;
    [SerializeField] GridCell _currentGridCell;
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

    protected virtual void Awake()
    {
        Init();
    }

    void OnGameStart()
    {
        // if(Body) Destroy(Body);
        Body.isKinematic = false;
        Part neighbor;

        Body.interpolation = RigidbodyInterpolation.None;
        Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (HaveRequireNeighbors(out neighbor) || Type.Category == PartCategory.Cabin || Type.Category == PartCategory.Grid)
        {
            Body.useGravity = false;
            
            // if (Type.Category != PartCategory.Cabin) Joint.connectedBody = neighbor.Body;
            // else Joint.connectedBody = PlayerController.Instance.Body;
            
            Joint.connectedBody = PlayerController.Instance.Body;
            
            AddMod();
        }
        else
        {
            Disconnect();
        }
        
        SwitchObjects(true);
        SwitchColliders(true);
        SwitchGridCollider(false);
    }

    void AddMod()
    {
        PlayerController.Instance.AddPart(this);
    }
    
    void RemoveMod()
    {
        PlayerController.Instance.RemovePart(this);
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
    }
    
    void FixedUpdate()
    {
        
    }
    
    public void SetGrid(GridCell cell)
    {
        if (_currentGridCell)
        {
            if (_currentGridCell.AdditionalPart && _currentGridCell.AdditionalPart == this) _currentGridCell.UnRegistryAdditional();
            else if (_currentGridCell.Part == this) _currentGridCell.UnRegistry();
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
        
        RemoveMod();
        
        Disconnect();
        Body.isKinematic = true;
        
        PlayerController.OnRepair -= RepairPart;
        PlayerController.OnCrash -= CrashPart;
    }

    void Disconnect()
    {
        Destroy(Joint);
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
        SetActions(false);
        GameManager.OnMergeGame -= DestroyPart;
        
        // if(_currentMergeCell) _currentMergeCell.UnRegistry();
        
        Destroy(gameObject);
    }

    protected virtual void OnMouseDown()
    {
        if (GameManager.GameStarted || _blockAll || BlockMove) return;
        
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

    protected virtual void OnMouseDrag()
    {
        if (GameManager.GameStarted || _blockAll || BlockMove) return;
        
        ChangeColor(true);
        if (_dragedPart != null && _dragedPart == this)
        {
            // if(MergeCell.SelectedCell) Debug.Log(MergeCell.SelectedCell + ", " + MergeCell.SelectedCell.Part);
            // else Debug.Log(null);
            
            GridCell selectedGrid = GridCell.SelectedCell;
            if (selectedGrid) PlayerGrid.Instance.UpdateRequireOrientation();
            
            if (selectedGrid)
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
            }

            if (!MergeCell.SelectedCell && !GridCell.SelectedCell && !DeletePart.Selected)
            {
                ChangeColor(false);
                transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * 1.3f, 6f * Time.deltaTime);
            }
            else
            {
                ChangeColor(true);
                transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * defaultSize, 6f * Time.deltaTime);
            }
        }
    }

    void SetMousePosition()
    {
        mousePos = Input.mousePosition;
        mousePos.z = Cam.transform.position.x;
            
        worldPos = Cam.ScreenToWorldPoint(mousePos);
        transform.position = worldPos;
                
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
                else if (!selectedGrid.Part && (!_currentGridCell || selectedGrid != _currentGridCell)
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
                                else _currentGridCell.UnRegistry();
                                
                                mergePart._currentMergeCell.UnRegistry();

                                Part part = Type.GetPart(Level + 1);
                                MergeGrid.Instance.SpawnPartToCell(part, mergePart._currentMergeCell);

                                mergePart.DestroyPart();
                                DestroyPart();
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

    private bool condition(GameObject go) =>
        go.layer == LayerMask.NameToLayer("Hit") 
    //|| go.layer == LayerMask.NameToLayer("Ground")
    ;
    
    public virtual void OnCollisionEnter(Collision other)
    {
        // Debug.Log("COLLISING WITH " + other.gameObject);
        if (condition(other.gameObject))
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
