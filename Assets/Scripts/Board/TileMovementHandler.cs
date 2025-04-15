using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tilebreakers.Board; // Add this using directive for TileMergeHandler

/// <summary>
/// Handles movement operations for tiles, including animation and state management.
/// This replaces the TileMover component for a more centralized approach.
/// </summary>
public class TileMovementHandler : MonoBehaviour
{
    private static TileMovementHandler _instance;
    public static TileMovementHandler Instance 
    {
        get 
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TileMovementHandler>();
                if (_instance == null)
                {
                    GameObject handler = new GameObject("TileMovementHandler");
                    _instance = handler.AddComponent<TileMovementHandler>();
                    DontDestroyOnLoad(handler);
                }
            }
            return _instance;
        }
    }

    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool useUnscaledTime = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Moves a tile to a target position over time.
    /// </summary>
    public IEnumerator MoveTile(GameObject tileObject, Vector2 targetPosition, float duration)
    {
        if (tileObject == null) yield break;

        SpriteRenderer sr = tileObject.GetComponent<SpriteRenderer>();
        int originalSortingOrder = sr != null ? sr.sortingOrder : 0;

        // Lower sorting order during movement
        if (sr != null)
        {
            StoreSortingOrder(tileObject, originalSortingOrder);
            SetAnimationSortingOrder(tileObject, sr);
        }

        Vector3 startPosition = tileObject.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (tileObject == null) break;  // Check if the object still exists

            float t = elapsed / duration;
            float curveValue = movementCurve.Evaluate(t);
            tileObject.transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
            
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        if (tileObject != null)
        {
            tileObject.transform.position = targetPosition;
            
            // Restore original sorting order
            if (sr != null && originalSortingOrder != sr.sortingOrder)
            {
                RestoreSortingOrder(tileObject, sr);
            }
        }
    }

    /// <summary>
    /// Moves a tile to a target position and then executes a callback.
    /// </summary>
    public IEnumerator MoveTileWithCallback(GameObject tileObject, Vector2 targetPosition, float duration, System.Action onComplete)
    {
        yield return MoveTile(tileObject, targetPosition, duration);
        onComplete?.Invoke();
    }

    /// <summary>
    /// Moves a tile for a merge operation and then performs merge logic.
    /// </summary>
    public IEnumerator MoveTileForMerge(Tile sourceTile, Tile targetTile, Vector2Int sourcePos, System.Action onComplete)
    {
        if (sourceTile == null || targetTile == null) yield break;

        GameObject sourceTileObject = sourceTile.gameObject;
        
        // Disable collider during movement
        Collider2D tileCollider = sourceTileObject.GetComponent<Collider2D>();
        if (tileCollider != null)
        {
            tileCollider.enabled = false;
        }

        // Move the tile to the target position
        yield return MoveTile(sourceTileObject, targetTile.transform.position, Constants.TILE_MOVE_DURATION);
        
        // Execute callback when movement is complete
        onComplete?.Invoke();
        
        // Verify the source tile was properly cleaned up
        yield return new WaitForSeconds(0.1f);
        if (sourceTileObject != null)
        {
            Debug.LogWarning($"TileMovementHandler: Source tile from {sourcePos} was not properly destroyed after merge. Double-checking...");
            
            if (sourceTileObject)
            {
                Debug.LogError($"TileMovementHandler: Source tile still exists after merge. Destroying it now.");
                Destroy(sourceTileObject);
            }
        }
    }

    /// <summary>
    /// Stores the original sorting order of a tile before animation.
    /// </summary>
    public void StoreSortingOrder(GameObject tileObject, int sortingOrder)
    {
        tileObject.GetComponent<SpriteRenderer>()?.material?.SetInt("_OriginalSortingOrder", sortingOrder);
    }

    /// <summary>
    /// Sets a temporary sorting order for use during animations.
    /// </summary>
    public void SetAnimationSortingOrder(GameObject tileObject, SpriteRenderer sr = null)
    {
        if (sr == null) sr = tileObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = -1;  // Below other tiles during movement
        }
    }

    /// <summary>
    /// Restores the original sorting order after animation.
    /// </summary>
    public void RestoreSortingOrder(GameObject tileObject, SpriteRenderer sr = null)
    {
        if (sr == null) sr = tileObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            int originalOrder = 0;  // Default value
            if (sr.material != null && sr.material.HasProperty("_OriginalSortingOrder"))
            {
                originalOrder = sr.material.GetInt("_OriginalSortingOrder");
            }
            sr.sortingOrder = originalOrder;
        }
    }

    /// <summary>
    /// Moves a tile from one grid position to another with animation.
    /// </summary>
    public void MoveTile(Tile tile, Vector2Int startPosition, Vector2Int targetPosition)
    {
        if (tile == null || !BoardManager.Instance.IsWithinBounds(startPosition) || 
            !BoardManager.Instance.IsWithinBounds(targetPosition))
        {
            Debug.LogError($"TileMovementHandler: Invalid move operation. Tile: {tile}, Start: {startPosition}, Target: {targetPosition}");
            return;
        }

        if (tile.HasMerged())
        {
            Debug.LogWarning($"TileMovementHandler: Cannot move tile at {startPosition} because it has already merged this turn.");
            return;
        }

        BoardManager boardManager = BoardManager.Instance;
        boardManager.ClearCell(startPosition); // Ensure the starting cell is cleared
        boardManager.SetTileAtPosition(targetPosition, tile); // Mark the target cell as occupied

        // Log the movement operation
        Debug.Log($"TileMovementHandler: Moving tile from {startPosition} to {targetPosition}.");

        // Disable the tile's collider during movement to prevent collision overlap.
        Collider2D tileCollider = tile.GetComponent<Collider2D>();
        if (tileCollider != null)
        {
            tileCollider.enabled = false;
        }
        
        // Lower the sorting order to render the moving tile beneath static ones.
        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        int originalSortingOrder = sr != null ? sr.sortingOrder : 0;
        if (sr != null)
        {
            sr.sortingOrder = -1;
        }
        
        // Start the movement animation coroutine
        BoardManager.Instance.StartCoroutine(
            MoveTileAndReenable(tile, boardManager.GetWorldPosition(targetPosition), 
                Constants.TILE_MOVE_DURATION, tileCollider, originalSortingOrder, sr)
        );
        
        // Animation: trigger visual effects using TileAnimator.
        TileAnimator animator = tile.GetComponent<TileAnimator>();
        if (animator != null)
        {
            animator.PlayMoveAnimation(boardManager.GetWorldPosition(targetPosition), Constants.TILE_MOVE_DURATION);
        }
        
        boardManager.MarkCellAsOccupied(targetPosition); // Ensure the target cell is removed from emptyCells

        // Count the move after successfully moving the tile
        // This ensures moves are counted properly
        StartCoroutine(CountMoveAfterAnimation(Constants.TILE_MOVE_DURATION));
    }

    /// <summary>
    /// Counts a move after the animation completes to ensure proper timing
    /// </summary>
    private IEnumerator CountMoveAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Ensure move is counted only for player-initiated moves (not auto moves from effects)
        if (GameStateManager.Instance.IsInState<MovingTilesState>() || 
            GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            Debug.Log("TileMovementHandler: Animation complete, counting move");
            GameManager.Instance.EndTurn();
        }
    }

    /// <summary>
    /// Finds the furthest valid position a tile can move in a given direction.
    /// </summary>
    /// <param name="startPosition">The starting grid position of the tile</param>
    /// <param name="direction">The direction to move</param>
    /// <param name="maxSteps">Maximum number of steps allowed (typically the tile's value)</param>
    /// <returns>The furthest valid grid position the tile can move to</returns>
    public Vector2Int FindTargetPosition(Vector2Int startPosition, Vector2Int direction, int maxSteps)
    {
        Vector2Int currentPosition = startPosition;

        for (int step = 0; step < maxSteps; step++)
        {
            Vector2Int nextPosition = currentPosition + direction;

            if (!BoardManager.Instance.IsWithinBounds(nextPosition) || BoardManager.Instance.IsCellOccupied(nextPosition))
            {
                break;
            }

            currentPosition = nextPosition;
        }

        return currentPosition;
    }
    
    /// <summary>
    /// Determines if a move from start to target position is valid for a given tile.
    /// Checks orthogonal movement, distance within tile number, and path clearance.
    /// </summary>
    /// <param name="startPosition">The starting position of the tile</param>
    /// <param name="targetPosition">The target position to move to</param>
    /// <param name="tile">The tile being moved</param>
    /// <param name="pathClear">Output parameter indicating if the path is clear</param>
    /// <returns>True if the move is valid, false otherwise</returns>
    public bool IsValidMove(Vector2Int startPosition, Vector2Int targetPosition, Tile tile, out bool pathClear)
    {
        pathClear = false;
        
        if (tile == null)
        {
            Debug.LogWarning("TileMovementHandler: Cannot validate move for null tile");
            return false;
        }
        
        // Check if the target is the same as start (no movement)
        if (targetPosition == startPosition)
        {
            return false;
        }
        
        // Check if positions are within bounds
        if (!BoardManager.Instance.IsWithinBounds(startPosition) || !BoardManager.Instance.IsWithinBounds(targetPosition))
        {
            Debug.LogWarning($"TileMovementHandler: Out of bounds positions - Start: {startPosition}, Target: {targetPosition}");
            return false;
        }
        
        // Check if the move is orthogonal (along one axis only)
        Vector2Int direction = targetPosition - startPosition;
        bool isOrthogonal = direction.x == 0 || direction.y == 0;
        
        if (!isOrthogonal)
        {
            Debug.Log("TileMovementHandler: Move is not orthogonal");
            return false;
        }
        
        // Check if the distance is within the tile's number (movement range)
        int distance = Mathf.Abs(direction.x) + Mathf.Abs(direction.y);
        if (distance > tile.number)
        {
            Debug.Log($"TileMovementHandler: Distance {distance} exceeds tile movement range {tile.number}");
            return false;
        }
        
        // Check if the path is clear
        pathClear = IsPathClear(startPosition, targetPosition);
        
        return isOrthogonal && distance <= tile.number && pathClear;
    }
    
    /// <summary>
    /// Checks if there are any tiles obstructing the path between two positions.
    /// </summary>
    public bool IsPathClear(Vector2Int startPos, Vector2Int endPos)
    {
        // Determine the direction vector
        Vector2Int direction = new Vector2Int(
            Mathf.Clamp(endPos.x - startPos.x, -1, 1),
            Mathf.Clamp(endPos.y - startPos.y, -1, 1)
        );
        
        // Only orthogonal movements are valid for merges
        if (direction.x != 0 && direction.y != 0)
        {
            return false; // Diagonal movement is not allowed
        }
        
        Vector2Int currentPos = startPos + direction; // Skip the starting position
        
        // Check all positions along the path except the start and end positions
        while (currentPos != endPos)
        {
            if (BoardManager.Instance.IsCellOccupied(currentPos))
            {
                Debug.LogWarning($"TileMovementHandler: Path obstructed at {currentPos}");
                return false;
            }
            currentPos += direction;
        }
        
        return true;
    }

    /// <summary>
    /// Performs a merge operation between two tiles.
    /// </summary>
    /// <param name="sourceTile">The tile being moved for merging</param>
    /// <param name="targetTile">The target tile to merge with</param>
    /// <param name="sourcePos">The source position of the moving tile</param>
    /// <param name="targetPos">The target position for the merge</param>
    /// <returns>Coroutine for animation and merge operation</returns>
    public IEnumerator PerformMerge(Tile sourceTile, Tile targetTile, Vector2Int sourcePos, Vector2Int targetPos)
    {
        // Use TileMovementHandler to move the source tile to target position
        yield return MoveTile(sourceTile.gameObject, targetTile.transform.position, Constants.TILE_MOVE_DURATION);
        
        // Now do the actual merge and update the board state
        TileMergeHandler mergeHandler = FindObjectOfType<TileMergeHandler>();
        if (mergeHandler != null && mergeHandler.MergeTiles(targetTile, sourceTile))
        {
            // Play a merge animation on the target tile
            TileAnimator animator = targetTile.GetComponent<TileAnimator>();
            if (animator != null)
            {
                animator.PlayMergeAnimation();
            }
        }
    }

    /// <summary>
    /// Performs a move and then merge operation between tiles.
    /// </summary>
    /// <param name="sourceTile">The tile to move and merge</param>
    /// <param name="targetTile">The target tile to merge with</param>
    /// <param name="sourcePos">Starting position</param>
    /// <param name="onComplete">Callback to execute after merge</param>
    public IEnumerator PerformMoveAndMerge(Tile sourceTile, Tile targetTile, Vector2Int sourcePos, System.Action onComplete)
    {
        // Track the source position for proper board cleanup
        
        // Disable collider during animation
        Collider2D tileCollider = sourceTile.GetComponent<Collider2D>();
        if (tileCollider != null)
        {
            tileCollider.enabled = false;
        }
        
        // Track the source tile GameObject for later verification
        GameObject sourceTileObject = sourceTile.gameObject;
        
        // Use TileMovementHandler for movement
        yield return MoveTileForMerge(sourceTile, targetTile, sourcePos, () => {
            // Clear the cell on the board array
            BoardManager.Instance.ClearCell(sourcePos);
            if (!BoardManager.Instance.emptyCells.Contains(sourcePos))
            {
                BoardManager.Instance.emptyCells.Add(sourcePos);
            }
            
            // Execute the callback when movement is complete
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// Animation coroutine to move a tile and restore its properties afterward.
    /// </summary>
    public IEnumerator MoveTileAndReenable(Tile tile, Vector2 targetPosition, float duration, 
                                          Collider2D tileCollider, int originalSortingOrder, SpriteRenderer sr)
    {
        // Set the tile state to Moving before starting animation
        if (tile != null)
        {
            tile.SetState(Tile.TileState.Moving);
        }
        
        // Remove the unused 'completed' variable
        
        // Move the try-catch outside of the yield return to fix CS1626 error
        IEnumerator moveCoroutine = null;
        try
        {
            // Create the movement coroutine
            moveCoroutine = MoveTile(tile.gameObject, targetPosition, duration);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TileMovementHandler: Error during tile movement animation: {e.Message}");
        }
        
        // Execute the coroutine if it was successfully created
        if (moveCoroutine != null)
        {
            yield return moveCoroutine;
        }
        
        // After movement completes (or errors), re-enable collider and restore sorting order
        if (tileCollider != null)
        {
            tileCollider.enabled = true;
        }
        
        if (sr != null)
        {
            sr.sortingOrder = originalSortingOrder;
        }
        
        // Set the tile state back to Idle after animation completes
        if (tile != null)
        {
            tile.SetState(Tile.TileState.Idle);
        }
    }
}
