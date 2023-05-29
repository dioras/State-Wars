using System;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float cameraTargetSpeed = 10f;
    [SerializeField] private float cameraTargetSpeedRotation = 10f;
    [SerializeField] private float cameraMoveDuration = 1.5f;
    [SerializeField] private float cameraRotateDuration = 1f;
    [SerializeField] private float orthographicSize = 0f;

    private Transform target = default;
    private Sequence camSequence = default;
    private Camera currencyCamera = default;
    
    private float ratio = 1.77f;
    private float height = 0f;
    private float width = 0f;

    public static Action<Transform> SetTargetAction = default;
    public static Action<Vector3, Vector3, Action> SetTweenCameraPositionAction = default;
    public static Action<Vector3, Vector3> SetCameraPositionAction = default;
    public static Action<bool> SetOrthographicAction = default;
    public static Action<float, Action> SetOrthographicSizeAction = default;


    private void OnEnable()
    {
        SetTargetAction += SetCameraTarget;
        SetTweenCameraPositionAction += SetTweenCameraPosition;
        SetCameraPositionAction += SetCameraPosition;
        SetOrthographicAction += SetOrthographic;
        SetOrthographicSizeAction += SetOrthographicSize;
    }

    private void OnDisable()
    {
        SetTargetAction -= SetCameraTarget;
        SetTweenCameraPositionAction -= SetTweenCameraPosition;
        SetCameraPositionAction -= SetCameraPosition;
        SetOrthographicAction -= SetOrthographic;
        SetOrthographicSizeAction -= SetOrthographicSize;
    }

    private void Awake()
    {
        currencyCamera = GetComponent<Camera>();
    }

    private void Start()
    {
        RecalculateOffsetCameraFromResolution();
    }

    private void Update()
    {
        if (target != null)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, cameraTargetSpeedRotation * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, target.position, cameraTargetSpeed * Time.deltaTime);
        }
    }
    
    private void RecalculateOffsetCameraFromResolution()
    {
        height = Screen.height;
        width = Screen.width;

        float currentRatio = height / width;
        float newRatio = currentRatio / ratio;

        currencyCamera.fieldOfView *= newRatio;
    }

    private void SetCameraTarget(Transform _target)
    {
        ClearCameraTarget();
        target = _target;
    }

    private void SetOrthographic(bool _orthographic)
    {
        currencyCamera.orthographic = _orthographic;
        currencyCamera.orthographicSize = orthographicSize;
    }

    private void SetOrthographicSize(float _toValue, Action _action)
    {
        currencyCamera.DOOrthoSize(_toValue, .5f).OnComplete(() => _action?.Invoke());
    }

    private void ClearCameraTarget()
    {
        target = null;
        if (camSequence != null)
        {
            camSequence.Complete();
        }
    }

    private void SetCameraPosition(Vector3 _position, Vector3 _euler)
    {
        ClearCameraTarget();
        transform.position = _position;
        transform.eulerAngles = _euler;
    }

    private void SetTweenCameraPosition(Vector3 _position, Vector3 _euler, Action _action = null)
    {
        ClearCameraTarget();
        camSequence = DOTween.Sequence();
        camSequence.Append(transform.DOMove(_position, cameraMoveDuration));
        camSequence.Join(transform.DORotate(_euler, cameraRotateDuration));
        camSequence.OnComplete(() => {
            _action?.Invoke();
            camSequence = null;
        });
    }
}
