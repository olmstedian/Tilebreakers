using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        InputManager.OnTileSelected += HandleTileSelection;
        InputManager.OnTileMoveConfirmed += HandleTileMoveConfirmation;
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

    private void MoveTile(Tile tile, Vector2Int startPosition, Vector2Int targetPosition)
    {
        ClearCell(startPosition);
        SetTileAtPosition(targetPosition, tile);
        
        // Movement Logic: update tile’s logical position using TileMover.
        TileMover mover = tile.GetComponent<TileMover>();
        if (mover != null)
        {
            StartCoroutine(mover.MoveTile(GetWorldPosition(targetPosition), Constants.TILE_MOVE_DURATION));
        }
        else
        {
            tile.transform.position = GetWorldPosition(targetPosition);
        }
        
        // Animation: trigger visual effects using TileAnimator.
        TileAnimator animator = tile.GetComponent<TileAnimator>();
        if (animator != null)
        {
            animator.PlayMoveAnimation(GetWorldPosition(targetPosition), Constants.TILE_MOVE_DURATION);
        }
        
        MarkCellAsOccupied(targetPosition);
    }

    private bool IsCellOccupied(Vector2Int position)
    {
        return board[position.x, position.y] != null;
    }

    private Tile GetTileAtPosition(Vector2Int position)
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
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile currentTile = GetTileAtPosition(new Vector2Int(x, y));
                if (currentTile == null) continue;

                // Check adjacent tiles for a valid move
                List<Vector2Int> adjacentPositions = new List<Vector2Int>
                {
                    new Vector2Int(x + 1, y),
                    new Vector2Int(x - 1, y),
                    new Vector2Int(x, y + 1),
                    new Vector2Int(x, y - 1)
                };

                foreach (Vector2Int adjacent in adjacentPositions)
                {
                    Tile adjacentTile = GetTileAtPosition(adjacent);
                    if (adjacentTile != null && adjacentTile.number == currentTile.number && adjacentTile.tileColor == currentTile.tileColor)
                    {
                        return true; // Found a valid move
                    }
                }
            }
        }
        return false; // No valid moves found
    }

    private void HandleTileSelection(Vector2Int gridPosition)
    {
        Tile tile = GetTileAtPosition(gridPosition);
        if (tile != null)
        {
            selectedTile = tile;
            selectedTilePosition = gridPosition;
            HighlightValidMoves(gridPosition, tile.number);
        }
    }

    private void HighlightValidMoves(Vector2Int startPosition, int maxSteps)
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

    private void ClearHighlights()
    {
        // Clear all existing highlights
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Highlight")) // Ensure the tag is correctly assigned
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void HandleTileMoveConfirmation(Vector2Int targetPosition)
    {
        if (selectedTile != null && IsWithinBounds(targetPosition) && !IsCellOccupied(targetPosition))
        {
            MoveTile(selectedTile, selectedTilePosition, targetPosition);
            selectedTile = null;
            ClearHighlights(); // Ensure highlights are cleared after the move
            GameManager.Instance.EndTurn();
        }
    }
}