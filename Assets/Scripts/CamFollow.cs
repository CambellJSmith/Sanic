using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform target;  // The object to follow, assignable in the editor

    [Header("Follow Settings")]
    public float smoothSpeed = 0.125f;  // Smoothing factor, higher = slower smoothing
    public float speedRampFactor = 0.1f;  // Speed ramping factor, larger value means more gradual speed increase

    private Vector3 currentVelocity;  // Keeps track of the current velocity for smoothing

    private void FixedUpdate()  // Use FixedUpdate for consistent physics-based movement
    {
        if (target != null)
        {
            // Get the target position without the Z axis (assuming 2D-like movement)
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

            // Calculate the distance to the target and adjust smoothing dynamically based on distance
            float distance = Vector3.Distance(transform.position, targetPosition);
            float smoothingFactor = Mathf.Lerp(0.05f, smoothSpeed, distance * speedRampFactor);

            // Apply smoothing with a consistent velocity
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothingFactor);
        }
    }
}
