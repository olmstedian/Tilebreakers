using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Checks if the game is over by verifying if the board is full and no valid merges exist.
    /// </summary>
    public void CheckGameOver()
    {
        if (IsBoardFull() && !AnyValidMergesExist())
        {
            GameStateManager.Instance.SetState(new GameOverState());
            UIManager.Instance.ShowGameOverScreen(ScoreManager.Instance.GetCurrentScore());
        }
    }

    /// <summary>
    /// Determines if the board is completely full (no empty cells).
    /// </summary>
    private bool IsBoardFull()
    {
        for (int x = 0; x < BoardManager.Instance.width; x++)
        {
            for (int y = 0; y < BoardManager.Instance.height; y++)
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
    /// Checks if any valid merges exist on the board.
    /// </summary>
    private bool AnyValidMergesExist()
    {
        for (int x = 0; x < BoardManager.Instance.width; x++)
        {
            for (int y = 0; y < BoardManager.Instance.height; y++)
            {
                Tile current = BoardManager.Instance.GetTileAtPosition(new Vector2Int(x, y));
                if (current == null) continue;

                // Check 4 orthogonal directions
                foreach (Vector2Int dir in DirectionUtils.Orthogonal)
                {
                    Vector2Int neighborPos = new Vector2Int(x + dir.x, y + dir.y);
                    if (BoardManager.Instance.IsWithinBounds(neighborPos))
                    {
                        Tile neighbor = BoardManager.Instance.GetTileAtPosition(neighborPos);
                        if (neighbor != null && BoardManager.Instance.CompareColors(current.tileColor, neighbor.tileColor))
                        {
                            return true; // A merge is possible
                        }
                    }
                }
            }
        }
        return false; // No valid merges anywhere
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
