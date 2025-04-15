using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Tilebreakers.Board; // Add this namespace for TileMergeHandler

// Fix the namespace import - remove the ambiguous alias and use proper class path
using Tilebreakers.Special;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance; // Singleton instance

    public int width = Constants.DEFAULT_WIDTH;
    public int height = Constants.DEFAULT_HEIGHT;
    public float cellSize = Constants.DEFAULT_CELL_SIZE;
    public GameObject tilePrefab;
    public GameObject cellIndicatorPrefab;
    public GameObject gridBackgroundPrefab;

    [Header("Visual Settings")]
    public Color gridBackgroundColor = new Color(0.2f, 0.2f, 0.25f);
    public Color gridLineColor = new Color(0.3f, 0.3f, 0.35f);
    public float gridLineWidth = 0.05f;
    public bool useRoundedCorners = true;
    public float cornerRadius = 0.1f;
    [Range(0f, 0.5f)] public float cellSpacing = 0.1f;
    [SerializeField] private Material gridCellMaterial;

    private Tile[,] board;
    public HashSet<Vector2Int> emptyCells;
    private Queue<Vector2Int> prioritizedSpawnLocations;
    private readonly Color[] tileColorPalette = {
        new Color(1f, 0.5f, 0.5f), // Light Red
        new Color(0.5f, 0.5f, 1f), // Light Blue
        new Color(0.5f, 1f, 0.5f), // Light Green
        new Color(1f, 1f, 0.5f)    // Light Yellow
    };

    public Vector2Int? lastMergedCellPosition;

    private List<SpecialTile> specialTiles = new List<SpecialTile>();

    private bool skipNextSpawn = false;

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

    private void Start()
    {
        board = new Tile[width, height];
        emptyCells = new HashSet<Vector2Int>();
        prioritizedSpawnLocations = new Queue<Vector2Int>();
        InitializeGrid();
        CreateGridBackground();

        // Subscribe to the events
        InputManager.OnTileSelected += HandleTileSelection;
        InputManager.OnTileMoveConfirmed += HandleTileMoveConfirmation;

        // Remove automatic tile initialization
        // Board initialization will now be triggered explicitly in InitGameState
    }

    // Add a new method to initialize the board explicitly
    public void InitializeBoard()
    {
        ClearBoard(); // Ensure the board is cleared before initialization
        GenerateRandomStartingTiles();
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
        // Create a parent object for the grid background
        GameObject gridParent = new GameObject("GridBackground");
        gridParent.transform.SetParent(transform);
        
        float actualCellSize = cellSize - cellSpacing;
        
        // First, create a background panel for the entire grid
        GameObject backgroundPanel = new GameObject("BoardBackground");
        backgroundPanel.transform.SetParent(gridParent.transform);
        
        SpriteRenderer panelRenderer = backgroundPanel.AddComponent<SpriteRenderer>();
        panelRenderer.sprite = CreateRoundedRectSprite(width * cellSize + cellSpacing, height * cellSize + cellSpacing, cornerRadius * 2);
        panelRenderer.color = gridBackgroundColor;
        panelRenderer.sortingOrder = -2;
        backgroundPanel.transform.position = new Vector3(0, 0, 0.1f);

        // Now create individual cell backgrounds
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = GetWorldPosition(new Vector2Int(x, y));
                GameObject gridCellBackground = new GameObject($"GridCell ({x}, {y})");
                gridCellBackground.transform.SetParent(gridParent.transform);
                gridCellBackground.transform.position = position;
                
                SpriteRenderer cellRenderer = gridCellBackground.AddComponent<SpriteRenderer>();
                cellRenderer.sprite = CreateRoundedRectSprite(actualCellSize, actualCellSize, cornerRadius);
                cellRenderer.color = gridLineColor;
                cellRenderer.sortingOrder = -1;
                
                // Apply custom material if available
                if (gridCellMaterial != null)
                {
                    cellRenderer.material = gridCellMaterial;
                    
                    // Set material properties
                    MaterialPropertyBlock props = new MaterialPropertyBlock();
                    cellRenderer.GetPropertyBlock(props);
                    props.SetColor("_Color", gridLineColor);
                    props.SetFloat("_CornerRadius", cornerRadius);
                    cellRenderer.SetPropertyBlock(props);
                }
                
                // Add subtle animation
                StartCoroutine(PulseCell(gridCellBackground, 0.95f, 1.0f, 2f + Random.value));
            }
        }
    }

    private Sprite CreateRoundedRectSprite(float width, float height, float radius)
    {
        int textureSize = 128;
        // Fix 2: Correct Texture2D constructor by providing width and height parameters
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        Color transparentColor = new Color(1f, 1f, 1f, 0f);
        Color whiteColor = Color.white;
        
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                // Normalize coordinates to -0.5...0.5
                float nx = (x / (float)textureSize) - 0.5f;
                float ny = (y / (float)textureSize) - 0.5f;
                
                // Scale to match target width/height
                float scaledX = nx * width;
                float scaledY = ny * height;
                
                // Calculate distance from nearest edge
                float dx = UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(scaledX) - width/2f + radius, 0);
                float dy = UnityEngine.Mathf.Max(UnityEngine.Mathf.Abs(scaledY) - height/2f + radius, 0);
                
                // If in corner region, calculate distance from corner
                float distanceFromCorner = UnityEngine.Mathf.Sqrt(dx * dx + dy * dy);
                
                if (distanceFromCorner <= radius)
                    texture.SetPixel(x, y, whiteColor);
                else if (UnityEngine.Mathf.Abs(scaledX) <= width/2f && UnityEngine.Mathf.Abs(scaledY) <= height/2f)
                    texture.SetPixel(x, y, whiteColor);
                else
                    texture.SetPixel(x, y, transparentColor);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), 100);
    }

    private IEnumerator PulseCell(GameObject cell, float minScale, float maxScale, float period)
    {
        while (cell != null)
        {
            float t = (1 + UnityEngine.Mathf.Sin(Time.time * 2 * UnityEngine.Mathf.PI / period)) / 2;
            float scale = UnityEngine.Mathf.Lerp(minScale, maxScale, t);
            
            if (cell != null) // Check again in case it was destroyed
                cell.transform.localScale = new Vector3(scale, scale, 1);
                
            yield return null;
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
        int x = UnityEngine.Mathf.FloorToInt((worldPosition.x + (width * cellSize) / 2) / cellSize);
        int y = UnityEngine.Mathf.FloorToInt((worldPosition.y + (height * cellSize) / 2) / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector2Int GetGridPositionFromWorldPosition(Vector3 worldPosition)
    {
        int x = UnityEngine.Mathf.FloorToInt((worldPosition.x + (width * cellSize) / 2) / cellSize);
        int y = UnityEngine.Mathf.FloorToInt((worldPosition.y + (height * cellSize) / 2) / cellSize);
        return new Vector2Int(x, y);
    }

    public bool IsWithinBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
    }

    public void MoveTile(Tile tile, Vector2Int startPosition, Vector2Int targetPosition)
    {
        if (tile == null || !IsWithinBounds(startPosition) || !IsWithinBounds(targetPosition))
        {
            Debug.LogError($"BoardManager: Invalid move operation. Tile: {tile}, Start: {startPosition}, Target: {targetPosition}");
            return;
        }

        if (tile.HasMerged())
        {
            Debug.LogWarning($"BoardManager: Cannot move tile at {startPosition} because it has already merged this turn.");
            return;
        }

        ClearCell(startPosition); // Ensure the starting cell is cleared
        SetTileAtPosition(targetPosition, tile); // Mark the target cell as occupied

        // Log the movement operation
        Debug.Log($"BoardManager: Moved tile from {startPosition} to {targetPosition}.");

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
        
        // Use TileMovementHandler for movement animation - now calling the extracted method
        StartCoroutine(TileMovementHandler.Instance.MoveTileAndReenable(
            tile, GetWorldPosition(targetPosition), Constants.TILE_MOVE_DURATION, 
            tileCollider, originalSortingOrder, sr));
        
        // Animation: trigger visual effects using TileAnimator.
        TileAnimator animator = tile.GetComponent<TileAnimator>();
        if (animator != null)
        {
            animator.PlayMoveAnimation(GetWorldPosition(targetPosition), Constants.TILE_MOVE_DURATION);
        }
        
        MarkCellAsOccupied(targetPosition); // Ensure the target cell is removed from emptyCells
    }

    private IEnumerator MoveTileToTargetForMerge(Tile sourceTile, Tile targetTile, System.Action onComplete)
    {
        // Track the source position for proper board cleanup
        Vector2Int sourcePos = GetGridPositionFromWorldPosition(sourceTile.transform.position);
        
        // Delegate to TileMovementHandler for move and merge
        yield return TileMovementHandler.Instance.PerformMoveAndMerge(sourceTile, targetTile, sourcePos, onComplete);
    }

    public bool IsCellOccupied(Vector2Int position)
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

    /// <summary>
    /// Set tile at position with additional empty cell tracking validation
    /// </summary>
    public void SetTileAtPosition(Vector2Int position, Tile tile)
    {
        if (!IsWithinBounds(position))
        {
            Debug.LogError($"BoardManager: Attempted to set tile at out-of-bounds position {position}");
            return;
        }
        
        board[position.x, position.y] = tile;
        
        // Enhanced emptyCells management - improve logging for debugging
        if (emptyCells.Contains(position))
        {
            emptyCells.Remove(position);
            Debug.Log($"BoardManager: Position {position} removed from emptyCells during SetTileAtPosition (Previous value: {(tile == null ? "null" : tile.number.ToString())})");
        }
        
        // Double-check that this position is actually removed from emptyCells
        if (emptyCells.Contains(position))
        {
            Debug.LogError($"BoardManager: CRITICAL - Position {position} is still in emptyCells after removal attempt. Forcing removal.");
            emptyCells.Remove(position);
        }
        
        // If the tile is not null, make sure it's physically at the correct position
        if (tile != null && tile.gameObject != null)
        {
            Vector2 worldPosition = GetWorldPosition(position);
            if (Vector2.Distance(tile.transform.position, worldPosition) > 0.1f)
            {
                Debug.Log($"BoardManager: Adjusting tile position to match grid position {position}");
                tile.transform.position = worldPosition;
            }
        }
    }

    /// <summary>
    /// Clear cell with enhanced validation
    /// </summary>
    public void ClearCell(Vector2Int position)
    {
        if (!IsWithinBounds(position))
        {
            Debug.LogWarning($"BoardManager: Attempted to clear cell at out-of-bounds position {position}");
            return;
        }

        // Get the current tile for logging
        Tile currentTile = board[position.x, position.y];
        
        // Clear the reference in the board array
        board[position.x, position.y] = null;
        
        // Improved emptyCells management with verification
        if (!emptyCells.Contains(position))
        {
            emptyCells.Add(position);
            Debug.Log($"BoardManager: Position {position} added to emptyCells during ClearCell (Previous tile: {(currentTile == null ? "null" : currentTile.number.ToString())})");
        }
        else
        {
            Debug.LogWarning($"BoardManager: Position {position} was already in emptyCells during ClearCell call - possible duplicate clearing");
        }
        
        // Verify physically empty by checking for any residual colliders
        Vector2 worldPos = GetWorldPosition(position);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.4f);
        foreach (var collider in colliders)
        {
            // Ignore triggers and UI elements
            if (collider.isTrigger || collider.gameObject.CompareTag("Highlight"))
                continue;
            
            // If we find a tile component, log a warning and attempt cleanup
            if (collider.GetComponent<Tile>() != null)
            {
                Debug.LogError($"BoardManager: ClearCell found physical tile at {position} that wasn't properly removed. Forcing destruction.");
                Destroy(collider.gameObject);
            }
        }
    }

    public void MarkCellAsOccupied(Vector2Int position)
    {
        if (!IsWithinBounds(position))
        {
            Debug.LogWarning($"BoardManager: Attempted to mark out-of-bounds position {position} as occupied");
            return;
        }
        
        if (emptyCells.Contains(position))
        {
            emptyCells.Remove(position);
            Debug.Log($"BoardManager: Position {position} removed from emptyCells in MarkCellAsOccupied");
        }
        
        // Extra verification: ensure the position actually has a tile
        if (board[position.x, position.y] == null)
        {
            // Check if there's a physical tile at this position that should be registered
            Vector2 worldPos = GetWorldPosition(position);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.4f);
            
            foreach (var collider in colliders)
            {
                if (!collider.isTrigger && collider.gameObject.CompareTag("Tile"))
                {
                    Tile tile = collider.GetComponent<Tile>();
                    if (tile != null)
                    {
                        Debug.LogWarning($"BoardManager: Found physical tile at {position} but board array shows null. Auto-registering tile.");
                        board[position.x, position.y] = tile;
                        break;
                    }
                }
            }
            
            // If we still don't have a tile, log a warning
            if (board[position.x, position.y] == null)
            {
                Debug.LogWarning($"BoardManager: MarkCellAsOccupied called for {position} but no tile is at this position in the board array.");
            }
        }
    }

    /// <summary>
    /// Re-registers a tile at a specific position, ensuring it's properly tracked.
    /// Used when a tile's properties change but its position remains the same.
    /// </summary>
    public void ReregisterTileAtPosition(Vector2Int position, Tile tile)
    {
        if (!IsWithinBounds(position))
        {
            Debug.LogWarning($"BoardManager: Attempted to re-register tile at out-of-bounds position {position}");
            return;
        }

        // Ensure the tile is properly set in the board array
        if (board[position.x, position.y] != tile)
        {
            Debug.LogWarning($"BoardManager: Re-registering tile at {position} - tile mismatch in board array. Fixing...");
            board[position.x, position.y] = tile;
        }
        
        // Ensure the position is marked as occupied
        if (emptyCells.Contains(position))
        {
            Debug.LogWarning($"BoardManager: Position {position} was incorrectly marked as empty during re-registration. Fixing...");
            emptyCells.Remove(position);
        }
        
        // Update the tile's position in world space to ensure consistency
        Vector2 worldPosition = GetWorldPosition(position);
        if (Vector2.Distance(tile.transform.position, worldPosition) > 0.1f)
        {
            Debug.LogWarning($"BoardManager: Tile at {position} was not at the correct world position. Adjusting position.");
            tile.transform.position = worldPosition;
        }
        
        // Call UpdateVisuals to ensure the tile display is updated
        tile.UpdateVisuals();
        
        Debug.Log($"BoardManager: Successfully re-registered tile at {position}");
    }

    /// <summary>
    /// Gets a random color from the tile color palette
    /// </summary>
    public Color GetRandomTileColor()
    {
        return tileColorPalette[Random.Range(0, tileColorPalette.Length)];
    }

    /// <summary>
    /// Checks if a cell is empty.
    /// </summary>
    public bool IsCellEmpty(Vector2Int position)
    {
        if (!IsWithinBounds(position))
            return false;

        bool isEmpty = board[position.x, position.y] == null;

        // Synchronize emptyCells with the board state without excessive logging
        if (isEmpty)
        {
            if (!emptyCells.Contains(position))
            {
                Debug.Log($"BoardManager: Synchronizing - adding empty cell {position} to emptyCells.");
                emptyCells.Add(position);
            }
        }
        else
        {
            if (emptyCells.Contains(position))
            {
                Debug.Log($"BoardManager: Synchronizing - removing occupied cell {position} from emptyCells.");
                emptyCells.Remove(position);
            }
        }

        // Extra validation: check for special tiles at this position
        if (isEmpty && SpecialTileManager.Instance != null)
        {
            // Fix: Use the correct method name for getting a special tile at a position
            SpecialTile specialTile = SpecialTileManager.Instance.GetTileAt(position);
            if (specialTile != null)
            {
                Debug.LogWarning($"BoardManager: Cell at {position} has special tile '{specialTile.specialAbilityName}'. Not empty.");
                return false;
            }
        }

        // Extra validation: ensure no unexpected physical colliders remain
        if (isEmpty)
        {
            Vector2 worldPos = GetWorldPosition(position);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.4f);
            foreach (var collider in colliders)
            {
                if (collider.isTrigger || collider.gameObject.CompareTag("Highlight"))
                    continue;
                if (collider.GetComponent<Tile>() != null || collider.GetComponent<SpecialTile>() != null)
                {
                    Debug.LogWarning($"BoardManager: Found collider at {position}. Auto-fixing by re-registering tile.");
                    Tile unregisteredTile = collider.GetComponent<Tile>();
                    if (unregisteredTile != null)
                    {
                        SetTileAtPosition(position, unregisteredTile);
                        emptyCells.Remove(position);
                        return false; // Now cell is not empty
                    }
                    return false;
                }
            }
        }
        
        return isEmpty;
    }

    public bool GenerateRandomStartingTiles(int minTiles = Constants.MIN_START_TILES, int maxTiles = Constants.MAX_START_TILES, Vector2Int? excludePosition = null)
    {
        int tileCount = Random.Range(minTiles, maxTiles + 1);
        List<Vector2Int> availableCells = new List<Vector2Int>(emptyCells);

        // Exclude adjacent cells of the specified position
        if (excludePosition.HasValue)
        {
            Vector2Int mergedCell = excludePosition.Value;
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                mergedCell + Vector2Int.up,
                mergedCell + Vector2Int.down,
                mergedCell + Vector2Int.left,
                mergedCell + Vector2Int.right
            };

            availableCells.RemoveAll(pos => pos == mergedCell || adjacentPositions.Contains(pos));
        }

        // Do a final validation pass to ensure all cells are truly empty
        for (int i = availableCells.Count - 1; i >= 0; i--)
        {
            Vector2Int pos = availableCells[i];
            // Check that board array and world objects agree this cell is empty
            if (!this.IsCellEmpty(pos))
            {
                Debug.LogWarning($"BoardManager: Cell at {pos} was in emptyCells but IsCellEmpty() returned false. Removing from available cells.");
                availableCells.RemoveAt(i);
                emptyCells.Remove(pos); // Also fix the emptyCells collection
            }
        }
        
        // Shuffle the available cells to ensure randomness
        ShuffleList(availableCells);

        // Ensure there are enough available cells
        if (availableCells.Count < tileCount)
        {
            tileCount = availableCells.Count;
            Debug.LogWarning($"BoardManager: Not enough empty cells for requested tile count. Reducing to {tileCount}.");
        }

        if (tileCount == 0)
        {
            Debug.LogWarning("BoardManager: No available cells to spawn tiles.");
            return false; // No tiles spawned
        }

        // Log all available cells for debugging
        Debug.Log($"BoardManager: Spawning {tileCount} tiles. Available cells: {string.Join(", ", availableCells)}");

        for (int i = 0; i < tileCount; i++)
        {
            Vector2Int spawnPosition = availableCells[i];
            // Extra check just to be sure
            if (!this.IsCellEmpty(spawnPosition))
            {
                Debug.LogError($"BoardManager: Cell at {spawnPosition} is not actually empty! Skipping tile spawn.");
                continue;
            }
            
            int randomNumber = Random.Range(Constants.MIN_TILE_NUMBER, Constants.MAX_TILE_NUMBER + 1);
            Color randomColor = tileColorPalette[Random.Range(0, tileColorPalette.Length)];

            // Ensure tilePrefab is assigned
            if (tilePrefab == null)
            {
                Debug.LogError("BoardManager: Tile prefab is not assigned. Cannot spawn tiles.");
                return false;
            }

            GameObject newTile = Instantiate(tilePrefab, GetWorldPosition(spawnPosition), Quaternion.identity, transform);
            Tile tileComponent = newTile.GetComponent<Tile>();

            if (tileComponent != null)
            {
                tileComponent.Initialize(randomColor, randomNumber);
                SetTileAtPosition(spawnPosition, tileComponent);
                MarkCellAsOccupied(spawnPosition);
                Debug.Log($"BoardManager: Spawned tile ({randomNumber}, {randomColor}) at {spawnPosition}");
            }
            else
            {
                Debug.LogError("BoardManager: Spawned tile does not have a Tile component.");
                Destroy(newTile);
            }
        }

        return true; // Tiles spawned successfully
    }

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

    public bool HasValidMove()
    {
        // Check for any empty cells - if there's an empty cell, there's a valid move
        if (emptyCells.Count > 0)
        {
            return true;
        }

        // If no empty cells, check for mergeable tiles within their movement range
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile currentTile = GetTileAtPosition(new Vector2Int(x, y));
                if (currentTile == null) continue;
                
                // Get the movement range of this tile (its number value)
                int moveRange = currentTile.number;
                Vector2Int currentPos = new Vector2Int(x, y);
                
                // Check in each orthogonal direction up to the tile's movement range
                foreach (Vector2Int dir in DirectionUtils.Orthogonal)
                {
                    for (int distance = 1; distance <= moveRange; distance++)
                    {
                        Vector2Int targetPos = currentPos + dir * distance;
                        
                        // Stop checking this direction if we go out of bounds
                        if (!IsWithinBounds(targetPos)) 
                            break;
                        
                        // Check if there's a tile at this position
                        Tile targetTile = GetTileAtPosition(targetPos);
                        
                        // If we hit a tile along the path...
                        if (targetTile != null)
                        {
                            // Check if it's the same color (mergeable)
                            if (TileMergeHandler.Instance.CompareColors(currentTile.tileColor, targetTile.tileColor))
                            {
                                // Also ensure path is clear up to this tile
                                if (TileMovementHandler.Instance.IsPathClear(currentPos, targetPos))
                                {
                                    Debug.Log($"HasValidMove: Found valid merge from {currentPos} to {targetPos} with distance {distance} <= tile number {moveRange}");
                                    return true; // Found a valid merge move
                                }
                            }
                            
                            // If this tile is not mergeable or path is blocked, stop checking this direction
                            break;
                        }
                        
                        // If we found an empty cell that's within range, it's a valid move
                        // (Note: This check shouldn't be reached because we already checked emptyCells.Count > 0,
                        // but it's here for completeness)
                        else 
                        {
                            // Check if path is clear to this empty position
                            if (TileMovementHandler.Instance.IsPathClear(currentPos, targetPos))
                            {
                                Debug.Log($"HasValidMove: Found valid movement to empty cell at {targetPos} with distance {distance} <= tile number {moveRange}");
                                return true; // Found a valid move to an empty cell
                            }
                        }
                    }
                }
            }
        }
        
        Debug.Log("HasValidMove: No valid moves found on the board.");
        return false; // No valid moves found
    }

    public void HandleTileSelection(Vector2Int gridPosition)
    {
        TileSelectionHandler.Instance?.HandleTileSelection(gridPosition);
    }

    public void HandleTileMoveConfirmation(Vector2Int targetPosition)
    {
        TileSelectionHandler.Instance?.HandleTileMoveConfirmation(targetPosition);
    }

    public void ClearSelection()
    {
        TileSelectionHandler.Instance?.ClearSelection();
    }

    public void ClearAllSelectionState()
    {
        TileSelectionHandler.Instance?.ClearAllSelectionState();
    }

    public void ClearHighlights()
    {
        TileSelectionHandler.Instance?.ClearHighlights();
    }

    public Vector2Int GetSelectedTilePosition()
    {
        return TileSelectionHandler.Instance != null ? 
            TileSelectionHandler.Instance.GetSelectedTilePosition() : Vector2Int.zero;
    }

    public Vector2Int GetTargetTilePosition()
    {
        return TileSelectionHandler.Instance != null ? 
            TileSelectionHandler.Instance.GetTargetTilePosition() : Vector2Int.zero;
    }

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
        ClearSelection();
    }

    public void PerformMergeOperation(Tile sourceTile, Tile targetTile, Vector2Int sourcePos, Vector2Int targetPos)
    {
        if (sourceTile == null || targetTile == null || !IsWithinBounds(sourcePos) || !IsWithinBounds(targetPos))
        {
            Debug.LogError($"BoardManager: Invalid merge operation. Source: {sourceTile}, Target: {targetTile}, SourcePos: {sourcePos}, TargetPos: {targetPos}");
            return;
        }

        ClearAllSelectionState();
        StartCoroutine(MoveTileToTargetForMerge(sourceTile, targetTile, () =>
        {
            // Clear the source cell on the board array
            ClearCell(sourcePos);
            emptyCells.Add(sourcePos);

            // Note: MergeTiles will now count the move automatically
            bool mergeSuccess = TileMergeHandler.Instance.MergeTiles(targetTile, sourceTile);
            Debug.Log($"BoardManager: Merge result: {(mergeSuccess ? "SUCCESS" : "FAILED")}");

            // Validate that the source tile is properly removed from the board array
            if (GetTileAtPosition(sourcePos) != null)
            {
                Debug.LogError($"BoardManager: Source tile at {sourcePos} was not properly removed from the board array after merging.");
                ClearCell(sourcePos); // Force clear it again
            }
            else
            {
                Debug.Log($"BoardManager: Source tile at {sourcePos} successfully removed from the board array.");
            }

            // Use our destruction utility if source tile GameObject still exists
            if (sourceTile != null && sourceTile.gameObject != null)
            {
                Debug.LogError("BoardManager: Source tile GameObject was not destroyed during merge. Forcing destruction using TileDestructionUtility.");
                DestroyTile(sourceTile, sourcePos);
            }

            if (mergeSuccess)
            {
                // Play merge animation
                TileAnimator animator = targetTile.GetComponent<TileAnimator>();
                animator?.PlayMergeAnimation();
                ClearSelection();
                ClearAllSelectionState();
                
                // IMPORTANT: Don't call EndTurn here as MergeTiles now does it
            }
            else
            {
                // Only call EndTurn in failure case, since TileMergeHandler.MergeTiles
                // would not have been called successfully
                GameManager.Instance.EndTurn();
            }
        }));
    }

    public int GetEmptyCellsCount()
    {
        return emptyCells.Count;
    }

    public void SpawnSpecialTile(Vector2Int position, string abilityName)
    {
        SpecialTileManager.Instance?.SpawnSpecialTile(position, abilityName);
    }

    public void PerformSplitOperation(Tile tile, Vector2Int position)
    {
        // Make sure the tile actually needs splitting (value > 12)
        if (tile == null || tile.number <= 12)
        {
            Debug.LogWarning($"BoardManager: PerformSplitOperation called for tile at {position} with value {(tile != null ? tile.number.ToString() : "null")}. Tile doesn't need splitting (value <= 12)");
            return;
        }
        
        Debug.Log($"BoardManager: Delegating split operation to TileSplitHandler for tile at {position} with value {tile.number}");
        
        // Save a reference to the tile's GameObject before delegating
        GameObject tileObj = tile.gameObject;
        
        // First make sure this tile position is properly tracked
        if (GetTileAtPosition(position) != tile)
        {
            Debug.LogWarning($"BoardManager: Tile at {position} is not properly registered in the board array. Fixing before split.");
            SetTileAtPosition(position, tile);
        }
        
        TileSplitHandler.PerformSplitOperation(tile, position);
        
        // EXPLICIT VERIFICATION: Check if original tile was properly removed
        if (GetTileAtPosition(position) == tile || GetTileAtPosition(position) != null)
        {
            Debug.LogWarning($"BoardManager: After splitting, position {position} still contains a tile! Forcing clear.");
            ClearCell(position);
            
            // Double-check the GameObject was destroyed
            if (tileObj != null)
            {
                Debug.LogError($"BoardManager: Original tile GameObject still exists after split! Destroying.");
                Destroy(tileObj);
            }
        }
        
        // Ensure this position is now in emptyCells
        if (!emptyCells.Contains(position))
        {
            Debug.LogWarning($"BoardManager: Position {position} not in emptyCells after splitting. Adding.");
            emptyCells.Add(position);
        }
    }

    /// <summary>
    /// Triggers the spawning of a special tile at a specified position.
    /// </summary>
    /// <param name="position">The grid position where the special tile should spawn.</param>
    public void TriggerSpecialTileSpawn(Vector2Int position)
    {
        // Double-check that we're not trying to spawn on an occupied cell
        if (!IsWithinBounds(position) || !this.IsCellEmpty(position))
        {
            Debug.LogWarning($"BoardManager: Cannot spawn special tile at {position}. Finding alternative position...");
            Vector2Int? alternatePosition = FindEmptyPositionNear(position);
            if (!alternatePosition.HasValue || !this.IsCellEmpty(alternatePosition.Value))
            {
                Debug.LogWarning($"BoardManager: Could not find a valid position for special tile spawn. Skipping.");
                return;
            }
            position = alternatePosition.Value;
        }

        // Triple-check the cell is actually empty
        if (!this.IsCellEmpty(position))
        {
            Debug.LogError($"BoardManager: CRITICAL - Cell at {position} is not empty but passed validation. Special tile spawn canceled.");
            // Resolve the inconsistency
            Tile existingTile = GetTileAtPosition(position);
            if (existingTile != null)
            {
                Debug.LogError($"BoardManager: Found tile with value {existingTile.number} at {position} while trying to spawn special tile!");
                return;
            }
            // Check for special tiles at this position
            SpecialTile specialTile = SpecialTileManager.Instance?.GetTileAt(position);
            if (specialTile != null)
            {
                Debug.LogError($"BoardManager: Found special tile '{specialTile.specialAbilityName}' at {position} while trying to spawn new special tile!");
                return;
            }
        }

        Debug.Log($"BoardManager: Confirmed cell at {position} is empty. Proceeding with special tile spawn.");
        if (Random.value < Constants.SPECIAL_TILE_CHANCE)
        {
            string[] specialTileTypes = { "Blaster", "Freeze", "Doubler", "Painter" };
            string randomSpecialTile = specialTileTypes[Random.Range(0, specialTileTypes.Length)];
            Debug.Log($"BoardManager: Spawning special tile '{randomSpecialTile}' at {position}.");
            SpecialTileManager.Instance.SpawnSpecialTile(position, randomSpecialTile);
        }
    }

    /// <summary>
    /// Finds an empty position near the specified position.
    /// </summary>
    private Vector2Int? FindEmptyPositionNear(Vector2Int position)
    {
        // First check if the original position is valid
        if (IsWithinBounds(position) && this.IsCellEmpty(position))
        {
            return position;
        }

        List<Vector2Int> candidatePositions = new List<Vector2Int>();
        
        // Check nearby positions in a spiral pattern
        for (int radius = 1; radius <= UnityEngine.Mathf.Max(width, height); radius++)
        {
            // Check positions in a square around the original position
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    // Only check positions on the perimeter of the square
                    if (UnityEngine.Mathf.Abs(dx) == radius || UnityEngine.Mathf.Abs(dy) == radius)
                    {
                        Vector2Int checkPos = new Vector2Int(position.x + dx, position.y + dy);
                        if (IsWithinBounds(checkPos) && this.IsCellEmpty(checkPos))
                        {
                            // Verify the position is really empty
                            if (GetTileAtPosition(checkPos) == null && 
                                // Fix: Use the correct method name
                                SpecialTileManager.Instance?.GetTileAt(checkPos) == null)
                            {
                                candidatePositions.Add(checkPos);
                            }
                        }
                    }
                }
            }
            // If we found at least one valid position, randomly choose one and return it
            if (candidatePositions.Count > 0)
            {
                Vector2Int selectedPos = candidatePositions[Random.Range(0, candidatePositions.Count)];
                Debug.Log($"BoardManager: Found empty position {selectedPos} near {position}.");
                return selectedPos;
            }
        }

        // If no positions are found in spiral search, check all empty cells
        if (emptyCells.Count > 0)
        {
            // Convert to list for random selection
            List<Vector2Int> allEmptyCells = new List<Vector2Int>(emptyCells);
            
            // Filter the list to ensure cells are actually empty
            allEmptyCells.RemoveAll(pos => GetTileAtPosition(pos) != null || 
                                      // Fix: Use the correct method name
                                      (SpecialTileManager.Instance?.GetTileAt(pos) != null));
            if (allEmptyCells.Count > 0)
            {
                Vector2Int randomEmptyCell = allEmptyCells[Random.Range(0, allEmptyCells.Count)];
                Debug.Log($"BoardManager: No nearby empty cells found. Using random empty cell {randomEmptyCell}.");
                return randomEmptyCell;
            }
        }

        // If we get here, there are no viable positions
        Debug.LogWarning($"BoardManager: No empty cells found on the entire board for special tile!");
        return null;
    }

    public void HandleSpecialTileActivation(Vector2Int gridPosition)
    {
        // Use SpecialTileManager to handle special tiles
        // Fix: Use the correct method name
        SpecialTile specialTile = SpecialTileManager.Instance.GetTileAt(gridPosition);
        if (specialTile != null)
        {
            Debug.Log($"BoardManager: Activating special tile '{specialTile.specialAbilityName}' at {gridPosition}");
            specialTile.Activate();
        }
        else
        {
            Debug.LogWarning("BoardManager: No special tile found at the selected position.");
        }
    }

    public void RegisterSpecialTile(SpecialTile specialTile)
    {
        if (!specialTiles.Contains(specialTile)) 
        {
            specialTiles.Add(specialTile);
        }
    }

    public void UnregisterSpecialTile(SpecialTile specialTile)
    {
        if (specialTiles.Contains(specialTile))
        {
            specialTiles.Remove(specialTile);
        }
    }

    public IEnumerable<Vector2Int> GetAllEmptyCells()
    {
        return emptyCells;
    }

    /// <summary>
    /// Skips the next tile spawn.
    /// </summary>
    public void SkipNextTileSpawn()
    {
        skipNextSpawn = true;
        Debug.Log("BoardManager: Next tile spawn will be skipped.");
    }

    private void SpawnTile()
    {
        if (skipNextSpawn)
        {
            skipNextSpawn = false;
            Debug.Log("BoardManager: Tile spawn skipped.");
            return;
        }
        // ...existing tile spawn logic...
    }

    public void AddToEmptyCells(Vector2Int position)
    {
        if (IsWithinBounds(position))
        {
            if (board[position.x, position.y] != null)
            {
                Debug.LogWarning($"BoardManager: Tried to add position {position} to emptyCells but it's still occupied! Clearing cell first.");
                
                // Check what's at this position for debugging
                Tile existingTile = board[position.x, position.y];
                if (existingTile != null)
                {
                    Debug.LogError($"BoardManager: Found tile with value {existingTile.number} at {position} in board array.");
                    
                    // Verify if the GameObject still exists
                    if (existingTile.gameObject != null)
                    {
                        Debug.LogError($"BoardManager: Tile GameObject at {position} still exists with name {existingTile.gameObject.name}. Destroying it.");
                        Destroy(existingTile.gameObject);
                    }
                    else
                    {
                        Debug.Log($"BoardManager: Tile GameObject is already null, just clearing the cell reference.");
                    }
                }
                // Always clear the cell in the data structure
                ClearCell(position); 
            }
            // Check for any physical objects at this position (belt and suspenders approach)
            Vector2 worldPos = GetWorldPosition(position);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.4f);
            foreach (var collider in colliders)
            {
                if (collider.isTrigger) continue; // Skip triggers
                if (collider.gameObject.CompareTag("Highlight")) continue; // Skip UI elements
                
                Debug.LogError($"BoardManager: Found physical collider at position {position} for GameObject {collider.gameObject.name}. This should be cleared before adding to emptyCells!");
                // Check if it's a tile or special tile
                Tile tileComp = collider.GetComponent<Tile>();
                SpecialTile specialTileComp = collider.GetComponent<SpecialTile>();
                if (tileComp != null || specialTileComp != null)
                {
                    Debug.LogError($"BoardManager: Destroying unexpected {(tileComp != null ? "Tile" : "SpecialTile")} at position {position}");
                    Destroy(collider.gameObject);
                }
            }
            
            // Finally add to empty cells if it's not already there
            if (!emptyCells.Contains(position))
            {
                emptyCells.Add(position);
                Debug.Log($"BoardManager: Position {position} added to emptyCells collection");
            }
            else
            {
                Debug.Log($"BoardManager: Position {position} was already in emptyCells collection");
            }
        }
        else
        {
            Debug.LogWarning($"BoardManager: Tried to add out-of-bounds position {position} to emptyCells");
        }
    }

    public void DestroyTile(Tile tile, Vector2Int position)
    {
        if (tile == null || !IsWithinBounds(position))
        {
            Debug.LogError($"BoardManager: Invalid destroy operation. Tile: {tile}, Position: {position}");
            return;
        }

        // Delegate to the TileDestructionHandler for proper destruction handling
        TileDestructionHandler.Instance.DestroyTile(tile.gameObject, position);
    }

    public void SpawnTile(Vector2Int position, int number, Color color)
    {
        if (!IsCellEmpty(position))
        {
            Debug.LogError($"BoardManager: Cannot spawn tile at {position} because the cell is not empty.");
            return;
        }

        GameObject newTile = Instantiate(tilePrefab, GetWorldPosition(position), Quaternion.identity, transform);
        Tile tileComponent = newTile.GetComponent<Tile>();
        if (tileComponent != null)
        {
            tileComponent.Initialize(color, number);
            SetTileAtPosition(position, tileComponent);
            // Log the spawning operation
            Debug.Log($"BoardManager: Spawned tile with number {number} and color {color} at {position}.");
        }
        else
        {
            Debug.LogError("BoardManager: Spawned tile does not have a Tile component.");
            Destroy(newTile);
        }
    }

    /// <summary>
    /// Verifies and corrects the consistency between the board data structure and the actual tile objects in the scene.
    /// </summary>
    public void VerifyBoardConsistency()
    {
        Debug.Log("BoardManager: Verifying board consistency");
        
        // Step 1: Ensure all tiles in the board array actually exist at the correct position
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Tile boardTile = GetTileAtPosition(pos);
                
                if (boardTile != null)
                {
                    // Verify the tile's world position matches where it should be
                    Vector2 expectedWorldPos = GetWorldPosition(pos);
                    Vector2 actualWorldPos = boardTile.transform.position;
                    
                    // If the tile exists but is not at the expected position
                    if (Vector2.Distance(expectedWorldPos, actualWorldPos) > 0.1f)
                    {
                        Debug.LogWarning($"BoardManager: Tile position mismatch at {pos}. Expected: {expectedWorldPos}, Actual: {actualWorldPos}. Correcting position.");
                        boardTile.transform.position = expectedWorldPos;
                    }
                }
            }
        }
        
        // Step 2: Find all physical tiles in the scene and ensure they're properly registered in the board
        foreach (Tile tile in FindObjectsOfType<Tile>())
        {
            if (tile.GetComponent<SpecialTile>() != null)
                continue; // Skip special tiles
                
            Vector2Int gridPos = GetGridPositionFromWorldPosition(tile.transform.position);
            
            if (!IsWithinBounds(gridPos))
            {
                Debug.LogError($"BoardManager: Tile at {tile.transform.position} is out of bounds at grid position {gridPos}. Destroying it.");
                Destroy(tile.gameObject);
                continue;
            }
            
            Tile boardTile = GetTileAtPosition(gridPos);
            
            // If there's a tile in the scene at a position where the board shows empty
            if (boardTile == null)
            {
                Debug.LogWarning($"BoardManager: Found physical tile at {gridPos} but board shows empty. Registering tile.");
                SetTileAtPosition(gridPos, tile);
            }
            // If there's a different tile in the board array than physically exists
            else if (boardTile != tile)
            {
                Debug.LogError($"BoardManager: Tile mismatch at {gridPos}. Board has {boardTile.name} but found {tile.name}. Prioritizing physical tile.");
                
                // If the board tile is still in the scene, destroy it to avoid duplicates
                if (boardTile != null && boardTile.gameObject != null)
                {
                    Debug.LogWarning($"BoardManager: Destroying duplicate tile {boardTile.name} at {gridPos}");
                    Destroy(boardTile.gameObject);
                }
                
                SetTileAtPosition(gridPos, tile);
            }
        }
        
        // Step 3: Fix emptyCells collection to match the board state
        HashSet<Vector2Int> correctEmptyCells = new HashSet<Vector2Int>();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Tile tile = GetTileAtPosition(pos);
                
                if (tile == null && IsWithinBounds(pos))
                {
                    correctEmptyCells.Add(pos);
                }
            }
        }
        
        // Update the emptyCells collection
        int addedCount = 0;
        int removedCount = 0;
        
        // Add cells that should be empty but aren't in the collection
        foreach (Vector2Int pos in correctEmptyCells)
        {
            if (!emptyCells.Contains(pos))
            {
                emptyCells.Add(pos);
                addedCount++;
            }
        }
        
        // Remove cells that are in the collection but shouldn't be empty
        List<Vector2Int> cellsToRemove = new List<Vector2Int>();
        foreach (Vector2Int pos in emptyCells)
        {
            if (!correctEmptyCells.Contains(pos))
            {
                cellsToRemove.Add(pos);
                removedCount++;
            }
        }
        
        foreach (Vector2Int pos in cellsToRemove)
        {
            emptyCells.Remove(pos);
        }
        
        Debug.Log($"BoardManager: Board consistency check complete. Added {addedCount} empty cells, removed {removedCount} invalid empty cells.");
    }

    /// <summary>
    /// Performs a comprehensive verification of the emptyCells collection to ensure it accurately represents the board state
    /// </summary>
    public void ValidateEmptyCellsCollection()
    {
        Debug.Log("BoardManager: Validating emptyCells collection");
        HashSet<Vector2Int> actualEmptyCells = new HashSet<Vector2Int>();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (board[x, y] == null)
                {
                    actualEmptyCells.Add(pos);
                }
            }
        }
        
        // Find cells that should be marked as empty but aren't
        int missingCount = 0;
        foreach (Vector2Int pos in actualEmptyCells)
        {
            if (!emptyCells.Contains(pos))
            {
                Debug.LogError($"BoardManager: Position {pos} should be empty but is not in emptyCells. Adding it.");
                emptyCells.Add(pos);
                missingCount++;
            }
        }
        
        // Find cells that are incorrectly marked as empty
        List<Vector2Int> incorrectlyMarkedEmpty = new List<Vector2Int>();
        foreach (Vector2Int pos in emptyCells)
        {
            if (!actualEmptyCells.Contains(pos))
            {
                Debug.LogError($"BoardManager: Position {pos} is in emptyCells but actually contains a tile. Removing it.");
                incorrectlyMarkedEmpty.Add(pos);
            }
        }
        
        // Remove the incorrect entries
        foreach (Vector2Int pos in incorrectlyMarkedEmpty)
        {
            emptyCells.Remove(pos);
        }
        
        Debug.Log($"BoardManager: emptyCells validation completed. Added {missingCount} missing entries, removed {incorrectlyMarkedEmpty.Count} incorrect entries.");
    }

    /// <summary>
    /// Checks if there's a currently selected tile
    /// </summary>
    /// <returns>True if a tile is selected, false otherwise</returns>
    public bool HasSelectedTile()
    {
        return TileSelectionHandler.Instance != null && TileSelectionHandler.Instance.HasSelectedTile();
    }

    /// <summary>
    /// Checks if a cell is empty or mergeable with the given tile.
    /// </summary>
    public bool IsCellEmptyOrMergeable(Vector2Int position, Tile tile)
    {
        if (!IsWithinBounds(position)) return false;
        Tile targetTile = GetTileAtPosition(position);
        return targetTile == null || TileMergeHandler.Instance.CompareColors(tile.tileColor, targetTile.tileColor);
    }

    /// <summary>
    /// Registers a tile that was created from splitting at the given position.
    /// </summary>
    public void RegisterSplitTile(Vector2Int position, Tile tile)
    {
        // Skip invalid positions
        if (!IsWithinBounds(position))
        {
            Debug.LogWarning($"BoardManager: Attempted to register split tile at out-of-bounds position {position}");
            return;
        }

        // Make sure we're not putting a tile in an occupied cell
        if (GetTileAtPosition(position) != null)
        {
            Debug.LogWarning($"BoardManager: Attempted to register split tile at occupied position {position}");
            return;
        }

        // Add the tile to the board at the specified position
        SetTileAtPosition(position, tile);
        
        // Mark the cell as occupied so it's not used for spawning
        if (emptyCells.Contains(position))
        {
            emptyCells.Remove(position);
        }
        
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
        
        Debug.Log($"BoardManager: Registered split tile with value {tile.number} at position {position}");
    }

    /// <summary>
    /// Validate and fix the emptyCells collection after state transitions
    /// </summary>
    public void ValidateEmptyCellsAfterStateChange()
    {
        Debug.Log("BoardManager: Validating emptyCells collection after state transition");
        
        // Check for inconsistencies in both directions
        bool foundIssue = false;
        
        // Check 1: cells that are marked empty but contain tiles in the board array
        List<Vector2Int> incorrectlyMarkedEmpty = new List<Vector2Int>();
        foreach (Vector2Int pos in emptyCells)
        {
            if (board[pos.x, pos.y] != null)
            {
                foundIssue = true;
                Debug.LogError($"BoardManager: Position {pos} is marked as empty but contains tile with value {board[pos.x, pos.y].number}");
                incorrectlyMarkedEmpty.Add(pos);
            }
        }
        
        // Check 2: cells that should be empty but aren't in the emptyCells collection
        int missingEmptyCount = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (board[x, y] == null && !emptyCells.Contains(pos))
                {
                    foundIssue = true;
                    Debug.LogError($"BoardManager: Position {pos} contains no tile but is not in emptyCells collection");
                    emptyCells.Add(pos);
                    missingEmptyCount++;
                }
            }
        }
        
        // Remove incorrectly marked cells
        foreach (Vector2Int pos in incorrectlyMarkedEmpty)
        {
            emptyCells.Remove(pos);
        }
        
        if (foundIssue)
        {
            Debug.LogWarning($"BoardManager: Fixed emptyCells collection: removed {incorrectlyMarkedEmpty.Count} incorrect entries, added {missingEmptyCount} missing entries");
        }
        else
        {
            Debug.Log("BoardManager: emptyCells collection is consistent with board state");
        }
    }

    // Call this method after key state transitions
    public void PostMergeCleanup()
    {
        Debug.Log("BoardManager: Running post-merge cleanup");
        ValidateEmptyCellsAfterStateChange();
        
        // Clean up any destroyed but lingering GameObjects
        CleanupDestroyedTiles();
        
        // Re-sync the visuals with the data
        UpdateAllTileVisuals();
    }

    /// <summary>
    /// Cleanup any destroyed but lingering tile GameObjects
    /// </summary>
    private void CleanupDestroyedTiles()
    {
        // Find all tiles with null references in the board array
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = board[x, y];
                if (tile != null && (tile.gameObject == null || !tile.gameObject.activeInHierarchy))
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Debug.LogWarning($"BoardManager: Found null/inactive tile reference at {pos}. Clearing cell.");
                    ClearCell(pos);
                }
            }
        }
        
        // Also check for tiles in the scene that might not be properly tracked
        foreach (Tile tile in FindObjectsOfType<Tile>())
        {
            if (tile.GetComponent<SpecialTile>() != null)
                continue; // Skip special tiles
            
            // Get grid position for this tile
            Vector2Int gridPos = GetGridPositionFromWorldPosition(tile.transform.position);
            if (!IsWithinBounds(gridPos))
            {
                Debug.LogError($"BoardManager: Found tile outside board bounds at world position {tile.transform.position}. Destroying it.");
                Destroy(tile.gameObject);
                continue;
            }
            
            // Check if this physical tile is properly tracked in the board array
            Tile trackedTile = GetTileAtPosition(gridPos);
            if (trackedTile == null)
            {
                Debug.LogWarning($"BoardManager: Found unregistered tile at {gridPos}. Registering it to the board array.");
                SetTileAtPosition(gridPos, tile);
            }
            else if (trackedTile != tile)
            {
                Debug.LogError($"BoardManager: Position {gridPos} has mismatched tile references. Fixing...");
                DestroyTile(trackedTile, gridPos);
                SetTileAtPosition(gridPos, tile);
            }
        }
    }

    /// <summary>
    /// Update visuals for all tiles on the board
    /// </summary>
    private void UpdateAllTileVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = board[x, y];
                if (tile != null && tile.gameObject != null)
                {
                    tile.UpdateVisuals();
                }
            }
        }
    }

    /// <summary>
    /// Checks if the board array data structure matches the physical tiles in the scene
    /// </summary>
    public bool VerifyBoardMatchesScene()
    {
        Debug.Log("BoardManager: Verifying board array matches scene tiles");
        bool isConsistent = true;
        int mismatchCount = 0;
        
        // First check: Each tile in board array should have a matching GameObject in the scene
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Tile boardTile = board[x, y];
                
                // If there's a tile in the board array, verify the GameObject exists
                if (boardTile != null)
                {
                    if (boardTile.gameObject == null)
                    {
                        Debug.LogError($"BoardManager: Tile at {pos} exists in board array but GameObject is null");
                        isConsistent = false;
                        mismatchCount++;
                    }
                    else if (!boardTile.gameObject.activeInHierarchy)
                    {
                        Debug.LogError($"BoardManager: Tile at {pos} exists in board array but GameObject is inactive");
                        isConsistent = false;
                        mismatchCount++;
                    }
                    
                    // Verify the tile's world position matches where it should be
                    Vector2 expectedWorldPos = GetWorldPosition(pos);
                    Vector2 actualWorldPos = boardTile.transform.position;
                    
                    if (Vector2.Distance(expectedWorldPos, actualWorldPos) > 0.1f)
                    {
                        Debug.LogWarning($"BoardManager: Tile position mismatch at {pos}. Expected: {expectedWorldPos}, Actual: {actualWorldPos}");
                        isConsistent = false;
                        mismatchCount++;
                    }
                }
            }
        }
        
        // Second check: Each physical tile in the scene should be in the board array
        foreach (Tile sceneTile in FindObjectsOfType<Tile>())
        {
            if (sceneTile.GetComponent<SpecialTile>() != null)
                continue; // Skip special tiles
                
            Vector2Int gridPos = GetGridPositionFromWorldPosition(sceneTile.transform.position);
            
            if (!IsWithinBounds(gridPos))
            {
                Debug.LogError($"BoardManager: Found physical tile at {sceneTile.transform.position} which is outside board bounds");
                isConsistent = false;
                mismatchCount++;
                continue;
            }
            
            Tile boardTile = board[gridPos.x, gridPos.y];
            
            // If the board position doesn't have this tile
            if (boardTile == null)
            {
                Debug.LogError($"BoardManager: Physical tile at {gridPos} exists in scene but not in board array");
                isConsistent = false;
                mismatchCount++;
            }
            else if (boardTile != sceneTile)
            {
                Debug.LogError($"BoardManager: Different tile references at {gridPos}. Board has {boardTile.name} but scene has {sceneTile.name}");
                isConsistent = false;
                mismatchCount++;
            }
        }
        
        // Log the result
        if (isConsistent)
        {
            Debug.Log("BoardManager: Board array perfectly matches scene tiles");
        }
        else
        {
            Debug.LogWarning($"BoardManager: Found {mismatchCount} mismatches between board array and scene tiles");
        }
        
        return isConsistent;
    }

    /// <summary>
    /// Fixes any mismatches between the board array and physical tiles in the scene
    /// </summary>
    public void RepairBoardSceneMismatch()
    {
        Debug.Log("BoardManager: Repairing mismatches between board array and scene tiles");
        int fixCount = 0;
        
        // Create a dictionary of all tiles in the scene
        Dictionary<Vector2Int, Tile> sceneTiles = new Dictionary<Vector2Int, Tile>();
        foreach (Tile sceneTile in FindObjectsOfType<Tile>())
        {
            if (sceneTile.GetComponent<SpecialTile>() != null)
                continue; // Skip special tiles
                
            Vector2Int gridPos = GetGridPositionFromWorldPosition(sceneTile.transform.position);
            if (IsWithinBounds(gridPos))
            {
                // If multiple tiles at the same position, take the one closest to the grid position
                if (sceneTiles.ContainsKey(gridPos))
                {
                    Vector2 expectedWorldPos = GetWorldPosition(gridPos);
                    Vector2 existingDist = (Vector2)sceneTiles[gridPos].transform.position - expectedWorldPos;
                    Vector2 newDist = (Vector2)sceneTile.transform.position - expectedWorldPos;
                    
                    if (newDist.sqrMagnitude < existingDist.sqrMagnitude)
                    {
                        Debug.LogWarning($"BoardManager: Found multiple tiles at {gridPos}, keeping the one closest to grid center");
                        Destroy(sceneTiles[gridPos].gameObject);
                        sceneTiles[gridPos] = sceneTile;
                        fixCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"BoardManager: Found multiple tiles at {gridPos}, destroying the one further from grid center");
                        Destroy(sceneTile.gameObject);
                        fixCount++;
                    }
                }
                else
                {
                    sceneTiles.Add(gridPos, sceneTile);
                }
            }
            else
            {
                Debug.LogError($"BoardManager: Found tile outside bounds at {sceneTile.transform.position}. Destroying it.");
                Destroy(sceneTile.gameObject);
                fixCount++;
            }
        }
        
        // Now fix the board array using scene data
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Tile boardTile = board[x, y];
                bool hasSceneTile = sceneTiles.TryGetValue(pos, out Tile sceneTile);
                
                // Case 1: Board has tile but scene doesn't
                if (boardTile != null && !hasSceneTile)
                {
                    Debug.LogWarning($"BoardManager: Board has tile at {pos} but no scene tile exists. Clearing board cell.");
                    ClearCell(pos);
                    if (!emptyCells.Contains(pos))
                    {
                        emptyCells.Add(pos);
                    }
                    fixCount++;
                }
                // Case 2: Scene has tile but board doesn't
                else if (boardTile == null && hasSceneTile)
                {
                    Debug.LogWarning($"BoardManager: Scene has tile at {pos} but board cell is empty. Registering tile.");
                    SetTileAtPosition(pos, sceneTile);
                    if (emptyCells.Contains(pos))
                    {
                        emptyCells.Remove(pos);
                    }
                    fixCount++;
                }
                // Case 3: Both have tiles but they're different
                else if (boardTile != null && hasSceneTile && boardTile != sceneTile)
                {
                    Debug.LogWarning($"BoardManager: Mismatched tiles at {pos}. Prioritizing scene tile.");
                    board[x, y] = sceneTile;
                    fixCount++;
                }
                // Case 4: Both match - nothing to do
                
                // Extra: If scene tile exists, make sure it's positioned correctly
                if (hasSceneTile)
                {
                    Vector2 expectedWorldPos = GetWorldPosition(pos);
                    if (Vector2.Distance((Vector2)sceneTile.transform.position, expectedWorldPos) > 0.1f)
                    {
                        Debug.Log($"BoardManager: Fixing tile position at {pos}");
                        sceneTile.transform.position = expectedWorldPos;
                        fixCount++;
                    }
                }
            }
        }
        
        Debug.Log($"BoardManager: Finished repairing board-scene mismatches. Fixed {fixCount} issues.");
    }

    /// <summary>
    /// Places a blocked tile at the specified position
    /// </summary>
    public void PlaceBlockedTile(Vector2Int position)
    {
        if (!IsWithinBounds(position))
        {
            Debug.LogWarning($"BoardManager: Cannot place blocked tile at out-of-bounds position {position}");
            return;
        }

        // Check if the position is already occupied
        if (!IsCellEmpty(position))
        {
            Debug.LogWarning($"BoardManager: Cannot place blocked tile at occupied position {position}");
            return;
        }

        // Create a blocked tile GameObject
        GameObject blockedTile = new GameObject($"BlockedTile_{position.x}_{position.y}");
        blockedTile.transform.position = GetWorldPosition(position);
        blockedTile.transform.SetParent(transform);

        // Add a sprite renderer component
        SpriteRenderer renderer = blockedTile.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateBlockedTileSprite();
        renderer.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        renderer.sortingOrder = 0;

        // Add a collider to block movement
        BoxCollider2D collider = blockedTile.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(cellSize * 0.9f, cellSize * 0.9f);

        // Add a blocked tile component
        BlockedTile blockedTileComponent = blockedTile.AddComponent<BlockedTile>();
        blockedTile.tag = "BlockedTile";

        // Mark the cell as occupied
        MarkCellAsOccupied(position);

        Debug.Log($"BoardManager: Placed blocked tile at {position}");
    }

    /// <summary>
    /// Creates a sprite for blocked tiles
    /// </summary>
    private Sprite CreateBlockedTileSprite()
    {
        int textureSize = 128;
        // Fix 9: Correct Texture2D constructor by providing width and height parameters
        Texture2D texture = new Texture2D(textureSize, textureSize);

        Color solidColor = new Color(0.25f, 0.25f, 0.25f);
        Color transparentColor = new Color(0.25f, 0.25f, 0.25f, 0f);
        float borderWidth = 0.15f;

        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                float normalizedX = x / (float)textureSize;
                float normalizedY = y / (float)textureSize;

                // Create a solid square with rounded corners and a border
                bool isInside = normalizedX >= borderWidth && normalizedX <= (1 - borderWidth) &&
                               normalizedY >= borderWidth && normalizedY <= (1 - borderWidth);

                // Draw X pattern
                bool isOnX = UnityEngine.Mathf.Abs(normalizedX - normalizedY) < 0.08f || 
                             UnityEngine.Mathf.Abs(normalizedX - (1 - normalizedY)) < 0.08f;

                if (isInside)
                {
                    texture.SetPixel(x, y, isOnX ? solidColor : new Color(0.3f, 0.3f, 0.3f));
                }
                else
                {
                    // Create rounded corners
                    float distFromCenter = UnityEngine.Mathf.Sqrt(
                        UnityEngine.Mathf.Pow(normalizedX - 0.5f, 2) + 
                        UnityEngine.Mathf.Pow(normalizedY - 0.5f, 2));
                    
                    texture.SetPixel(x, y, distFromCenter < 0.5f ? solidColor : transparentColor);
                }
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>
    /// Spawns a new tile after a move if the move was valid.
    /// </summary>
    public void SpawnTileAfterMove()
    {
        if (skipNextSpawn)
        {
            skipNextSpawn = false;
            Debug.Log("BoardManager: Tile spawn skipped.");
            return;
        }

        // Ensure there are empty cells available
        if (emptyCells.Count == 0)
        {
            Debug.LogWarning("BoardManager: No empty cells available for spawning a new tile.");
            return;
        }

        // Select a random empty cell
        Vector2Int spawnPosition = emptyCells.ElementAt(Random.Range(0, emptyCells.Count));

        // Generate a random tile value and color
        int randomNumber = Random.Range(Constants.MIN_TILE_NUMBER, Constants.MAX_TILE_NUMBER + 1);
        Color randomColor = tileColorPalette[Random.Range(0, tileColorPalette.Length)];

        // Spawn the tile
        SpawnTile(spawnPosition, randomNumber, randomColor);
        Debug.Log($"BoardManager: Spawned tile with value {randomNumber} at {spawnPosition}.");
    }

    /// <summary>
    /// Call this method after a move to handle spawning and other post-move logic.
    /// </summary>
    public void HandlePostMove()
    {
        Debug.Log("BoardManager: Handling post-move logic.");
        SpawnTileAfterMove();

        // Check if there are any valid moves left
        if (!HasValidMove())
        {
            Debug.LogWarning("BoardManager: No valid moves left. Triggering game over.");
            GameOverManager.Instance?.CheckGameOver();
        }
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
            Debug.LogWarning("BoardManager: Cannot validate move for null tile.");
            return false;
        }

        // Check if the target is the same as start (no movement)
        if (targetPosition == startPosition)
        {
            return false;
        }

        // Check if positions are within bounds
        if (!IsWithinBounds(startPosition) || !IsWithinBounds(targetPosition))
        {
            Debug.LogWarning($"BoardManager: Out of bounds positions - Start: {startPosition}, Target: {targetPosition}");
            return false;
        }

        // Check if the move is orthogonal (along one axis only)
        Vector2Int direction = targetPosition - startPosition;
        bool isOrthogonal = direction.x == 0 || direction.y == 0;

        if (!isOrthogonal)
        {
            Debug.Log("BoardManager: Move is not orthogonal.");
            return false;
        }

        // Check if the distance is within the tile's number (movement range)
        int distance = Mathf.Abs(direction.x) + Mathf.Abs(direction.y);
        if (distance > tile.number)
        {
            Debug.Log($"BoardManager: Distance {distance} exceeds tile movement range {tile.number}.");
            return false;
        }

        // Check if the path is clear
        pathClear = TileMovementHandler.Instance.IsPathClear(startPosition, targetPosition);

        return isOrthogonal && distance <= tile.number && pathClear;
    }
}