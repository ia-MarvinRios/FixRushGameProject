using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Controller Settings")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private HoverDetector _hoverDetector;

    [SerializeField] private Vector2 _maxMinZ;
    [SerializeField] private float _cameraSliderLenght;
    [SerializeField] private float _zOffset;
    [SerializeField] private float _xOffset;
    [SerializeField] private float _hoverSmoothTime = 1f;

    float _playerZ;
    float _leftBound;
    float _rightBound;
    private float _startX;
    private float _currentX;
    private float _targetX;
    private float _xVelocity;
    private float _halfLength;
    private float _cameraSliderPositionZ;
    private Vector3 _targetPosition;
    private Transform _targetPlayer;

    #region UNITY_CALLBACKS

    private void OnEnable()
    {
        PlayerSpawner.OnLocalPlayerSpawned += SetPlayer;
    }

    private void OnDisable()
    {
        PlayerSpawner.OnLocalPlayerSpawned -= SetPlayer;
    }

    private void Start()
    {
        _targetPosition = _mainCamera.transform.position;
        _startX         = _mainCamera.transform.position.x;
        _currentX       = _startX;
    }

    private void Update()
    {
        CheckHover();
        FollowPlayerZ();

        _mainCamera.transform.position = _targetPosition;
    }

    #endregion

    private void SetPlayer(PlayerController player) { _targetPlayer = player.transform; }
    private Vector3 GetMaxMinCenter() { return ( new Vector3(-10, 0, _maxMinZ.x) + new Vector3(-10, 0, _maxMinZ.y) ) / 2f; }
    private void FollowPlayerZ()
    {
        if (_targetPlayer == null) { return; }

        _playerZ = _targetPlayer.position.z;

        // Current camera position
        _targetPosition.z = _mainCamera.transform.position.z;

        // Dead zone limits
        _halfLength = _cameraSliderLenght / 2f;

        _leftBound = _targetPosition.z - _halfLength + _zOffset;
        _rightBound = _targetPosition.z + _halfLength + _zOffset;

        // Player exceeded right side
        if (_playerZ > _rightBound)
        {
            _targetPosition.z = _playerZ - _halfLength - _zOffset;
        }

        // Player exceeded left side
        else if (_playerZ < _leftBound)
        {
            _targetPosition.z = _playerZ + _halfLength - _zOffset;
        }

        // Clamp camera to world limits
        _targetPosition.z = Mathf.Clamp(
            _targetPosition.z,
            _maxMinZ.y + _halfLength,
            _maxMinZ.x - _halfLength
        );
    }

    private void CheckHover()
    {
        _targetX =
            _hoverDetector.IsHovered
            ? _startX + _xOffset
            : _startX;

        _currentX = Mathf.SmoothDamp(
            _currentX,
            _targetX,
            ref _xVelocity,
            _hoverSmoothTime
        );

        _targetPosition.x = _currentX;
    }

    [ContextMenu("Adjust With Area Length")]
    private void AdjustWithAreaLength()
    {
        _maxMinZ.x += _cameraSliderLenght / 2f;
        _maxMinZ.y -= _cameraSliderLenght / 2f;
    }

    private void OnDrawGizmos()
    {
        if (_mainCamera == null) { return; }

        _cameraSliderPositionZ = _mainCamera.transform.position.z;

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(new Vector3(-10, 0, _maxMinZ.x), Vector3.one);
        Gizmos.DrawWireCube(new Vector3(-10, 0, _maxMinZ.y), Vector3.one);
        Gizmos.DrawWireCube(new Vector3(-10, 0, _cameraSliderPositionZ), new Vector3(5f, 1f, _cameraSliderLenght));
    }
}
