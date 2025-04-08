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
                
                if (gridCellMaterial != null)
                {
                    cellRenderer.material = gridCellMaterial;
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

    public void SetTileAtPosition(Vector2Int position, Tile tile)
    {
        board[position.x, position.y] = tile;
    }

    public void ClearCell(Vector2Int position)
    {
        board[position.x, position.y] = null;
    }

    public void MarkCellAsOccupied(Vector2Int position)
    {
        emptyCells.Remove(position);
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

        // Shuffle the available cells to ensure randomness
        ShuffleList(availableCells);

        // Ensure there are enough available cells
        if (availableCells.Count < tileCount)
        {
            tileCount = availableCells.Count;
        }

        if (tileCount == 0)
        {
            Debug.LogWarning("BoardManager: No available cells to spawn tiles.");
            return false; // No tiles spawned
        }

        for (int i = 0; i < tileCount; i++)
        {
            Vector2Int spawnPosition = availableCells[i];
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

        // If no empty cells, check for mergeable adjacent tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile currentTile = GetTileAtPosition(new Vector2Int(x, y));
                if (currentTile == null) continue;

                // Check adjacent tiles for a valid merge
                foreach (Vector2Int dir in DirectionUtils.Orthogonal)
                {
                    Vector2Int neighborPos = new Vector2Int(x + dir.x, y + dir.y);
                    if (IsWithinBounds(neighborPos))
                    {
                        Tile neighborTile = GetTileAtPosition(neighborPos);
                        if (neighborTile != null && CompareColors(currentTile.tileColor, neighborTile.tileColor))
                        {
                            return true; // Found a valid move
                        }
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
        ClearHighlights();
    }

    private void HandleTileSelection(Vector2Int gridPosition)
    {
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            return;
        }

        Tile tile = GetTileAtPosition(gridPosition);

        if (tile != null)
        {
            if (selectedTile == tile)
            {
                // Deselect the tile if it's already selected
                ClearSelection();
                return;
            }

            if (selectedTile != null)
            {
                // Check if the tiles can merge based on distance, number, and direction
                Vector2Int selectedPos = selectedTilePosition;
                Vector2Int direction = gridPosition - selectedPos;

                // Ensure the direction is orthogonal (left, right, up, down)
                if ((direction.x == 0 || direction.y == 0) && Mathf.Abs(direction.x + direction.y) <= selectedTile.number)
                {
                    if (CompareColors(selectedTile.tileColor, tile.tileColor))
                    {
                        Tile tempSourceTile = selectedTile;
                        Vector2Int tempSourcePos = selectedTilePosition;

                        ClearAllSelectionState();

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
        GameObject cellIndicator = Instantiate(cellIndicatorPrefab, GetWorldPosition(position), Quaternion.identity, transform);
        cellIndicator.tag = "Highlight";
        
        SpriteRenderer highlightRenderer = cellIndicator.GetComponent<SpriteRenderer>();
        if (highlightRenderer != null)
        {
            // Use a more attractive highlight color with gentle gradient
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
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsInState<WaitingForInputState>())
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
            return;
        }

        // Check if the move is valid by ensuring the target is within the allowed distance
        Vector2Int direction = targetPosition - selectedTilePosition;
        if ((Mathf.Abs(direction.x) + Mathf.Abs(direction.y)) <= selectedTile.number && 
            (direction.x == 0 || direction.y == 0))
        {
            if (IsCellOccupied(targetPosition))
            {
                Tile targetTile = GetTileAtPosition(targetPosition);

                if (CompareColors(selectedTile.tileColor, targetTile.tileColor))
                {
                    ClearCell(selectedTilePosition);
                    emptyCells.Add(selectedTilePosition);

                    if (TileMerger.MergeTiles(targetTile, selectedTile))
                    {
                        TileAnimator animator = targetTile.GetComponent<TileAnimator>();
                        animator?.PlayMergeAnimation();
                    }

                    selectedTile = null;
                    ClearHighlights();
                    GameManager.Instance.EndTurn();
                    return;
                }
            }
            else
            {
                MoveTile(selectedTile, selectedTilePosition, targetPosition);
                ClearAllSelectionState();
                GameManager.Instance.EndTurn();
            }
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
        ClearAllSelectionState();

        StartCoroutine(MoveTileToTargetForMerge(sourceTile, targetTile, () =>
        {
            ClearCell(sourcePos);
            emptyCells.Add(sourcePos);

            if (TileMerger.MergeTiles(targetTile, sourceTile))
            {
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

    // Add this helper method to help with debugging
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
        if (tile != null && IsWithinBounds(position))
        {
            TileSplitter.SplitTile(tile, position);
        }
    }

    /// <summary>
    /// Triggers the spawning of a special tile at a specified position.
    /// </summary>
    /// <param name="position">The grid position where the special tile should spawn.</param>
    public void TriggerSpecialTileSpawn(Vector2Int position)
    {
        if (!IsWithinBounds(position) || !IsCellEmpty(position))
        {
            Vector2Int? alternativePosition = FindAlternativePosition(position);

            if (!alternativePosition.HasValue)
            {
                Debug.LogWarning($"BoardManager: Cannot spawn special tile at {position} or find an alternative position. Skipping spawn.");
                return;
            }

            position = alternativePosition.Value; // Use the valid alternative position
        }

        if (Random.value < Constants.SPECIAL_TILE_CHANCE)
        {
            string[] specialTileTypes = { "Blaster", "Freeze", "Doubler", "Painter" };
            string randomSpecialTile = specialTileTypes[Random.Range(0, specialTileTypes.Length)];
            Debug.Log($"BoardManager: Spawning special tile '{randomSpecialTile}' at {position}.");
            SpecialTileManager.Instance.SpawnSpecialTile(position, randomSpecialTile);
        }
    }

    private Vector2Int? FindAlternativePosition(Vector2Int originalPosition)
    {
        HashSet<Vector2Int> checkedPositions = new HashSet<Vector2Int>();
        Queue<Vector2Int> positionsToCheck = new Queue<Vector2Int>();

        // Start with the original position's neighbors
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            Vector2Int.up + Vector2Int.left, Vector2Int.up + Vector2Int.right,
            Vector2Int.down + Vector2Int.left, Vector2Int.down + Vector2Int.right
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int newPosition = originalPosition + direction;
            if (IsWithinBounds(newPosition) && !checkedPositions.Contains(newPosition))
            {
                positionsToCheck.Enqueue(newPosition);
                checkedPositions.Add(newPosition);
            }
        }

        // Check nearby positions first
        while (positionsToCheck.Count > 0)
        {
            Vector2Int position = positionsToCheck.Dequeue();
            if (IsCellEmpty(position))
            {
                return position; // Return the first valid position
            }
        }

        // Fallback: Check all empty cells on the board
        foreach (Vector2Int position in GetAllEmptyCells())
        {
            if (!checkedPositions.Contains(position))
            {
                return position; // Return the first available empty cell
            }
        }

        // No valid position found
        Debug.LogWarning($"BoardManager: No valid alternative position found for original position {originalPosition}.");
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
}