using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Add this to enable LINQ methods like Sum

public class SpecialTileManager : MonoBehaviour
{
    public static SpecialTileManager Instance;

    [SerializeField]
    private List<SpecialTile> specialTilePrefabs; // List of special tile prefabs

    private Dictionary<string, SpecialTile> specialTilePrefabMap = new Dictionary<string, SpecialTile>();
    private List<SpecialTile> activeSpecialTiles = new List<SpecialTile>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePrefabMap();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initializes the prefab map for quick lookup by ability name.
    /// </summary>
    public void InitializePrefabMap() // Changed from private to public
    {
        foreach (var prefab in specialTilePrefabs)
        {
            if (prefab != null && !specialTilePrefabMap.ContainsKey(prefab.specialAbilityName))
            {
                specialTilePrefabMap.Add(prefab.specialAbilityName, prefab);
                Debug.Log($"SpecialTileManager: Registered special tile prefab '{prefab.specialAbilityName}'.");
            }
        }

        // Ensure all expected special tiles are included
        string[] expectedTiles = { "Blaster", "Freeze", "Doubler", "Painter" };
        foreach (string tileName in expectedTiles)
        {
            if (!specialTilePrefabMap.ContainsKey(tileName))
            {
                Debug.LogError($"SpecialTileManager: Missing prefab for special tile '{tileName}'. Ensure it is assigned in the inspector.");
            }
        }
    }

    /// <summary>
    /// Registers a special tile when it is spawned.
    /// </summary>
    public void RegisterSpecialTile(SpecialTile specialTile)
    {
        if (!activeSpecialTiles.Contains(specialTile))
        {
            activeSpecialTiles.Add(specialTile);
            Debug.Log($"SpecialTileManager: Registered special tile '{specialTile.specialAbilityName}'.");
        }
    }

    /// <summary>
    /// Unregisters a special tile when it is destroyed.
    /// </summary>
    public void UnregisterSpecialTile(SpecialTile specialTile)
    {
        if (activeSpecialTiles.Contains(specialTile))
        {
            activeSpecialTiles.Remove(specialTile);
            // Remove reference to BoardManager.UnregisterSpecialTile as it doesn't exist
            Debug.Log($"SpecialTileManager: Unregistered special tile '{specialTile.specialAbilityName}'.");
        }
    }

