using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Handles coin collection, coin UI display, and showing a congratulatory panel
/// when all coins are collected in the 3D virtual environment.
/// </summary>
/*
 * Description: This script tracks the number of coins collected by the player using Unity Physics triggers.
 * It updates the UI, plays sound effects on collection, and shows a congrats panel when all are collected.
 */
public class PlayerCoinCollector : MonoBehaviour
{
    [Header("Coin Settings")]

    /// <summary>
    /// Total number of coins the player must collect in the scene.
    /// </summary>
    public int totalCoins = 25;

    /// <summary>
    /// Number of coins currently collected by the player.
    /// </summary>
    private int collectedCoins = 0;

    [Header("UI")]

    /// <summary>
    /// Text field that displays the current coin count to the player.
    /// </summary>
    public TextMeshProUGUI coinText;

    /// <summary>
    /// Panel that appears when all coins have been collected.
    /// </summary>
    public GameObject congratsPanel;

    /// <summary>
    /// Duration (in seconds) that the congrats panel stays visible.
    /// </summary>
    public float congratsDuration = 3f;

    [Header("Audio")]

    /// <summary>
    /// Audio clip that plays when a coin is collected.
    /// </summary>
    [Tooltip("Sound to play when a coin is collected")]
    public AudioSource coinSFX;

    /// <summary>
    /// Returns the number of coins collected by the player.
    /// Useful for end-game scoring or UI updates.
    /// </summary>
    public int GetCollectedCoins()
    {
        return collectedCoins;
    }

    void Start()
    {
        // Initialize UI at start of game
        UpdateCoinUI();

        // Ensure congrats panel is hidden initially
        if (congratsPanel != null)
            congratsPanel.SetActive(false);
    }

    /// <summary>
    /// Unity Physics trigger that detects coin collision.
    /// Increases coin count, plays SFX, and updates UI.
    /// </summary>
    /// <param name="other">The collider the player triggered.</param>
    void OnTriggerEnter(Collider other)
    {
        // Check if player collided with a coin object
        if (other.CompareTag(GameTags.Coin))
        {
            // Play coin sound if available
            if (coinSFX != null)
                coinSFX.Play();

            // Increase coin count and destroy the coin object
            collectedCoins++;
            Destroy(other.gameObject);

            // Update the coin count display on screen
            UpdateCoinUI();

            // If all coins collected, show congratulations panel
            if (collectedCoins >= totalCoins && congratsPanel != null)
            {
                StartCoroutine(ShowCongrats());
            }
        }
    }

    /// <summary>
    /// Updates the on-screen UI to reflect the current number of coins collected.
    /// </summary>
    void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = collectedCoins + " / " + totalCoins;
    }

    /// <summary>
    /// Displays the congratulatory panel temporarily when all coins are collected.
    /// </summary>
    IEnumerator ShowCongrats()
    {
        // Show the panel
        congratsPanel.SetActive(true);

        // Wait for the defined duration
        yield return new WaitForSeconds(congratsDuration);

        // Hide the panel after the duration
        congratsPanel.SetActive(false);
    }
}
