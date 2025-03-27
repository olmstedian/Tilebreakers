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

        // Initialize move count
        moveCount = 0;
        UpdateMoveText();
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
        HideGameOverScreen();
        GameStateManager.Instance.SetState(new MainMenuState());
    }

    public void StartGame()
    {
        Debug.Log("UIManager: StartGame button clicked.");
        HideMainMenu();
        GameStateManager.Instance.SetState(new InitState());
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
    }
}
