using UnityEngine;

/// <summary>
/// Allows the player to collect a gas mask and adds it to their inventory.
/// Implements IInteractable for unified interaction system.
/// </summary>
public class GasMaskPickupHandler : MonoBehaviour, IInteractable
{
    public string GetDescription()
    {
        return "Pick up Gas Mask";
    }

    public void Interact()
    {
        PlayerInventory inventory = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerInventory>();
        
        if (inventory != null)
        {
            inventory.GiveGasMask();
            Debug.Log("[GasMask] Collected!");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("[GasMask] PlayerInventory not found!");
        }
    }
}