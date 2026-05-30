using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Controller Settings")]
    [SerializeField] private Camera _mainCamera;

    [SerializeField] private Vector2 _maxMinZ;
    [SerializeField] private float _cameraSliderLenght;
    [SerializeField] private float _zOffset;

    float _playerZ;
    float _leftBound;
    float _rightBound;
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
        _targetPosition = new Vector3(-10, 0, _cameraSliderPositionZ);
    }

    private void Update()
    {
        FollowPlayerZ();
    }

    #endregion

    private void SetPlayer(PlayerController player) { _targetPlayer = player.transform; }
    private Vector3 GetMaxMinCenter() { return ( new Vector3(-10, 0, _maxMinZ.x) + new Vector3(-10, 0, _maxMinZ.y) ) / 2f; }
    private void FollowPlayerZ()
    {
        if (_targetPlayer == null) { return; }

        _playerZ = _targetPlayer.position.z;

        // Current camera position
        _targetPosition = _mainCamera.transform.position;

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

        _mainCamera.transform.position = _targetPosition;
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
