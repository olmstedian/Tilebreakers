using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro; // Add this line to fix the missing TextMeshProUGUI reference
using Tilebreakers.Special;
using Tilebreakers.Core; // For ScoreFacade and ScoreManager

/// <summary>
/// Manages level loading, transitions, and completion logic
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Levels")]
    [SerializeField] private LevelData[] levels;
    [SerializeField] private bool goToInfiniteModeAfterAllLevels = true;

    [Header("Win Conditions")]
    [SerializeField] private int movesTarget = 20;
    [SerializeField] private int scoreTarget = 1000;

    [Header("Special Tile Settings")]
    [SerializeField] [Range(0f, 1f)] private float specialTileChance = Constants.SPECIAL_TILE_CHANCE;
    [SerializeField] private bool enableBlasterTile = true;
    [SerializeField] private bool enableFreezerTile = true;
    [SerializeField] private bool enableDoublerTile = true;
    [SerializeField] private bool enablePainterTile = true;

    // Private state
    private int currentLevelIndex = 0;
    private bool infiniteMode = false;
    private bool levelCompleted = false;
    private int movesMade = 0;
    private int levelMovesTarget = 0;
    private int levelScoreTarget = 0;
    private int remainingMoves = 20; // Example value for moves left

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int TotalLevels { get { return levels.Length; } }

    public int CurrentLevelIndex => currentLevelIndex;

    public string CurrentLevel => $"Level {currentLevelIndex + 1}";

    public bool IsInfiniteMode => infiniteMode;

    public string GetLevelObjective()
    {
        if (infiniteMode)
        {
            return "Survive as long as possible!";
        }
        else if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length)
        {
            // Use the GetObjectiveText method which checks for custom objective text
            return levels[currentLevelIndex].GetObjectiveText();
        }
        return "Complete the level!";
    }

    public float GetLevelProgress()
    {
        // Example: Calculate progress as a percentage of the level objective
        int objective = currentLevelIndex * 100;
        return objective > 0 ? Mathf.Clamp01((float)ScoreManager.Instance.Score / objective) : 0f;
    }

    public int GetRemainingMoves()
    {
        return remainingMoves;
    }

    public void SetInfiniteMode(bool isInfinite)
    {
        infiniteMode = isInfinite;
    }

    public string GetDifficulty()
    {
        // Example: Return difficulty based on level index
        return currentLevelIndex < 5 ? "Easy" : currentLevelIndex < 10 ? "Medium" : "Hard";
    }

    /// <summary>
    /// Checks if the level is complete based on configured win conditions
    /// </summary>
    public bool IsLevelComplete()
    {
        // In infinite mode, there's no win condition
        if (infiniteMode)
        {
            return false;
        }

        // FIX: Use the Score property instead of GetScore()
        int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;

        // Check if we've met the score target (if it's set)
        bool scoreTargetMet = levelScoreTarget <= 0 || currentScore >= levelScoreTarget;

        // Check if we've made the target number of moves or fewer (if it's set)
        bool moveTargetMet = levelMovesTarget <= 0 || movesMade <= levelMovesTarget;

        // Level is complete if we've met both targets
        bool isComplete = scoreTargetMet && moveTargetMet;

        Debug.Log($"LevelManager: Level completion check - Score: {currentScore}/{levelScoreTarget} ({scoreTargetMet}), " +
                  $"Moves: {movesMade}/{levelMovesTarget} ({moveTargetMet}), Complete: {isComplete}");

        return isComplete;
    }

    /// <summary>
    /// Loads a level by index
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length)
        {
            Debug.LogWarning($"LevelManager: Level index {levelIndex} out of range (0-{levels.Length-1})");
            
            // If we're trying to load a non-existent level, check if we should go to infinite mode
            if (goToInfiniteModeAfterAllLevels && levelIndex >= levels.Length)
            {
                StartInfiniteMode();
                return;
            }
            
            levelIndex = Mathf.Clamp(levelIndex, 0, levels.Length - 1);
        }

        currentLevelIndex = levelIndex;
        infiniteMode = false;
        levelCompleted = false;
        movesMade = 0;

        // Reset score for the new level
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        // Reset move count
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetMoves();
        }

        // Load the level data
        LoadLevelData(levels[levelIndex]);

        // Transition to waiting for input state
        GameStateManager.Instance?.SetState(new WaitingForInputState());

        Debug.Log($"LevelManager: Loaded level {levelIndex} ({levels[levelIndex].levelName})");
    }

    /// <summary>
    /// Checks if the current level's objectives have been met and transitions to completion state if so
    /// </summary>
    public void CheckLevelCompletion()
    {
        if (levelCompleted) return; // Already completed

        if (IsLevelComplete())
        {
            levelCompleted = true;
            
            // Wait a moment before showing level complete screen
            StartCoroutine(DelayedLevelComplete());
        }
    }

    private IEnumerator DelayedLevelComplete()
    {
        // Wait for animations to finish
        yield return new WaitForSeconds(0.7f);

        // Get the next level index for UI
        int nextLevelIndex = currentLevelIndex + 1;
        
        // If next level doesn't exist and infinite mode is enabled, indicate infinite mode
        if (nextLevelIndex >= levels.Length && goToInfiniteModeAfterAllLevels)
        {
            nextLevelIndex = -1; // Special value to indicate infinite mode
        }
        
        // Transition to level complete state
        GameStateManager.Instance?.EnterLevelCompleteState(nextLevelIndex);
    }

    /// <summary>
    /// Configures the board based on the current level data
    /// </summary>
    private void LoadLevelData(LevelData levelData)
    {
        if (levelData == null || BoardManager.Instance == null)
        {
            Debug.LogError("LevelManager: Missing level data or board manager");
            return;
        }

        // Set the level targets
        levelMovesTarget = levelData.movesTarget > 0 ? levelData.movesTarget : movesTarget;
        levelScoreTarget = levelData.scoreTarget > 0 ? levelData.scoreTarget : scoreTarget;

        // Set up the board
        SetupBoard();
        
        // Use UIManager to show level info with description
        UIManager.Instance?.ShowLevelInfo(
            levelData.levelName, 
            levelData.levelNumber,
            levelMovesTarget, 
            levelScoreTarget,
            levelData.levelDescription);
        
        Debug.Log($"LevelManager: Loaded level data for {levelData.levelName} with description: {levelData.levelDescription}");
    }

    /// <summary>
    /// Sets up the board for the current level
    /// </summary>
    private void SetupBoard()
    {
        // Clear the board first
        BoardManager.Instance.ClearBoard();

        // TODO: Add level-specific board setup logic here
        // For now, just generate random starting tiles
        BoardManager.Instance.GenerateRandomStartingTiles();
        
        // Configure special tiles based on level settings
        ConfigureSpecialTiles(specialTileChance, enableBlasterTile, enableFreezerTile, enableDoublerTile, enablePainterTile);
    }

    /// <summary>
    /// Notifies the manager when a move is made
    /// </summary>
    public void NotifyMoveMade()
    {
        movesMade++;
        Debug.Log($"LevelManager: Move made. Total moves: {movesMade}");
        
        // Check if we've reached the move target
        if (levelMovesTarget > 0 && movesMade > levelMovesTarget)
        {
            Debug.Log($"LevelManager: Exceeded move target ({movesMade}/{levelMovesTarget})");
            
            // In non-infinite mode, this affects level completion
            if (!infiniteMode)
            {
                CheckLevelCompletion();
            }
        }
    }

    /// <summary>
    /// Configure special tile settings for the current level
    /// </summary>
    private void ConfigureSpecialTiles(float chance, bool enableBlaster, bool enableFreezer, bool enableDoubler, bool enablePainter)
    {
        if (SpecialTileManager.Instance != null)
        {
            SpecialTileManager.Instance.ConfigureEnabledTileTypes(enableBlaster, enableFreezer, enableDoubler, enablePainter);
            // FIX: Remove assignment to 'specialTileChance'; the property does not exist.
            // SpecialTileManager.Instance.specialTileChance = chance;
        }
        
        Debug.Log($"LevelManager: Configured special tiles - Chance: {chance}, Blaster: {enableBlaster}, Freezer: {enableFreezer}, Doubler: {enableDoubler}, Painter: {enablePainter}");
    }

    /// <summary>
    /// Resets all level tracking data
    /// </summary>
    private void ResetLevelTracking()
    {
        movesMade = 0;
        levelCompleted = false;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetMoves();
        }
        
        // Fix: Use ScoreManager instance directly instead of ScoreFacade
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
    }

    /// <summary>
    /// Advances to the next level or starts infinite mode if all levels are completed.
    /// </summary>
    public void AdvanceToNextLevel()
    {
        currentLevelIndex++;
        
        Debug.Log($"LevelManager: Advancing to level {currentLevelIndex}. Total levels: {levels.Length}");
        
        // Check if we've completed all levels
        if (currentLevelIndex >= levels.Length)
        {
            Debug.Log("LevelManager: All levels completed. Starting infinite mode.");
            StartInfiniteMode();
        }
        else
        {
            LoadLevel(currentLevelIndex);
        }
        
        // CRITICAL FIX: Ensure we transition to WaitingForInputState after setup is complete
        GameStateManager.Instance.StartCoroutine(DelayedStateTransition());
    }

    private IEnumerator DelayedStateTransition()
    {
        // Wait a short time to ensure all level setup is complete
        yield return new WaitForSeconds(0.5f);
        
        // Log current state
        Debug.Log($"LevelManager: Current game state before transition: {GameStateManager.Instance.GetCurrentStateName()}");
        
        // Force transition to WaitingForInputState to allow player input
        if (!GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            Debug.Log("LevelManager: Forcing transition to WaitingForInputState");
            GameStateManager.Instance.SetState(new WaitingForInputState());
        }
    }

    /// <summary>
    /// Starts the infinite mode after all levels are completed.
    /// </summary>
    public void StartInfiniteMode()
    {
        Debug.Log("LevelManager: Setting up infinite mode...");
        
        // Reset level tracking data
        ResetLevelTracking();
        
        // Create an infinite level
        currentLevelIndex = -1; // Use -1 to indicate infinite mode
        infiniteMode = true;
        
        // Clear the board
        BoardManager.Instance.ClearBoard();
        
        // Setup the board with random tiles
        SetupBoard();
        
        // CRITICAL FIX: Make sure we always transition to WaitingForInputState
        GameStateManager.Instance.SetState(new WaitingForInputState());
        
        Debug.Log("LevelManager: Started Infinite Mode");
    }

    /// <summary>
    /// Restarts the current level
    /// </summary>
    public void RestartCurrentLevel()
    {
        Debug.Log("LevelManager: Restarting current level");
        
        if (infiniteMode)
        {
            StartInfiniteMode();
        }
        else if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length)
        {
            LoadLevel(currentLevelIndex);
        }
        else
        {
            Debug.LogError($"LevelManager: Cannot restart - invalid level index {currentLevelIndex}");
            // Load the first level as a fallback
            LoadLevel(0);
        }
    }

    /// <summary>
    /// Gets the current level data
    /// </summary>
    public LevelData GetCurrentLevelData()
    {
        if (infiniteMode)
        {
            return null; // No specific level data for infinite mode
        }
        else if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length)
        {
            return levels[currentLevelIndex];
        }
        return null;
    }

    /// <summary>
    /// Gets the current level name
    /// </summary>
    public string GetCurrentLevelName()
    {
        if (infiniteMode)
        {
            return "Infinite Mode";
        }
        else if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length)
        {
            return levels[currentLevelIndex].levelName;
        }
        return "Unknown Level";
    }
    
    /// <summary>
    /// Gets the current level description
    /// </summary>
    public string GetLevelDescription()
    {
        if (infiniteMode)
        {
            return "Play as long as possible! The board gets more challenging as you progress.";
        }
        else if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length)
        {
            return !string.IsNullOrEmpty(levels[currentLevelIndex].levelDescription) ? 
                levels[currentLevelIndex].levelDescription : 
                "Break tiles and reach the target score!";
        }
        return "Break some tiles!";
    }

    /// <summary>
    /// Gets the current level number
    /// </summary>
    public int GetCurrentLevelNumber()
    {
        if (infiniteMode)
        {
            return -1; // Special value for infinite mode
        }
        else if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length)
        {
            return levels[currentLevelIndex].levelNumber;
        }
        return 0;
    }

    /// <summary>
    /// Gets the current level score target
    /// </summary>
    public int GetLevelScoreTarget()
    {
        return levelScoreTarget;
    }

    /// <summary>
    /// Gets the current level moves target
    /// </summary>
    public int GetLevelMovesTarget()
    {
        return levelMovesTarget;
    }

    public string GetObjectiveType()
    {
        return infiniteMode ? "Survive" : "Score";
    }

    public int GetObjectiveValue()
    {
        return infiniteMode ? 0 : levelScoreTarget;
    }

    public int GetMaxMoves()
    {
        // Return the maximum number of moves allowed for the current level
        return 20 + (currentLevelIndex * 5); // Example formula: base 20 moves + 5 per level
    }

    /// <summary>
    /// Updates UI objective text using the current level data
    /// Called by UIManager when refreshing objective display
    /// </summary>
    public void UpdateObjectiveText(TextMeshProUGUI levelObjectiveText)
    {
        if (levelObjectiveText != null)
        {
            LevelData currentLevel = GetCurrentLevelData();
            if (currentLevel != null)
            {
                levelObjectiveText.text = currentLevel.GetObjectiveText();
            }
            else
            {
                levelObjectiveText.text = GetLevelObjective();
            }
            
            Debug.Log($"LevelManager: Updated objective text to: {levelObjectiveText.text}");
        }
    }
}

/// <summary>
/// Class to store state for infinite mode
/// </summary>
[System.Serializable]
public class InfiniteModeState
{
    public int CurrentTurn = 0;
    public int HighestTurn = 0;
    public int CurrentScore = 0;
    public int HighScore = 0;
}

/// <summary>
/// Helper class for serializing high score data
/// </summary>
[System.Serializable]
public class HighScoreData
{
    public Dictionary<int, int> scores = new Dictionary<int, int>();
}

/// <summary>
/// Helper component to observe ScoreManager events and relay them to LevelManager
/// </summary>
public class ScoreObserver : MonoBehaviour
{
    private System.Action<int> scoreChangedCallback;
    private int lastScore = 0;
    
    public void Initialize(System.Action<int> callback)
    {
        scoreChangedCallback = callback;
        lastScore = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        Debug.Log("ScoreObserver: Initialized to watch ScoreManager score changes");
    }
    
    private void Update()
    {
        if (ScoreManager.Instance != null && scoreChangedCallback != null)
        {
            // Poll the score each frame and invoke callback only when it changes
            int currentScore = ScoreManager.Instance.Score;
            if (currentScore != lastScore)
            {
                lastScore = currentScore;
                scoreChangedCallback(currentScore);
            }
        }
    }
}