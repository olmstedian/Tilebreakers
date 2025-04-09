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
        Debug.Log($"TileSplitter: Splitting tile at {originalPosition} with value {tile.number}.");

        int originalValue = tile.number;
        Color originalColor = tile.tileColor;

        // Store the reference to the GameObject before destruction
        GameObject tileObject = tile.gameObject;

        // Determine how many tiles to split into, at least 2, but could be more based on value
        int maxSplitCount = Mathf.Max(2, Mathf.CeilToInt(originalValue / 12f) + 1);
        int minSplitCount = Mathf.Min(2, maxSplitCount); // Always at least 2 splits
        int splitCount = Random.Range(minSplitCount, Mathf.Min(maxSplitCount + 1, 5)); // Cap at 5 splits
        
        List<Vector2Int> availablePositions = FindSplitPositions(originalPosition);

        // Adjust split count based on available positions
        splitCount = Mathf.Min(splitCount, availablePositions.Count);
        if (splitCount < 2)
        {
            Debug.LogWarning("TileSplitter: Not enough available positions to split the tile.");

            // Trigger game-over check
            GameOverManager.Instance?.CheckGameOver();
            return;
        }

        List<int> splitValues = GenerateSplitValues(originalValue, splitCount, 12); // Enforce max value of 12

        // Use the utility to safely destroy the original tile
        TileDestructionUtility.DestroyTile(tileObject, originalPosition);

        for (int i = 0; i < splitCount; i++)
        {
            Vector2Int spawnPos = availablePositions[i];
            int value = splitValues[i];
            Color randomColor = global::BoardManager.Instance.GetRandomTileColor(); // resolved ambiguity

            Debug.Log($"TileSplitter: Creating tile at {spawnPos} with value {value} and color {randomColor}.");
            CreateTileAtPosition(BoardManager.Instance.tilePrefab, spawnPos, value, randomColor);
        }

        // Use our new specialized method for special tile spawning
        SpawnSpecialTileAfterSplit(originalPosition);

        ScoreManager.Instance.AddSplitScore(originalValue);
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
                if (pos == originalPos || !global::BoardManager.Instance.IsCellEmpty(pos))
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
                    if (pos != originalPos && global::BoardManager.Instance.IsCellEmpty(pos) && !availablePositions.Contains(pos))
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

    /// <summary>
    /// Generates a list of values that add up to the target sum, ensuring no value exceeds maxValue.
    /// </summary>
    /// <param name="targetSum">The sum of all values</param>
    /// <param name="count">How many values to generate</param>
    /// <param name="maxValue">Maximum value allowed for any individual tile</param>
    private static List<int> GenerateSplitValues(int targetSum, int count, int maxValue = int.MaxValue)
    {
        List<int> values = new List<int>();
        int remaining = targetSum;
        
        // First, ensure we have enough values by giving each tile at least 1
        for (int i = 0; i < count; i++)
        {
            values.Add(1);
        }
        remaining -= count;
        
        // If we have less remaining than count, we can safely distribute
        if (remaining <= 0)
            return values;

        // Calculate the maximum we can add to each tile to stay under maxValue
        List<int> maxAdditions = new List<int>();
        for (int i = 0; i < count; i++)
        {
            maxAdditions.Add(maxValue - values[i]);
        }

        // Distribute remaining points, respecting maxValue
        while (remaining > 0)
        {
            // Check if we still have tiles that can accept more points
            bool canDistribute = maxAdditions.Exists(max => max > 0);
            if (!canDistribute)
            {
                Debug.LogWarning($"TileSplitter: Cannot distribute remaining {remaining} points while keeping all tiles under {maxValue}.");
                break;
            }
            
            // Randomly select a tile to add a point to
            int idx;
            int attempts = 0;
            do {
                idx = Random.Range(0, count);
                attempts++;
                // Break out if we've tried too many times to avoid infinite loop
                if (attempts > 100)
                {
                    Debug.LogError("TileSplitter: Failed to find valid tile to add value to. Aborting distribution.");
                    break;
                }
            } while (maxAdditions[idx] <= 0);
            
            // Add one point and reduce remaining
            values[idx]++;
            maxAdditions[idx]--;
            remaining--;
        }
        
        // If we still have remaining value but couldn't distribute it, log a warning
        if (remaining > 0)
        {
            Debug.LogWarning($"TileSplitter: Couldn't distribute all value. Original: {targetSum}, Sum of splits: {targetSum - remaining}");
            
            // Last effort - add remaining value to the first tile, ignoring maxValue
            // This should never happen with proper distribution above
            if (values.Count > 0)
            {
                Debug.LogWarning($"TileSplitter: Adding remaining {remaining} to first tile, which might exceed maxValue!");
                values[0] += remaining;
            }
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

    /// <summary>
    /// Spawns a random special tile near a given position, with an increased chance for common tiles.
    /// </summary>
    public static void SpawnSpecialTileAfterSplit(Vector2Int splitPosition)
    {
        if (Random.value < Constants.SPECIAL_TILE_CHANCE)
        {
            Debug.Log($"TileSplitter: Attempting to spawn a random special tile near {splitPosition}.");
            
            // First find an empty position for spawning
            Vector2Int? spawnPosition = FindEmptyPositionForSpecialTile(splitPosition);
            
            if (!spawnPosition.HasValue)
            {
                Debug.LogWarning("TileSplitter: Could not find valid position for special tile spawn after split.");
                return;
            }
            
            Debug.Log($"TileSplitter: Found valid position {spawnPosition.Value} for special tile spawn.");
            SpecialTileManager.Instance?.SpawnSpecialTile(spawnPosition.Value, "Random");
        }
        else
        {
            Debug.Log("TileSplitter: Random chance did not trigger special tile spawn.");
        }
    }

    /// <summary>
    /// Finds a suitable empty position for spawning a special tile after splitting.
    /// </summary>
    private static Vector2Int? FindEmptyPositionForSpecialTile(Vector2Int origin)
    {
        // Cache the board dimensions for better readability
        int width = BoardManager.Instance.width;
        int height = BoardManager.Instance.height;
        
        // List to store candidate positions
        List<Vector2Int> candidates = new List<Vector2Int>();
        
        // First check adjacent positions (orthogonal + diagonal)
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left,
            Vector2Int.up + Vector2Int.right, Vector2Int.up + Vector2Int.left,
            Vector2Int.down + Vector2Int.right, Vector2Int.down + Vector2Int.left
        };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int pos = origin + dir;
            
            if (IsPositionValidForSpecialTile(pos))
            {
                candidates.Add(pos);
            }
        }
        
        // If we found at least one valid position, return a random one
        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }
        
        // If no adjacent positions work, check further positions in increasing radius
        for (int radius = 2; radius <= Mathf.Max(width, height); radius++)
        {
            candidates.Clear();
            
            // Check positions in a square with the current radius
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    // Only check positions on the perimeter of the square
                    if (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius)
                    {
                        Vector2Int pos = origin + new Vector2Int(x, y);
                        
                        if (IsPositionValidForSpecialTile(pos))
                        {
                            candidates.Add(pos);
                        }
                    }
                }
            }
            
            if (candidates.Count > 0)
            {
                return candidates[Random.Range(0, candidates.Count)];
            }
        }
        
        // If still no valid positions, check every cell on the board
        candidates.Clear();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                
                if (IsPositionValidForSpecialTile(pos))
                {
                    candidates.Add(pos);
                }
            }
        }
        
        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }
        
        // If we get here, no valid position was found
        return null;
    }

    /// <summary>
    /// Checks if a position is valid for spawning a special tile.
    /// </summary>
    private static bool IsPositionValidForSpecialTile(Vector2Int pos)
    {
        // Basic checks
        if (!BoardManager.Instance.IsWithinBounds(pos)) return false;
        if (!global::BoardManager.Instance.IsCellEmpty(pos)) return false;
        
        // Advanced check: Is there already a special tile here?
        SpecialTile existingTile = SpecialTileManager.Instance.GetSpecialTileAtPosition(pos);
        if (existingTile != null) return false;
        
        // Check if there are any colliders at this position
        Vector2 worldPos = BoardManager.Instance.GetWorldPosition(pos);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.4f); // Use a smaller radius to avoid edge cases
        
        if (colliders.Length > 0)
        {
            foreach (var collider in colliders)
            {
                // Skip triggers
                if (collider.isTrigger) continue;
                
                // Check if it's a special tile or regular tile
                if (collider.GetComponent<Tile>() != null || collider.GetComponent<SpecialTile>() != null)
                {
                    return false; // Position is occupied by a tile
                }
            }
        }
        
        // Position passed all checks and is valid
        return true;
    }
}
