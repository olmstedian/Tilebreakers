// Manages the grid state, tiles, and board updates

using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public int width = 6;
    public int height = 6;
    public GameObject tilePrefab; // Reference to the Tile prefab
    public GameObject cellIndicatorPrefab; // Reference to the visual cell indicator prefab
    public GameObject gridBackgroundPrefab; // Reference to the grid background prefab

    private Tile[,] board;
    private HashSet<Vector2Int> emptyCells; // Tracks empty cells for O(1) lookup
    private Queue<Vector2Int> prioritizedSpawnLocations; // Queue for prioritized spawn locations

    private readonly Color[] tileColorPalette = 
    {
        new Color(1f, 0.5f, 0.5f), // Light Red
        new Color(0.5f, 0.5f, 1f), // Light Blue
        new Color(0.5f, 1f, 0.5f), // Light Green
        new Color(1f, 1f, 0.5f)    // Light Yellow
    };

    void Start()
    {
        board = new Tile[width, height];
        emptyCells = new HashSet<Vector2Int>();
        prioritizedSpawnLocations = new Queue<Vector2Int>();
        InitializeGrid();
        CreateGridBackground(); // Add background after initializing the grid

        // Generate random starting tiles instead of filling the grid
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

                // Add all cells to the emptyCells set initially
                emptyCells.Add(gridPosition);

                // Instantiate visual cell indicator with enhanced styling
                GameObject cellIndicator = Instantiate(cellIndicatorPrefab, position, Quaternion.identity, transform);
                EnhanceCellIndicator(cellIndicator);
            }
        }
    }

    private void EnhanceCellIndicator(GameObject cellIndicator)
    {
        // Get the sprite renderer component
        SpriteRenderer renderer = cellIndicator.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // Ensure it renders behind tiles but above the grid background
            renderer.sortingOrder = -1;
            
            // Add a subtle scale to ensure it fits within the cell properly
            cellIndicator.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
            
            // Keep the sprite's original color
        }
    }

    private void CreateGridBackground()
    {
        // Use a single neutral color for all grid cells
        Color backgroundColor = new Color(0.85f, 0.85f, 0.85f); // Light neutral gray

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = GetWorldPosition(new Vector2Int(x, y));

                // Instantiate a grid background cell at each tile position
                GameObject gridCellBackground = Instantiate(gridBackgroundPrefab, position, Quaternion.identity, transform);

                // Apply uniform color to all background cells
                SpriteRenderer bgRenderer = gridCellBackground.GetComponent<SpriteRenderer>();
                if (bgRenderer != null)
                {
                    bgRenderer.color = backgroundColor;
                    
                    // Make sure the background renders behind tiles
                    bgRenderer.sortingOrder = -1;
                }

                // Size the cell to ensure no gaps
                gridCellBackground.transform.localScale = new Vector3(1.02f, 1.02f, 1f);
                
                gridCellBackground.name = $"GridBackground ({x}, {y})";
            }
        }
    }

    public bool IsCellEmpty(Vector2Int position)
    {
        // Check if the position is in the emptyCells set
        return emptyCells.Contains(position);
    }

    public Vector2 GetWorldPosition(Vector2Int gridPosition)
    {
        // Calculate the position of the cell based on grid coordinates
        float offsetX = -width / 2f + 0.5f;
        float offsetY = -height / 2f + 0.5f;

        return new Vector2(gridPosition.x + offsetX, gridPosition.y + offsetY);
    }

    public Vector2Int GetGridPosition(Vector2 worldPosition)
    {
        float gridWidth = width;
        float gridHeight = height;

        // Center offsets for grid alignment
        float offsetX = -gridWidth / 2 + 0.5f;
        float offsetY = -gridHeight / 2 + 0.5f;

        // Calculate grid position from world position
        int x = Mathf.RoundToInt(worldPosition.x - offsetX);
        int y = Mathf.RoundToInt(worldPosition.y - offsetY);

        return new Vector2Int(x, y);
    }

    public Tile GetTileAtPosition(Vector2Int gridPosition)
    {
        // Check if the position is within bounds
        if (gridPosition.x < 0 || gridPosition.x >= width || gridPosition.y < 0 || gridPosition.y >= height)
            return null;

        // Return the tile at the specified position
        return board[gridPosition.x, gridPosition.y];
    }

    public void SetTileAtPosition(Vector2Int gridPosition, Tile tile)
    {
        // Check if the position is within bounds
        if (gridPosition.x < 0 || gridPosition.x >= width || gridPosition.y < 0 || gridPosition.y >= height)
            return;

        // Set the tile at the specified position
        board[gridPosition.x, gridPosition.y] = tile;
    }

    public bool IsCellOccupied(Vector2Int gridPosition)
    {
        // Check if the position is within bounds
        if (gridPosition.x < 0 || gridPosition.x >= width || gridPosition.y < 0 || gridPosition.y >= height)
            return false;

        // Return true if the cell is occupied by a tile
        return board[gridPosition.x, gridPosition.y] != null;
    }

    public void ClearCell(Vector2Int gridPosition)
    {
        // Check if the position is within bounds
        if (gridPosition.x < 0 || gridPosition.x >= width || gridPosition.y < 0 || gridPosition.y >= height)
            return;

        // Clear the cell by setting it to null
        board[gridPosition.x, gridPosition.y] = null;
    }

    public void MarkCellAsOccupied(Vector2Int gridPosition)
    {
        // Remove the cell from the emptyCells set
        emptyCells.Remove(gridPosition);
    }

    public void MarkCellAsEmpty(Vector2Int gridPosition)
    {
        // Add the cell to the emptyCells set
        emptyCells.Add(gridPosition);
    }

    public void AddPrioritizedSpawnLocation(Vector2Int gridPosition)
    {
        // Add the position to the queue if it's not already in it and is empty
        if (IsCellEmpty(gridPosition) && !prioritizedSpawnLocations.Contains(gridPosition))
        {
            prioritizedSpawnLocations.Enqueue(gridPosition);
        }
    }

    public Vector2Int GetNextSpawnLocation()
    {
        // Check if there are prioritized locations
        if (prioritizedSpawnLocations.Count > 0)
        {
            Vector2Int prioritizedLocation = prioritizedSpawnLocations.Dequeue();

            // Ensure the location is still empty before returning it
            if (IsCellEmpty(prioritizedLocation))
            {
                return prioritizedLocation;
            }
        }

        // Fallback: Return a random empty cell if no prioritized locations are available
        return GetRandomEmptyCell();
    }

    private Vector2Int GetRandomEmptyCell()
    {
        // Select a random empty cell from the HashSet
        if (emptyCells.Count > 0)
        {
            int randomIndex = Random.Range(0, emptyCells.Count);
            foreach (Vector2Int cell in emptyCells)
            {
                if (randomIndex == 0)
                    return cell;
                randomIndex--;
            }
        }

        // Return an invalid position if no empty cells are available
        return new Vector2Int(-1, -1);
    }

    public List<Vector2Int> GetRandomEmptyCellsNear(Vector2Int gridPosition, int maxCells)
    {
        List<Vector2Int> nearbyEmptyCells = new List<Vector2Int>();
        List<Vector2Int> potentialCells = new List<Vector2Int>
        {
            gridPosition + Vector2Int.up,
            gridPosition + Vector2Int.down,
            gridPosition + Vector2Int.left,
            gridPosition + Vector2Int.right
        };

        foreach (Vector2Int cell in potentialCells)
        {
            if (IsCellEmpty(cell))
            {
                nearbyEmptyCells.Add(cell);
            }
        }

        // Shuffle the list to randomize the order
        for (int i = 0; i < nearbyEmptyCells.Count; i++)
        {
            int randomIndex = Random.Range(i, nearbyEmptyCells.Count);
            Vector2Int temp = nearbyEmptyCells[i];
            nearbyEmptyCells[i] = nearbyEmptyCells[randomIndex];
            nearbyEmptyCells[randomIndex] = temp;
        }

        // Return up to the specified number of cells
        return nearbyEmptyCells.GetRange(0, Mathf.Min(maxCells, nearbyEmptyCells.Count));
    }

    public void GenerateRandomStartingTiles(int minTiles = 3, int maxTiles = 5)
    {
        int tileCount = Random.Range(minTiles, maxTiles + 1);

        // Get a list of all empty cells
        List<Vector2Int> availableCells = new List<Vector2Int>(emptyCells);

        // Shuffle the list to randomize placement
        for (int i = 0; i < availableCells.Count; i++)
        {
            int randomIndex = Random.Range(i, availableCells.Count);
            Vector2Int temp = availableCells[i];
            availableCells[i] = availableCells[randomIndex];
            availableCells[randomIndex] = temp;
        }

        // Place tiles evenly across the grid
        for (int i = 0; i < tileCount && i < availableCells.Count; i++)
        {
            Vector2Int spawnPosition = availableCells[i];

            // Generate a random number between 1 and 5
            int randomNumber = Random.Range(1, 6);

            // Generate a random color from the palette
            Color randomColor = GetRandomTileColor();

            // Instantiate a new tile at the spawn position
            GameObject newTile = Instantiate(tilePrefab, GetWorldPosition(spawnPosition), Quaternion.identity, transform);
            Tile tileComponent = newTile.GetComponent<Tile>();

            // Initialize the tile with a random color and number
            tileComponent.Initialize(randomColor, randomNumber);

            // Mark the cell as occupied
            SetTileAtPosition(spawnPosition, tileComponent);
            MarkCellAsOccupied(spawnPosition);
        }

        // Ensure at least one valid move exists
        if (!HasValidMove())
        {
            ClearBoard();
            GenerateRandomStartingTiles(minTiles, maxTiles);
        }
    }

    private bool HasValidMove()
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

    private void ClearBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                Tile tile = GetTileAtPosition(position);
                if (tile != null)
                {
                    Destroy(tile.gameObject);
                    ClearCell(position);
                }
            }
        }
    }

    private Vector2Int GetStrategicSpawnPosition()
    {
        // Get a random empty cell
        Vector2Int spawnPosition = GetRandomEmptyCell();

        // Ensure the position does not create an immediate merge
        for (int attempts = 0; attempts < 10; attempts++) // Limit attempts to avoid infinite loops
        {
            if (IsPositionSafeFromImmediateMerge(spawnPosition))
            {
                return spawnPosition;
            }

            // Try another random empty cell
            spawnPosition = GetRandomEmptyCell();
        }

        // Return the last attempted position if no safe position is found
        return spawnPosition;
    }

    private bool IsPositionSafeFromImmediateMerge(Vector2Int position)
    {
        // Check adjacent cells for potential merges
        List<Vector2Int> adjacentPositions = new List<Vector2Int>
        {
            position + Vector2Int.up,
            position + Vector2Int.down,
            position + Vector2Int.left,
            position + Vector2Int.right
        };

        foreach (Vector2Int adjacent in adjacentPositions)
        {
            Tile adjacentTile = GetTileAtPosition(adjacent);
            if (adjacentTile != null && adjacentTile.number == Random.Range(1, 6))
            {
                // If an adjacent tile has the same number, this position is not safe
                return false;
            }
        }

        // Position is safe if no adjacent tiles can merge
        return true;
    }

    private Color GetRandomTileColor()
    {
        // Select a random color from the predefined palette
        return tileColorPalette[Random.Range(0, tileColorPalette.Length)];
    }
}