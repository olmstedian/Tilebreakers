using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private List<LevelData> levels; // List of levels
    [SerializeField] private int maxLevel = 10; // Maximum level before ending the game
    
    private int currentLevelIndex = 0;
    private int moveCount = 0;

    public LevelData CurrentLevel { get; private set; }
    public int CurrentLevelIndex => currentLevelIndex;
    public int MoveCount => moveCount;
    public int TotalLevels => levels.Count;
    public bool IsLastLevel => currentLevelIndex >= levels.Count - 1;
    public bool IsGameComplete => currentLevelIndex >= maxLevel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (levels.Count > 0)
        {
            LoadLevel(0); // Start with the first level
        }
        else
        {
            Debug.LogError("LevelManager: No levels configured in the LevelManager.");
        }
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError($"LevelManager: Invalid level index {levelIndex}. Cannot load level.");
            return;
        }

        currentLevelIndex = levelIndex;
        CurrentLevel = levels[currentLevelIndex];
        moveCount = 0;
        
        Debug.Log($"LevelManager: Loaded level {currentLevelIndex + 1}: {CurrentLevel.name}");

        // Apply level-specific configurations
        BoardManager.Instance.width = CurrentLevel.gridSizeX;
        BoardManager.Instance.height = CurrentLevel.gridSizeY;
        BoardManager.Instance.InitializeBoard();

        // Spawn initial tiles
        BoardManager.Instance.GenerateRandomStartingTiles(CurrentLevel.startingTileCount);

        // Reset score and UI
        ScoreManager.Instance.ResetScore();
        UIManager.Instance.ResetTopBar();
        UIManager.Instance.UpdateLevelText();
        UIManager.Instance.UpdateMoveCount(0);
    }

    public void IncrementMoveCount()
    {
        moveCount++;
        UIManager.Instance.UpdateMoveCount(moveCount);
        
        // If this level has a move limit, check if we're over it
        if (CurrentLevel.maxMoves > 0 && moveCount >= CurrentLevel.maxMoves)
        {
            CheckLevelCompletion();
        }
    }
    
    public bool IsLevelComplete()
    {
        // Check if the score target has been reached
        bool scoreReached = ScoreManager.Instance.GetCurrentScore() >= CurrentLevel.scoreTarget;
        
        // If there's a move limit, also check if we've used all moves
        if (CurrentLevel.maxMoves > 0)
        {
            // Level is complete if score is reached OR all moves are used up
            return scoreReached || moveCount >= CurrentLevel.maxMoves;
        }
        
        // Otherwise, level is complete when score target is reached
        return scoreReached;
    }

    public void AdvanceToNextLevel()
    {
        if (IsGameComplete || currentLevelIndex + 1 >= levels.Count)
        {
            Debug.Log("LevelManager: Game complete! No more levels.");
            GameStateManager.Instance.SetState(new GameCompleteState());
            return;
        }
        
        Debug.Log("LevelManager: Advancing to the next level...");
        GameStateManager.Instance.SetState(new LevelCompleteState(currentLevelIndex + 1));
    }

    public void RestartCurrentLevel()
    {
        Debug.Log("LevelManager: Restarting current level...");
        LoadLevel(currentLevelIndex);
        GameStateManager.Instance.SetState(new WaitingForInputState());
    }

    public void CheckLevelCompletion()
    {
        if (IsLevelComplete())
        {
            if (ScoreManager.Instance.GetCurrentScore() >= CurrentLevel.scoreTarget)
            {
                Debug.Log("LevelManager: Level complete! Score target reached.");
                AdvanceToNextLevel();
            }
            else if (CurrentLevel.maxMoves > 0 && moveCount >= CurrentLevel.maxMoves)
            {
                Debug.Log("LevelManager: Level failed! Used all available moves without reaching score target.");
                GameStateManager.Instance.SetState(new LevelFailedState());
            }
        }
        else if (!BoardManager.Instance.HasValidMove())
        {
            Debug.Log("LevelManager: No more valid moves! Level failed.");
            GameStateManager.Instance.SetState(new LevelFailedState());
        }
    }
    
    public string GetLevelDescription()
    {
        if (CurrentLevel == null) return "No level loaded";
        
        string description = $"Level {currentLevelIndex + 1}";
        
        if (!string.IsNullOrEmpty(CurrentLevel.levelDescription))
        {
            description += $": {CurrentLevel.levelDescription}";
        }
        
        return description;
    }
    
    public string GetLevelObjective()
    {
        if (CurrentLevel == null) return "";
        
        return $"Score {CurrentLevel.scoreTarget} points" + 
               (CurrentLevel.maxMoves > 0 ? $" in {CurrentLevel.maxMoves} moves" : "");
    }
    
    // Get progress as a percentage (for UI progress bars)
    public float GetLevelProgress()
    {
        if (CurrentLevel == null) return 0f;
        
        float currentScore = ScoreManager.Instance.GetCurrentScore();
        return Mathf.Clamp01(currentScore / CurrentLevel.scoreTarget);
    }
}