using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    private bool gameLive;
    private bool playerInEscapeZone;
    private float elapsedTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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

    private void SetupStartState()
    {
        gameLive = false;
        playerInEscapeZone = false;
        elapsedTime = 0f;

        if (startOverlay != null) startOverlay.SetActive(true);
        TogglePlayerControl(false);
        ShowEscapePrompt(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UpdateTimerLabel();
    }

    public void BeginGame()
    {
        gameLive = true;
        elapsedTime = 0f;

        if (startOverlay != null) startOverlay.SetActive(false);
        TogglePlayerControl(true);
        ShowEscapePrompt(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RegisterEscapeZone(bool isInside)
    {
        playerInEscapeZone = isInside;
        ShowEscapePrompt(isInside);
    }

    public float GetElapsedTime() => elapsedTime;

    public void FinishGame()
    {
        if (!gameLive) return;

        gameLive = false;
        Time.timeScale = 0f;
        TogglePlayerControl(false);
        playerInEscapeZone = false;
        ShowEscapePrompt(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        int deaths = playerHealth != null ? playerHealth.GetDeathCount() : 0;
        int coins = coinCollector != null ? coinCollector.GetCollectedCoins() : 0;
        int totalCoins = coinCollector != null ? coinCollector.totalCoins : 25;

        float coinScore = (coins / (float)totalCoins) * 700f;
        float deathPenalty = Mathf.Min(200f, deaths * 40f);
        float timePenalty = CalculateTimePenalty(elapsedTime);

        int finalScore = Mathf.RoundToInt(999f + coinScore - deathPenalty - timePenalty - 700f);
        finalScore = Mathf.Clamp(finalScore, 0, 999);

        if (endPanel != null)
        {
            endPanel.SetActive(true);
            if (scoreText != null) scoreText.text = $"Score: {finalScore}";
            if (timeText != null) timeText.text = $"Time: {Mathf.FloorToInt(elapsedTime / 60f)}min {Mathf.FloorToInt(elapsedTime % 60f)}s";
            if (deathText != null) deathText.text = $"Deaths: {deaths}";
            if (coinText != null) coinText.text = $"Coins: {coins}/{totalCoins}";
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private float CalculateTimePenalty(float time)
    {
        if (time <= 150f) return 0f;
        if (time <= 300f) return Mathf.Min(99f, (time - 150f) * 0.66f);
        return 150f + (time - 300f) * 2f;
    }

    private void UpdateTimerLabel()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void TogglePlayerControl(bool enable)
    {
        if (movementScript != null) movementScript.enabled = enable;
        if (lookScript != null) lookScript.enabled = enable;
    }

    public void ShowEscapePrompt(bool visible)
    {
        if (escapePrompt == null) return;
        escapePrompt.SetActive(visible && gameLive);
    }
}
