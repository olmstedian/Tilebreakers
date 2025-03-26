// Controls game flow, turn sequence, game over condition

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Initialize game (board, UI, etc.)
        BoardManager.Instance.GenerateRandomStartingTiles();
    }

    public void EndTurn()
    {
        // Spawn one new tile using the constants-defined spawn count (1 tile)
        BoardManager.Instance.GenerateRandomStartingTiles(1, 1);
        // Check for game over
        if (!BoardManager.Instance.HasValidMove())
        {
            Debug.Log("Game Over!");
            // Trigger game over logic (e.g., show game over screen)
        }
    }
}