    /// <summary>
    /// Spawns a special tile at the specified position with the given ability name.
    /// </summary>
    public void SpawnSpecialTile(Vector2Int position, string abilityName)
    {
        // If no specific ability name is provided, get a weighted random one
        if (string.IsNullOrEmpty(abilityName) || abilityName == "Random")
        {
            abilityName = GetWeightedRandomCommonSpecialTile();
        }

        Debug.Log($"SpecialTileManager: Attempting to spawn special tile '{abilityName}' at {position}.");

        // Multi-layer validation to ensure the position is valid and empty
        if (!ValidateSpawnPosition(position, out Vector2Int validPosition))
        {
            Debug.LogError($"SpecialTileManager: Failed to find valid position for special tile '{abilityName}'. Aborting spawn.");
            return;
        }
        
        position = validPosition;

        // Final safety check - is the cell STILL empty?
        if (!global::BoardManager.Instance.IsCellEmpty(position))
        {
            Debug.LogError($"SpecialTileManager: CRITICAL VALIDATION FAILURE - Cell at {position} is STILL occupied after all validation. Cannot spawn special tile.");
            return;
        }

        // Extra check - are there any existing special tiles at this position?
        foreach (var existingTile in activeSpecialTiles)
        {
            if (existingTile == null) continue;
            
            Vector2Int existingPos = BoardManager.Instance.GetGridPositionFromWorldPosition(existingTile.transform.position);
            if (existingPos == position)
            {
                Debug.LogError($"SpecialTileManager: Found another special tile '{existingTile.specialAbilityName}' already at position {position}. Aborting spawn.");
                return;
            }
        }

        if (specialTilePrefabMap.TryGetValue(abilityName, out SpecialTile prefab))
        {
            Vector2 worldPosition = BoardManager.Instance.GetWorldPosition(position);
            
            // Check if there's already something at the world position
            Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPosition, 0.1f);
            if (colliders.Length > 0)
            {
                Debug.LogError($"SpecialTileManager: Physical collision detected at {worldPosition}! Found {colliders.Length} colliders. Aborting spawn.");
                foreach (var collider in colliders)
                {
                    Debug.LogError($"  - Collider: {collider.name} on GameObject: {collider.gameObject.name}");
                }
                return;
            }
            
            SpecialTile specialTile = Instantiate(prefab, worldPosition, Quaternion.identity, BoardManager.Instance.transform);
            activeSpecialTiles.Add(specialTile);
            BoardManager.Instance.MarkCellAsOccupied(position);
            // Instead of calling BoardManager.RegisterSpecialTile, simply update local state
            RegisterSpecialTile(specialTile);

            Debug.Log($"SpecialTileManager: Successfully spawned special tile '{abilityName}' at {position}.");
        }
        else
        {
            Debug.LogError($"SpecialTileManager: No prefab found for special tile '{abilityName}'.");
        }
    }

    /// <summary>
    /// Validates a spawn position and finds an alternative if needed.
    /// </summary>
    /// <param name="position">The original position to try</param>
    /// <param name="validPosition">The output valid position (original or alternative)</param>
    /// <returns>True if a valid position was found, false otherwise</returns>
    private bool ValidateSpawnPosition(Vector2Int position, out Vector2Int validPosition)
    {
        validPosition = position;
        
        // Check bounds and emptiness
        if (!BoardManager.Instance.IsWithinBounds(position) || !global::BoardManager.Instance.IsCellEmpty(position))
        {
            Debug.LogWarning($"SpecialTileManager: Position {position} is invalid or occupied. Searching for alternative...");
            
            // Try to find an alternative
            Vector2Int? alternativePos = FindTrueEmptyPosition(position);
            if (!alternativePos.HasValue)
            {
                Debug.LogError("SpecialTileManager: No valid alternative position found anywhere on the board.");
                return false;
            }
            
            validPosition = alternativePos.Value;
        }
        
        // Double-check this position
        if (!BoardManager.Instance.IsWithinBounds(validPosition))
        {
            Debug.LogError($"SpecialTileManager: Position {validPosition} is out of bounds. This should never happen!");
            return false;
        }
        
        if (!global::BoardManager.Instance.IsCellEmpty(validPosition))
        {
            Debug.LogError($"SpecialTileManager: Position {validPosition} is occupied despite validation! Tile there: {BoardManager.Instance.GetTileAtPosition(validPosition)?.number.ToString() ?? "none"}");
            
            // Check for special tiles too
            SpecialTile existingTile = GetSpecialTileAtPosition(validPosition);
            if (existingTile != null)
            {
                Debug.LogError($"SpecialTileManager: Found existing special tile '{existingTile.specialAbilityName}' at {validPosition}");
            }
            
            return false;
        }
        
        // Check against all active special tiles (for redundancy)
        foreach (var tile in activeSpecialTiles)
        {
            if (tile == null) continue;
            
            Vector2Int tilePos = BoardManager.Instance.GetGridPositionFromWorldPosition(tile.transform.position);
            if (tilePos == validPosition)
            {
                Debug.LogError($"SpecialTileManager: Position {validPosition} already has special tile '{tile.specialAbilityName}'. Finding another position...");
                
                // Try again with another position
                Vector2Int? newAlternativePos = FindTrueEmptyPosition(validPosition, true); // true = exclude existing special tile positions
                if (!newAlternativePos.HasValue)
                {
                    return false;
                }
                
                validPosition = newAlternativePos.Value;
                break;
            }
        }
        
        Debug.Log($"SpecialTileManager: Position {validPosition} validated as empty and within bounds.");
        return true;
    }

    /// <summary>
    /// Finds a truly empty position that has no special tiles or regular tiles
    /// </summary>
    private Vector2Int? FindTrueEmptyPosition(Vector2Int startPosition, bool avoidExistingSpecialTiles = false)
    {
        // First check adjacent positions
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left,
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 1)
        };
        
        // Check immediate neighbors first
        foreach (Vector2Int dir in directions)
        {
            Vector2Int pos = startPosition + dir;
            bool emptyCheck1 = global::BoardManager.Instance.IsCellEmpty(pos); // resolved ambiguity
            if (IsTrulyEmpty(pos, avoidExistingSpecialTiles))
            {
                return pos;
            }
        }
        
        // Next try a spiral search pattern for nearby positions
        for (int radius = 2; radius <= 5; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    // Only check positions on the perimeter
                    if (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius)
                    {
                        Vector2Int pos = new Vector2Int(startPosition.x + x, startPosition.y + y);
                        if (IsTrulyEmpty(pos, avoidExistingSpecialTiles))
                        {
                            return pos;
                        }
                    }
                }
            }
        }
        
        // If needed, check all grid positions
        for (int x = 0; x < BoardManager.Instance.width; x++)
        {
            for (int y = 0; y < BoardManager.Instance.height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (IsTrulyEmpty(pos, avoidExistingSpecialTiles))
                {
                    return pos;
                }
            }
        }
        
        return null; // No position found
    }

    /// <summary>
    /// Checks if a position is truly empty (no special tiles, no regular tiles)
    /// </summary>
    private bool IsTrulyEmpty(Vector2Int position, bool avoidExistingSpecialTiles = false)
    {
        // Basic checks
        if (!BoardManager.Instance.IsWithinBounds(position)) return false;
        if (!global::BoardManager.Instance.IsCellEmpty(position)) return false;
        
        // Extra check for special tiles if required
        if (avoidExistingSpecialTiles)
        {
            foreach (var tile in activeSpecialTiles)
            {
                if (tile == null) continue;
                
                Vector2Int tilePos = BoardManager.Instance.GetGridPositionFromWorldPosition(tile.transform.position);
                if (tilePos == position)
                {
                    return false; // Position has a special tile
                }
            }
        }
        
        return true;
    }

    /// <summary>
    /// Spawns a random special tile near a given position, with an increased chance for common tiles.
    /// </summary>
    public void SpawnRandomSpecialTile(Vector2Int splitPosition)
    {
        string selectedTileType = GetWeightedRandomCommonSpecialTile();
        
        // Force Doubler in test mode 50% of the time
        if (Constants.TESTING_MODE && Random.value < 0.5f)
        {
            selectedTileType = "Doubler";
            Debug.Log("SpecialTileManager: Testing mode forced a Doubler tile to spawn.");
        }
        
        // Validate the position before spawning
        if (!ValidateSpawnPosition(splitPosition, out Vector2Int validPosition))
        {
            // Try to find a nearby position
            Vector2Int? nearbyPosition = FindRandomEmptyCellNear(splitPosition);
            
            if (!nearbyPosition.HasValue)
            {
                Debug.LogWarning("SpecialTileManager: Could not find any valid empty cell for special tile.");
                return;
            }
            
            validPosition = nearbyPosition.Value;
        }
        
        Debug.Log($"SpecialTileManager: Spawning {selectedTileType} at validated position {validPosition}");
        SpawnSpecialTile(validPosition, selectedTileType);
    }

    /// <summary>
    /// Gets a weighted random common special tile type to increase the likelihood of spawning certain tiles.
    /// </summary>
    private string GetWeightedRandomCommonSpecialTile()
    {
        string[] specialTileTypes = { "Blaster", "Freeze", "Doubler", "Painter" };
        
        // Handle empty array case
        if (specialTileTypes.Length == 0)
        {
            Debug.LogError("SpecialTileManager: No special tile types defined!");
            return "Doubler"; // Ultimate fallback
        }
        
        // Default to first tile type as fallback
        string selectedTile = specialTileTypes[0];
        
        // Use testing weights if in testing mode
        float[] weights;
        if (Constants.TESTING_MODE)
        {
            weights = new float[] { 
                Constants.BLASTER_WEIGHT,
                Constants.FREEZE_WEIGHT,
                Constants.DOUBLER_WEIGHT, // Higher weight for Doubler during testing
                Constants.PAINTER_WEIGHT
            };
            Debug.Log("SpecialTileManager: Using testing mode weights for special tiles. Doubler has higher chance.");
        }
        else
        {
            weights = new float[] { 0.25f, 0.25f, 0.25f, 0.25f }; // Equal weights by default
        }
        
        // Make sure weights array matches the specialTileTypes array length
        if (weights.Length != specialTileTypes.Length)
        {
            Debug.LogWarning($"SpecialTileManager: Weights array length ({weights.Length}) doesn't match tile types array length ({specialTileTypes.Length}). Using first available tile type.");
            return selectedTile;
        }

        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            totalWeight += weight;
        }

        // If total weight is zero or very small, return the default tile
        if (totalWeight <= Mathf.Epsilon)
        {
            Debug.LogWarning("SpecialTileManager: Total weight is zero, using first tile type.");
            return selectedTile;
        }

        // Try weighted random selection
        float randomValue = Random.value * totalWeight;
        float cumulativeWeight = 0f;
        
        for (int i = 0; i < specialTileTypes.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
            {
                selectedTile = specialTileTypes[i];
                Debug.Log($"SpecialTileManager: Selected {selectedTile} tile to spawn based on weight.");
                break; // Use break instead of return so we reach the end of the method
            }
        }
        
        // This line is now properly reachable since we use break above instead of return
        return selectedTile;
    }

    /// <summary>
    /// Gets a random common special tile type.
    /// </summary>
    private string GetRandomCommonSpecialTile()
    {
        string[] commonSpecialTiles = { "Blaster", "Freeze", "Doubler", "Painter" };
        return commonSpecialTiles[Random.Range(0, commonSpecialTiles.Length)];
    }

    /// <summary>
    /// Finds a random empty cell near a given position.
    /// </summary>
    private Vector2Int? FindRandomEmptyCellNear(Vector2Int origin)
    {
        List<Vector2Int> nearbyPositions = new List<Vector2Int>();

        // Check orthogonal and diagonal positions
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            Vector2Int.up + Vector2Int.left, Vector2Int.up + Vector2Int.right,
            Vector2Int.down + Vector2Int.left, Vector2Int.down + Vector2Int.right
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int newPosition = origin + direction;
            bool emptyCheck2 = global::BoardManager.Instance.IsCellEmpty(newPosition); // resolved ambiguity
            if (BoardManager.Instance.IsWithinBounds(newPosition) && global::BoardManager.Instance.IsCellEmpty(newPosition))
            {
                nearbyPositions.Add(newPosition);
            }
        }

        if (nearbyPositions.Count > 0)
        {
            // Shuffle and return a random position
            ShuffleList(nearbyPositions);
            return nearbyPositions[0];
        }

        return null; // No valid positions found
    }

    /// <summary>
    /// Shuffles a list of positions.
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Finds an alternative position for spawning a special tile if the original position is invalid.
    /// </summary>
    private Vector2Int? FindAlternativePosition(Vector2Int originalPosition)
    {
        // First try adjacent positions (starting with orthogonal, then diagonal)
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left,
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 1)
        };
        
        // Check immediate neighbors first
        foreach (Vector2Int dir in directions)
        {
            Vector2Int newPos = originalPosition + dir;
            var status = global::BoardManager.Instance.IsCellEmpty(newPos); // resolved ambiguity
            if (BoardManager.Instance.IsWithinBounds(newPos) && global::BoardManager.Instance.IsCellEmpty(newPos))
            {
                Debug.Log($"SpecialTileManager: Found empty adjacent cell at {newPos}");
                return newPos;
            }
        }
        
        // Next, check cells with Manhattan distance 2
        foreach (Vector2Int dir1 in directions.Take(4)) // Use only orthogonal directions
        {
            Vector2Int newPos = originalPosition + dir1 * 2;
            if (BoardManager.Instance.IsWithinBounds(newPos) && global::BoardManager.Instance.IsCellEmpty(newPos))
            {
                return newPos;
            }
        }

        // If none of the nearby cells work, try any empty cell on the board
        // Instead of using GetAllEmptyCells(), use emptyCells directly
        var emptyCells = GetAllEmptyCellsFromBoard();
        if (emptyCells.Count > 0)
        {
            return emptyCells.First(); // Return the first empty cell found
        }

        return null; // No valid position found
    }

    /// <summary>
    /// Gets all empty cells from the board by checking each position.
    /// </summary>
    private List<Vector2Int> GetAllEmptyCellsFromBoard()
    {
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        
        for (int x = 0; x < BoardManager.Instance.width; x++)
        {
            for (int y = 0; y < BoardManager.Instance.height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                bool check1 = global::BoardManager.Instance.IsCellEmpty(pos); // resolved ambiguity
                if (global::BoardManager.Instance.IsCellEmpty(pos))
                {
                    emptyPositions.Add(pos);
                }
            }
        }
        
        return emptyPositions;
    }

    /// <summary>
    /// Retrieves the special tile at the specified grid position, if any.
    /// </summary>
    public SpecialTile GetSpecialTileAtPosition(Vector2Int position)
    {
        // Clean up any destroyed special tiles from the list
        activeSpecialTiles.RemoveAll(tile => tile == null);

        foreach (var specialTile in activeSpecialTiles)
        {
            Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(specialTile.transform.position);
            if (tilePosition == position)
            {
                return specialTile;
            }
        }
        return null;
    }

    /// <summary>
    /// Activates all special tiles on the board.
    /// </summary>
    public void ActivateAllSpecialTiles()
    {
        Debug.Log("SpecialTileManager: Activating all special tiles.");
        foreach (var specialTile in new List<SpecialTile>(activeSpecialTiles))
        {
            Debug.Log($"SpecialTileManager: Activating special tile '{specialTile.specialAbilityName}'.");
            specialTile.Activate();
        }
    }

    /// <summary>
    /// Clears all special tiles from the board.
    /// </summary>
    public void ClearAllSpecialTiles()
    {
        foreach (var specialTile in new List<SpecialTile>(activeSpecialTiles))
        {
            Destroy(specialTile.gameObject);
        }
        activeSpecialTiles.Clear();
    }

    /// <summary>
    /// Checks if there are any active special tiles on the board.
    /// </summary>
    public bool HasActiveSpecialTiles()
    {
        return activeSpecialTiles.Count > 0;
    }
}
