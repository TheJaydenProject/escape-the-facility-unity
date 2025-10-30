using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Handles player's health, damage effects, death and respawn system, and death counter.
/// </summary>
/*
 * Author: Jayden Wong
 * Date: 16/06/2025
 * Description: Manages the player's health system, including taking damage, showing visual/audio feedback,
 * handling death and respawn logic, cancelling hazard effects, and updating on-screen health UI.
 */
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]

    /// <summary>
    /// Maximum health value the player can have.
    /// </summary>
    public int maxHealth = 100;

    /// <summary>
    /// Current health value of the player.
    /// </summary>
    private int currentHealth;

    [Header("UI")]

    /// <summary>
    /// UI text component used to display current health value.
    /// </summary>
    public TextMeshProUGUI healthText;

    [Header("Respawn")]

    /// <summary>
    /// Transform position where the player will respawn after dying.
    /// </summary>
    public Transform spawnPoint;

    [Header("Damage Feedback")]

    /// <summary>
    /// UI overlay panel that flashes red when the player takes damage.
    /// </summary>
    [Tooltip("Red overlay panel that flashes when taking damage")]
    public GameObject DamageOverlay;

    /// <summary>
    /// Duration (in seconds) that the red damage flash stays visible.
    /// </summary>
    [Tooltip("Duration of red overlay flash in seconds")]
    public float damageFlashDuration = 0.4f;

    /// <summary>
    /// Audio clip that plays when the player takes damage.
    /// </summary>
    [Tooltip("Sound to play when taking damage")]
    public AudioSource damageSFX;

    /// <summary>
    /// Tracks how many times the player has died in this session.
    /// </summary>
    private int deathCount = 0;

    /// <summary>
    /// Returns how many times the player has died.
    /// </summary>
    public int GetDeathCount()
    {
        return deathCount;
    }

    void Start()
    {
        // Set health to maximum at start
        currentHealth = maxHealth;

        // Update health display
        UpdateHealthUI();
    }

    /// <summary>
    /// Reduces player's health by the specified amount.
    /// Triggers feedback visuals, sound, and checks for death.
    /// </summary>
    /// <param name="amount">Amount of damage to apply.</param>
    /// <param name="sourceTag">Optional tag describing what caused the damage (for logging).</param>
    public void TakeDamage(int amount, string sourceTag = "Unknown")
    {
        // Decrease health and clamp it between 0 and max
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        // Log damage event
        Debug.Log($"Damaged by: {sourceTag} | Amount: {amount}");
        Debug.Log($"Player HP: {currentHealth}");

        // Start flashing red overlay if assigned
        if (DamageOverlay != null)
            StartCoroutine(FlashDamageOverlay());

        // Play damage sound if available and not already playing
        if (damageSFX != null && !damageSFX.isPlaying)
            damageSFX.Play();

        // If health has dropped to zero, trigger death sequence
        if (currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// Instantly kills the player, setting health to 0 and triggering the death sequence.
    /// </summary>
    /// <param name="sourceTag">Optional tag describing what caused the instant kill.</param>
    public void InstantKill(string sourceTag = "Unknown")
    {
        currentHealth = 0;
        UpdateHealthUI();
        Debug.Log($"Instant killed by: {sourceTag}");
        Die();
    }

    /// <summary>
    /// Handles death: cancels hazard effects, resets position, and restores health.
    /// </summary>
    public void Die()
    {
        deathCount++; // Track total deaths
        Debug.Log("Player died.");
        Debug.Log("Deaths this run: " + deathCount);

        // Stop all active hazard effects on the player
        foreach (BaseHazard hazard in Object.FindObjectsByType<BaseHazard>(FindObjectsSortMode.None))
            hazard.CancelDamageFor(gameObject);

        // Temporarily disable character controller for teleporting
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        // Move player to respawn point, if assigned
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        // Re-enable character controller after repositioning
        if (controller != null) controller.enabled = true;

        // Restore full health
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    /// <summary>
    /// Returns the current health value of the player.
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Updates the UI element to reflect the current health value.
    /// </summary>
    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = "" + currentHealth + " / " + maxHealth;
    }

    /// <summary>
    /// Briefly shows a red overlay UI panel to indicate damage.
    /// </summary>
    private IEnumerator FlashDamageOverlay()
    {
        if (DamageOverlay != null)
        {
            DamageOverlay.SetActive(true); // Show overlay
            yield return new WaitForSeconds(damageFlashDuration); // Wait for duration
            DamageOverlay.SetActive(false); // Hide overlay
        }
    }
}
