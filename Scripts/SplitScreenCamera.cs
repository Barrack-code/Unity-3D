using UnityEngine;

public class SplitScreenCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public bool isTopScreen = true;  // true for top screen (Player 1), false for bottom (Player 2)

    [Header("Follow Settings")]
    public float distance = 6.0f;
    public float height = 2.5f;
    public float smoothSpeed = 10f;
    public float rotationSmoothSpeed = 5f;
    public float lookAheadDistance = 3f;

    private Camera cam;
    private Vector3 smoothVelocity;
    private float currentRotationY;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = gameObject.AddComponent<Camera>();
        }

        // Set up split screen
        cam.rect = isTopScreen ? 
            new Rect(0, 0.5f, 1, 0.5f) :  // Top half
            new Rect(0, 0, 1, 0.5f);      // Bottom half

        // Adjust camera settings
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 1000f;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position
        Vector3 desiredPosition = target.position - (target.forward * distance);
        desiredPosition.y = target.position.y + height;

        // Smoothly move camera
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref smoothVelocity, smoothSpeed * Time.deltaTime);

        // Calculate look-ahead point
        Vector3 lookAtPoint = target.position + (target.forward * lookAheadDistance);
        lookAtPoint.y = target.position.y;

        // Smoothly rotate to look at target
        Quaternion targetRotation = Quaternion.LookRotation(lookAtPoint - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Draw camera target position
            Gizmos.color = Color.yellow;
            Vector3 targetPos = target.position - (target.forward * distance);
            targetPos.y = target.position.y + height;
            Gizmos.DrawWireSphere(targetPos, 0.5f);

            // Draw look-ahead point
            Gizmos.color = Color.red;
            Vector3 lookAtPoint = target.position + (target.forward * lookAheadDistance);
            lookAtPoint.y = target.position.y;
            Gizmos.DrawWireSphere(lookAtPoint, 0.3f);

            // Draw line from camera to target
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(targetPos, lookAtPoint);
        }
    }
#endif
}
