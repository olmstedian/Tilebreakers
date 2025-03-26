using UnityEngine;
using System.Collections.Generic;

public class TileMerger : MonoBehaviour
{
    /// <summary>
    /// Merges movingTile into staticTile if they have matching colors.
    /// The staticTile's number increases by movingTile's number.
    /// Splits the tile if it exceeds the threshold after merging.
    /// </summary>
    /// <param name="staticTile">The tile that remains in place</param>
    /// <param name="movingTile">The tile being merged (will be destroyed)</param>
    /// <param name="splitThreshold">Value at which tiles split (defaults to 12)</param>
    /// <returns>True if merge was successful, false otherwise</returns>
    public static bool MergeTiles(Tile staticTile, Tile movingTile, int splitThreshold = 12)
    {
        if (staticTile == null || movingTile == null || staticTile == movingTile) return false;

        staticTile.ClearSelectionState();
        movingTile.ClearSelectionState();

        if (!ColorMatch(staticTile.tileColor, movingTile.tileColor)) return false;

        staticTile.number += movingTile.number;
        staticTile.UpdateVisuals();
        Object.Destroy(movingTile.gameObject);

        staticTile.ClearSelectionState();

        if (staticTile.number > splitThreshold)
        {
            SplitTile(staticTile, BoardManager.Instance.GetGridPositionFromWorldPosition(staticTile.transform.position));
        }

        return true;
    }

    private static bool ColorMatch(Color a, Color b)
    {
        const float tolerance = 0.01f;
        return Mathf.Abs(a.r - b.r) < tolerance && 
               Mathf.Abs(a.g - b.g) < tolerance && 
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    /// <summary>
    /// Splits a tile into multiple smaller tiles with random values and colors.
    /// The sum of the new tile values equals the original tile value.
    /// New tiles are placed in random available positions on the board.
    /// </summary>
    /// <param name="tile">The tile to split</param>
    /// <param name="originalPosition">Grid position of the original tile</param>
    private static void SplitTile(Tile tile, Vector2Int originalPosition)
    {
        int originalValue = tile.number;
        Color originalColor = tile.tileColor;
        GameObject originalTilePrefab = tile.gameObject;
        
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
        
        // Create new tiles at the random positions
        for (int i = 0; i < splitCount; i++)
        {
            Vector2Int spawnPos = availablePositions[i];
            int value = splitValues[i];
            
            // Choose a random color from the predefined palette
            Color randomColor = BoardManager.Instance.GetRandomTileColor();
            
            // Create new tile
            CreateTileAtPosition(originalTilePrefab, spawnPos, value, randomColor);
        }
    }
    
    /// <summary>
    /// Finds suitable positions for spawning split tiles.
    /// Avoids the original position and tries to find non-adjacent positions.
    /// </summary>
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
    
    /// <summary>
    /// Generates random values that sum up to the target value
    /// </summary>
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
    
    /// <summary>
    /// Creates a new tile at the specified position
    /// </summary>
    private static void CreateTileAtPosition(GameObject tilePrefab, Vector2Int position, int value, Color color)
    {
        // Get world position for spawning the tile
        Vector2 worldPos = BoardManager.Instance.GetWorldPosition(position);
        
        try
        {
            // Create a unique name for debugging
            string tileName = $"SplitTile_{value}_{position.x}_{position.y}";
            
            // First create a proper clone of the prefab
            GameObject newTileObj = Object.Instantiate(
                BoardManager.Instance.tilePrefab, // Use the clean prefab from BoardManager rather than the reference tile
                worldPos,
                Quaternion.identity,
                BoardManager.Instance.transform
            );
            newTileObj.name = tileName;
            
            // Get the tile component and initialize
            Tile newTile = newTileObj.GetComponent<Tile>();
            if (newTile == null)
            {
                Object.Destroy(newTileObj);
                return;
            }
            
            // Initialize the tile properly with the desired values
            newTile.number = value; // Set the value explicitly first
            newTile.Initialize(color, value);
            
            // Ensure the tile has all needed components
            Transform textTransform = newTile.transform.Find("NumberText");
            TMPro.TextMeshPro textComponent = null;
            
            if (textTransform != null)
            {
                textComponent = textTransform.GetComponent<TMPro.TextMeshPro>();
                if (textComponent != null)
                {
                    textComponent.text = value.ToString();
                    textComponent.ForceMeshUpdate();
                }
            }
            
            // Register with the board manager
            BoardManager.Instance.RegisterSplitTile(position, newTile);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating split tile: {e.Message}\n{e.StackTrace}");
        }
    }
    
    /// <summary>
    /// Shuffles a list of positions randomly
    /// </summary>
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
