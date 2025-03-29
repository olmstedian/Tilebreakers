using UnityEngine;
using System.Collections.Generic;

public class TileSplitter : MonoBehaviour
{
    /// <summary>
    /// Splits a tile into multiple smaller tiles with random values and colors.
    /// The sum of the new tile values equals the original tile value.
    /// New tiles are placed in random available positions on the board.
    /// </summary>
    /// <param name="tile">The tile to split</param>
    /// <param name="originalPosition">Grid position of the original tile</param>
    public static void SplitTile(Tile tile, Vector2Int originalPosition)
    {
        int originalValue = tile.number;
        Color originalColor = tile.tileColor;

        // Determine how many tiles to split into (2-4 tiles)
        int splitCount = Random.Range(2, Mathf.Min(5, originalValue));

        // Find available cells for spawning the split tiles
        List<Vector2Int> availablePositions = FindSplitPositions(originalPosition);
        if (availablePositions.Count < splitCount)
        {
            splitCount = Mathf.Max(2, availablePositions.Count);
        }

        // Generate random values that sum to the original value
        List<int> splitValues = GenerateSplitValues(originalValue, splitCount);

        // Destroy the original tile
        Object.Destroy(tile.gameObject);

        // Spawn a special tile at the original position
        if (Random.value < Constants.SPECIAL_TILE_CHANCE)
        {
            BoardManager.Instance.SpawnSpecialTile(originalPosition, "Blaster");
        }

        // Create new tiles at the random positions
        for (int i = 0; i < splitCount; i++)
        {
            Vector2Int spawnPos = availablePositions[i];
            int value = splitValues[i];

            // Choose a random color from the predefined palette
            Color randomColor = BoardManager.Instance.GetRandomTileColor();

            // Create new tile
            CreateTileAtPosition(BoardManager.Instance.tilePrefab, spawnPos, value, randomColor);
        }

        // Add score for the split
        ScoreManager.Instance.AddSplitScore(originalValue); // Add points for the total value of resulting split tiles
    }

    private static List<Vector2Int> FindSplitPositions(Vector2Int originalPos)
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        BoardManager boardManager = BoardManager.Instance;

        // Check all cells on the board
        for (int x = 0; x < boardManager.width; x++)
        {
            for (int y = 0; y < boardManager.height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // Skip original position and occupied cells
                if (pos == originalPos || !boardManager.IsCellEmpty(pos))
                    continue;

                // Prioritize cells that aren't adjacent to the original position
                int manhattanDistance = Mathf.Abs(pos.x - originalPos.x) + Mathf.Abs(pos.y - originalPos.y);
                if (manhattanDistance > 1)
                {
                    availablePositions.Add(pos);
                }
            }
        }

        // If we don't have enough non-adjacent cells, add adjacent empty cells
        if (availablePositions.Count < 2)
        {
            for (int x = 0; x < boardManager.width; x++)
            {
                for (int y = 0; y < boardManager.height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (pos != originalPos && boardManager.IsCellEmpty(pos) && !availablePositions.Contains(pos))
                    {
                        availablePositions.Add(pos);
                    }
                }
            }
        }

        // Shuffle the positions
        ShufflePositions(availablePositions);

        return availablePositions;
    }

    private static List<int> GenerateSplitValues(int targetSum, int count)
    {
        List<int> values = new List<int>();

        // First, give each tile at least 1
        int remaining = targetSum - count;
        for (int i = 0; i < count; i++)
        {
            values.Add(1);
        }

        // Distribute the remaining value randomly
        while (remaining > 0)
        {
            int idx = Random.Range(0, count);
            values[idx]++;
            remaining--;
        }

        return values;
    }

    private static void CreateTileAtPosition(GameObject tilePrefab, Vector2Int position, int value, Color color)
    {
        // Ensure tilePrefab is assigned
        if (tilePrefab == null)
        {
            Debug.LogError("TileSplitter: Tile prefab is not assigned. Cannot create tiles.");
            return;
        }

        // Get world position for spawning the tile
        Vector2 worldPos = BoardManager.Instance.GetWorldPosition(position);

        GameObject newTileObj = Object.Instantiate(tilePrefab, worldPos, Quaternion.identity, BoardManager.Instance.transform);
        Tile newTile = newTileObj.GetComponent<Tile>();

        if (newTile != null)
        {
            newTile.Initialize(color, value);
            BoardManager.Instance.RegisterSplitTile(position, newTile);
        }
        else
        {
            Debug.LogError("TileSplitter: Spawned tile does not have a Tile component.");
            Destroy(newTileObj);
        }
    }

    private static void ShufflePositions(List<Vector2Int> positions)
    {
        int n = positions.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            Vector2Int temp = positions[k];
            positions[k] = positions[n];
            positions[n] = temp;
        }
    }
}
