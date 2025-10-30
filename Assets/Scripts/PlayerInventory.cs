using UnityEngine;

/// <summary>
/// Tracks items collected by the player (e.g., keycard and gas mask).
/// Provides public methods to update and check item possession status.
/// </summary>
/*
 * Description: This script manages the player's inventory flags for special items like the keycard
 * and gas mask. Other scripts query this component to determine if the player can access
 * restricted areas or is immune to hazards.
 */
public class PlayerInventory : MonoBehaviour
{
    /// <summary>
    /// Whether the player has collected the keycard.
    /// Used to unlock restricted doors.
    /// </summary>
    public bool hasKeycard = false;

    /// <summary>
    /// Whether the player has collected the gas mask.
    /// Used to resist gas hazard damage.
    /// </summary>
    public bool hasGasMask = false;

    /// <summary>
    /// Grants the player a keycard.
    /// Typically called when the player collects a keycard object.
    /// </summary>
    public void GiveKeycard()
    {
        hasKeycard = true;
    }

    /// <summary>
    /// Grants the player a gas mask.
    /// Typically called when the player collects a gas mask object.
    /// </summary>
    public void GiveGasMask()
    {
        hasGasMask = true;
    }

    /// <summary>
    /// Checks if the player currently has the keycard.
    /// </summary>
    /// <returns>True if the player has the keycard.</returns>
    public bool HasKeycard()
    {
        return hasKeycard;
    }

    /// <summary>
    /// Checks if the player currently has the gas mask.
    /// </summary>
    /// <returns>True if the player has the gas mask.</returns>
    public bool HasGasMask()
    {
        return hasGasMask;
    }
}
