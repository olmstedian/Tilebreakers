using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TMPro.TextMeshProUGUI finalScoreText;
    [SerializeField] private GameObject splashScreen;
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private UnityEngine.UI.Button restartButton;
    [SerializeField] private UnityEngine.UI.Button mainMenuButton;
    [SerializeField] private UnityEngine.UI.Button playButton;
    [SerializeField] private UnityEngine.UI.Button quitButton;
    [SerializeField] private GameObject topBarPanel;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI moveText;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private UnityEngine.UI.Button pauseButton;
    [SerializeField] private UnityEngine.UI.Button resumeButton;
    [SerializeField] private TMPro.TextMeshProUGUI levelText; // Add a reference for the level display

    private int moveCount;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Assign button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseGame);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        // Initialize move count
        moveCount = 0;
        UpdateMoveText();

        // Update the level text at the start
        UpdateLevelText();
    }

    public void ShowGameOverScreen(int finalScore)
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore}";
        }
    }

    public void HideGameOverScreen()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }
    }

    public void ShowSplashScreen()
    {
        if (splashScreen != null)
        {
            splashScreen.SetActive(true);
        }
    }

    public void HideSplashScreen()
    {
        if (splashScreen != null)
        {
            splashScreen.SetActive(false);
        }
    }

    public void ShowMainMenu()
    {
        if (mainMenuScreen != null)
        {
            mainMenuScreen.SetActive(true);
        }
    }

    public void HideMainMenu()
    {
        if (mainMenuScreen != null)
        {
            mainMenuScreen.SetActive(false);
        }
    }

    public void RestartGame()
    {
        HideGameOverScreen();
        GameStateManager.Instance.RestartGame();
    }

    public void GoToMainMenu()
    {
        Debug.Log("UIManager: GoToMainMenu button clicked.");

        // Hide the Pause Panel if it is active
        if (pausePanel != null && pausePanel.activeSelf)
        {
            pausePanel.SetActive(false);
        }

        // Hide the Game Over Screen if it is active
        HideGameOverScreen();

        // Transition to MainMenuState
        GameStateManager.Instance.SetState(new MainMenuState());
    }

    public void StartGame()
    {
        Debug.Log("UIManager: StartGame button clicked.");
        HideMainMenu();
        GameStateManager.Instance.SetState(new InitGameState());
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void IncrementMoveCount()
    {
        moveCount++;
        UpdateMoveText();
    }

    private void UpdateMoveText()
    {
        if (moveText != null)
        {
            moveText.text = $"Moves: {moveCount}";
        }
    }

    public void ResetTopBar()
    {
        moveCount = 0;
        UpdateMoveText();
        UpdateScore(0);
        UpdateLevelText(); // Ensure the level text is updated when resetting the top bar
    }

    // Show the pause panel and transition to PauseState
    public void PauseGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        GameStateManager.Instance.SetState(new PauseState());
    }

    // Hide the pause panel and return to the previous state
    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        GameStateManager.Instance.SetState(new WaitingForInputState());
    }

    public void UpdateLevelText()
    {
        if (levelText != null && LevelManager.Instance != null)
        {
            LevelData currentLevel = LevelManager.Instance.CurrentLevel;
            if (currentLevel != null)
            {
                levelText.text = $"Level: {LevelManager.Instance.CurrentLevel.name}";
            }
            else
            {
                levelText.text = "Level: 1"; // Default to Level 1 if no level is loaded
            }
        }
    }

    public void UpdateSpecialTileUI()
    {
        Debug.Log("UIManager: Updating special tile UI to include PainterTile.");
        // Add logic to display PainterTile in the special tile UI if applicable
    }
}
