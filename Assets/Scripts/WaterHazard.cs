using UnityEngine;

/// <summary>
/// Water hazard that damages the player when they enter water.
/// </summary>
public class WaterHazard : BaseHazard
{
    protected override string GetHazardName() => "WaterHazard";
}
