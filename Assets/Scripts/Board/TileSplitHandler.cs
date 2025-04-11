using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Add this namespace for Enumerable extension methods

public class TileSplitHandler : MonoBehaviour
{
    // Singleton instance
    public static TileSplitHandler Instance;
    
    private static List<Vector2Int> registeredTilesToSplit = new List<Vector2Int>();
    
    [SerializeField] private ParticleSystem splitEffect;
    [SerializeField] [Range(0f, 1f)] private float specialTileSpawnChance = Constants.SPECIAL_TILE_CHANCE;

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
    /// Register tiles that need to be split, called from PostMergeEvaluationState
    /// </summary>
    public static void RegisterTilesToSplit(List<Vector2Int> positions)
    {
        // CRITICAL FIX: Don't clear existing registrations unless explicitly requested
        // This prevents losing high-value tile positions during state transitions
        if (positions == null || positions.Count == 0)
        {
            Debug.LogWarning("TileSplitHandler: RegisterTilesToSplit called with empty positions list");
            return;
        }
        
        // Check for duplicates and merge with existing registrations
        bool added = false;
        foreach (var pos in positions)
        {
            if (!registeredTilesToSplit.Contains(pos))
            {
                registeredTilesToSplit.Add(pos);
                added = true;
                Debug.Log($"TileSplitHandler: Added position {pos} to split registry");
            }
        }
        
        if (added)
        {
            Debug.LogWarning($"TileSplitHandler: Now tracking {registeredTilesToSplit.Count} positions for splitting");
        }
        else
        {
            Debug.Log($"TileSplitHandler: No new positions to add, maintaining {registeredTilesToSplit.Count} registered positions");
        }
    }

    // Add a new method to force clear and set positions
    public static void ClearAndSetTilesToSplit(List<Vector2Int> positions)
    {
        registeredTilesToSplit.Clear();
        registeredTilesToSplit.AddRange(positions);
        Debug.Log($"TileSplitHandler: Cleared previous registry and registered {positions.Count} tiles for splitting");
    }

    /// <summary>
    /// Get all registered tiles to split
    /// </summary>
    public static List<Vector2Int> GetTilesToSplit()
    {
        return new List<Vector2Int>(registeredTilesToSplit);
    }

    /// <summary>
    /// Clear all registered tiles after processing
    /// </summary>
    public static void ClearRegisteredTiles()
    {
        registeredTilesToSplit.Clear();
    }
    
