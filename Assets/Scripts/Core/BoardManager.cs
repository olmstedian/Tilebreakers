using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance; // Singleton instance

    public int width = 6;
    public int height = 6;
    public float cellSize = 1.5f;
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
        InputManager.OnSwipe += HandleSwipe;
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

    private void HandleSwipe(Vector2Int direction, int swipeDistance)
    {
        Vector2Int startPosition = GetGridPositionFromSwipeStart(InputManager.Instance.startTouchPosition);
        Tile swipedTile = GetTileAtPosition(startPosition);

        if (swipedTile != null)
        {
            // Limit the movement to the smaller of swipe distance or the tile's number
            int maxSteps = Mathf.Min(swipeDistance, swipedTile.number);

            // Find the target position based on the limited steps
            Vector2Int targetPosition = FindTargetPosition(startPosition, direction, maxSteps);

            if (targetPosition != startPosition)
            {
                MoveTile(swipedTile, startPosition, targetPosition);
                GameManager.Instance.EndTurn();
            }
        }
    }

    private Vector2Int GetGridPositionFromSwipeStart(Vector2 swipeStartPosition)
    {
        // Convert swipe start position in world space to grid coordinates
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(swipeStartPosition);
        int x = Mathf.FloorToInt((worldPosition.x + (width * cellSize) / 2) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.y + (height * cellSize) / 2) / cellSize);
        return new Vector2Int(x, y);
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
        tile.MoveTo(GetWorldPosition(targetPosition), 0.2f);
        MarkCellAsOccupied(targetPosition);
    }

    private bool IsWithinBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
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

    public void GenerateRandomStartingTiles(int minTiles = 3, int maxTiles = 5)
    {
        int tileCount = Random.Range(minTiles, maxTiles + 1);
        List<Vector2Int> availableCells = new List<Vector2Int>(emptyCells);

        for (int i = 0; i < tileCount && i < availableCells.Count; i++)
        {
            Vector2Int spawnPosition = availableCells[i];
            int randomNumber = Random.Range(1, 6);
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
}