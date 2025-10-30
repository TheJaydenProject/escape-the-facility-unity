using UnityEngine;

/// <summary>
/// Allows the player to collect a keycard and adds it to their inventory.
/// Implements IInteractable for unified interaction system.
/// </summary>
public class KeycardPickupHandler : MonoBehaviour, IInteractable
{
    public string GetDescription()
    {
        return "Pick up Keycard";
    }

    public void Interact()
    {
        PlayerInventory inventory = GameObject.FindGameObjectWithTag(GameTags.Player)?.GetComponent<PlayerInventory>();
        
        if (inventory != null)
        {
            inventory.GiveKeycard();
            Debug.Log("[Keycard] Collected!");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("[Keycard] PlayerInventory not found!");
        }
    }
}
