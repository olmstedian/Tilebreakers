using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility class to check and fix inconsistencies in the board's cell tracking.
/// Can be called from the Inspector during debugging or programmatically.
/// </summary>
public class CellConsistencyChecker : MonoBehaviour
{
    [SerializeField] private bool autoCheckOnStart = true;
    [SerializeField] private bool fixInconsistencies = true;
    [SerializeField] private bool logResults = true;
    
    private void Start()
    {
        if (autoCheckOnStart)
        {
            Invoke("CheckAndFixBoardConsistency", 1.0f); // Delay to ensure board is initialized
        }
    }
    
    [ContextMenu("Check Board Consistency")]
    public void CheckAndFixBoardConsistency()
    {
        if (BoardManager.Instance == null)
        {
            Debug.LogError("CellConsistencyChecker: BoardManager.Instance is null!");
            return;
        }
        
        int width = BoardManager.Instance.width;
        int height = BoardManager.Instance.height;
        
        int inconsistenciesFound = 0;
        int inconsistenciesFixed = 0;
        
        List<Vector2Int> emptyCellsToAdd = new List<Vector2Int>();
        List<Vector2Int> emptyCellsToRemove = new List<Vector2Int>();
        
        // Check each cell in the grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                bool hasTile = BoardManager.Instance.GetTileAtPosition(pos) != null;
                bool isEmpty = global::BoardManager.Instance.IsCellEmpty(pos); // resolved ambiguity
                bool inEmptyCells = BoardManager.Instance.emptyCells.Contains(pos);
                
                // Check for any physical objects at this position
                Vector2 worldPos = BoardManager.Instance.GetWorldPosition(pos);
                Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.4f);
                bool hasPhysicalTile = false;
                
                foreach (var collider in colliders)
                {
                    if (collider.isTrigger) continue;
                    if (collider.gameObject.CompareTag("Highlight")) continue;
                    
                    Tile tileComp = collider.GetComponent<Tile>();
                    SpecialTile specialTileComp = collider.GetComponent<SpecialTile>();
                    
                    if (tileComp != null || specialTileComp != null)
                    {
                        hasPhysicalTile = true;
                        break;
                    }
                }
                
                // Conditions for inconsistencies
                bool inconsistency1 = hasTile && inEmptyCells; // Cell has tile but is marked as empty
                bool inconsistency2 = !hasTile && !inEmptyCells; // Cell has no tile but is not marked as empty
                bool inconsistency3 = hasPhysicalTile && !hasTile; // Has physical tile object but not in board array
                bool inconsistency4 = hasTile && !hasPhysicalTile; // Has tile in board array but no physical object
                
                if (inconsistency1 || inconsistency2 || inconsistency3 || inconsistency4)
                {
                    inconsistenciesFound++;
                    if (logResults)
                    {
                        Debug.LogWarning($"CellConsistencyChecker: Inconsistency at {pos}: " +
                                        $"hasTile={hasTile}, inEmptyCells={inEmptyCells}, " +
                                        $"hasPhysicalTile={hasPhysicalTile}");
                    }
                    
                    if (fixInconsistencies)
                    {
                        // Fix inconsistency 1: Remove from emptyCells if it has a tile
                        if (inconsistency1)
                        {
                            emptyCellsToRemove.Add(pos);
                        }
                        
                        // Fix inconsistency 2: Add to emptyCells if it has no tile
                        if (inconsistency2)
                        {
                            emptyCellsToAdd.Add(pos);
                        }
                        
                        // Fix inconsistency 3: Register physical tile in board array
                        if (inconsistency3)
                        {
                            foreach (var collider in colliders)
                            {
                                Tile tileComp = collider.GetComponent<Tile>();
                                if (tileComp != null)
                                {
                                    BoardManager.Instance.SetTileAtPosition(pos, tileComp);
                                    emptyCellsToRemove.Add(pos);
                                    if (logResults) Debug.Log($"CellConsistencyChecker: Registered physical tile at {pos}");
                                    break;
                                }
                            }
                        }
                        
                        // Fix inconsistency 4: Remove reference from board array
                        if (inconsistency4)
                        {
                            BoardManager.Instance.ClearCell(pos);
                            emptyCellsToAdd.Add(pos);
                            if (logResults) Debug.Log($"CellConsistencyChecker: Cleared phantom tile reference at {pos}");
                        }
                        
                        inconsistenciesFixed++;
                    }
                }
            }
        }
        
        // Apply the emptyCells changes
        if (fixInconsistencies)
        {
            foreach (Vector2Int pos in emptyCellsToAdd)
            {
                if (!BoardManager.Instance.emptyCells.Contains(pos))
                {
                    BoardManager.Instance.emptyCells.Add(pos);
                }
            }
            
            foreach (Vector2Int pos in emptyCellsToRemove)
            {
                BoardManager.Instance.emptyCells.Remove(pos);
            }
        }
        
        if (logResults)
        {
            Debug.Log($"CellConsistencyChecker: Check complete. " +
                    $"Found {inconsistenciesFound} inconsistencies. " +
                    $"Fixed {inconsistenciesFixed} inconsistencies.");
        }
    }
    
    [ContextMenu("Debug Print Empty Cells")]
    public void PrintEmptyCells()
    {
        if (BoardManager.Instance == null)
        {
            Debug.LogError("CellConsistencyChecker: BoardManager.Instance is null!");
            return;
        }
        
        string cells = string.Join(", ", BoardManager.Instance.emptyCells);
        Debug.Log($"Empty Cells: {cells}");
    }
}
