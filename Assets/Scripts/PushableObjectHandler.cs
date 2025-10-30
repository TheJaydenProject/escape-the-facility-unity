using UnityEngine;

/// <summary>
/// Applies push force to rigidbody objects when the player collides with them using a CharacterController.
/// Useful for simulating realistic physics like pushing crates or barrels.
/// </summary>
/*
 * Description: Enables the player to push non-kinematic rigidbody objects by applying horizontal force
 * during collisions. Also logs a message when pushing, throttled to once every few seconds to prevent spam.
 */
public class PushableObjectHandler : MonoBehaviour
{
    /// <summary>
    /// CharacterController attached to the player.
    /// Required for detecting collision interactions.
    /// </summary>
    public CharacterController controller;

    /// <summary>
    /// Strength of the push applied to the object.
    /// </summary>
    public float pushPower = 4f;

    /// <summary>
    /// Minimum delay (in seconds) between push debug logs.
    /// </summary>
    public float pushLogCooldown = 3f;

    /// <summary>
    /// Timestamp of the last push log.
    /// Used to throttle how often push events are logged.
    /// </summary>
    private float lastPushTime = -Mathf.Infinity;

    /// <summary>
    /// Called when the CharacterController collides with another collider.
    /// Applies a force to the other object if it has a non-kinematic Rigidbody.
    /// </summary>
    /// <param name="hit">Collision information from the controller.</param>
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Get the Rigidbody attached to the collided object
        Rigidbody body = hit.collider.attachedRigidbody;

        // Skip if the object has no Rigidbody or is marked as kinematic (immovable)
        if (body == null || body.isKinematic)
            return;

        // Prevent pushing if the player is falling down onto the object
        if (hit.moveDirection.y < -0.3f)
            return;

        // Calculate push direction (horizontal only)
        Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // Apply an impulse force to the object at the collision point
        body.AddForceAtPosition(pushDirection * pushPower, hit.point, ForceMode.Impulse);

        // Log push event with cooldown to avoid spamming console
        if (Time.time - lastPushTime >= pushLogCooldown)
        {
            Debug.Log($"[PushableObjectHandler] Pushed: {body.name}");
            lastPushTime = Time.time;
        }
    }
}
