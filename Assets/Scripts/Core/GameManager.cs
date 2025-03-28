// Controls game flow, turn sequence, game over condition

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject gameStateManagerPrefab;
    [SerializeField] private GameObject specialTileUIPrefab; // Keep this for UI

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (GameStateManager.Instance == null && gameStateManagerPrefab != null)
        {
            Instantiate(gameStateManagerPrefab);
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

        GameStateManager.Instance?.SetState(new BootState());

        // Initialize SpecialTileUI
        if (specialTileUIPrefab != null)
        {
            GameObject specialTileUI = Instantiate(specialTileUIPrefab);
            specialTileUI.SetActive(false); // Ensure the prefab is not visible in the scene
        }

        // Load the first level using LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(0); // Start with the first level
        }
    }

    public void EndTurn()
    {
        GameStateManager.Instance?.SetState(new SpawningNewTileState());
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
        Debug.Log($"GameManager: Spawning special tile '{abilityName}' at position {position}");
        GameStateManager.Instance?.SetState(new SpecialTileSpawningState(position, abilityName));
    }
}