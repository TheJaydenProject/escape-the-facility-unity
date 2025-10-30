using UnityEngine;

/// <summary>
/// Unified raycast-based interaction system for all interactable objects.
/// Replaces separate door, keycard, and gas mask interactors.
/// </summary>
public class UnifiedPlayerInteractor : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Maximum distance to interact with objects")]
    public float interactDistance = 2f;
    
    [Tooltip("Origin point for raycast (usually camera)")]
    public Transform rayOrigin;

    [Header("Door Prompts")]
    [Tooltip("Prompt shown when door can be opened")]
    public GameObject doorOpenPrompt;
    
    [Tooltip("Prompt shown when door can be closed")]
    public GameObject doorClosePrompt;
    
    [Tooltip("Prompt shown when door is locked")]
    public GameObject doorLockedPrompt;

    [Header("Item Prompts")]
    [Tooltip("Prompt shown when looking at keycard")]
    public GameObject keycardPrompt;
    
    [Tooltip("Prompt shown when looking at gas mask")]
    public GameObject gasMaskPrompt;

    [Header("Feedback Panels (Temporary Messages)")]
    [Tooltip("Panel shown briefly when door is unlocked")]
    public GameObject doorUnlockedPanel;
    
    [Tooltip("Panel shown briefly when trying to open locked door without keycard")]
    public GameObject lockedMessagePanel;

    [Header("Status Indicators (Persistent)")]
    [Tooltip("Shows keycard icon when owned")]
    public GameObject keycardAcquiredIndicator;
    
    [Tooltip("Shows gas mask icon when owned")]
    public GameObject gasMaskAcquiredIndicator;

    private IInteractable currentTarget;
    private float feedbackTimer = 0f;
    private GameObject activeFeedbackPanel;
    private PlayerInventory inventory;

    void Start()
    {
        inventory = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerInventory>();
        if (inventory == null)
            Debug.LogWarning("[UnifiedInteractor] PlayerInventory not found!");
    }

    void Update()
    {
        HandleFeedbackTimer();
        UpdateInventoryIndicators();
        PerformRaycast();
    }

    private void PerformRaycast()
    {
        if (rayOrigin == null) return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.cyan);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // Try to get interactable component
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable == null)
                interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                currentTarget = interactable;
                ShowAppropriatePrompt(hit.collider.gameObject);

                // Handle interaction input
                if (Input.GetKeyDown(KeyCode.E))
                {
                    HandleInteraction(hit.collider.gameObject);
                }
                return;
            }
        }

        // Nothing targeted - hide all prompts
        currentTarget = null;
        HideAllPrompts();
    }

    private void ShowAppropriatePrompt(GameObject target)
    {
        // Check for basic doors
        BasicDoorController basicDoor = target.GetComponent<BasicDoorController>() 
            ?? target.GetComponentInParent<BasicDoorController>();
        
        if (basicDoor != null)
        {
            ShowPrompt(basicDoor.IsOpen() ? doorClosePrompt : doorOpenPrompt);
            return;
        }

        // Check for locked doors
        LockedDoorController lockedDoor = target.GetComponent<LockedDoorController>() 
            ?? target.GetComponentInParent<LockedDoorController>();
        
        if (lockedDoor != null)
        {
            bool hasKey = inventory != null && inventory.HasKeycard();
            bool isOpen = lockedDoor.IsOpen();
            
            if (hasKey)
                ShowPrompt(isOpen ? doorClosePrompt : doorOpenPrompt);
            else
                ShowPrompt(doorLockedPrompt);
            return;
        }

        // Check for keycard pickup
        if (target.CompareTag("Keycard"))
        {
            ShowPrompt(keycardPrompt);
            return;
        }

        // Check for gas mask pickup
        if (target.CompareTag("GasMask"))
        {
            ShowPrompt(gasMaskPrompt);
            return;
        }

        // Unknown interactable - hide prompts
        HideAllPrompts();
    }

    private void HandleInteraction(GameObject target)
    {
        // Handle locked doors specially (check for keycard + show feedback)
        LockedDoorController lockedDoor = target.GetComponent<LockedDoorController>() 
            ?? target.GetComponentInParent<LockedDoorController>();
        
        if (lockedDoor != null)
        {
            if (inventory != null && inventory.HasKeycard())
            {
                // Check state BEFORE interaction
                bool wasClosedBeforeInteract = !lockedDoor.IsOpen();
                
                // Perform the interaction
                currentTarget.Interact();
                
                // Show unlock message only when we just opened it
                if (wasClosedBeforeInteract)
                    ShowFeedback(doorUnlockedPanel, 1.5f);
            }
            else
            {
                // Show "need keycard" message
                ShowFeedback(lockedMessagePanel, 1.2f);
            }
            return;
        }

        // All other interactions (pickups, basic doors, etc.)
        currentTarget.Interact();
    }

    private void ShowPrompt(GameObject prompt)
    {
        HideAllPrompts();
        if (prompt != null)
            prompt.SetActive(true);
    }

    private void HideAllPrompts()
    {
        // Hide door prompts
        if (doorOpenPrompt != null) doorOpenPrompt.SetActive(false);
        if (doorClosePrompt != null) doorClosePrompt.SetActive(false);
        if (doorLockedPrompt != null) doorLockedPrompt.SetActive(false);
        
        // Hide item prompts
        if (keycardPrompt != null) keycardPrompt.SetActive(false);
        if (gasMaskPrompt != null) gasMaskPrompt.SetActive(false);
    }

    private void ShowFeedback(GameObject panel, float duration)
    {
        if (panel != null)
        {
            // Hide previous feedback
            if (activeFeedbackPanel != null)
                activeFeedbackPanel.SetActive(false);

            panel.SetActive(true);
            activeFeedbackPanel = panel;
            feedbackTimer = duration;
        }
    }

    private void HandleFeedbackTimer()
    {
        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.unscaledDeltaTime;
            
            if (feedbackTimer <= 0f && activeFeedbackPanel != null)
            {
                activeFeedbackPanel.SetActive(false);
                activeFeedbackPanel = null;
            }
        }
    }

    private void UpdateInventoryIndicators()
    {
        if (inventory == null) return;

        // Update persistent status indicators (always visible when owned)
        if (keycardAcquiredIndicator != null)
            keycardAcquiredIndicator.SetActive(inventory.HasKeycard());
        
        if (gasMaskAcquiredIndicator != null)
            gasMaskAcquiredIndicator.SetActive(inventory.HasGasMask());
    }

    void OnDrawGizmosSelected()
    {
        if (rayOrigin == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(rayOrigin.position, rayOrigin.position + rayOrigin.forward * interactDistance);
    }
}