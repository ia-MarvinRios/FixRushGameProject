using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -8f);
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Tolerancia")]
    [SerializeField] private float movementTolerance = 0.08f;
    [SerializeField] private float stopTolerance = 0.01f;
    [SerializeField] private bool ignoreYMovement = true;

    private Transform target;
    private Vector3 velocity = Vector3.zero;

    private Vector3 stableTargetPosition;
    private bool hasStablePosition = false;

    private void LateUpdate()
    {
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

        Vector3 currentTargetPosition = target.position;

        if (!hasStablePosition)
        {
            stableTargetPosition = currentTargetPosition;
            hasStablePosition = true;
        }

        Vector3 movementDelta = currentTargetPosition - stableTargetPosition;

        if (ignoreYMovement)
        {
            movementDelta.y = 0f;
        }

        // Solo actualiza la posición objetivo si el player se movió lo suficiente
        if (movementDelta.magnitude >= movementTolerance)
        {
            if (ignoreYMovement)
            {
                currentTargetPosition.y = stableTargetPosition.y;
            }

            stableTargetPosition = currentTargetPosition;
        }

        Vector3 desiredPosition = stableTargetPosition + offset;

        // Si la cámara ya está demasiado cerca, no seguir moviéndola
        if ((transform.position - desiredPosition).sqrMagnitude <= stopTolerance * stopTolerance)
        {
            transform.position = desiredPosition;
            velocity = Vector3.zero;
            return;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        // No uses LookAt si te genera temblor
        // transform.LookAt(target);
    }
}