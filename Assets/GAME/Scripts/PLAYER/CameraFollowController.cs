using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[ExecuteInEditMode]
public class CameraFollowController : MonoBehaviour
{
    // [SerializeField] private Vector3 offsetInGame;
    // [SerializeField] private Vector3 rotationInGame;
    //
    // private CinemachineTransposer _transposer;
    // private CinemachineVirtualCamera _camera;
    //
    // private Transform _target;
    //
    // private void Awake()
    // {
    //     _camera = GetComponent<CinemachineVirtualCamera>();
    //     _transposer = _camera.GetCinemachineComponent<CinemachineTransposer>();
    //
    //     GameManager.OnGameStart += OnGameStart;
    // }
    //
    // private void OnGameStart()
    // {
    //     _camera.Follow = PlayerController.Follow;
    //     _camera.m_Lens.FieldOfView = 60;
    //     
    //     _transposer.m_FollowOffset = offsetInGame;
    //     transform.rotation = Quaternion.Euler(rotationInGame);
    // }
    
    public static CameraFollowController Instance { get; private set; }
    
    [SerializeField] private Transform defaultTarget;
    [SerializeField] private Camera cam;
    [Space]
    [SerializeField] private float distanceToTarget = 9f;
    [SerializeField] private float upSpace = 2f;
    [Space]
    [SerializeField] private float speedMove = 5f;
    
    private Transform target;
    
    public void SetTarget(Transform trg) => target = trg;
    public void ResetTarget() => SetTarget(defaultTarget);
    public void ResetPosition() => transform.position = position;
    
    public void Reset()
    {
        ResetTarget();
        ResetPosition();
    }
    
    private Vector3 position => target.position - 
        transform.rotation * new Vector3(0,0,1) * distanceToTarget + new Vector3(0f, upSpace, 0f);
    
    void Awake()
    {
        Instance = this;
        GameManager.OnGameStart += Reset;
    }

    void Start()
    {
        Reset();
    }
    
    void Update()
    {
        if (target != null)
        {
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localEulerAngles = Vector3.zero;
            cam.fieldOfView = 60;
            
            transform.position = Vector3.Slerp(transform.position, position, speedMove * Time.deltaTime);
        }
    }
}
