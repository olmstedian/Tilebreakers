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

    private Tile selectedTile;
    private Vector2Int selectedTilePosition;
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
                float dx = Mathf.Max(Mathf.Abs(scaledX) - width/2f + radius, 0);
                float dy = Mathf.Max(Mathf.Abs(scaledY) - height/2f + radius, 0);
                
                // If in corner region, calculate distance from corner
                float distanceFromCorner = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (distanceFromCorner <= radius)
                    texture.SetPixel(x, y, whiteColor);
                else if (Mathf.Abs(scaledX) <= width/2f && Mathf.Abs(scaledY) <= height/2f)
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
            float t = (1 + Mathf.Sin(Time.time * 2 * Mathf.PI / period)) / 2;
            float scale = Mathf.Lerp(minScale, maxScale, t);
            
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
        if (tile == null || !IsWithinBounds(startPosition) || !IsWithinBounds(targetPosition))
        {
            Debug.LogError($"BoardManager: Invalid move operation. Tile: {tile}, Start: {startPosition}, Target: {targetPosition}");
            return;
        }

        if (tile.hasMerged)
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
        
        MarkCellAsOccupied(targetPosition); // Ensure the target cell is removed from emptyCells
    }

    private IEnumerator MoveTileAndReenable(Tile tile, Vector2 targetPosition, float duration, Collider2D tileCollider, int originalSortingOrder, SpriteRenderer sr)
    {
        // Set the tile state to Moving before starting animation
        if (tile != null)
        {
            tile.SetState(Tile.TileState.Moving);
        }
        
        IEnumerator movementCoroutine = null;
        bool completed = false;
        
        try
        {
            // Store the coroutine reference but don't yield inside the try block
            movementCoroutine = tile.GetComponent<TileMover>().MoveTile(targetPosition, duration);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BoardManager: Error during tile movement animation: {e.Message}");
            completed = true;
        }
        
        // Yield the coroutine outside the try block if it was created successfully
        if (movementCoroutine != null && !completed)
        {
            yield return StartCoroutine(movementCoroutine);
        }
        
        // After movement completes (or errors), re-enable collider and restore sorting order
        // This will run even if there's an exception during the animation
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
    
    private IEnumerator MoveTileToTargetForMerge(Tile sourceTile, Tile targetTile, System.Action onComplete)
    {
        // Track the source position for proper board cleanup
        Vector2Int sourcePos = GetGridPositionFromWorldPosition(sourceTile.transform.position);
        
        // Disable collider during animation
        Collider2D tileCollider = sourceTile.GetComponent<Collider2D>();
        if (tileCollider != null)
        {
            tileCollider.enabled = false;
        }
        
        // Track the source tile GameObject for later verification
        GameObject sourceTileObject = sourceTile.gameObject;
        
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
        
        // Double check that the source cell is cleared properly
        ClearCell(sourcePos);
        if (!emptyCells.Contains(sourcePos))
        {
            emptyCells.Add(sourcePos);
        }
        
        // Execute the callback when movement is complete
        if (onComplete != null)
        {
            onComplete();
        }
        
        // Verify that the source tile was destroyed
        yield return new WaitForSeconds(0.1f); // Short delay to allow destruction to occur
        
        if (sourceTileObject != null)
        {
            Debug.LogWarning($"BoardManager: Source tile at {sourcePos} may not have been properly destroyed after merge. Double-checking.");
            
            // Check if the GameObject still exists in the scene
            if (sourceTileObject)
            {
                Debug.LogError($"BoardManager: Source tile at {sourcePos} was not destroyed after merge. Destroying it now.");
                Destroy(sourceTileObject);
            }
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

    public void SetTileAtPosition(Vector2Int position, Tile tile)
    {
        if (!IsWithinBounds(position))
        {
            Debug.LogError($"BoardManager: Attempted to set tile at out-of-bounds position {position}");
            return;
        }
        
        board[position.x, position.y] = tile;
        
        // Ensure the cell is marked occupied by removing it from emptyCells
        if (emptyCells.Contains(position))
        {
            emptyCells.Remove(position);
            Debug.Log($"BoardManager: Position {position} removed from emptyCells during SetTileAtPosition");
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

    public void ClearCell(Vector2Int position)
    {
        if (!IsWithinBounds(position))
        {
            Debug.LogWarning($"BoardManager: Attempted to clear cell at out-of-bounds position {position}");
            return;
        }

        // Clear the reference in the board array
        board[position.x, position.y] = null;
        
        // Ensure the cell is marked empty by adding it to emptyCells
        if (!emptyCells.Contains(position))
        {
            emptyCells.Add(position);
            Debug.Log($"BoardManager: Position {position} added to emptyCells during ClearCell");
        }
        
        // Optional: Check for any physical objects that might still be at this position
        Vector2 worldPos = GetWorldPosition(position);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.4f);
        foreach (var collider in colliders)
        {
            // Ignore certain objects
            if (collider.isTrigger || collider.gameObject.CompareTag("Highlight"))
                continue;
            
            // If we find a tile component, log a warning
            if (collider.GetComponent<Tile>() != null)
            {
                Debug.LogWarning($"BoardManager: ClearCell called for {position} but found a physical tile still at this position. This may indicate a synchronization issue.");
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
            SpecialTile specialTile = SpecialTileManager.Instance.GetSpecialTileAtPosition(position);
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
                            if (CompareColors(currentTile.tileColor, targetTile.tileColor))
                            {
                                // Also ensure path is clear up to this tile
                                if (IsPathClear(currentPos, targetPos))
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
                            if (IsPathClear(currentPos, targetPos))
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

    /// <summary>
    /// Explicitly clears the selection state (selected tile and position)
    /// </summary>
    public void ClearSelection()
    {
        Debug.Log("BoardManager: Clearing tile selection state");
        
        if (selectedTile != null)
        {
            selectedTile.ClearSelectionState();
            selectedTile = null;
        }
        
        selectedTilePosition = Vector2Int.zero;
        
        ClearHighlights();
        ClearSelectionHighlights();
    }

    /// <summary>
    /// Clears all selection states, including selected tiles and highlights.
    /// </summary>
    public void ClearAllSelectionState()
    {
        if (selectedTile != null)
        {
            selectedTile.ClearSelectionState();
            selectedTile = null;
        }

        selectedTilePosition = Vector2Int.zero;
        ClearHighlights(); // This already handles "Highlight" tagged objects
        ClearSelectionHighlights(); // This now uses our component-based approach
    }

    public void HandleTileSelection(Vector2Int gridPosition)
    {
        // First, verify that we're in WaitingForInputState
        if (GameStateManager.Instance == null || !GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            Debug.Log("BoardManager: HandleTileSelection aborted - not in WaitingForInputState");
            return;
        }

        Debug.Log($"BoardManager: HandleTileSelection called for position {gridPosition}");
        
        // First check if this is a special tile and delegate to SpecialTileManager if needed
        SpecialTile specialTile = SpecialTileManager.Instance?.GetSpecialTileAtPosition(gridPosition);
        if (specialTile != null)
        {
            Debug.Log($"BoardManager: Found special tile '{specialTile.specialAbilityName}' at {gridPosition}. Activating...");
            specialTile.Activate();
            return;
        }

        // Check if we already have a selected tile
        if (selectedTile != null)
        {
            // We have a selected tile, so this click is either selecting another tile or moving to an empty space
            Tile clickedTile = GetTileAtPosition(gridPosition);
            
            if (clickedTile != null)
            {
                // Clicking on another tile
                if (clickedTile == selectedTile)
                {
                    // Clicking on the same tile - deselect it
                    Debug.Log("BoardManager: Deselecting currently selected tile");
                    ClearSelection();
                    return;
                }
                else
                {
                    // Clicking on a different tile - handle potential merge
                    HandlePotentialMerge(gridPosition, clickedTile);
                }
            }
            else
            {
                // Clicking on an empty cell - try to move the selected tile there
                Debug.Log($"BoardManager: Empty cell clicked at {gridPosition}. Attempting move from {selectedTilePosition}.");
                HandleTileMoveConfirmation(gridPosition);
            }
            return;
        }

        // No tile is currently selected, so try to select the clicked tile
        Tile tile = GetTileAtPosition(gridPosition);
        if (tile != null)
        {
            // Select this tile
            Debug.Log($"BoardManager: Selecting tile at {gridPosition} with number {tile.number} and color {tile.tileColor}");
            selectedTile = tile;
            selectedTilePosition = gridPosition;
            tile.SetState(Tile.TileState.Selected);
            
            // Create external selection highlight
            CreateSelectionHighlight(gridPosition);
            
            // Highlight valid moves
            HighlightValidMoves(gridPosition, tile.number);
        }
        else
        {
            // Clicked on an empty cell with no selected tile
            Debug.Log($"BoardManager: Clicked on empty cell at {gridPosition} with no tile selected.");
            // Clear any lingering selection state just to be safe
            ClearSelection();
        }
    }

    // Helper method to handle potential merges between tiles
    private void HandlePotentialMerge(Vector2Int gridPosition, Tile targetTile)
    {
        Debug.Log($"BoardManager: Already have selected tile at {selectedTilePosition} with number {selectedTile.number}");
        
        // Check if these tiles can merge
        Vector2Int direction = gridPosition - selectedTilePosition;
        int distance = Mathf.Abs(direction.x) + Mathf.Abs(direction.y);
        
        Debug.Log($"BoardManager: Checking merge - direction: {direction}, distance: {distance}, max distance allowed: {selectedTile.number}");
        Debug.Log($"BoardManager: Color match: {CompareColors(selectedTile.tileColor, targetTile.tileColor)}");
        
        // Check if the target is a special tile (cannot merge with special tiles)
        if (targetTile.GetComponent<SpecialTile>() != null)
        {
            Debug.Log("BoardManager: Target tile is a special tile. Merge is not allowed.");
            ClearSelection();
            selectedTile = targetTile;
            selectedTilePosition = gridPosition;
            targetTile.SetState(Tile.TileState.Selected);
            CreateSelectionHighlight(gridPosition);
            HighlightValidMoves(gridPosition, targetTile.number);
            return;
        }
        
        // Check valid orthogonal direction for merge
        bool isValidDirection = direction.x == 0 || direction.y == 0; // Ensure movement is along one axis only
        bool isWithinRange = distance <= selectedTile.number;         // Ensure distance is within the tile's value
        bool isSameColor = CompareColors(selectedTile.tileColor, targetTile.tileColor); // Ensure colors match
        
        Debug.Log($"BoardManager: Merge validation - Valid direction: {isValidDirection}, Within range: {isWithinRange}, Same color: {isSameColor}");
        
        // Merge is only possible if all three conditions are met
        if (isValidDirection && isWithinRange && isSameColor)
        {
            Debug.Log("BoardManager: Valid merge detected! Proceeding with merge operation.");
            Debug.Log($"BoardManager: Current lastMergedCellPosition is {lastMergedCellPosition}");
            
            Tile tempSourceTile = selectedTile;
            Vector2Int tempSourcePos = selectedTilePosition;
            
            ClearAllSelectionState();
            
            // Check if there are any obstacles in the merge path
            bool pathClear = IsPathClear(tempSourcePos, gridPosition);
            if (!pathClear)
            {
                Debug.LogWarning("BoardManager: Merge path is obstructed by other tiles. Cannot merge.");
                // Reselect the target tile since the original selection was cleared
                selectedTile = targetTile;
                selectedTilePosition = gridPosition;
                targetTile.SetState(Tile.TileState.Selected);
                CreateSelectionHighlight(gridPosition);
                HighlightValidMoves(gridPosition, targetTile.number);
                return;
            }
            
            StartCoroutine(MoveTileToTargetForMerge(tempSourceTile, targetTile, () =>
            {
                // Clear the cell on the board array
                ClearCell(tempSourcePos);
                emptyCells.Add(tempSourcePos);

                bool mergeSuccess = TileMerger.MergeTiles(targetTile, tempSourceTile);
                Debug.Log($"BoardManager: Merge result: {(mergeSuccess ? "SUCCESS" : "FAILED")}");
                Debug.Log($"BoardManager: After merge, lastMergedCellPosition is {lastMergedCellPosition}");
                
                if (mergeSuccess)
                {
                    // Make sure the resulting merged tile has its collider enabled
                    Collider2D mergedTileCollider = targetTile.GetComponent<Collider2D>();
                    if (mergedTileCollider != null && !mergedTileCollider.enabled)
                    {
                        Debug.Log("BoardManager: Re-enabling collider on merged tile after animation");
                        mergedTileCollider.enabled = true;
                    }
                    
                    // Play merge animation
                    TileAnimator animator = targetTile.GetComponent<TileAnimator>();
                    if (animator != null)
                    {
                        animator.PlayMergeAnimation();
                    }
                }
            }));
            
            GameManager.Instance.EndTurn();
        }
        else
        {
            Debug.Log("BoardManager: Cannot merge - " + 
                     (!isValidDirection ? "invalid direction (must be orthogonal)" : "") + 
                     (!isWithinRange ? "distance too great" : "") + 
                     (!isSameColor ? "colors don't match" : ""));
                     
            // Deselect the current tile and select the new one instead
            ClearSelection();
            selectedTile = targetTile;
            selectedTilePosition = gridPosition;
            targetTile.SetState(Tile.TileState.Selected);
            CreateSelectionHighlight(gridPosition);
            HighlightValidMoves(gridPosition, targetTile.number);
        }
    }

    // Add this new method to check if there are any tiles obstructing the merge path
    private bool IsPathClear(Vector2Int startPos, Vector2Int endPos)
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
            if (IsCellOccupied(currentPos))
            {
                Debug.LogWarning($"BoardManager: Path obstructed at {currentPos}");
                return false;
            }
            currentPos += direction;
        }
        
        return true;
    }

    /// <summary>
    /// Creates a visual highlight around a selected tile.
    /// </summary>
    private void CreateSelectionHighlight(Vector2Int position)
    {
        // Only create highlights when in WaitingForInputState
        if (GameStateManager.Instance == null || !GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            Debug.Log("BoardManager: CreateSelectionHighlight aborted - not in WaitingForInputState");
            return;
        }

        GameObject highlight = new GameObject($"Selection_{position.x}_{position.y}");
        // Change the tag to "Highlight" which should already be defined in your project
        highlight.tag = "Highlight";
        highlight.transform.position = GetWorldPosition(position);
        highlight.transform.SetParent(transform);

        // Add a component we can use to identify it as a selection highlight
        highlight.AddComponent<SelectionHighlightIdentifier>();
        SpriteRenderer renderer = highlight.AddComponent<SpriteRenderer>();
        renderer.sprite = selectedTile.GetComponent<SpriteRenderer>().sprite;
        renderer.color = new Color(1f, 0.8f, 0.2f, 0.6f); // Golden highlight
        renderer.sortingOrder = -1; // Just behind the tile

        // Make the selection larger than the tile
        highlight.transform.localScale = new Vector3(cellSize * 1.2f, cellSize * 1.2f, 1f);

        // Animate the selection
        LeanTween.rotateZ(highlight, 360f, 4f).setLoopClamp();
        LeanTween.scale(highlight, new Vector3(cellSize * 1.3f, cellSize * 1.3f, 1f), 0.7f)
                .setEaseInOutSine()
                .setLoopPingPong();

        // Use a shader for glow effect if available
        if (renderer.material != null && renderer.material.HasProperty("_GlowIntensity"))
        {
            renderer.material.SetFloat("_GlowIntensity", 0.3f);
            renderer.material.SetColor("_GlowColor", new Color(1f, 0.9f, 0.5f));
        }
    }

    /// <summary>
    /// Clears all highlights related to tile selection.
    /// </summary>
    private void ClearSelectionHighlights()
    {
        // Find all objects with our identifier component instead of by tag
        SelectionHighlightIdentifier[] highlights = FindObjectsOfType<SelectionHighlightIdentifier>();
        foreach (var highlight in highlights)
        {
            Destroy(highlight.gameObject);
        }
    }

    private void HighlightMergeableTiles(Tile selectedTile)
    {
        ClearHighlights();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int targetPosition = new Vector2Int(x, y);
                Tile targetTile = GetTileAtPosition(targetPosition);
                if (targetTile != null && targetTile != selectedTile)
                {
                    // Skip special tiles as merge targets (new check)
                    if (targetTile.GetComponent<SpecialTile>() != null)
                    {
                        // Mark special tile as an invalid merge target with a red highlight
                        HighlightCellAsInvalidTarget(targetPosition);
                        continue;
                    }
                    
                    // Check if the tiles can merge based on color and distance
                    Vector2Int selectedPosition = selectedTilePosition;
                    Vector2Int direction = targetPosition - selectedPosition;
                    int distance = Mathf.Abs(direction.x) + Mathf.Abs(direction.y);
                    if (CompareColors(selectedTile.tileColor, targetTile.tileColor) && distance <= selectedTile.number)
                    {
                        HighlightCell(targetPosition);
                    }
                }
            }
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
        // First highlight valid movement cells
        Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int direction in directions)
        {
            for (int step = 1; step <= maxSteps; step++)
            {
                Vector2Int targetPosition = startPosition + direction * step;
                if (!IsWithinBounds(targetPosition)) break;
                if (IsCellOccupied(targetPosition)) 
                {
                    // Found a tile - check if it can be merged
                    Tile targetTile = GetTileAtPosition(targetPosition);
                    Tile sourceTile = GetTileAtPosition(startPosition);
                    
                    // Skip special tiles as merge targets
                    if (targetTile != null && targetTile.GetComponent<SpecialTile>() != null)
                    {
                        // Mark this as an invalid target with a red highlight
                        HighlightCellAsInvalidTarget(targetPosition);
                        break; // Stop highlighting in this direction
                    }
                    
                    // Highlight if same color (mergeable)
                    if (targetTile != null && sourceTile != null && 
                        CompareColors(sourceTile.tileColor, targetTile.tileColor))
                    {
                        // Highlight as merge target
                        HighlightCellAsMergeTarget(targetPosition);
                    }
                    else {
                        // Highlight as blocking tile (non-matching color)
                        HighlightCellAsBlockingTile(targetPosition);
                    }
                    break; // Stop highlighting in this direction
                }
                // Highlight as move target
                HighlightCellAsMoveTarget(targetPosition);
            }
        }
    }

    private void HighlightCellAsMoveTarget(Vector2Int position)
    {
        GameObject cellIndicator = Instantiate(cellIndicatorPrefab, GetWorldPosition(position), Quaternion.identity, transform);
        cellIndicator.tag = "Highlight";
        SpriteRenderer highlightRenderer = cellIndicator.GetComponent<SpriteRenderer>();
        if (highlightRenderer != null)
        {
            // Use a blue highlight color for movement
            highlightRenderer.color = new Color(0.4f, 0.8f, 1f, 0.6f);
            highlightRenderer.material = new Material(Shader.Find("Sprites/Default"));
            highlightRenderer.material.SetColor("_Color", highlightRenderer.color);
            // Make it slightly smaller than the cell to create a nice border effect
            cellIndicator.transform.localScale = new Vector3(cellSize * 0.9f, cellSize * 0.9f, 1f);
            // Add pulsing animation for the highlight
            LeanTween.scale(cellIndicator, new Vector3(cellSize * 0.95f, cellSize * 0.95f, 1f), 0.6f)
                .setEaseInOutSine()
                .setLoopPingPong();
            // Animate the opacity for better visibility
            LeanTween.alpha(cellIndicator, 0.4f, 0.8f)
                .setEaseInOutSine()
                .setLoopPingPong();
        }
    }

    private void HighlightCellAsMergeTarget(Vector2Int position)
    {
        GameObject cellIndicator = Instantiate(cellIndicatorPrefab, GetWorldPosition(position), Quaternion.identity, transform);
        cellIndicator.tag = "Highlight";
        SpriteRenderer highlightRenderer = cellIndicator.GetComponent<SpriteRenderer>();
        if (highlightRenderer != null)
        {
            // Use a golden/orange highlight color for merge targets
            highlightRenderer.color = new Color(1f, 0.7f, 0.2f, 0.7f);
            highlightRenderer.material = new Material(Shader.Find("Sprites/Default"));
            highlightRenderer.material.SetColor("_Color", highlightRenderer.color);
            // Make it slightly larger and with more distinct animation for merge targets
            cellIndicator.transform.localScale = new Vector3(cellSize * 0.95f, cellSize * 0.95f, 1f);
            // Add more dynamic pulsing animation for merge targets
            LeanTween.scale(cellIndicator, new Vector3(cellSize * 1.05f, cellSize * 1.05f, 1f), 0.4f)
                .setEaseInOutSine()
                .setLoopPingPong();
            // More vibrant opacity animation for merge targets
            LeanTween.alpha(cellIndicator, 0.9f, 0.6f)
                .setEaseInOutSine()
                .setLoopPingPong();
            // Add a rotation effect for extra emphasis
            LeanTween.rotateZ(cellIndicator, 10f, 1.5f)
                .setEaseInOutSine()
                .setLoopPingPong();
        }
    }

    // Add a new method to highlight invalid targets
    private void HighlightCellAsInvalidTarget(Vector2Int position)
    {
        GameObject cellIndicator = Instantiate(cellIndicatorPrefab, GetWorldPosition(position), Quaternion.identity, transform);
        cellIndicator.tag = "Highlight";
        
        SpriteRenderer highlightRenderer = cellIndicator.GetComponent<SpriteRenderer>();
        if (highlightRenderer != null)
        {
            // Red highlight color for invalid targets
            highlightRenderer.color = new Color(1f, 0.3f, 0.3f, 0.5f);
            highlightRenderer.material = new Material(Shader.Find("Sprites/Default"));
            highlightRenderer.material.SetColor("_Color", highlightRenderer.color);
            
            // Add tooltip GameObject with text
            GameObject tooltip = new GameObject("InvalidMergeTooltip");
            tooltip.transform.SetParent(cellIndicator.transform);
            tooltip.transform.localPosition = new Vector3(0, 0.75f, -0.1f);
            
            // Add TextMesh component for the tooltip
            TMPro.TextMeshPro tooltipText = tooltip.AddComponent<TMPro.TextMeshPro>();
            tooltipText.text = "Cannot merge with\nspecial tiles";
            tooltipText.fontSize = 2f;
            tooltipText.alignment = TMPro.TextAlignmentOptions.Center;
            tooltipText.color = Color.white;
            tooltipText.outlineWidth = 0.2f;
            tooltipText.outlineColor = Color.black;
            
            // Slightly smaller outline around the special tile
            cellIndicator.transform.localScale = new Vector3(cellSize * 0.85f, cellSize * 0.85f, 1f);
            
            // Add a subtle animation to draw attention
            LeanTween.rotateZ(cellIndicator, 10f, 1.5f)
                .setEaseInOutSine()
                .setLoopPingPong();
            
            // Animate tooltip
            LeanTween.moveLocalY(tooltip, 0.9f, 1f)
                .setEaseInOutSine()
                .setLoopPingPong();
        }
    }

    // Add a new method to highlight blocking tiles (non-matching color)
    private void HighlightCellAsBlockingTile(Vector2Int position)
    {
        GameObject cellIndicator = Instantiate(cellIndicatorPrefab, GetWorldPosition(position), Quaternion.identity, transform);
        cellIndicator.tag = "Highlight";
        
        SpriteRenderer highlightRenderer = cellIndicator.GetComponent<SpriteRenderer>();
        if (highlightRenderer != null)
        {
            // Gray highlight for non-matching color tiles
            highlightRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.4f);
            highlightRenderer.material = new Material(Shader.Find("Sprites/Default"));
            highlightRenderer.material.SetColor("_Color", highlightRenderer.color);
            
            // Slightly smaller outline around the tile
            cellIndicator.transform.localScale = new Vector3(cellSize * 0.85f, cellSize * 0.85f, 1f);
            
            // No animation needed for blocking tiles - just static highlight
        }
    }

    public void ClearHighlights()
    {
        // First, find all highlights using FindObjectsOfType for a more thorough search
        GameObject[] allHighlights = GameObject.FindGameObjectsWithTag("Highlight");
        foreach (GameObject highlight in allHighlights)
        {
            Destroy(highlight);
        }
        // Also make sure we clear any SelectionHighlightIdentifier objects
        ClearSelectionHighlights();
        // Then also check children as a backup (in case tag-based search misses any)
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Highlight") || child.GetComponent<SelectionHighlightIdentifier>() != null)
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

    public void HandleTileMoveConfirmation(Vector2Int targetPosition)
    {
        // First, verify that we're in WaitingForInputState
        if (GameStateManager.Instance == null || !GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            Debug.Log("BoardManager: HandleTileMoveConfirmation aborted - not in WaitingForInputState");
            return;
        }

        if (selectedTile == null)
        {
            Debug.LogWarning("BoardManager: HandleTileMoveConfirmation called with no selected tile");
            return;
        }

        if (!IsWithinBounds(targetPosition))
        {
            Debug.LogWarning($"BoardManager: HandleTileMoveConfirmation called with out-of-bounds position {targetPosition}");
            return;
        }

        // If the clicked cell is the same as the selected tile's cell, do nothing.
        if (targetPosition == selectedTilePosition)
        {
            Debug.Log($"BoardManager: Target position {targetPosition} is the same as selected tile position. No movement needed.");
            return;
        }

        Debug.Log($"BoardManager: Checking movement from {selectedTilePosition} to {targetPosition}");
        
        // Check if the move is valid by ensuring the target is within the allowed distance
        Vector2Int direction = targetPosition - selectedTilePosition;
        int manhattanDistance = Mathf.Abs(direction.x) + Mathf.Abs(direction.y);
        bool isOrthogonal = direction.x == 0 || direction.y == 0; // Movement must be along one axis only
        
        Debug.Log($"BoardManager: Move check - Manhattan distance: {manhattanDistance}, Max allowed: {selectedTile.number}, Orthogonal: {isOrthogonal}");

        if (manhattanDistance <= selectedTile.number && isOrthogonal)
        {
            // Check if the path is clear
            bool pathClear = IsPathClear(selectedTilePosition, targetPosition);
            if (!pathClear)
            {
                Debug.LogWarning($"BoardManager: Path from {selectedTilePosition} to {targetPosition} is obstructed");
                return;
            }
            
            if (IsCellOccupied(targetPosition))
            {
                Tile targetTile = GetTileAtPosition(targetPosition);
                // Check for color matching + other merge conditions
                if (targetTile != null && CompareColors(selectedTile.tileColor, targetTile.tileColor))
                {
                    // Store references before clearing selection
                    Tile sourceTile = selectedTile;
                    Vector2Int sourcePos = selectedTilePosition;

                    // Merge operation
                    Debug.Log($"BoardManager: Target cell at {targetPosition} has a compatible tile. Merging...");
                    
                    ClearAllSelectionState(); // Clear selection UI first
                    
                    StartCoroutine(MoveTileToTargetForMerge(sourceTile, targetTile, () => {
                        // Clear the source cell on the board array
                        ClearCell(sourcePos);
                        emptyCells.Add(sourcePos);

                        bool mergeSuccess = TileMerger.MergeTiles(targetTile, sourceTile);
                        
                        if (mergeSuccess)
                        {
                            TileAnimator animator = targetTile.GetComponent<TileAnimator>();
                            if (animator != null)
                            {
                                animator.PlayMergeAnimation();
                            }
                            
                            GameManager.Instance.EndTurn();
                        }
                    }));
                }
                else
                {
                    Debug.Log($"BoardManager: Target cell at {targetPosition} has an incompatible tile. Movement aborted.");
                    return;
                }
            }
            else
            {
                // Target cell is empty, perform movement
                Debug.Log($"BoardManager: Moving tile from {selectedTilePosition} to empty cell at {targetPosition}");
                
                // Store a reference to the selected tile before clearing selection
                Tile tileToMove = selectedTile;
                Vector2Int startPos = selectedTilePosition;
                
                ClearAllSelectionState(); // Clear selection UI first
                
                // Now perform the move operation
                MoveTile(tileToMove, startPos, targetPosition);
                GameManager.Instance.EndTurn();
            }
        }
        else
        {
            Debug.LogWarning($"BoardManager: Invalid move - " + 
                            (!isOrthogonal ? "movement must be orthogonal" : "distance too great"));
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

    private void HighlightCell(Vector2Int position)
    {
        GameObject cellIndicator = Instantiate(cellIndicatorPrefab, GetWorldPosition(position), Quaternion.identity, transform);
        cellIndicator.tag = "Highlight";
        SpriteRenderer highlightRenderer = cellIndicator.GetComponent<SpriteRenderer>();
        if (highlightRenderer != null)
        {
            // Use a general highlight color (between move and merge colors)
            highlightRenderer.color = new Color(0.7f, 0.7f, 1f, 0.6f);
            highlightRenderer.material = new Material(Shader.Find("Sprites/Default"));
            highlightRenderer.material.SetColor("_Color", highlightRenderer.color);
            // Make it slightly smaller than the cell to create a nice border effect
            cellIndicator.transform.localScale = new Vector3(cellSize * 0.9f, cellSize * 0.9f, 1f);
            // Add pulsing animation for the highlight
            LeanTween.scale(cellIndicator, new Vector3(cellSize * 0.95f, cellSize * 0.95f, 1f), 0.6f)
                .setEaseInOutSine()
                .setLoopPingPong();
            // Animate the opacity for better visibility
            LeanTween.alpha(cellIndicator, 0.4f, 0.8f)
                .setEaseInOutSine()
                .setLoopPingPong();
        }
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

            bool mergeSuccess = TileMerger.MergeTiles(targetTile, sourceTile);
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

            // Verify the source tile GameObject was destroyed
            if (sourceTile != null && sourceTile.gameObject != null)
            {
                Debug.LogError($"BoardManager: Source tile GameObject was not destroyed during merge. Destroying it now.");
                Destroy(sourceTile.gameObject);
            }

            if (mergeSuccess)
            {
                // Play merge animation
                TileAnimator animator = targetTile.GetComponent<TileAnimator>();
                animator?.PlayMergeAnimation();
                ClearSelection();
                ClearAllSelectionState();
            }
            else
            {
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
        if (tile == null || !IsWithinBounds(position))
        {
            Debug.LogError($"BoardManager: Invalid split operation. Tile: {tile}, Position: {position}");
            return;
        }

        TileSplitter.SplitTile(tile, position);
        Debug.Log($"BoardManager: Split operation performed on tile at {position}.");
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
            SpecialTile specialTile = SpecialTileManager.Instance?.GetSpecialTileAtPosition(position);
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
        for (int radius = 1; radius <= Mathf.Max(width, height); radius++)
        {
            // Check positions in a square around the original position
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    // Only check positions on the perimeter of the square
                    if (Mathf.Abs(dx) == radius || Mathf.Abs(dy) == radius)
                    {
                        Vector2Int checkPos = new Vector2Int(position.x + dx, position.y + dy);
                        if (IsWithinBounds(checkPos) && this.IsCellEmpty(checkPos))
                        {
                            // Verify the position is really empty
                            if (GetTileAtPosition(checkPos) == null && 
                                SpecialTileManager.Instance?.GetSpecialTileAtPosition(checkPos) == null)
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
                                          SpecialTileManager.Instance?.GetSpecialTileAtPosition(pos) != null);
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
        SpecialTile specialTile = SpecialTileManager.Instance.GetSpecialTileAtPosition(gridPosition);
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
        if (!specialTiles.Contains(specialTile)) // Corrected 'contains' to 'Contains'
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

    /// <summary>
    /// Checks if a cell is empty or mergeable with the given tile.
    /// </summary>
    public bool IsCellEmptyOrMergeable(Vector2Int position, Tile tile)
    {
        if (!IsWithinBounds(position)) return false;
        Tile targetTile = GetTileAtPosition(position);
        return targetTile == null || CompareColors(tile.tileColor, targetTile.tileColor);
    }

    /// <summary>
    /// Adds a position to the emptyCells collection when a tile is destroyed
    /// </summary>
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

        tile.ResetState(); // Reset the tile's state before destruction
        Destroy(tile.gameObject);
        ClearCell(position); // Ensure the cell is cleared and added to emptyCells
        // Log the destruction operation
        Debug.Log($"BoardManager: Destroyed tile at {position}.");
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
        return selectedTile != null;
    }
}