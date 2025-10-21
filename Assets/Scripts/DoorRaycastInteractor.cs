using UnityEngine;

/// <summary>
/// Handles player interaction with normal and locked doors using raycasting.
/// Displays appropriate UI prompts and messages, and logs key events for debugging.
/// </summary>
/*
 * Author: Jayden Wong
 * Date: 16/06/2025
 * Description: Detects and interacts with regular and locked doors using a raycast.
 * Shows specific UI prompts for each type and handles 'E' key interactions.
 */

public class PlayerDoorInteractor : MonoBehaviour
{
    [Header("Settings")]

    /// <summary>
    /// Maximum distance the player can interact with a door via raycast.
    /// </summary>
    [Tooltip("Maximum distance to interact with doors")]
    public float interactDistance = 2f;

    [Header("References")]

    /// <summary>
    /// Origin point from which the raycast will be cast (usually the player camera).
    /// </summary>
    public Transform checkOrigin;

    /// <summary>
    /// UI prompt shown when a door can be opened.
    /// </summary>
    public GameObject interactPromptOpen;

    /// <summary>
    /// UI prompt shown when a door can be closed.
    /// </summary>
    public GameObject interactPromptClose;

    /// <summary>
    /// UI prompt shown when a door is locked and cannot be opened.
    /// </summary>
    public GameObject interactPromptLocked;

    /// <summary>
    /// Panel that briefly shows a locked door message when the player lacks a keycard.
    /// </summary>
    public GameObject lockedMessagePanel;

    /// <summary>
    /// UI panel that briefly shows when a locked door is successfully unlocked.
    /// </summary>
    public GameObject doorUnlockedMessagePanel;

    /// <summary>
    /// Timer for how long the locked message panel stays visible.
    /// </summary>
    private float lockedMessageTimer = 0f;

    /// <summary>
    /// Timer for how long the unlocked door message panel stays visible.
    /// </summary>
    private float unlockedMessageTimer = 0f;

    void Update()
    {
        // Exit if no origin for raycast is set (e.g., missing reference)
        if (checkOrigin == null) return;

        // Handle timer-based hiding of the locked door message panel
        if (lockedMessagePanel != null && lockedMessagePanel.activeSelf)
        {
            lockedMessageTimer -= Time.unscaledDeltaTime;

            // Hide the message when the timer runs out
            if (lockedMessageTimer <= 0f)
                lockedMessagePanel.SetActive(false);
        }

        // Handle timer-based hiding of the unlocked door message panel
        if (doorUnlockedMessagePanel != null && doorUnlockedMessagePanel.activeSelf)
        {
            unlockedMessageTimer -= Time.unscaledDeltaTime;

            // Hide the unlocked message when time runs out
            if (unlockedMessageTimer <= 0f)
                doorUnlockedMessagePanel.SetActive(false);
        }

        // Create a ray from the origin forward to detect interactable objects
        Ray ray = new Ray(checkOrigin.position, checkOrigin.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red); // Debug line in editor
        RaycastHit hit;

        // Perform the raycast
        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // Try to get the relevant door or trigger component from the object hit
            BasicDoorController door = hit.collider.GetComponentInParent<BasicDoorController>();
            LockedDoorController lockedDoor = hit.collider.GetComponentInParent<LockedDoorController>();

            // If a basic door was hit, handle open/close interaction
            if (door != null)
            {
                bool isOpen = door.IsOpen();

                // Show appropriate prompt depending on door state
                SetPromptStates(!isOpen, isOpen, false);

                // If 'E' is pressed, toggle the door state
                if (Input.GetKeyDown(KeyCode.E))
                {
                    door.Interact();
                    Debug.Log(isOpen ? "[DoorInteractor] Closed door" : "[DoorInteractor] Opened door");
                }

                return;
            }

            // If a locked door was hit, check inventory for keycard before allowing interaction
            if (lockedDoor != null)
            {
                var inventory = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerInventory>();
                bool hasKeycard = inventory != null && inventory.HasKeycard();
                bool isOpen = lockedDoor.IsOpen();

                // Show prompts based on lock status and possession of keycard
                SetPromptStates(hasKeycard && !isOpen, hasKeycard && isOpen, !hasKeycard);

                // Handle interaction input
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (hasKeycard)
                    {
                        lockedDoor.Interact();

                        // Show "Door Unlocked" prompt only when opening (not closing)
                        if (!isOpen && doorUnlockedMessagePanel != null)
                        {
                            doorUnlockedMessagePanel.SetActive(true);
                            unlockedMessageTimer = 1.5f;
                        }

                        Debug.Log(isOpen ? "[DoorInteractor] Closed locked door" : "[DoorInteractor] Opened locked door");
                    }
                    else if (lockedMessagePanel != null)
                    {
                        // Show warning panel if keycard is missing
                        lockedMessagePanel.SetActive(true);
                        lockedMessageTimer = 1.2f;
                    }
                }

                return;
            }
        }

        // No interactable object in front, hide all prompts
        SetPromptStates(false, false, false);
    }

    /// <summary>
    /// Controls the visibility of different interaction prompts.
    /// </summary>
    /// <param name="open">Show open door prompt</param>
    /// <param name="close">Show close door prompt</param>
    /// <param name="locked">Show locked door prompt</param>
    void SetPromptStates(bool open, bool close, bool locked)
    {
        if (interactPromptOpen != null) interactPromptOpen.SetActive(open);
        if (interactPromptClose != null) interactPromptClose.SetActive(close);
        if (interactPromptLocked != null) interactPromptLocked.SetActive(locked);
    }

    /// <summary>
    /// Visualizes the raycast in the editor when the object is selected.
    /// Helps in debugging the raycast direction and range.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (checkOrigin == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(checkOrigin.position, checkOrigin.position + checkOrigin.forward * interactDistance);
    }
}
