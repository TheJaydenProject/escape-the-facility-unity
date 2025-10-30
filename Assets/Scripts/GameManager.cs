using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Central game manager handling game state, UI, scoring, and player control.
/// Singleton pattern ensures only one instance exists.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Singleton
    
    public static GameManager Instance { get; private set; }
    
    #endregion

    #region Score Constants
    
    private const float MAX_COIN_SCORE = 700f;
    private const float DEATH_PENALTY_PER_DEATH = 40f;
    private const float MAX_DEATH_PENALTY = 200f;
    private const int BASE_SCORE = 999;
    private const int MIN_SCORE = 0;
    private const int MAX_SCORE = 999;
    
    private const float FAST_TIME_THRESHOLD = 150f;  // 2.5 minutes - no penalty
    private const float MEDIUM_TIME_THRESHOLD = 300f;  // 5 minutes
    private const float MEDIUM_TIME_PENALTY_RATE = 0.66f;
    private const float SLOW_TIME_PENALTY_RATE = 2f;
    private const float MEDIUM_TIME_MAX_PENALTY = 99f;
    private const float SLOW_TIME_BASE_PENALTY = 150f;
    
    #endregion

    #region Serialized Fields
    
    [Header("Start Overlay")]
    [SerializeField] private GameObject startOverlay;
    [SerializeField] private Behaviour movementScript;
    [SerializeField] private MonoBehaviour lookScript;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject escapePrompt;

    [Header("End Screen")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Stats Sources")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerCoinCollector coinCollector;
    
    #endregion

    #region Private Fields
    
    private bool gameLive;
    private bool playerInEscapeZone;
    private float elapsedTime;
    
    #endregion

    #region Unity Lifecycle
    
    void Awake()
    {
        InitializeSingleton();
    }

    void Start()
    {
        Time.timeScale = 1f;
        SetupStartState();
    }

    void Update()
    {
        if (!gameLive) return;

        elapsedTime += Time.deltaTime;
        UpdateTimerLabel();

        if (playerInEscapeZone && Input.GetKeyDown(KeyCode.E))
        {
            FinishGame();
        }
    }
    
    #endregion

    #region Initialization
    
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void SetupStartState()
    {
        gameLive = false;
        playerInEscapeZone = false;
        elapsedTime = 0f;

        if (startOverlay != null) 
            startOverlay.SetActive(true);
        
        TogglePlayerControl(false);
        ShowEscapePrompt(false);

        SetCursorState(visible: true, locked: false);
        UpdateTimerLabel();
    }
    
    #endregion

    #region Public Methods
    
    /// <summary>
    /// Starts the game, enabling player control and hiding the start overlay.
    /// </summary>
    public void BeginGame()
    {
        gameLive = true;
        elapsedTime = 0f;

        if (startOverlay != null) 
            startOverlay.SetActive(false);
        
        TogglePlayerControl(true);
        ShowEscapePrompt(false);

        SetCursorState(visible: false, locked: true);
    }

    /// <summary>
    /// Registers whether the player is inside the escape zone.
    /// </summary>
    /// <param name="isInside">True if player entered the zone, false if exited.</param>
    public void RegisterEscapeZone(bool isInside)
    {
        playerInEscapeZone = isInside;
        ShowEscapePrompt(isInside);
    }

    /// <summary>
    /// Gets the current elapsed game time in seconds.
    /// </summary>
    public float GetElapsedTime() => elapsedTime;

    /// <summary>
    /// Ends the game, displays the end screen with stats and score.
    /// </summary>
    public void FinishGame()
    {
        if (!gameLive) return;

        PauseGame();
        ShowEndScreen();
    }

    /// <summary>
    /// Restarts the current scene, resetting all game state.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Shows or hides the escape prompt UI.
    /// </summary>
    /// <param name="visible">Whether the prompt should be visible.</param>
    public void ShowEscapePrompt(bool visible)
    {
        if (escapePrompt == null) return;
        escapePrompt.SetActive(visible && gameLive);
    }
    
    #endregion

    #region Game State Management
    
    private void PauseGame()
    {
        gameLive = false;
        Time.timeScale = 0f;
        TogglePlayerControl(false);
        playerInEscapeZone = false;
        ShowEscapePrompt(false);

        SetCursorState(visible: true, locked: false);
    }

    private void TogglePlayerControl(bool enable)
    {
        if (movementScript != null) 
            movementScript.enabled = enable;
        
        if (lookScript != null) 
            lookScript.enabled = enable;
    }

    private void SetCursorState(bool visible, bool locked)
    {
        Cursor.visible = visible;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }
    
    #endregion

    #region End Screen & Scoring
    
    private void ShowEndScreen()
    {
        if (endPanel == null) return;

        GameStats stats = CalculateGameStats();
        DisplayStats(stats);
        
        endPanel.SetActive(true);
    }

    private GameStats CalculateGameStats()
    {
        int deaths = playerHealth?.GetDeathCount() ?? 0;
        int coins = coinCollector?.GetCollectedCoins() ?? 0;
        int totalCoins = coinCollector?.totalCoins ?? 25;

        float coinScore = CalculateCoinScore(coins, totalCoins);
        float deathPenalty = CalculateDeathPenalty(deaths);
        float timePenalty = CalculateTimePenalty(elapsedTime);

        int finalScore = CalculateFinalScore(coinScore, deathPenalty, timePenalty);

        return new GameStats
        {
            Score = finalScore,
            Time = elapsedTime,
            Deaths = deaths,
            Coins = coins,
            TotalCoins = totalCoins
        };
    }

    private float CalculateCoinScore(int coins, int totalCoins)
    {
        if (totalCoins == 0) return 0f;
        return (coins / (float)totalCoins) * MAX_COIN_SCORE;
    }

    private float CalculateDeathPenalty(int deaths)
    {
        return Mathf.Min(MAX_DEATH_PENALTY, deaths * DEATH_PENALTY_PER_DEATH);
    }

    private float CalculateTimePenalty(float time)
    {
        if (time <= FAST_TIME_THRESHOLD) 
            return 0f;
        
        if (time <= MEDIUM_TIME_THRESHOLD) 
            return Mathf.Min(MEDIUM_TIME_MAX_PENALTY, 
                           (time - FAST_TIME_THRESHOLD) * MEDIUM_TIME_PENALTY_RATE);
        
        return SLOW_TIME_BASE_PENALTY + 
               (time - MEDIUM_TIME_THRESHOLD) * SLOW_TIME_PENALTY_RATE;
    }

    private int CalculateFinalScore(float coinScore, float deathPenalty, float timePenalty)
    {
        int score = Mathf.RoundToInt(BASE_SCORE + coinScore - deathPenalty - timePenalty - MAX_COIN_SCORE);
        return Mathf.Clamp(score, MIN_SCORE, MAX_SCORE);
    }

    private void DisplayStats(GameStats stats)
    {
        if (scoreText != null) 
            scoreText.text = $"Score: {stats.Score}";
        
        if (timeText != null) 
            timeText.text = FormatTime(stats.Time);
        
        if (deathText != null) 
            deathText.text = $"Deaths: {stats.Deaths}";
        
        if (coinText != null) 
            coinText.text = $"Coins: {stats.Coins:D2}/{stats.TotalCoins:D2}";
    }
    
    #endregion

    #region UI Helpers
    
    private void UpdateTimerLabel()
    {
        if (timerText == null) return;
        
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"Time: {minutes}min {secs}s";
    }
    
    #endregion

    #region Helper Structs
    
    /// <summary>
    /// Container for end-game statistics.
    /// </summary>
    private struct GameStats
    {
        public int Score;
        public float Time;
        public int Deaths;
        public int Coins;
        public int TotalCoins;
    }
    
    #endregion
}