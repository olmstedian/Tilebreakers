using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility class for repairing and diagnosing board state issues.
/// </summary>
public class BoardRepairUtility : MonoBehaviour
{
    [SerializeField] private KeyCode repairKey = KeyCode.R;
    [SerializeField] private bool enableDebugRepair = false;
    
    private void Update()
    {
        if (enableDebugRepair && Input.GetKeyDown(repairKey))
        {
            Debug.Log("BoardRepairUtility: Running board repair...");
            RepairBoard();
        }
    }
    
    /// <summary>
    /// Attempts to fix common issues with the board state
    /// </summary>
    public void RepairBoard()
    {
        if (BoardManager.Instance == null)
        {
            Debug.LogError("BoardRepairUtility: BoardManager is null, cannot repair board");
            return;
        }
        
        // Check and fix emptyCells collection
        RepairEmptyCells();
        
        // Fix tile colliders
        RepairTileColliders();
        
        // Reset selection state
        BoardManager.Instance.ClearAllSelectionState();
        
        // Verify all tiles in board array have matching GameObjects
        VerifyBoardArray();
        
        Debug.Log("BoardRepairUtility: Board repair complete!");
    }
    
    private void RepairEmptyCells()
    {
        // Create a new emptyCells collection
        HashSet<Vector2Int> repairedEmptyCells = new HashSet<Vector2Int>();
        
        // Scan the entire board
        for (int x = 0; x < BoardManager.Instance.width; x++)
        {
            for (int y = 0; y < BoardManager.Instance.height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Tile tile = BoardManager.Instance.GetTileAtPosition(pos);
                
                if (tile == null)
                {
                    // Check if this position also has a special tile
                    SpecialTile specialTile = SpecialTileManager.Instance?.GetSpecialTileAtPosition(pos);
                    
                    if (specialTile == null)
                    {
                        // This cell is truly empty
                        repairedEmptyCells.Add(pos);
                    }
                }
            }
        }
        
        // Replace the emptyCells collection with the repaired one
        int oldCount = BoardManager.Instance.emptyCells.Count;
        BoardManager.Instance.emptyCells = repairedEmptyCells;
        
        Debug.Log($"BoardRepairUtility: Repaired emptyCells collection. Before: {oldCount}, After: {repairedEmptyCells.Count}");
    }
    
    private void RepairTileColliders()
    {
        // Find all tiles and ensure their colliders are enabled
        foreach (Tile tile in FindObjectsOfType<Tile>())
        {
            Collider2D collider = tile.GetComponent<Collider2D>();
            if (collider != null && !collider.enabled)
            {
                collider.enabled = true;
                Debug.Log($"BoardRepairUtility: Re-enabled collider on tile at {tile.transform.position}");
            }
        }
        
        // Also check special tiles
        foreach (SpecialTile specialTile in FindObjectsOfType<SpecialTile>())
        {
            Collider2D collider = specialTile.GetComponent<Collider2D>();
            if (collider != null && !collider.enabled)
            {
                collider.enabled = true;
                Debug.Log($"BoardRepairUtility: Re-enabled collider on special tile at {specialTile.transform.position}");
            }
        }
    }
    
    private void VerifyBoardArray()
    {
        // Check each position in the board array
        for (int x = 0; x < BoardManager.Instance.width; x++)
        {
            for (int y = 0; y < BoardManager.Instance.height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Tile tile = BoardManager.Instance.GetTileAtPosition(pos);
                
                if (tile != null)
                {
                    // Verify the tile's GameObject still exists
                    if (tile.gameObject == null)
                    {
                        Debug.LogError($"BoardRepairUtility: Tile at {pos} has null GameObject! Clearing cell.");
                        BoardManager.Instance.ClearCell(pos);
                        BoardManager.Instance.AddToEmptyCells(pos);
                    }
                    else
                    {
                        // Verify the tile's position matches its board position
                        Vector2 worldPos = BoardManager.Instance.GetWorldPosition(pos);
                        if (Vector2.Distance(worldPos, tile.transform.position) > 0.1f)
                        {
                            Debug.LogWarning($"BoardRepairUtility: Tile at {pos} has incorrect world position. Fixing.");
                            tile.transform.position = worldPos;
                        }
                    }
                }
            }
        }
    }
}
