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
            BoardManager.Instance.UnregisterSpecialTile(specialTile); // Unregister the special tile
            Debug.Log($"SpecialTileManager: Unregistered special tile '{specialTile.specialAbilityName}'.");
        }
    }

    /// <summary>
    /// Spawns a special tile at the specified position with the given ability name.
    /// </summary>
    public void SpawnSpecialTile(Vector2Int position, string abilityName)
    {
        Debug.Log($"SpecialTileManager: Attempting to spawn special tile '{abilityName}' at {position}.");

        if (!BoardManager.Instance.IsWithinBounds(position) || !BoardManager.Instance.IsCellEmpty(position))
        {
            Debug.LogWarning($"SpecialTileManager: Cannot spawn special tile '{abilityName}' at {position}. Position is invalid or occupied. Finding alternative position...");
            position = FindAlternativePosition(position) ?? position;

            if (!BoardManager.Instance.IsCellEmpty(position))
            {
                Debug.LogError($"SpecialTileManager: No valid position found to spawn special tile '{abilityName}'.");
                return;
            }
        }

        if (specialTilePrefabMap.TryGetValue(abilityName, out SpecialTile prefab))
        {
            Vector2 worldPosition = BoardManager.Instance.GetWorldPosition(position);
            SpecialTile specialTile = Instantiate(prefab, worldPosition, Quaternion.identity, BoardManager.Instance.transform);
            activeSpecialTiles.Add(specialTile);
            BoardManager.Instance.MarkCellAsOccupied(position);
            BoardManager.Instance.RegisterSpecialTile(specialTile);

            Debug.Log($"SpecialTileManager: Spawned special tile '{abilityName}' at {position}.");
        }
        else
        {
            Debug.LogError($"SpecialTileManager: No prefab found for special tile '{abilityName}'.");
        }
    }

    /// <summary>
    /// Spawns a random special tile near a given position, with an increased chance for common tiles.
    /// </summary>
    public void SpawnRandomSpecialTile(Vector2Int splitPosition)
    {
        string selectedTileType = GetWeightedRandomCommonSpecialTile();
        Vector2Int? spawnPosition = FindRandomEmptyCellNear(splitPosition);
        if (spawnPosition.HasValue)
        {
            SpawnSpecialTile(spawnPosition.Value, selectedTileType);
        }
    }

    /// <summary>
    /// Gets a weighted random common special tile type to increase the likelihood of spawning certain tiles.
    /// </summary>
    private string GetWeightedRandomCommonSpecialTile()
    {
        string[] commonSpecialTiles = { "Blaster", "Freeze", "Doubler", "Painter" };
        float[] weights = { 0.2f, 0.2f, 0.2f, 0.4f }; // Increase weight for PainterTile

        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            totalWeight += weight;
        }

        float randomValue = Random.value * totalWeight;
        float cumulativeWeight = 0f;

        for (int i = 0; i < commonSpecialTiles.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
            {
                return commonSpecialTiles[i];
            }
        }

        return commonSpecialTiles[0]; // Fallback to the first tile
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
            if (BoardManager.Instance.IsWithinBounds(newPosition) && BoardManager.Instance.IsCellEmpty(newPosition))
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
        foreach (Vector2Int direction in DirectionUtils.Orthogonal)
        {
            Vector2Int newPosition = originalPosition + direction;
            if (BoardManager.Instance.IsWithinBounds(newPosition) && BoardManager.Instance.IsCellEmpty(newPosition))
            {
                return newPosition;
            }
        }

        // If no orthogonal positions are available, check the entire board
        foreach (Vector2Int position in BoardManager.Instance.GetAllEmptyCells())
        {
            return position; // Return the first available empty cell
        }

        return null; // No valid position found
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
