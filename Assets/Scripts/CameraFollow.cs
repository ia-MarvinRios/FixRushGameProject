using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private float smoothSpeed = 5f;

    private Transform target;

    void LateUpdate()
    {
        // Buscar player local si aún no existe
        if (target == null)
        {
            if (PlayerSpawner.Instance != null &&
                PlayerSpawner.Instance.Controller != null)
            {
                target = PlayerSpawner.Instance.Controller.transform;
            }
            else
            {
                return;
            }
        }

        // Movimiento suave
        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        // Opcional:
        transform.LookAt(target);
    }
}
