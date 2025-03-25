// Manages the grid state, tiles, and board updates

using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public int width = 6;
    public int height = 6;
    public float cellSpacing = 1.1f; // Spacing between cells
    public GameObject tilePrefab; // Reference to the Tile prefab
    public GameObject cellIndicatorPrefab; // Reference to the visual cell indicator prefab

    private Tile[,] board;

    void Start()
    {
        board = new Tile[width, height];
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = GetWorldPosition(new Vector2Int(x, y));

                // Instantiate visual cell indicator
                Instantiate(cellIndicatorPrefab, position, Quaternion.identity, transform);

                // Instantiate tile (if needed for initialization)
                GameObject cell = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                cell.name = $"Tile ({x}, {y})";
                board[x, y] = cell.GetComponent<Tile>();
            }
        }
    }

    public bool IsCellEmpty(Vector2Int position)
    {
        if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height)
            return false;

        return board[position.x, position.y] == null;
    }

    public Vector2 GetWorldPosition(Vector2Int gridPosition)
    {
        float gridWidth = width * cellSpacing;
        float gridHeight = height * cellSpacing;

        // Center the grid dynamically based on screen dimensions
        float offsetX = -gridWidth / 2 + cellSpacing / 2;
        float offsetY = -gridHeight / 2 + cellSpacing / 2;

        return new Vector2(gridPosition.x * cellSpacing + offsetX, gridPosition.y * cellSpacing + offsetY);
    }
}