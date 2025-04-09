using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Game Screens")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject levelCompleteScreen;
    [SerializeField] private GameObject levelFailedScreen;
    [SerializeField] private GameObject gameCompleteScreen;
    [SerializeField] private GameObject splashScreen;
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject pausePanel;
    
    [Header("UI Panels")]
    [SerializeField] private GameObject topBarPanel;
    
    [Header("UI Text Elements")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI moveText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI levelDescriptionText;
    [SerializeField] private TextMeshProUGUI levelObjectiveText;
    [SerializeField] private TextMeshProUGUI completeScoreText;
    [SerializeField] private TextMeshProUGUI failedScoreText;
    [SerializeField] private TextMeshProUGUI gameCompleteScoreText;
    
    [Header("Progress Bars")]
    [SerializeField] private Image levelProgressBar;
    
    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button retryLevelButton;

    [Header("Level Complete UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TMPro.TextMeshProUGUI levelCompleteScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI levelCompleteHighScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI nextLevelText;
    [SerializeField] private Button continueButton;

    [Header("Level Failed UI")]
    [SerializeField] private GameObject levelFailedPanel;
    [SerializeField] private TMPro.TextMeshProUGUI levelFailedScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI levelFailedHighScoreText;
    [SerializeField] private Button retryButton;

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
        
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(GoToNextLevel);
        }
        
        if (retryLevelButton != null)
        {
            retryLevelButton.onClick.AddListener(RetryLevel);
        }

        // Initialize UI
        UpdateLevelText();
    }

    #region Screen Management
    
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
    
    public void ShowLevelCompleteScreen(int score)
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            
            // Update score text
            if (levelCompleteScoreText != null)
            {
                levelCompleteScoreText.text = $"Score: {score}";
            }
            
            // Update high score text
            if (levelCompleteHighScoreText != null && ScoreManager.Instance != null)
            {
                int highScore = ScoreManager.Instance.GetHighScore();
                levelCompleteHighScoreText.text = $"High Score: {highScore}";
            }
            
            // Update next level text
            if (nextLevelText != null && LevelManager.Instance != null)
            {
                int nextLevel = LevelManager.Instance.CurrentLevelIndex + 2; // +2 because index is 0-based and we want to show level numbers starting from 1
                nextLevelText.text = $"Next Level: {nextLevel}";
            }
            
            // Set up continue button
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(() => {
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.AdvanceToNextLevel();
                    }
                    else
                    {
                        GameStateManager.Instance?.SetState(new WaitingForInputState());
                    }
                    HideLevelCompleteScreen();
                });
            }
            
            // Add some animation
            if (levelCompletePanel.transform.localScale == Vector3.one)
            {
                levelCompletePanel.transform.localScale = Vector3.zero;
                LeanTween.scale(levelCompletePanel, Vector3.one, 0.5f).setEaseOutBack();
            }
        }
    }

    public void HideLevelCompleteScreen()
    {
        if (levelCompletePanel != null)
        {
            LeanTween.scale(levelCompletePanel, Vector3.zero, 0.3f).setEaseInBack().setOnComplete(() => {
                levelCompletePanel.SetActive(false);
                levelCompletePanel.transform.localScale = Vector3.one;
            });
        }
    }
    
    public void ShowLevelFailedScreen(int score)
    {
        if (levelFailedPanel != null)
        {
            levelFailedPanel.SetActive(true);
            
            // Update score text
            if (levelFailedScoreText != null)
            {
                levelFailedScoreText.text = $"Score: {score}";
            }
            
            // Update high score text
            if (levelFailedHighScoreText != null && ScoreManager.Instance != null)
            {
                int highScore = ScoreManager.Instance.GetHighScore();
                levelFailedHighScoreText.text = $"High Score: {highScore}";
            }
            
            // Set up retry button
            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(() => {
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.RestartCurrentLevel();
                    }
                    else
                    {
                        GameStateManager.Instance?.SetState(new InitGameState());
                    }
                    HideLevelFailedScreen();
                });
            }
            
            // Set up main menu button
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(() => {
                    GameStateManager.Instance?.GoToMainMenu();
                    HideLevelFailedScreen();
                });
            }
            
            // Add animation
            if (levelFailedPanel.transform.localScale == Vector3.one)
            {
                levelFailedPanel.transform.localScale = Vector3.zero;
                LeanTween.scale(levelFailedPanel, Vector3.one, 0.5f).setEaseOutBack();
            }
        }
    }

    public void HideLevelFailedScreen()
    {
        if (levelFailedPanel != null)
        {
            LeanTween.scale(levelFailedPanel, Vector3.zero, 0.3f).setEaseInBack().setOnComplete(() => {
                levelFailedPanel.SetActive(false);
                levelFailedPanel.transform.localScale = Vector3.one;
            });
        }
    }
    
    public void ShowGameCompleteScreen(int score)
    {
        if (gameCompleteScreen != null)
        {
            gameCompleteScreen.SetActive(true);
        }
        
        if (gameCompleteScoreText != null)
        {
            gameCompleteScoreText.text = $"Final Score: {score}";
        }
    }
    
    public void HideGameCompleteScreen()
    {
        if (gameCompleteScreen != null)
        {
            gameCompleteScreen.SetActive(false);
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
    
    #endregion

    #region Button Actions
    
    public void RestartGame()
    {
        HideGameOverScreen();
        HideLevelFailedScreen();
        HideGameCompleteScreen();
        GameStateManager.Instance.RestartGame();
    }

    public void GoToMainMenu()
    {
        Debug.Log("UIManager: GoToMainMenu button clicked.");

        // Hide all screens
        HideGameOverScreen();
        HideLevelCompleteScreen();
        HideLevelFailedScreen();
        HideGameCompleteScreen();
        
        if (pausePanel != null && pausePanel.activeSelf)
        {
            pausePanel.SetActive(false);
        }

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
    
    public void GoToNextLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AdvanceToNextLevel();
        }
    }
    
    public void RetryLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartCurrentLevel();
        }
    }
    
    #endregion

    #region UI Updates
    
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
        
        // Update level progress if available
        UpdateLevelProgress();
    }

    public void UpdateMoveCount(int moves)
    {
        if (moveText != null)
        {
            if (LevelManager.Instance != null && 
                LevelManager.Instance.CurrentLevel != null && 
                LevelManager.Instance.CurrentLevel.maxMoves > 0)
            {
                moveText.text = $"Moves: {moves}/{LevelManager.Instance.CurrentLevel.maxMoves}";
            }
            else
            {
                moveText.text = $"Moves: {moves}";
            }
        }
    }

    public void ResetTopBar()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: 0";
        }
        
        if (moveText != null)
        {
            moveText.text = "Moves: 0";
        }
        
        UpdateLevelText();
        UpdateLevelProgress();
    }

    public void UpdateLevelText()
    {
        if (levelText != null && LevelManager.Instance != null)
        {
            levelText.text = $"Level: {LevelManager.Instance.CurrentLevelIndex + 1}/{LevelManager.Instance.TotalLevels}";
            
            // Update additional level information if available
            if (levelDescriptionText != null)
            {
                levelDescriptionText.text = LevelManager.Instance.GetLevelDescription();
            }
            
            if (levelObjectiveText != null)
            {
                levelObjectiveText.text = LevelManager.Instance.GetLevelObjective();
            }
        }
        else if (levelText != null)
        {
            levelText.text = "Level: 1";
        }
    }
    
    public void UpdateLevelProgress()
    {
        if (levelProgressBar != null && LevelManager.Instance != null)
        {
            levelProgressBar.fillAmount = LevelManager.Instance.GetLevelProgress();
        }
    }
    
    #endregion
}
