// Controls game flow, turn sequence, game over condition

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject gameStateManagerPrefab;
    // Remove the specialTileUIPrefab reference
    [SerializeField] private GameObject gameOverManagerPrefab; // Add a reference to the GameOverManager prefab
    [SerializeField] private GameObject gridManagerPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (GameStateManager.Instance == null && gameStateManagerPrefab != null)
        {
            Instantiate(gameStateManagerPrefab);
        }

        // Ensure GameOverManager is instantiated
        if (GameOverManager.Instance == null && gameOverManagerPrefab != null)
        {
            Instantiate(gameOverManagerPrefab);
            Debug.Log("GameManager: GameOverManager instantiated successfully.");
        }
        else if (GameOverManager.Instance == null)
        {
            Debug.LogError("GameManager: GameOverManager prefab is missing. Ensure it is assigned in the inspector.");
        }

        // Ensure all special tile prefabs are assigned
        if (SpecialTileManager.Instance != null)
        {
            Debug.Log("GameManager: Verifying special tile prefabs...");
            SpecialTileManager.Instance.InitializePrefabMap();
        }
    }

    private void Start()
    {
        Debug.Log("GameManager: Starting game...");

        // Ensure tilePrefab is assigned
        if (BoardManager.Instance.tilePrefab == null)
        {
            Debug.LogError("GameManager: Tile prefab is not assigned in BoardManager. Cannot start the game.");
            return;
        }

        // Initialize the Grid Manager for enhanced visual effects
        if (gridManagerPrefab != null && FindObjectOfType<GridManager>() == null)
        {
            Instantiate(gridManagerPrefab, transform);
        }

        GameStateManager.Instance?.SetState(new BootState());

        // Remove initialization of SpecialTileUI

        // Load the first level using LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(0); // Start with the first level
        }
    }

    public void EndTurn()
    {
        GameStateManager.Instance?.EndTurn();
    }

    public void LoadNextLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AdvanceToNextLevel();
        }
    }

    public void RestartCurrentLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartCurrentLevel();
        }
    }

    public void ActivateSpecialTile(Vector2Int gridPosition)
    {
        Debug.Log("GameManager: Activating special tile at position " + gridPosition);
        GameStateManager.Instance?.SetState(new SpecialTileActivationState());
    }

    public void SpawnSpecialTile(Vector2Int position, string abilityName)
    {
        // Allow passing "Random" to use the weighted random selection
        if (string.IsNullOrEmpty(abilityName) || abilityName == "Blaster")
        {
            if (Constants.TESTING_MODE && Random.value < 0.5f)
            {
                abilityName = "Doubler";
                Debug.Log("GameManager: Testing mode forced a Doubler tile to spawn instead of Blaster.");
            }
        }
        
        Debug.Log($"GameManager: Spawning special tile '{abilityName}' at position {position}");
        GameStateManager.Instance?.SetState(new SpecialTileSpawningState(position, abilityName));
    }
}