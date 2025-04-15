using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Tilebreakers.Core; // Added to access ScoreManager and LevelManager
using System.Collections; // Add this for IEnumerator
using Tilebreakers.UI; // Added to access ScorePopup

/// <summary>
/// Manages UI elements and interactions
/// </summary>
public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>();
                if (_instance == null)
                {
                    GameObject uiManagerObject = new GameObject("UIManager");
                    _instance = uiManagerObject.AddComponent<UIManager>();
                    DontDestroyOnLoad(uiManagerObject);
                }
            }
            return _instance;
        }
    }

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
    [SerializeField] private TextMeshProUGUI levelDescriptionText;  // This one
    [SerializeField] private TextMeshProUGUI levelObjectiveText;    // And this one
    [SerializeField] private TextMeshProUGUI highScoreText; // Added missing field
    // Removed these two redundant fields:
    // [SerializeField] private TextMeshProUGUI currentLevelText;
    // [SerializeField] private TextMeshProUGUI gameCompleteScoreText;
    
    [Header("Progress Bars")]
    [SerializeField] private Image levelProgressBar;
    [SerializeField] private Image progressBarImage;  // Added missing progressBarImage reference
    
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

    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private RectTransform popupParent;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Verify UI references on startup
        ValidateUIReferences();
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

    private void ValidateUIReferences()
    {
        // Check if move count text is assigned - update to use the moveText from UI Text Elements instead
        if (moveText == null)
        {
            Debug.LogError("UIManager: moveText reference is missing! Move counter won't work.");
        }

        // Set initial move count to 0
        UpdateMoveCount(0);
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
                int highScore = ScoreManager.Instance != null ? PlayerPrefs.GetInt("HighScore", 0) : 0;
                levelCompleteHighScoreText.text = $"High Score: {highScore}";
            }
            
            // Update next level text
            if (nextLevelText != null && LevelManager.Instance != null)
            {
                int nextLevel = LevelManager.Instance.GetCurrentLevelNumber() + 1; // +1 because index is 0-based and we want to show level numbers starting from 1
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

    public void ShowLevelCompleteScreen(int score, int moveCount, int nextLevelIndex = -1)
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
                int highScore = ScoreManager.Instance != null ? PlayerPrefs.GetInt("HighScore", 0) : 0;
                levelCompleteHighScoreText.text = $"High Score: {highScore}";
            }
            
            // Update next level text
            if (nextLevelText != null && LevelManager.Instance != null)
            {
                int nextLevel = LevelManager.Instance.GetCurrentLevelNumber() + 1; // +1 because index is 0-based and we want to show level numbers starting from 1
                nextLevelText.text = $"Next Level: {nextLevel}";
            }
            
            // Set up continue button
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(() =>
                {
                    Debug.Log("UIManager: User clicked Next Level/Continue. Transitioning to LoadingLevelState.");
                    
                    // CRITICAL FIX: First transition to LoadingLevelState before advancing
                    GameStateManager.Instance.SetState(new LoadingLevelState());
                    
                    // Hide level complete UI
                    HideLevelCompleteScreen();
                    
                    // Then advance to next level
                    GameManager.Instance.LoadNextLevel();
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
                int highScore = ScoreManager.Instance != null ? PlayerPrefs.GetInt("HighScore", 0) : 0;
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
        
        // Find the score text component directly in the game complete screen
        TextMeshProUGUI gameCompleteScoreText = gameCompleteScreen?.GetComponentInChildren<TextMeshProUGUI>();
        if (gameCompleteScoreText != null)
        {
            gameCompleteScoreText.text = $"Final Score: {score}";
        }
        else if (finalScoreText != null)
        {
            // Fallback to using finalScoreText if available
            finalScoreText.text = $"Final Score: {score}";
        }
        else
        {
            Debug.LogWarning("UIManager: No score text component found in game complete screen!");
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

    public void ShowScorePopup(int score, string text)
    {
        if (scorePopupPrefab != null && popupParent != null)
        {
            GameObject popup = Instantiate(scorePopupPrefab, popupParent);
            ScorePopup popupComponent = popup.GetComponent<ScorePopup>();
            
            if (popupComponent != null)
            {
                popupComponent.SetValue(score, text);
            }
        }
    }

    private void DisplayScorePopup(int points)
    {
        // Example usage of ShowScorePopup
        ScoreManager.ShowScorePopup(points, $"+{points}");
    }

    /// <summary>
    /// Updates the move counter display
    /// </summary>
    public void UpdateMoveCount(int moveCount)
    {
        // Add a debug log to see if this method is being called
        Debug.Log($"UIManager: UpdateMoveCount called with value: {moveCount}");
        
        if (moveText != null)
        {
            string previousText = moveText.text;
            moveText.text = $"Moves: {moveCount}";
            Debug.Log($"UIManager: Move count text updated from '{previousText}' to '{moveText.text}'");
            
            // Add visual feedback when move count changes - update to use TopBarPanel
            // We can apply the pulse effect to the moveText directly instead of the removed movesPanel
            LeanTween.scale(moveText.gameObject, new Vector3(1.1f, 1.1f, 1), 0.2f)
                .setEase(LeanTweenType.easeOutQuad)
                .setLoopPingPong(1);
        }
        else
        {
            Debug.LogError("UIManager: moveText reference is null! Cannot update move count display.");
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
            // This updates the text in the TopBarPanel showing current level number
            // Fix to show "Level: X/Y" format instead of "LevelX:LevelX"
            levelText.text = $"Level: {LevelManager.Instance.GetCurrentLevelNumber()}/{LevelManager.Instance.TotalLevels}";
            
            // Update additional level information if available
            if (levelDescriptionText != null)
            {
                string levelDescription = LevelManager.Instance != null ? 
                    LevelManager.Instance.GetLevelDescription() : "N/A";
                levelDescriptionText.text = levelDescription;
            }
            
            if (levelObjectiveText != null)
            {
                string levelObjective = LevelManager.Instance != null ? 
                    LevelManager.Instance.GetLevelObjective() : "Objective not defined";
                levelObjectiveText.text = levelObjective;
            }
        }
        else if (levelText != null)
        {
            levelText.text = "Level: 1/1";
        }
    }

    public void UpdateLevelText(string levelName)
    {
        if (levelText != null)
        {
            // Use a consistent format even when manually setting the level name
            // Extract level number from levelName if it follows the format "Level X"
            if (levelName.StartsWith("Level "))
            {
                string levelNum = levelName.Substring(6);
                int totalLevels = LevelManager.Instance != null ? LevelManager.Instance.TotalLevels : 1;
                levelText.text = $"Level: {levelNum}/{totalLevels}";
            }
            else
            {
                levelText.text = levelName;
            }
        }
    }

    /// <summary>
    /// Shows information about the current level in the UI
    /// </summary>
    public void ShowLevelInfo(string levelName, int levelNumber, int movesTarget, int scoreTarget, string levelDescription = null)
    {
        // Replace references to LevelManager.CurrentLevelIndex with actual accessor methods
        int currentLevelIndex = LevelManager.Instance.GetCurrentLevelNumber() - 1; // Subtract 1 to get zero-based index
        
        // Update level info directly to the Text Elements group instead
        if (levelText != null)
        {
            // Fix to maintain consistent "Level: X/Y" format
            int totalLevels = LevelManager.Instance != null ? LevelManager.Instance.TotalLevels : 1;
            levelText.text = $"Level: {levelNumber}/{totalLevels}";
        }
        
        // Update objective information
        if (levelObjectiveText != null)
        {
            string scoreObjective = scoreTarget > 0 ? $"Score at least {scoreTarget} points" : "No score target";
            string movesObjective = movesTarget > 0 ? $" within {movesTarget} moves" : "";
            levelObjectiveText.text = scoreObjective + movesObjective;
        }
        
        // Update level description if available
        if (levelDescriptionText != null)
        {
            levelDescriptionText.text = !string.IsNullOrEmpty(levelDescription) ? 
                levelDescription : 
                "Break tiles and create matches to score points!";
        }
        
        // Update the pause panel level info if it's active
        if (pausePanel != null && pausePanel.activeInHierarchy)
        {
            UpdatePausePanelLevelInfo();
        }
    }

    public void UpdateObjectiveText(string objective)
    {
        if (levelObjectiveText != null)
        {
            levelObjectiveText.text = objective;
        }
    }

    public void UpdateLevelProgress()
    {
        if (levelProgressBar != null && LevelManager.Instance != null)
        {
            int currentLevel = LevelManager.Instance != null ? LevelManager.Instance.GetCurrentLevelNumber() : 0;
            string levelDescription = LevelManager.Instance != null ? LevelManager.Instance.GetCurrentLevelName() : "N/A";
            string levelObjective = LevelManager.Instance != null ? "Objective not defined" : "";
            float progress = LevelManager.Instance.GetLevelScoreTarget() > 0 ? 
                (float)ScoreManager.Instance.Score / LevelManager.Instance.GetLevelScoreTarget() : 
                0f; // Calculate progress
            
            UpdateProgressBar(progress);
        }
    }

    /// <summary>
    /// Updates the progress bar
    /// </summary>
    private void UpdateProgressBar(float normalizedProgress)
    {
        if (progressBarImage != null)
        {
            progressBarImage.fillAmount = normalizedProgress;
        }
    }

    // Example: When updating high score UI, use the ScoreManager property (fallback via PlayerPrefs if needed)
    void UpdateHighScoreDisplay()
    {
        int highScore = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        highScoreText.text = $"High Score: {highScore}";
    }

    // Example: When transitioning to level complete, call UI method on LevelCompleteState
    public void OnLevelCompleted(int nextLevelIndex)
    {
        // Assuming LevelCompleteState is designed to show the UI via UIManager:
        UIManager.Instance.ShowLevelCompletePanel(nextLevelIndex);
    }

    /// <summary>
    /// Shows the level complete panel with appropriate next level information
    /// </summary>
    public void ShowLevelCompletePanel(int nextLevelIndex)
    {
        Debug.Log($"UIManager: Showing level complete panel for next level index: {nextLevelIndex}");
        
        // Make sure the panel exists
        if (levelCompletePanel == null)
        {
            Debug.LogError("UIManager: Level complete panel is null!");
            return;
        }
        
        // Ensure the panel is active
        levelCompletePanel.SetActive(true);
        
        // Find and update UI components
        Transform contentTransform = levelCompletePanel.transform.Find("Content");
        if (contentTransform == null)
        {
            Debug.LogError("UIManager: Cannot find Content transform in level complete panel");
            return;
        }
        
        // Get title text component
        TextMeshProUGUI titleText = contentTransform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        
        // CRITICAL FIX: Check for null before accessing
        if (titleText != null)
        {
            titleText.text = "Level Complete!";
        }
        
        // Get score text component
        TextMeshProUGUI scoreText = contentTransform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        
        // CRITICAL FIX: Check for null before accessing
        if (scoreText != null)
        {
            scoreText.text = $"Score: {ScoreManager.Instance?.Score ?? 0}";
        }
        
        // Get next level button
        Button nextLevelButton = contentTransform.Find("NextLevelButton")?.GetComponent<Button>();
        
        // CRITICAL FIX: Check for null before accessing
        if (nextLevelButton != null)
        {
            // Update button text and action based on next level index
            TextMeshProUGUI buttonText = nextLevelButton.GetComponentInChildren<TextMeshProUGUI>();
            
            // CRITICAL FIX: Additional null check
            if (buttonText != null)
            {
                // Modify button text based on whether we're going to infinite mode
                buttonText.text = nextLevelIndex == -1 ? "Start Infinite Mode" : "Next Level";
            }
            
            // Update button click action
            nextLevelButton.onClick.RemoveAllListeners();
            
            // CRITICAL FIX: Set up appropriate action based on next level index
            if (nextLevelIndex == -1)
            {
                // Going to infinite mode
                nextLevelButton.onClick.AddListener(() => {
                    Debug.Log("UIManager: Starting infinite mode");
                    levelCompletePanel.SetActive(false);
                    LevelManager.Instance?.StartInfiniteMode();
                });
            }
            else
            {
                // Going to next level
                nextLevelButton.onClick.AddListener(() => {
                    Debug.Log($"UIManager: Advancing to level {nextLevelIndex}");
                    levelCompletePanel.SetActive(false);
                    GameStateManager.Instance?.SetState(new LevelTransitionState(nextLevelIndex));
                });
            }
        }
        
        // Find and set up restart level button if it exists
        Button restartButton = contentTransform.Find("RestartButton")?.GetComponent<Button>();
        
        // CRITICAL FIX: Check for null before accessing
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => {
                Debug.Log("UIManager: Restarting current level");
                levelCompletePanel.SetActive(false);
                LevelManager.Instance?.RestartCurrentLevel();
            });
        }
        
        // Optional: Find and set up main menu button if it exists
        Button menuButton = contentTransform.Find("MenuButton")?.GetComponent<Button>();
        
        // CRITICAL FIX: Check for null before accessing
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() => {
                Debug.Log("UIManager: Returning to main menu");
                levelCompletePanel.SetActive(false);
                // Navigate to main menu scene or state
                GameStateManager.Instance?.SetState(new MainMenuState());
            });
        }
        
        // Show the panel with animation
        StartCoroutine(ShowPanelWithAnimation(levelCompletePanel));
    }

    /// <summary>
    /// Shows a panel with a smooth animation effect
    /// </summary>
    /// <param name="panel">The panel GameObject to animate</param>
    /// <returns>Coroutine for animation sequence</returns>
    private IEnumerator ShowPanelWithAnimation(GameObject panel)
    {
        if (panel == null) yield break;
        
        // Make sure the panel is active
        panel.SetActive(true);
        
        // Get or add a CanvasGroup component
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        
        // Start with zero opacity
        canvasGroup.alpha = 0f;
        
        // Save initial scale
        Vector3 initialScale = panel.transform.localScale;
        
        // Start slightly smaller for a pop effect
        panel.transform.localScale = initialScale * 0.8f;
        
        // Fade in and scale up
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;
            
            // Ease in function
            float easeValue = 1f - Mathf.Cos(normalizedTime * Mathf.PI * 0.5f);
            
            // Update alpha and scale
            canvasGroup.alpha = easeValue;
            panel.transform.localScale = Vector3.Lerp(initialScale * 0.8f, initialScale, easeValue);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final values are set
        canvasGroup.alpha = 1f;
        panel.transform.localScale = initialScale;
    }

    /// <summary>
    /// Hides a panel with a smooth animation effect
    /// </summary>
    /// <param name="panel">The panel GameObject to animate</param>
    /// <returns>Coroutine for animation sequence</returns>
    private IEnumerator HidePanelWithAnimation(GameObject panel)
    {
        if (panel == null) yield break;
        
        // Get or add a CanvasGroup component
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        
        // Save initial scale
        Vector3 initialScale = panel.transform.localScale;
        
        // Fade out and scale down
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;
            
            // Ease out function
            float easeValue = Mathf.Sin((1f - normalizedTime) * Mathf.PI * 0.5f);
            
            // Update alpha and scale
            canvasGroup.alpha = easeValue;
            panel.transform.localScale = Vector3.Lerp(initialScale * 0.8f, initialScale, easeValue);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Hide panel when animation completes
        panel.SetActive(false);
        
        // Reset to initial state for next time
        canvasGroup.alpha = 1f;
        panel.transform.localScale = initialScale;
    }

    /// <summary>
    /// Shows a transition effect when moving between levels
    /// </summary>
    public void ShowLevelTransition()
    {
        Debug.Log("UIManager: Showing level transition effect");
        
        // Create a screen-wide panel for the transition effect
        GameObject transitionPanel = new GameObject("LevelTransition");
        transitionPanel.transform.SetParent(transform, false);
        
        // Add a canvas group for fade in/out
        CanvasGroup canvasGroup = transitionPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        
        // Add image component with full screen coverage
        Image transitionImage = transitionPanel.AddComponent<Image>();
        transitionImage.color = new Color(0.1f, 0.1f, 0.2f, 1f); // Dark blue
        transitionImage.raycastTarget = false;
        
        // Make it cover the entire screen
        RectTransform rect = transitionPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Create text indicating level transition
        GameObject textObject = new GameObject("TransitionText");
        textObject.transform.SetParent(transitionPanel.transform, false);
        
        // Add text component
        Text levelText = textObject.AddComponent<Text>();
        levelText.text = "LEVEL LOADING...";
        levelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        levelText.fontSize = 24;
        levelText.alignment = TextAnchor.MiddleCenter;
        levelText.color = Color.white;
        
        // Position the text
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(300, 50);
        
        // Animate the transition
        StartCoroutine(AnimateLevelTransition(transitionPanel));
    }

    private IEnumerator AnimateLevelTransition(GameObject transitionPanel)
    {
        if (transitionPanel == null) yield break;
        
        CanvasGroup canvasGroup = transitionPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) yield break;
        
        // Fade in
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        // Hold for a moment
        yield return new WaitForSeconds(0.5f);
        
        // Fade out after a delay (the state transition will likely have happened by now)
        yield return new WaitForSeconds(0.5f);
        
        elapsed = 0f;
        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Clean up
        Destroy(transitionPanel);
    }

    private void UpdatePausePanelLevelInfo()
    {
        if (pausePanel == null) return;
        
        // Use the UI Text Elements instead of the removed Level Info UI elements
        
        // Get current level info
        string levelName = "Unknown Level";
        string levelDescription = "No description available";
        string levelObjective = "No objective defined";
        
        if (LevelManager.Instance != null)
        {
            // Use the levelText, levelDescriptionText, and levelObjectiveText from UI Text Elements
            levelName = levelText != null ? levelText.text : $"Level {LevelManager.Instance.GetCurrentLevelNumber()}: {LevelManager.Instance.GetCurrentLevelName()}";
            levelDescription = levelDescriptionText != null ? levelDescriptionText.text : LevelManager.Instance.GetLevelDescription();
            levelObjective = levelObjectiveText != null ? levelObjectiveText.text : LevelManager.Instance.GetLevelObjective();
            
            // If in infinite mode, adjust the display
            if (LevelManager.Instance.IsInfiniteMode)
            {
                levelName = "Infinite Mode";
            }
        }
        
        // Find or create level info container in pause panel
        Transform levelInfoContainer = pausePanel.transform.Find("LevelInfoContainer");
        
        // ...existing code for creating/updating level info container...
    }
    
    #endregion
}
