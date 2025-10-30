using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Base class for environmental hazards that damage the player over time.
/// Handles damage logic, visual overlays, and coroutine management.
/// </summary>
public abstract class BaseHazard : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Damage dealt per tick")]
    public int damageAmount = 20;
    
    [Tooltip("Time between damage ticks")]
    public float damageInterval = 1f;

    [Header("Visual Feedback")]
    [Tooltip("Overlay panel shown when player is in hazard")]
    public GameObject hazardOverlay;

    protected Dictionary<GameObject, Coroutine> activeCoroutines = new Dictionary<GameObject, Coroutine>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GameTags.Player) && !activeCoroutines.ContainsKey(other.gameObject))
        {
            Coroutine c = StartCoroutine(DamageOverTime(other.gameObject));
            activeCoroutines.Add(other.gameObject, c);

            if (hazardOverlay != null)
                hazardOverlay.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(GameTags.Player) && activeCoroutines.ContainsKey(other.gameObject))
        {
            StopCoroutine(activeCoroutines[other.gameObject]);
            activeCoroutines.Remove(other.gameObject);

            if (hazardOverlay != null)
                hazardOverlay.SetActive(false);
        }
    }

    private IEnumerator DamageOverTime(GameObject player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();

        while (health != null && health.GetCurrentHealth() > 0)
        {
            // Check if player should take damage (can be overridden)
            if (ShouldDamagePlayer(player))
            {
                health.TakeDamage(damageAmount, GetHazardName());
            }

            yield return new WaitForSeconds(damageInterval);
        }

        if (hazardOverlay != null)
            hazardOverlay.SetActive(false);

        activeCoroutines.Remove(player);
    }

    /// <summary>
    /// Override this to add immunity checks (e.g., gas mask for gas hazards).
    /// </summary>
    protected virtual bool ShouldDamagePlayer(GameObject player)
    {
        return true; // Default: always damage
    }

    /// <summary>
    /// Override to return the hazard type name for damage logs.
    /// </summary>
    protected virtual string GetHazardName()
    {
        return gameObject.tag;
    }

    /// <summary>
    /// Cancels ongoing damage for a specific player (used during respawn).
    /// </summary>
    public void CancelDamageFor(GameObject player)
    {
        if (activeCoroutines.ContainsKey(player))
        {
            StopCoroutine(activeCoroutines[player]);
            activeCoroutines.Remove(player);

            if (hazardOverlay != null)
                hazardOverlay.SetActive(false);
        }
    }
}