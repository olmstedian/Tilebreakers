// Controls game flow, turn sequence, game over condition

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject gameStateManagerPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (GameStateManager.Instance == null && gameStateManagerPrefab != null)
        {
            Instantiate(gameStateManagerPrefab);
        }
    }

    void Start()
    {
        Invoke(nameof(InitializeGame), 0.1f);
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