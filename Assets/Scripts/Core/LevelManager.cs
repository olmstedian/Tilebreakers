using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public LevelData currentLevel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void LoadLevel(LevelData levelData)
    {
        currentLevel = levelData;
        Debug.Log($"Loaded level: {levelData.name}");

        // Example usage:
        // BoardManager.Instance.SetupGrid(currentLevel.gridSizeX, currentLevel.gridSizeY);
        // BoardManager.Instance.SpawnInitialTiles(currentLevel.startingTileCount);
    }

    public bool IsLevelComplete()
    {
        // Placeholder logic
        return ScoreManager.Instance.CurrentScore >= currentLevel.scoreTarget;
    }

    public void AdvanceToNextLevel()
    {
        Debug.Log("Level complete! Load next level here...");
        // Implement logic to move to the next LevelData
    }
}