using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("GameOverManager: Initialized successfully.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Checks if the game is over by verifying if the board is full and no valid moves or merges exist,
    /// or if the level objectives have been met.
    /// </summary>
    public void CheckGameOver()
    {
        // First check if level is complete
        if (LevelManager.Instance != null && LevelManager.Instance.IsLevelComplete())
        {
            Debug.Log("GameOverManager: Level objectives met. Transitioning to level complete state.");
            LevelManager.Instance.CheckLevelCompletion();
            return;
        }
        
        // Then check for board state (no more valid moves)
        if (IsBoardFull() && !AnyValidMovesOrMergesExist())
        {
            Debug.Log("GameOverManager: No valid moves or merges left. Game over.");
            
            // Level failed - no more valid moves
            if (LevelManager.Instance != null)
            {
                GameStateManager.Instance.SetState(new LevelFailedState());
            }
            else
            {
                GameStateManager.Instance.SetState(new GameOverState());
            }
        }
        else
        {
            Debug.Log("GameOverManager: Valid moves or merges still exist. Transitioning to WaitingForInputState.");
            GameStateManager.Instance.SetState(new WaitingForInputState());
        }
    }

    /// <summary>
    /// Determines if the board is completely full (no empty cells).
    /// </summary>
    public bool IsBoardFull()
    {
        if (BoardManager.Instance == null)
        {
            Debug.LogError("GameOverManager: BoardManager.Instance is null. Cannot check if board is full.");
            return false;
        }

        for (int x = 0; x < BoardManager.Instance.width; x++)
        {
            for (int y = 0; x < BoardManager.Instance.height; y++)
            {
                if (BoardManager.Instance.GetTileAtPosition(new Vector2Int(x, y)) == null)
                {
                    return false; // Found an empty space
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Checks if any valid moves or merges exist on the board.
    /// </summary>
    public bool AnyValidMovesOrMergesExist()
    {
        if (BoardManager.Instance == null)
        {
            Debug.LogError("GameOverManager: BoardManager.Instance is null. Cannot check for valid moves or merges.");
            return false;
        }

        for (int x = 0; x < BoardManager.Instance.width; x++)
        {
            for (int y = 0; x < BoardManager.Instance.height; y++)
            {
                Tile current = BoardManager.Instance.GetTileAtPosition(new Vector2Int(x, y));
                if (current == null) continue;

                // Check 4 orthogonal directions for valid moves or merges
                foreach (Vector2Int dir in DirectionUtils.Orthogonal)
                {
                    Vector2Int neighborPos = new Vector2Int(x + dir.x, y + dir.y);
                    if (BoardManager.Instance.IsWithinBounds(neighborPos))
                    {
                        Tile neighbor = BoardManager.Instance.GetTileAtPosition(neighborPos);
                        if (neighbor == null || BoardManager.Instance.CompareColors(current.tileColor, neighbor.tileColor))
                        {
                            return true; // A valid move or merge is possible
                        }
                    }
                }
            }
        }
        return false; // No valid moves or merges anywhere
    }
}

/// <summary>
/// Utility class for directional vectors.
/// </summary>
public static class DirectionUtils
{
    public static readonly Vector2Int[] Orthogonal = {
        new Vector2Int(1, 0),  // Right
        new Vector2Int(-1, 0), // Left
        new Vector2Int(0, 1),  // Up
        new Vector2Int(0, -1)  // Down
    };
}
