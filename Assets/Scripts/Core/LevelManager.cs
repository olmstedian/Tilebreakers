using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private List<LevelData> levels; // List of levels
    private int currentLevelIndex = 0;

    public LevelData CurrentLevel { get; private set; }

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
        Debug.Log($"LevelManager: Loaded level {CurrentLevel.name}");

        // Apply level-specific configurations
        BoardManager.Instance.width = CurrentLevel.gridSizeX;
        BoardManager.Instance.height = CurrentLevel.gridSizeY;
        BoardManager.Instance.InitializeBoard();

        // Spawn initial tiles
        BoardManager.Instance.GenerateRandomStartingTiles(CurrentLevel.startingTileCount);

        // Reset score and UI
        ScoreManager.Instance.ResetScore();
        UIManager.Instance.ResetTopBar();
    }

    public bool IsLevelComplete()
    {
        return ScoreManager.Instance.GetCurrentScore() >= CurrentLevel.scoreTarget;
    }

    public void AdvanceToNextLevel()
    {
        if (currentLevelIndex + 1 < levels.Count)
        {
            Debug.Log("LevelManager: Advancing to the next level...");
            LoadLevel(currentLevelIndex + 1);
        }
        else
        {
            Debug.Log("LevelManager: All levels completed!");
            UIManager.Instance.ShowGameOverScreen(ScoreManager.Instance.GetCurrentScore());
        }
    }

    public void RestartCurrentLevel()
    {
        Debug.Log("LevelManager: Restarting current level...");
        LoadLevel(currentLevelIndex);
    }

    public void CheckLevelCompletion()
    {
        if (IsLevelComplete())
        {
            Debug.Log("LevelManager: Level complete!");
            AdvanceToNextLevel();
        }
    }
}