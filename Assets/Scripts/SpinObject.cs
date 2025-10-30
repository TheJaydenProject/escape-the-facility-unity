using UnityEngine;

/// <summary>
/// Continuously rotates the GameObject around the Y-axis at a constant speed.
/// Intended for spinning collectibles or objects that draw player attention.
/// </summary>
/*
 * Description: This script rotates an object around its Y-axis to create a visual cue,
 * typically used for collectibles or interactive items in a Unity 3D environment.
 */
public class SpinObject : MonoBehaviour
{
    /// <summary>
    /// Rotation speed around the Y-axis in degrees per second.
    /// Can be adjusted in the Inspector.
    /// </summary>
    public float yRotationSpeed = 90f;

    /// <summary>
    /// Unity's built-in method called every frame.
    /// Applies a Y-axis rotation to make the object spin.
    /// </summary>
    void Update()
    {
        // Rotate the object around the Y-axis continuously to draw player attention
        transform.Rotate(0, yRotationSpeed * Time.deltaTime, 0);
    }
}
