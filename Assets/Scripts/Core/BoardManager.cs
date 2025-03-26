using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections; // Ensure this is included

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance; // Singleton instance

    public int width = Constants.DEFAULT_WIDTH;
    public int height = Constants.DEFAULT_HEIGHT;
    public float cellSize = Constants.DEFAULT_CELL_SIZE;
    public GameObject tilePrefab;
    public GameObject cellIndicatorPrefab;
    public GameObject gridBackgroundPrefab;

    private Tile[,] board;
    private HashSet<Vector2Int> emptyCells;
    private Queue<Vector2Int> prioritizedSpawnLocations;
    private readonly Color[] tileColorPalette = {
        new Color(1f, 0.5f, 0.5f), // Light Red
        new Color(0.5f, 0.5f, 1f), // Light Blue
        new Color(0.5f, 1f, 0.5f), // Light Green
        new Color(1f, 1f, 0.5f)    // Light Yellow
    };

    private Tile selectedTile;
    private Vector2Int selectedTilePosition;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        board = new Tile[width, height];
        emptyCells = new HashSet<Vector2Int>();
        prioritizedSpawnLocations = new Queue<Vector2Int>();
        InitializeGrid();
        CreateGridBackground();
        
        // Subscribe to the events - these should use instance methods, not local functions
        InputManager.OnTileSelected += HandleTileSelection;
        InputManager.OnTileMoveConfirmed += HandleTileMoveConfirmation;
        
        // Only initialize tiles if we're NOT using GameStateManager (as it will handle this)
        if (GameStateManager.Instance == null)
        {
            GenerateRandomStartingTiles();
        }
    }

    private void InitializeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int gridPosition = new Vector2Int(x, y);
                Vector2 position = GetWorldPosition(gridPosition);
                emptyCells.Add(gridPosition);
                Instantiate(cellIndicatorPrefab, position, Quaternion.identity, transform);
            }
        }
    }

    private void CreateGridBackground()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = GetWorldPosition(new Vector2Int(x, y));
                GameObject gridCellBackground = Instantiate(gridBackgroundPrefab, position, Quaternion.identity, transform);
                gridCellBackground.transform.localScale = new Vector3(cellSize * 1.02f, cellSize * 1.02f, 1f);
                gridCellBackground.name = $"GridBackground ({x}, {y})";
            }
        }
    }

    public Vector2 GetWorldPosition(Vector2Int gridPosition)
    {
        float gridWidth = width * cellSize;
        float gridHeight = height * cellSize;
        float offsetX = -gridWidth / 2 + cellSize / 2;
        float offsetY = -gridHeight / 2 + cellSize / 2;
        return new Vector2(gridPosition.x * cellSize + offsetX, gridPosition.y * cellSize + offsetY);
    }

    private Vector2Int GetGridPositionFromSwipeStart(Vector2 swipeStartPosition)
    {
        // Convert swipe start position in world space to grid coordinates
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(swipeStartPosition);
        int x = Mathf.FloorToInt((worldPosition.x + (width * cellSize) / 2) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.y + (height * cellSize) / 2) / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector2Int GetGridPositionFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x + (width * cellSize) / 2) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.y + (height * cellSize) / 2) / cellSize);
        return new Vector2Int(x, y);
    }

    public bool IsWithinBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
    }

    private Vector2Int FindTargetPosition(Vector2Int startPosition, Vector2Int direction, int maxSteps)
    {
        Vector2Int currentPosition = startPosition;

        for (int step = 0; step < maxSteps; step++)
        {
            Vector2Int nextPosition = currentPosition + direction;

            if (!IsWithinBounds(nextPosition) || IsCellOccupied(nextPosition))
            {
                break;
            }

            currentPosition = nextPosition;
        }

        return currentPosition;
    }

    public void MoveTile(Tile tile, Vector2Int startPosition, Vector2Int targetPosition)
    {
        ClearCell(startPosition);
        SetTileAtPosition(targetPosition, tile);

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
        
        // Movement Logic using TileMover and re-enable collider and restore sorting order after movement completes.
        TileMover mover = tile.GetComponent<TileMover>();
        if (mover != null)
        {
            StartCoroutine(MoveTileAndReenable(tile, GetWorldPosition(targetPosition), Constants.TILE_MOVE_DURATION, tileCollider, originalSortingOrder, sr));
        }
        else
        {
            tile.transform.position = GetWorldPosition(targetPosition);
            if (tileCollider != null)
            {
                tileCollider.enabled = true;
            }
            if (sr != null)
            {
                sr.sortingOrder = originalSortingOrder;
            }
        }
        
        // Animation: trigger visual effects using TileAnimator.
        TileAnimator animator = tile.GetComponent<TileAnimator>();
        if (animator != null)
        {
            animator.PlayMoveAnimation(GetWorldPosition(targetPosition), Constants.TILE_MOVE_DURATION);
        }
        
        MarkCellAsOccupied(targetPosition);
    }

    private IEnumerator MoveTileAndReenable(Tile tile, Vector2 targetPosition, float duration, Collider2D tileCollider, int originalSortingOrder, SpriteRenderer sr)
    {
        yield return StartCoroutine(tile.GetComponent<TileMover>().MoveTile(targetPosition, duration));
        if (tileCollider != null)
        {
            tileCollider.enabled = true;
        }
        if (sr != null)
        {
            sr.sortingOrder = originalSortingOrder;
        }
    }
    
    private IEnumerator MoveTileToTargetForMerge(Tile sourceTile, Tile targetTile, System.Action onComplete)
    {
        // Disable collider during animation
        Collider2D tileCollider = sourceTile.GetComponent<Collider2D>();
        if (tileCollider != null)
        {
            tileCollider.enabled = false;
        }
        
        // Move the tile to the target position
        TileMover mover = sourceTile.GetComponent<TileMover>();
        if (mover != null)
        {
            yield return StartCoroutine(mover.MoveTile(targetTile.transform.position, Constants.TILE_MOVE_DURATION));
        }
        else
        {
            sourceTile.transform.position = targetTile.transform.position;
        }
        
        // Execute the callback when movement is complete
        if (onComplete != null)
        {
            onComplete();
        }
    }

    private bool IsCellOccupied(Vector2Int position)
    {
        return board[position.x, position.y] != null;
    }

    public Tile GetTileAtPosition(Vector2Int position)
    {
        if (!IsWithinBounds(position)) // Ensure position is within bounds
        {
            return null;
        }
        return board[position.x, position.y];
    }

    private void SetTileAtPosition(Vector2Int position, Tile tile)
    {
        board[position.x, position.y] = tile;
    }

    private void ClearCell(Vector2Int position)
    {
        board[position.x, position.y] = null;
    }

    private void MarkCellAsOccupied(Vector2Int position)
    {
        emptyCells.Remove(position);
    }

    public void GenerateRandomStartingTiles(int minTiles = Constants.MIN_START_TILES, int maxTiles = Constants.MAX_START_TILES)
    {
        int tileCount = Random.Range(minTiles, maxTiles + 1);
        List<Vector2Int> availableCells = new List<Vector2Int>(emptyCells);

        for (int i = 0; i < tileCount && i < availableCells.Count; i++)
        {
            Vector2Int spawnPosition = availableCells[i];
            int randomNumber = Random.Range(Constants.MIN_TILE_NUMBER, Constants.MAX_TILE_NUMBER + 1);
            Color randomColor = tileColorPalette[Random.Range(0, tileColorPalette.Length)];
            GameObject newTile = Instantiate(tilePrefab, GetWorldPosition(spawnPosition), Quaternion.identity, transform);
            Tile tileComponent = newTile.GetComponent<Tile>();
            tileComponent.Initialize(randomColor, randomNumber);
            SetTileAtPosition(spawnPosition, tileComponent);
            MarkCellAsOccupied(spawnPosition);
        }
    }

    public bool HasValidMove()
    {
        // Check for any empty cells - if there's an empty cell, there's a valid move
        if (emptyCells.Count > 0)
        {
            return true;
        }

        // If no empty cells, check for mergeable adjacent tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile currentTile = GetTileAtPosition(new Vector2Int(x, y));
                if (currentTile == null) continue;

                // Check adjacent tiles for a valid merge
                List<Vector2Int> adjacentPositions = new List<Vector2Int>
                {
                    new Vector2Int(x + 1, y),
                    new Vector2Int(x - 1, y),
                    new Vector2Int(x, y + 1),
                    new Vector2Int(x, y - 1)
                };

                foreach (Vector2Int adjacent in adjacentPositions)
                {
                    if (!IsWithinBounds(adjacent)) continue;
                    
                    Tile adjacentTile = GetTileAtPosition(adjacent);
                    if (adjacentTile != null && CompareColors(adjacentTile.tileColor, currentTile.tileColor))
                    {
                        return true; // Found a valid move
                    }
                }
            }
        }
        return false; // No valid moves found
    }

    /// <summary>
    /// Explicitly clears the selection state (selected tile and position)
    /// </summary>
    public void ClearSelection()
    {
        if (selectedTile != null)
        {
            selectedTile.ClearSelectionState();
            selectedTile = null;
        }
        
        selectedTilePosition = Vector2Int.zero;
        
        ClearHighlights();
    }

    private void HandleTileSelection(Vector2Int gridPosition)
    {
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsInState<PlayerTurnState>())
        {
            return;
        }

        Tile tile = GetTileAtPosition(gridPosition);

        if (tile != null)
        {
            if (selectedTile != null && selectedTile != tile)
            {
                if (IsAdjacent(selectedTilePosition, gridPosition) &&
                    CompareColors(selectedTile.tileColor, tile.tileColor))
                {
                    Tile tempSourceTile = selectedTile;
                    Vector2Int tempSourcePos = selectedTilePosition;

                    ClearSelection();

                    StartCoroutine(MoveTileToTargetForMerge(tempSourceTile, tile, () =>
                    {
                        if (TileMerger.MergeTiles(tile, tempSourceTile))
                        {
                            ClearCell(tempSourcePos);
                            emptyCells.Add(tempSourcePos);

                            SetTileAtPosition(gridPosition, tile);
                        }
                    }));

                    GameManager.Instance.EndTurn();
                    return;
                }
            }

            if (selectedTile != null)
            {
                ClearHighlights();
            }

            selectedTile = tile;
            selectedTilePosition = gridPosition;
            HighlightValidMoves(gridPosition, tile.number);
        }
        else if (selectedTile != null)
        {
            HandleTileMoveConfirmation(gridPosition);
        }
    }

    // Helper method to check if two positions are adjacent
    public bool IsAdjacent(Vector2Int pos1, Vector2Int pos2)
    {
        // Check if horizontally or vertically adjacent (Manhattan distance = 1)
        int manhattanDistance = Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);
        return manhattanDistance == 1;
    }

    public void HighlightValidMoves(Vector2Int startPosition, int maxSteps)
    {
        ClearHighlights();
        Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int direction in directions)
        {
            for (int step = 1; step <= maxSteps; step++)
            {
                Vector2Int targetPosition = startPosition + direction * step;
                if (!IsWithinBounds(targetPosition)) break;
                if (IsCellOccupied(targetPosition)) break; // Stop highlighting if tile found
                HighlightCell(targetPosition);
            }
        }
    }

    private void HighlightCell(Vector2Int position)
    {
        // Add visual feedback for valid move cells (e.g., change cell color)
        GameObject cellIndicator = Instantiate(cellIndicatorPrefab, GetWorldPosition(position), Quaternion.identity, transform);
        cellIndicator.tag = "Highlight"; // Assign the correct tag
        cellIndicator.GetComponent<SpriteRenderer>().color = new Color(0.5f, 1f, 0.5f, 0.5f); // Highlight color
    }

    public void ClearHighlights()
    {
        // First, find all highlights using FindObjectsOfType for a more thorough search
        GameObject[] allHighlights = GameObject.FindGameObjectsWithTag("Highlight");
        foreach (GameObject highlight in allHighlights)
        {
            Destroy(highlight);
        }
        
        // Then also check children as a backup (in case tag-based search misses any)
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Highlight"))
            {
                Destroy(child.gameObject);
            }
        }
        
        // Finally check the scene for any strays
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (obj.CompareTag("Highlight"))
            {
                Destroy(obj);
            }
        }
    }

    private void HandleTileMoveConfirmation(Vector2Int targetPosition)
    {
        // If we're not in PlayerTurnState, ignore move
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsInState<PlayerTurnState>())
        {
            return;
        }

        if (selectedTile == null)
        {
            return;
        }
        
        if (!IsWithinBounds(targetPosition))
        {
            return;
        }

        // If the clicked cell is the same as the selected tile's cell, do nothing.
        if (targetPosition == selectedTilePosition)
        {
            // Don't deselect the tile - keep it selected
            return;
        }

        // Check if the target cell is already occupied for potential merge.
        if (IsCellOccupied(targetPosition))
        {
            Tile targetTile = GetTileAtPosition(targetPosition);
            
            bool colorsMatch = CompareColors(targetTile.tileColor, selectedTile.tileColor);
            
            if (colorsMatch && targetTile != selectedTile)
            {
                // Clear the selected tile's position before moving it
                ClearCell(selectedTilePosition);
                emptyCells.Add(selectedTilePosition);
                
                // Execute merge immediately to avoid race conditions
                bool mergeSuccessful = TileMerger.MergeTiles(targetTile, selectedTile);
                
                if (mergeSuccessful) 
                {
                    // Play a merge animation on the target tile
                    TileAnimator animator = targetTile.GetComponent<TileAnimator>();
                    if (animator != null)
                    {
                        animator.PlayMergeAnimation();
                    }
                }
                
                selectedTile = null;
                ClearHighlights();
                GameManager.Instance.EndTurn();
                return;
            }
            else
            {
                // Don't deselect - allow the player to try a different move
                return;
            }
        }

        // Check if the move is valid by checking if targetPosition is among the highlighted valid moves
        if (IsValidMove(selectedTilePosition, targetPosition, selectedTile.number))
        {
            // Store the tile and position before clearing selection
            Tile tileToMove = selectedTile;
            Vector2Int startPos = selectedTilePosition;
            
            // Clear selection before moving
            ClearSelection();
            
            // Use the stored references to move the tile
            MoveTile(tileToMove, startPos, targetPosition);
            
            // Update to use GameStateManager for state transition
            if (GameStateManager.Instance != null)
            {
                // Use transition with delay to let animation complete
                GameStateManager.Instance.SetStateWithDelay(new PostTurnState(), 0.5f);
            }
            else
            {
                // Fallback if state manager is not available
                GameManager.Instance.EndTurn();
            }
        }
        else
        {
            // Don't deselect - allow player to try a different move
        }
    }

    // Improved color comparison method with tolerance for floating point precision
    public bool CompareColors(Color a, Color b)
    {
        const float tolerance = 0.01f; // Adjust tolerance if needed
        return Mathf.Abs(a.r - b.r) < tolerance && 
               Mathf.Abs(a.g - b.g) < tolerance && 
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    private IEnumerator PerformMerge(Tile sourceTile, Tile targetTile, Vector2Int sourcePos, Vector2Int targetPos)
    {
        // Animate the source tile moving to the target position
        TileMover mover = sourceTile.GetComponent<TileMover>();
        if (mover != null)
        {
            yield return StartCoroutine(mover.MoveTile(GetWorldPosition(targetPos), Constants.TILE_MOVE_DURATION));
        }
        
        // Now do the actual merge and update the board state
        if (TileMerger.MergeTiles(targetTile, sourceTile))
        {
            // Play a merge animation on the target tile
            TileAnimator animator = targetTile.GetComponent<TileAnimator>();
            if (animator != null)
            {
                animator.PlayMergeAnimation();
            }
        }
    }

    // Helper method to check if a move is valid
    public bool IsValidMove(Vector2Int startPosition, Vector2Int targetPosition, int maxSteps)
    {
        // Find which direction this move is in
        Vector2Int diff = targetPosition - startPosition;
        
        // Only allow cardinal directions
        if (diff.x != 0 && diff.y != 0)
        {
            return false;
        }
        
        Vector2Int direction;
        int distance;
        
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            direction = new Vector2Int((int)Mathf.Sign(diff.x), 0);  // Cast to int
            distance = Mathf.Abs(diff.x);
        }
        else
        {
            direction = new Vector2Int(0, (int)Mathf.Sign(diff.y));  // Cast to int
            distance = Mathf.Abs(diff.y);
        }
        
        // Check if the move is too far
        if (distance > maxSteps)
        {
            return false;
        }
        
        // Check if there are obstacles in the path
        for (int step = 1; step <= distance; step++)
        {
            Vector2Int checkPosition = startPosition + direction * step;
            
            // If we're not at the target and there's an obstacle, move is invalid
            if (step < distance && IsCellOccupied(checkPosition))
            {
                return false;
            }
        }
        
        // All checks passed
        return true;
    }

    /// <summary>
    /// Checks if a cell is empty.
    /// </summary>
    public bool IsCellEmpty(Vector2Int position)
    {
        if (!IsWithinBounds(position))
            return false;
            
        return board[position.x, position.y] == null;
    }

    /// <summary>
    /// Gets a random color from the tile color palette
    /// </summary>
    public Color GetRandomTileColor()
    {
        return tileColorPalette[Random.Range(0, tileColorPalette.Length)];
    }

    /// <summary>
    /// Registers a tile that was created from splitting at the given position.
    /// </summary>
    public void RegisterSplitTile(Vector2Int position, Tile tile)
    {
        // Skip invalid positions
        if (!IsWithinBounds(position))
        {
            return;
        }
        
        // Make sure we're not putting a tile in an occupied cell
        if (board[position.x, position.y] != null)
        {
            return;
        }
        
        // Add the tile to the board at the specified position
        SetTileAtPosition(position, tile);
        // Mark the cell as occupied so it's not used for spawning
        emptyCells.Remove(position);
        
        // Ensure the tile is positioned correctly in world space
        tile.transform.position = GetWorldPosition(position);
        
        // Verify the tile's values and text component
        var textComp = tile.GetComponentInChildren<TMPro.TextMeshPro>();
        if (textComp != null)
        {
            // Ensure the text matches the tile's number
            if (textComp.text != tile.number.ToString())
            {
                textComp.text = tile.number.ToString();
            }
            textComp.ForceMeshUpdate();
        }
        
        // Update the tile's visuals one more time to ensure everything is set up correctly
        tile.UpdateVisuals();
    }

    // Add this method to allow clearing the board for game restart
    public void ClearBoard()
    {
        // Destroy all tile GameObjects
        foreach (Tile tile in board)
        {
            if (tile != null)
            {
                Destroy(tile.gameObject);
            }
        }
        
        // Reset the board array
        board = new Tile[width, height];
        
        // Reset empty cells
        emptyCells.Clear();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                emptyCells.Add(new Vector2Int(x, y));
            }
        }
        
        // Clear any highlights
        ClearHighlights();
        
        // Reset selection
        selectedTile = null;
        selectedTilePosition = Vector2Int.zero;
    }

    public void PerformMergeOperation(Tile sourceTile, Tile targetTile, Vector2Int sourcePos, Vector2Int targetPos)
    {
        ClearSelection();
        PlayerTurnState.ClearAllSelectionState();

        StartCoroutine(MoveTileToTargetForMerge(sourceTile, targetTile, () =>
        {
            ClearCell(sourcePos);
            emptyCells.Add(sourcePos);

            if (TileMerger.MergeTiles(targetTile, sourceTile))
            {
                TileAnimator animator = targetTile.GetComponent<TileAnimator>();
                animator?.PlayMergeAnimation();

                ClearSelection();
                PlayerTurnState.ClearAllSelectionState();
            }
            else
            {
                GameManager.Instance.EndTurn();
            }
        }));
    }

    // Add this helper method to help with debugging
    public int GetEmptyCellsCount()
    {
        return emptyCells.Count;
    }
}