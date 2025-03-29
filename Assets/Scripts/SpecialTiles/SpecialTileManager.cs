using UnityEngine;
using System.Collections.Generic;

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
    private void InitializePrefabMap()
    {
        foreach (var prefab in specialTilePrefabs)
        {
            if (prefab != null && !specialTilePrefabMap.ContainsKey(prefab.specialAbilityName))
            {
                specialTilePrefabMap.Add(prefab.specialAbilityName, prefab);
                Debug.Log($"SpecialTileManager: Added prefab for ability '{prefab.specialAbilityName}'.");
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
        }
    }

    /// <summary>
    /// Spawns a special tile at the specified position with the given ability name.
    /// </summary>
    public void SpawnSpecialTile(Vector2Int position, string abilityName)
    {
        if (!BoardManager.Instance.IsWithinBounds(position) || !BoardManager.Instance.IsCellEmpty(position))
        {
            Debug.LogWarning($"SpecialTileManager: Cannot spawn special tile at {position}. Cell is either out of bounds or occupied. Attempting to find an alternative position.");

            // Attempt to find an alternative position
            Vector2Int? alternativePosition = FindAlternativePosition(position);
            if (alternativePosition.HasValue)
            {
                position = alternativePosition.Value;
            }
            else
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
            Debug.Log($"SpecialTileManager: Successfully spawned special tile '{abilityName}' at position {position}.");
        }
        else
        {
            Debug.LogError($"SpecialTileManager: No prefab found for ability '{abilityName}'. Ensure it is added to the SpecialTileManager.");
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
        foreach (var specialTile in new List<SpecialTile>(activeSpecialTiles))
        {
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
