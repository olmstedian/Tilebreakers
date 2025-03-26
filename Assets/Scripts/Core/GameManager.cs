// Controls game flow, turn sequence, game over condition

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject gameStateManagerPrefab;
    [SerializeField] private GameObject specialTileUIPrefab;

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
        Invoke(nameof(InitializeGame), 0.1f);

        // Initialize SpecialTileUI
        if (specialTileUIPrefab != null)
        {
            Instantiate(specialTileUIPrefab);
        }
    }

    private void InitializeGame()
    {
        GameStateManager.Instance?.SetState(new InitState());
    }

    public void EndTurn()
    {
        GameStateManager.Instance?.SetState(new PostTurnState());
    }
}