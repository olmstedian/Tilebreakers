using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// AnimatingState - handles all animations in the game including tile movements and merges.
/// This state ensures animations complete before proceeding to the next game state.
/// </summary>
public class AnimatingState : GameState
{
    private List<Coroutine> activeAnimations = new List<Coroutine>();
    private GameState nextState;
    
    public AnimatingState(GameState nextState)
    {
        this.nextState = nextState;
    }
    
    public override void Enter()
    {
        Debug.Log("AnimatingState: Entering AnimatingState - handling tile animations");
        
        // First restore any tiles with incorrect sorting orders (safety check)
        TileSortingManager.RestoreAllSortingOrders();
        
        // Find all tiles that are currently in the Moving or Merging state
        Tile[] tiles = Object.FindObjectsOfType<Tile>();
        bool hasAnimatingTiles = false;
        int movingCount = 0;
        int mergingCount = 0;
        
        foreach (Tile tile in tiles)
        {
            if (tile.CurrentState == Tile.TileState.Moving)
            {
                hasAnimatingTiles = true;
                movingCount++;
                Debug.Log($"AnimatingState: Found moving tile at {tile.transform.position}");
            }
            else if (tile.CurrentState == Tile.TileState.Merging)
            {
                hasAnimatingTiles = true;
                mergingCount++;
                Debug.Log($"AnimatingState: Found merging tile at {tile.transform.position}");
                
                // Check if this tile has an animator component
                TileAnimator animator = tile.GetComponent<TileAnimator>();
                if (animator != null && !animator.IsPlayingMergeAnimation())
                {
                    Debug.Log($"AnimatingState: Starting merge animation for tile at {tile.transform.position}");
                    animator.PlayMergeAnimation();
                }
            }
        }
        
        Debug.Log($"AnimatingState: Found {movingCount} moving tiles and {mergingCount} merging tiles");
        
        if (!hasAnimatingTiles)
        {
            Debug.Log("AnimatingState: No animating tiles found. Proceeding to next state immediately.");
            GameStateManager.Instance.SetState(nextState);
            return;
        }
        
        // Start a coroutine to wait for animations to complete
        BoardManager.Instance.StartCoroutine(WaitForAnimationsToComplete());
    }
    