    /// <summary>
    /// Performs a split operation on the specified tile at the given position.
    /// </summary>
    public static void PerformSplitOperation(Tile tile, Vector2Int originalPosition)
    {
        if (tile == null || !BoardManager.Instance.IsWithinBounds(originalPosition))
        {
            Debug.LogError($"TileSplitHandler: Invalid split operation. Tile: {tile}, Position: {originalPosition}");
            return;
        }

        // CRITICAL FIX: Add additional validation that we're splitting a valid tile
        if (tile.number <= 12)
        {
            Debug.LogError($"TileSplitHandler: Attempted to split tile with value {tile.number} <= 12. This should not happen!");
            return;
        }

        Debug.LogWarning($"TileSplitHandler: Splitting high-value tile at {originalPosition} with value {tile.number}.");

        int originalValue = tile.number;
        Color originalColor = tile.tileColor;

        // Store the reference to the GameObject before destruction
        GameObject tileObject = tile.gameObject;

        // IMPROVED LOGIC: Calculate split count based on tile value
        // For extremely high values, create more tiles to distribute the values better
        int baseSplitCount = Mathf.Max(2, Mathf.FloorToInt(originalValue / 8f));
        int maxSplitCount = Mathf.Min(5, baseSplitCount + 1); // Cap at 5 splits
        int minSplitCount = Mathf.Min(2, maxSplitCount); // Ensure at least 2 splits
        
        // For very high values (24+), ensure we use more splits
        if (originalValue >= 24) {
            minSplitCount = Mathf.Min(3, maxSplitCount);
        }
        if (originalValue >= 36) {
            minSplitCount = Mathf.Min(4, maxSplitCount);
        }
        
        int splitCount = Random.Range(minSplitCount, maxSplitCount + 1);
        
        Debug.Log($"TileSplitHandler: Value {originalValue} will be split into {splitCount} tiles (min:{minSplitCount}, max:{maxSplitCount})");
        
        List<Vector2Int> availablePositions = FindSplitPositions(originalPosition);

        // Adjust split count based on available positions
        splitCount = Mathf.Min(splitCount, availablePositions.Count);
        if (splitCount < 2)
        {
            Debug.LogWarning("TileSplitHandler: Not enough available positions to split the tile.");

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

            Debug.Log($"TileSplitHandler: Creating tile at {spawnPos} with value {value} and color {randomColor}.");
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
    /// Additionally guarantees that at least 2 tiles have values lower than 6.
    /// </summary>
    private static List<int> GenerateSplitValues(int targetSum, int count, int maxValue = int.MaxValue)
    {
        List<int> values = new List<int>();
        int remaining = targetSum;
        
        // IMPROVED: The minimum low-value threshold based on the split count
        // For more splits, we can use higher low value thresholds
        int lowValueThreshold = 6;
        if (count >= 4) {
            lowValueThreshold = Mathf.Min(8, maxValue - 1); // For 4+ splits, threshold can be higher
        }
        
        // First, ensure we have enough values by giving each tile at least 1
        for (int i = 0; i < count; i++)
        {
            values.Add(1);
        }
        remaining -= count;
        
        // If we have less remaining than count, we can safely distribute
        if (remaining <= 0)
            return values;

        // Ensure at least 2 tiles have values less than the threshold (if possible)
        int lowValueTilesNeeded = 2;
        List<int> lowValueIndices = new List<int>();

        // IMPROVED DISTRIBUTION: Calculate the base value to distribute more evenly for high values
        int baseDistribution = 0;
        if (remaining >= count * 2) {
            // If we have enough value to distribute, give each tile some base value
            baseDistribution = Mathf.Min(remaining / count, maxValue - 1);
            
            // For each tile, add the base distribution
            for (int i = 0; i < count; i++) {
                int additionalValue = baseDistribution;
                values[i] += additionalValue;
                remaining -= additionalValue;
            }
        }

        // Calculate the maximum we can add to each tile to stay under maxValue
        List<int> maxAdditions = new List<int>();
        for (int i = 0; i < count; i++)
        {
            maxAdditions.Add(maxValue - values[i]);
        }

        // First pass: Allocate values for low-value tiles (less than threshold)
        for (int i = 0; i < count && lowValueTilesNeeded > 0; i++)
        {
            // Maximum we can add while keeping this tile under lowValueThreshold
            int maxLowAddition = Mathf.Min(lowValueThreshold - values[i] - 1, maxAdditions[i]);
            
            if (maxLowAddition > 0)
            {
                // Decide how much to add to this tile (between 1 and maxLowAddition)
                int toAdd = Mathf.Min(remaining, Random.Range(1, maxLowAddition + 1));
                
                values[i] += toAdd;
                maxAdditions[i] -= toAdd;
                remaining -= toAdd;
                
                lowValueIndices.Add(i);
                lowValueTilesNeeded--;
                
                Debug.Log($"TileSplitHandler: Allocated low value {values[i]} for tile {i}");
            }
        }

        // If we couldn't create enough low-value tiles with the above method,
        // just mark the lowest possible tiles as our low-value tiles
        if (lowValueTilesNeeded > 0 && count >= 2)
        {
            Debug.Log("TileSplitHandler: Couldn't create enough low-value tiles in first pass. Ensuring the lowest tiles are tracked.");
            
            // Clear existing low value indices
            lowValueIndices.Clear();
            
            // Create a list of indices sorted by current value
            List<int> sortedIndices = Enumerable.Range(0, count).ToList();
            sortedIndices.Sort((a, b) => values[a].CompareTo(values[b]));
            
            // Take the two lowest value tiles
            for (int i = 0; i < Mathf.Min(2, count) && lowValueTilesNeeded > 0; i++)
            {
                lowValueIndices.Add(sortedIndices[i]);
                lowValueTilesNeeded--;
            }
        }

        // Distribute remaining points, respecting maxValue and avoiding low value tiles if needed
        while (remaining > 0)
        {
            // Check if we still have tiles that can accept more points
            bool canDistribute = maxAdditions.Exists(max => max > 0);
            if (!canDistribute)
            {
                Debug.LogWarning($"TileSplitHandler: Cannot distribute remaining {remaining} points while keeping all tiles under {maxValue}.");
                break;
            }
            
            // Prioritize tiles that are not marked as low-value tiles
            List<int> priorityIndices = new List<int>();
            for (int i = 0; i < count; i++)
            {
                if (!lowValueIndices.Contains(i) && maxAdditions[i] > 0)
                {
                    priorityIndices.Add(i);
                }
            }
            
            // If we have priority indices (non-low-value tiles), use them
            List<int> distributionIndices = priorityIndices.Count > 0 ? priorityIndices : 
                Enumerable.Range(0, count).Where(i => maxAdditions[i] > 0).ToList();
            
            if (distributionIndices.Count == 0)
            {
                Debug.LogWarning("TileSplitHandler: No valid tiles for further distribution.");
                break;
            }
            
            // Randomly select a tile to add a point to
            int idx = distributionIndices[Random.Range(0, distributionIndices.Count)];
            
            // Add one point and reduce remaining
            values[idx]++;
            maxAdditions[idx]--;
            remaining--;
            
            // If this tile is now too high for a low value, remove it from that list
            if (lowValueIndices.Contains(idx) && values[idx] >= lowValueThreshold)
            {
                lowValueIndices.Remove(idx);
                Debug.LogWarning($"TileSplitHandler: Tile {idx} is no longer a low-value tile with value {values[idx]}");
            }
        }
        
        // If we still have remaining value but couldn't distribute it, log a warning
        if (remaining > 0)
        {
            Debug.LogWarning($"TileSplitHandler: Couldn't distribute all value. Original: {targetSum}, Sum of splits: {targetSum - remaining}");
            
            // Last effort - add remaining value to the first available non-low-value tile
            List<int> nonLowValueIndices = Enumerable.Range(0, count).Where(i => !lowValueIndices.Contains(i)).ToList();
            if (nonLowValueIndices.Count > 0)
            {
                int idx = nonLowValueIndices[0];
                Debug.LogWarning($"TileSplitHandler: Adding remaining {remaining} to non-low-value tile {idx}");
                values[idx] += remaining;
            }
            // If all tiles are low-value, add to the first tile
            else if (values.Count > 0)
            {
                Debug.LogWarning($"TileSplitHandler: Adding remaining {remaining} to first tile, which might exceed maxValue!");
                values[0] += remaining;
            }
        }
        
        // Validate that we have at least 2 low-value tiles (if we have enough tiles)
        if (count >= 2)
        {
            int lowValueCount = values.Count(v => v < lowValueThreshold);
            if (lowValueCount < 2)
            {
                Debug.LogWarning($"TileSplitHandler: Only {lowValueCount} low-value tiles created. Attempting to fix...");
                
                // Force adjust some values if needed
                List<int> indices = Enumerable.Range(0, count).ToList();
                indices.Sort((a, b) => values[a].CompareTo(values[b])); // Sort by value ascending
                
                // Force the two lowest tiles to be under the threshold
                for (int i = 0; i < Mathf.Min(2, count); i++)
                {
                    int idx = indices[i];
                    if (values[idx] >= lowValueThreshold)
                    {
                        int excess = values[idx] - (lowValueThreshold - 1);
                        values[idx] = lowValueThreshold - 1;
                        
                        // Try to redistribute the excess
                        for (int j = count - 1; j >= 0 && excess > 0; j--)
                        {
                            int highestIdx = indices[j];
                            if (highestIdx != idx && values[highestIdx] < maxValue)
                            {
                                int canAdd = Mathf.Min(excess, maxValue - values[highestIdx]);
                                values[highestIdx] += canAdd;
                                excess -= canAdd;
                            }
                        }
                        
                        Debug.Log($"TileSplitHandler: Adjusted tile {idx} to value {values[idx]}");
                    }
                }
            }
        }
        
        // Log the split operation details
        Debug.Log($"TileSplitHandler: Split tile with value {targetSum} into {count} tiles. " +
                  $"Base distribution: {baseDistribution}, Low value threshold: {lowValueThreshold}");
        
        // Log the final distribution
        Debug.Log($"TileSplitHandler: Final split values: {string.Join(", ", values)}");
        
        return values;
    }

    private static void CreateTileAtPosition(GameObject tilePrefab, Vector2Int position, int value, Color color)
    {
        // Ensure tilePrefab is assigned
        if (tilePrefab == null)
        {
            Debug.LogError("TileSplitHandler: Tile prefab is not assigned. Cannot create tiles.");
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
            Debug.LogError("TileSplitHandler: Spawned tile does not have a Tile component.");
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
            Debug.Log($"TileSplitHandler: Attempting to spawn a random special tile near {splitPosition}.");
            
            // First find an empty position for spawning
            Vector2Int? spawnPosition = FindEmptyPositionForSpecialTile(splitPosition);
            
            if (!spawnPosition.HasValue)
            {
                Debug.LogWarning("TileSplitHandler: Could not find valid position for special tile spawn after split.");
                return;
            }
            
            Debug.Log($"TileSplitHandler: Found valid position {spawnPosition.Value} for special tile spawn.");
            SpecialTileManager.Instance?.SpawnSpecialTile(spawnPosition.Value, "Random");
        }
        else
        {
            Debug.Log("TileSplitHandler: Random chance did not trigger special tile spawn.");
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
    
    /// <summary>
    /// Checks if a tile should be split based on its value.
    /// </summary>
    public static bool ShouldSplitTile(Tile tile)
    {
        // Tiles with value greater than 12 will be split
        return tile != null && tile.number > 12;
    }
    
    /// <summary>
    /// Finds all tiles that need splitting on the board.
    /// </summary>
    public static List<Vector2Int> FindTilesToSplit()
    {
        List<Vector2Int> tilesToSplit = new List<Vector2Int>();
        
        if (BoardManager.Instance == null) return tilesToSplit;

        for (int x = 0; x < BoardManager.Instance.width; x++)
        {
            for (int y = 0; y < BoardManager.Instance.height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                Tile tile = BoardManager.Instance.GetTileAtPosition(position);
                
                if (tile != null && ShouldSplitTile(tile))
                {
                    tilesToSplit.Add(position);
                    Debug.Log($"TileSplitHandler: Found tile at {position} with value {tile.number} that should be split");
                }
            }
        }
        
        return tilesToSplit;
    }
}