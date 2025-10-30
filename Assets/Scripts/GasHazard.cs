using UnityEngine;

/// <summary>
/// Gas hazard that damages the player unless they have a gas mask.
/// </summary>
public class GasHazard : BaseHazard
{
    protected override bool ShouldDamagePlayer(GameObject player)
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        return inventory == null || !inventory.HasGasMask();
    }

    protected override string GetHazardName() => "GasHazard";
}