    private IEnumerator WaitForAnimationsToComplete()
    {
        // Maximum time to wait for animations to avoid infinite wait
        float maxWaitTime = 3.0f; // 3 seconds should be more than enough
        float elapsedTime = 0f;
        
        // Use a two-phase approach: wait for TileMover animations, then TileAnimator animations
        
        // Phase 1: Wait for all tile movement animations to complete
        Debug.Log("AnimatingState: Phase 1 - Waiting for movement animations to complete");
        bool movementComplete = false;
        while (elapsedTime < maxWaitTime && !movementComplete)
        {
            movementComplete = true; // Assume all movements are complete unless we find one that isn't
            
            // Check all tiles to see if any are still in Moving state
            foreach (Tile tile in Object.FindObjectsOfType<Tile>())
            {
                if (tile.CurrentState == Tile.TileState.Moving)
                {
                    movementComplete = false;
                    break;
                }
            }
            
            if (movementComplete) break;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log($"AnimatingState: Movement animations completed in {elapsedTime} seconds");
        
        // Phase 2: Process and wait for merge animations
        Debug.Log("AnimatingState: Phase 2 - Processing merge animations");
        
        // Reset elapsed time for Phase 2
        elapsedTime = 0f;
        bool mergeAnimationsComplete = false;
        
        // First check if there are any tiles in Merging state
        bool hasMergingTiles = false;
        foreach (Tile tile in Object.FindObjectsOfType<Tile>())
        {
            if (tile.CurrentState == Tile.TileState.Merging)
            {
                hasMergingTiles = true;
                
                // Ensure this tile has an active merge animation
                TileAnimator animator = tile.GetComponent<TileAnimator>();
                if (animator != null && !animator.IsPlayingMergeAnimation())
                {
                    Debug.Log($"AnimatingState: Starting delayed merge animation for tile at {tile.transform.position}");
                    animator.PlayMergeAnimation();
                }
            }
        }
        
        // If there are no merging tiles, skip Phase 2
        if (!hasMergingTiles)
        {
            Debug.Log("AnimatingState: No merging tiles found, skipping Phase 2");
            mergeAnimationsComplete = true;
        }
        
        // Wait for merge animations to complete
        while (elapsedTime < maxWaitTime && !mergeAnimationsComplete)
        {
            mergeAnimationsComplete = true; // Assume all merge animations are complete unless we find one that isn't
            
            // Check all tile animators to see if any are still playing merge animations
            foreach (TileAnimator animator in Object.FindObjectsOfType<TileAnimator>())
            {
                if (animator.IsPlayingMergeAnimation())
                {
                    mergeAnimationsComplete = false;
                    break;
                }
            }
            
            // Also check for tiles still in Merging state
            foreach (Tile tile in Object.FindObjectsOfType<Tile>())
            {
                if (tile.CurrentState == Tile.TileState.Merging)
                {
                    mergeAnimationsComplete = false;
                    break;
                }
            }
            
            if (mergeAnimationsComplete) break;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log($"AnimatingState: Merge animations completed in {elapsedTime} seconds");
        
        // Log a warning if we timed out waiting for animations
        float totalElapsedTime = elapsedTime;
        if (!movementComplete || !mergeAnimationsComplete)
        {
            Debug.LogWarning($"AnimatingState: Timed out waiting for animations to complete after {totalElapsedTime} seconds!");
            
            // Force all tiles back to Idle state AND ensure colliders are re-enabled
            foreach (Tile tile in Object.FindObjectsOfType<Tile>())
            {
                // Set state to Idle
                tile.SetState(Tile.TileState.Idle);
                
                // Ensure collider is re-enabled
                Collider2D tileCollider = tile.GetComponent<Collider2D>();
                if (tileCollider != null && !tileCollider.enabled)
                {
                    Debug.LogWarning($"AnimatingState: Re-enabling disabled collider on tile at {tile.transform.position}");
                    tileCollider.enabled = true;
                }
                
                // Ensure sorting order is restored
                SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sortingOrder < 0)
                {
                    Debug.LogWarning($"AnimatingState: Restoring negative sorting order on tile at {tile.transform.position}");
                    spriteRenderer.sortingOrder = 0; // Reset to default sorting order
                }
                
                // Ensure board state is correctly updated
                Vector2Int tileGridPos = BoardManager.Instance.GetGridPositionFromWorldPosition(tile.transform.position);
                Tile existingTile = BoardManager.Instance.GetTileAtPosition(tileGridPos);
                
                // Make sure the board array reflects the actual tile positions
                if (existingTile != tile)
                {
                    Debug.LogWarning($"AnimatingState: Board inconsistency detected for tile at {tileGridPos}. Re-registering tile position.");
                    BoardManager.Instance.ReregisterTileAtPosition(tileGridPos, tile);
                }
            }
            
            // Check if there are any empty cells that should be occupied or vice versa
            CheckBoardConsistency();
        }
        
        // Check for any tiles in Merging state that haven't been properly destroyed
        foreach (Tile tile in Object.FindObjectsOfType<Tile>())
        {
            // Check if tile was supposed to be destroyed during merge
            if (tile.name.Contains("(Clone)") && tile.transform.parent == null)
            {
                // Get the grid position
                Vector2Int tilePos = BoardManager.Instance.GetGridPositionFromWorldPosition(tile.transform.position);
                
                // Check if there's another tile at the exact same position (merged target tile)
                Tile[] overlappingTiles = Object.FindObjectsOfType<Tile>()
                    .Where(t => t != tile && 
                              Vector2.Distance(t.transform.position, tile.transform.position) < 0.1f)
                    .ToArray();
                
                if (overlappingTiles.Length > 0)
                {
                    Debug.LogWarning($"AnimatingState: Found source tile that wasn't destroyed after merge at {tilePos}. Destroying it now.");
                    
                    // Clean up this lingering tile
                    Object.Destroy(tile.gameObject);
                }
            }
        }
        
        Debug.Log($"AnimatingState: All animations completed in {totalElapsedTime} seconds.");
        
        // Short delay to ensure everything settles
        yield return new WaitForSeconds(0.1f);
        
        // Transition to the next state
        GameStateManager.Instance.SetState(nextState);
    }
    
    // Add a method to ensure board consistency
    private void CheckBoardConsistency()
    {
        Debug.Log("AnimatingState: Checking board consistency after animation timeout");
        
        // Loop through all positions on the board
        for (int x = 0; x < BoardManager.Instance.width; x++)
        {
            for (int y = 0; y < BoardManager.Instance.height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Vector2 worldPos = BoardManager.Instance.GetWorldPosition(pos);
                
                // Check if there's a tile at this world position
                Collider2D[] colliders = Physics2D.OverlapPointAll(worldPos);
                Tile physicalTile = null;
                
                foreach (var collider in colliders)
                {
                    if (!collider.isTrigger && collider.gameObject.CompareTag("Tile"))
                    {
                        physicalTile = collider.GetComponent<Tile>();
                        if (physicalTile != null)
                            break;
                    }
                }
                
                // Get the logical tile from the board array
                Tile boardTile = BoardManager.Instance.GetTileAtPosition(pos);
                
                // Inconsistency #1: Physical tile exists but board shows empty
                if (physicalTile != null && boardTile == null)
                {
                    Debug.LogError($"AnimatingState: Board inconsistency - Found physical tile at {pos} but board shows empty. Fixing.");
                    BoardManager.Instance.SetTileAtPosition(pos, physicalTile);
                    
                    // Verify emptyCells is updated correctly - position shouldn't be in emptyCells anymore
                    if (BoardManager.Instance.emptyCells.Contains(pos))
                    {
                        Debug.LogError($"AnimatingState: Position {pos} is still in emptyCells after setting tile. Removing it.");
                        BoardManager.Instance.emptyCells.Remove(pos);
                    }
                }
                // Inconsistency #2: Board shows tile but no physical tile exists
                else if (boardTile != null && physicalTile == null)
                {
                    Debug.LogError($"AnimatingState: Board inconsistency - Board shows tile at {pos} but no physical tile exists. Fixing.");
                    BoardManager.Instance.ClearCell(pos);
                    
                    // Verify emptyCells is updated correctly - position should be in emptyCells
                    if (!BoardManager.Instance.emptyCells.Contains(pos))
                    {
                        Debug.LogError($"AnimatingState: Position {pos} is not in emptyCells after clearing cell. Adding it.");
                        BoardManager.Instance.emptyCells.Add(pos);
                    }
                }
                // Inconsistency #3: Physical tile doesn't match board tile
                else if (physicalTile != null && boardTile != null && physicalTile != boardTile)
                {
                    Debug.LogError($"AnimatingState: Board inconsistency - Different tiles at position {pos}. Fixing.");
                    // Prioritize the physical tile that's actually there
                    BoardManager.Instance.SetTileAtPosition(pos, physicalTile);
                    
                    // No need to manually update emptyCells since SetTileAtPosition handles it
                }
                
                // Additional validation - check if the emptyCells state matches the board state
                bool cellShouldBeEmpty = boardTile == null;
                bool cellMarkedAsEmpty = BoardManager.Instance.emptyCells.Contains(pos);
                
                if (cellShouldBeEmpty != cellMarkedAsEmpty)
                {
                    Debug.LogError($"AnimatingState: EmptyCells inconsistency at {pos}. Should be empty: {cellShouldBeEmpty}, Marked as empty: {cellMarkedAsEmpty}. Fixing.");
                    if (cellShouldBeEmpty)
                    {
                        BoardManager.Instance.emptyCells.Add(pos);
                    }
                    else
                    {
                        BoardManager.Instance.emptyCells.Remove(pos);
                    }
                }
            }
        }
        
        // Final validation pass - check for any cells that are in emptyCells but have physical tiles
        HashSet<Vector2Int> cellsToRemove = new HashSet<Vector2Int>();
        foreach (Vector2Int emptyCell in BoardManager.Instance.emptyCells)
        {
            if (BoardManager.Instance.IsWithinBounds(emptyCell))
            {
                Vector2 worldPos = BoardManager.Instance.GetWorldPosition(emptyCell);
                Collider2D[] colliders = Physics2D.OverlapPointAll(worldPos);
                
                foreach (var collider in colliders)
                {
                    if (!collider.isTrigger && collider.gameObject.CompareTag("Tile") && collider.GetComponent<Tile>() != null)
                    {
                        Debug.LogError($"AnimatingState: Found physical tile at position {emptyCell} which is marked as empty. Fixing.");
                        BoardManager.Instance.SetTileAtPosition(emptyCell, collider.GetComponent<Tile>());
                        cellsToRemove.Add(emptyCell);
                        break;
                    }
                }
            }
        }
        
        // Remove any cells that should not be empty
        foreach (Vector2Int cellToRemove in cellsToRemove)
        {
            if (BoardManager.Instance.emptyCells.Contains(cellToRemove))
            {
                BoardManager.Instance.emptyCells.Remove(cellToRemove);
            }
        }
    }
    
    public override void Update() 
    {
        // No update logic needed, animations are running via coroutines
    }
    
    public override void Exit()
    {
        Debug.Log("AnimatingState: Exiting AnimatingState - animations completed");
    }
}
