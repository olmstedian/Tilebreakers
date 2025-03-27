// Controls game flow, turn sequence, game over condition

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject gameStateManagerPrefab;
    [SerializeField] private GameObject specialTileUIPrefab; // Keep this for UI
    [SerializeField] private GameObject blasterTilePrefab; // Ensure this is assigned in the Inspector

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
        GameStateManager.Instance?.SetState(new BootState());

        Invoke(nameof(InitializeGame), 0.1f);

        // Initialize SpecialTileUI
        if (specialTileUIPrefab != null)
        {
            Instantiate(specialTileUIPrefab);
        }

        // Remove any unnecessary instantiation of BlasterTilePrefab
        // The BlasterTilePrefab should only be instantiated dynamically when needed
    }

    private void InitializeGame()
    {
        Debug.Log("GameManager: Initializing game...");
        GameStateManager.Instance?.SetState(new InitState());
    }

    public void EndTurn()
    {
        GameStateManager.Instance?.SetState(new PostTurnState());
    }
}