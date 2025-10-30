using UnityEngine;

/// <summary>
/// Displays a hazard warning UI when the player enters a trigger zone (e.g., near a dangerous area).
/// Warning is shown only once and disappears after a short duration.
/// </summary>
/*
 * Author: Jayden Wong
 * Date: 16/06/2025
 * Description: Handles player-triggered warnings for dangerous zones. Displays a warning UI once when 
 * the player enters the trigger area and hides it after a set time. Useful for introducing environmental hazards.
 */
public class HazardWarningTrigger : MonoBehaviour
{
    /// <summary>
    /// UI element (e.g., text panel) shown when the player enters the hazard zone.
    /// </summary>
    public GameObject warningUI;

    /// <summary>
    /// Duration in seconds that the warning UI stays visible.
    /// </summary>
    public float displayTime = 2f;

    /// <summary>
    /// Flag to ensure the warning is only shown once per playthrough.
    /// </summary>
    private bool hasShown = false;

    /// <summary>
    /// Triggered when any collider enters the hazard zone.
    /// If it's the player and the warning hasn't been shown, display it.
    /// </summary>
    /// <param name="other">The collider that entered the trigger zone.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Only show the warning once and only when the player enters
        if (!hasShown && other.CompareTag(GameTags.Player))
        {
            hasShown = true; // Prevent future triggers
            StartCoroutine(ShowWarning()); // Start the coroutine to display the UI
        }
    }

    /// <summary>
    /// Coroutine to show the warning UI for a limited time before hiding it.
    /// </summary>
    private System.Collections.IEnumerator ShowWarning()
    {
        // Activate the warning UI
        warningUI.SetActive(true);

        // Wait for the specified display duration
        yield return new WaitForSeconds(displayTime);

        // Deactivate the warning UI after the delay
        warningUI.SetActive(false);
    }
}
