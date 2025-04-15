using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Grid Appearance")]
    [SerializeField] private Color gridLineColor = new Color(0.3f, 0.3f, 0.35f, 0.5f);
    [SerializeField] private float gridLineWidth = 0.05f;
    // [SerializeField] [Range(0f, 0.5f)] private float cellPadding = 0.1f; // Remove or comment out this unused field
    [SerializeField] private bool animateGridLines = true;
    [SerializeField] private float animationSpeed = 0.5f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject highlightPrefab;
    [SerializeField] private Color validMoveHighlightColor = new Color(0.4f, 0.8f, 1f, 0.6f);
    [SerializeField] private Color mergeHighlightColor = new Color(1f, 0.7f, 0.2f, 0.7f);
    [SerializeField] private float cellAnimationDelay = 0.03f;
    [SerializeField] private ParticleSystem gridBurstEffect;
    [SerializeField] private Color gridHighlightColor = new Color(0.4f, 0.8f, 1f, 0.6f);

    private LineRenderer[] horizontalLines;
    private LineRenderer[] verticalLines;
    private Material gridLineMaterial;

    [Header("References")]
    [SerializeField] private BoardManager boardManager;

    private List<GameObject> gridCells = new List<GameObject>();

    /// <summary>
    /// Initializes the GridManager.
    /// Called by GridManagerPrefab and can be called manually if needed.
    /// </summary>
    public void Initialize()
    {
        // Find the BoardManager
        boardManager = FindObjectOfType<BoardManager>();
        if (boardManager == null)
        {
            Debug.LogError("GridManager: BoardManager not found. Cannot initialize grid.");
            return;
        }

        // Create grid visuals
        CreateGridLines();

        Debug.Log("GridManager: Successfully initialized.");
    }

    private void Start()
    {
        if (boardManager == null)
        {
            Initialize();
        }

        // Find all grid cells
        GameObject gridParent = transform.Find("GridBackground")?.gameObject;
        if (gridParent != null)
        {
            foreach (Transform child in gridParent.transform)
            {
                if (child.name.StartsWith("GridCell"))
                {
                    gridCells.Add(child.gameObject);
                }
            }
        }

        PlayGridIntroAnimation();
    }

    private void CreateGridLines()
    {
        if (boardManager == null) return;

        int width = boardManager.width;
        int height = boardManager.height;
        float cellSize = boardManager.cellSize;

        // Calculate grid dimensions and position
        float gridWidth = width * cellSize;
        float gridHeight = height * cellSize;
        float offsetX = -gridWidth / 2;
        float offsetY = -gridHeight / 2;

        // Create material for grid lines
        gridLineMaterial = new Material(Shader.Find("Sprites/Default"));
        gridLineMaterial.color = gridLineColor;

        // Create horizontal lines
        horizontalLines = new LineRenderer[height + 1];
        for (int i = 0; i <= height; i++)
        {
            LineRenderer line = CreateLine($"HorizontalLine_{i}", gridLineWidth);
            float y = i * cellSize + offsetY;
            line.SetPosition(0, new Vector3(offsetX, y, 0));
            line.SetPosition(1, new Vector3(offsetX + gridWidth, y, 0));
            horizontalLines[i] = line;
        }

        // Create vertical lines
        verticalLines = new LineRenderer[width + 1];
        for (int i = 0; i <= width; i++)
        {
            LineRenderer line = CreateLine($"VerticalLine_{i}", gridLineWidth);
            float x = i * cellSize + offsetX;
            line.SetPosition(0, new Vector3(x, offsetY, 0));
            line.SetPosition(1, new Vector3(x, offsetY + gridHeight, 0));
            verticalLines[i] = line;
        }

        if (animateGridLines)
        {
            StartCoroutine(AnimateGridLines());
        }
    }

    private LineRenderer CreateLine(string name, float width)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(transform);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = width;
        line.endWidth = width;
        line.material = gridLineMaterial;
        line.startColor = gridLineColor;
        line.endColor = gridLineColor;
        line.sortingOrder = -1; // Draw behind tiles

        return line;
    }

    private System.Collections.IEnumerator AnimateGridLines()
    {
        float time = 0;
        Color baseColor = gridLineColor;
        Color brightColor = new Color(
            Mathf.Min(baseColor.r + 0.2f, 1f),
            Mathf.Min(baseColor.g + 0.2f, 1f),
            Mathf.Min(baseColor.b + 0.2f, 1f),
            baseColor.a
        );

        while (true)
        {
            time += Time.deltaTime * animationSpeed;

            // Animate every other line with a different phase
            for (int i = 0; i < horizontalLines.Length; i++)
            {
                if (horizontalLines[i] != null)
                {
                    float t = (Mathf.Sin(time + i * 0.3f) + 1) * 0.5f;
                    horizontalLines[i].startColor = Color.Lerp(baseColor, brightColor, t);
                    horizontalLines[i].endColor = Color.Lerp(baseColor, brightColor, t);
                }
            }

            for (int i = 0; i < verticalLines.Length; i++)
            {
                if (verticalLines[i] != null)
                {
                    float t = (Mathf.Sin(time + i * 0.3f + 0.5f) + 1) * 0.5f;
                    verticalLines[i].startColor = Color.Lerp(baseColor, brightColor, t);
                    verticalLines[i].endColor = Color.Lerp(baseColor, brightColor, t);
                }
            }

            yield return null;
        }
    }

    public void PlayGridIntroAnimation()
    {
        // Hide all cells initially
        foreach (GameObject cell in gridCells)
        {
            cell.transform.localScale = Vector3.zero;
        }

        // Animate them appearing one by one
        StartCoroutine(AnimateCellsIntro());
    }

    private IEnumerator AnimateCellsIntro()
    {
        // Shuffle the grid cells for a random appearance order
        List<GameObject> shuffledCells = new List<GameObject>(gridCells);
        ShuffleList(shuffledCells);

        // Wait a moment before starting
        yield return new WaitForSeconds(0.2f);

        // Play a burst effect at the center of the grid
        if (gridBurstEffect != null)
        {
            Instantiate(gridBurstEffect, Vector3.zero, Quaternion.identity);
        }

        // Animate each cell
        foreach (GameObject cell in shuffledCells)
        {
            LeanTween.scale(cell, Vector3.one, 0.3f).setEaseOutBack();
            yield return new WaitForSeconds(cellAnimationDelay);
        }

        // After all cells appear, add a finishing touch
        yield return new WaitForSeconds(0.3f);
        AnimateGridFlash();
    }

    public void AnimateGridFlash()
    {
        foreach (GameObject cell in gridCells)
        {
            SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                Color originalColor = renderer.color;
                LeanTween.value(cell, 0f, 1f, 0.5f)
                    .setEaseInOutSine()
                    .setOnUpdate((float val) => {
                        renderer.color = Color.Lerp(gridHighlightColor, originalColor, val);
                    });
            }
        }
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

    /// <summary>
    /// Creates a highlight at the specified grid position.
    /// </summary>
    public void HighlightCell(Vector2Int position, bool isMergeTarget)
    {
        if (boardManager == null || highlightPrefab == null) return;

        Vector2 worldPos = boardManager.GetWorldPosition(position);
        GameObject highlight = Instantiate(highlightPrefab, worldPos, Quaternion.identity, transform);
        highlight.tag = "Highlight";

        SpriteRenderer renderer = highlight.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = isMergeTarget ? mergeHighlightColor : validMoveHighlightColor;
            renderer.sortingOrder = -1;
            
            // Make sure the highlight is the right size
            float highlightSize = boardManager.cellSize * 0.9f; // Slightly smaller than cell for better visuals
            highlight.transform.localScale = new Vector3(highlightSize, highlightSize, 1f);

            // Apply animations based on highlight type
            if (isMergeTarget)
            {
                // More vibrant animation for merge targets
                LeanTween.scale(highlight, Vector3.one * highlightSize * 1.1f, 0.4f).setEaseInOutSine().setLoopPingPong();
                LeanTween.rotateZ(highlight, 10f, 1.2f).setEaseInOutSine().setLoopPingPong();
            }
            else
            {
                // Subtle animation for move targets
                LeanTween.scale(highlight, Vector3.one * highlightSize * 0.95f, 0.6f).setEaseInOutSine().setLoopPingPong();
                LeanTween.alpha(highlight, 0.4f, 0.8f).setEaseInOutSine().setLoopPingPong();
            }
        }
    }
    
    /// <summary>
    /// Creates a selection highlight around a tile.
    /// </summary>
    public void HighlightSelectedTile(Vector2Int position)
    {
        if (boardManager == null || highlightPrefab == null) return;
        
        Vector2 worldPos = boardManager.GetWorldPosition(position);
        GameObject highlight = Instantiate(highlightPrefab, worldPos, Quaternion.identity, transform);
        highlight.tag = "TileSelection";
        
        SpriteRenderer renderer = highlight.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // Use a bright, noticeable color for selection
            renderer.color = new Color(1f, 0.8f, 0.2f, 0.7f); // Golden yellow
            renderer.sortingOrder = 5; // Above tile for better visibility
            
            // Make it slightly larger than the tile
            float selectionSize = boardManager.cellSize * 1.1f;
            highlight.transform.localScale = new Vector3(selectionSize, selectionSize, 1f);
            
            // Vibrant animation for selection
            LeanTween.scale(highlight, Vector3.one * selectionSize * 1.15f, 0.5f).setEaseInOutSine().setLoopPingPong();
            LeanTween.rotateZ(highlight, 8f, 1.5f).setEaseInOutSine().setLoopPingPong();
            LeanTween.alpha(highlight, 0.5f, 1f).setEaseInOutSine().setLoopPingPong();
        }
    }

    /// <summary>
    /// Clears all highlights from the grid.
    /// </summary>
    public void ClearHighlights()
    {
        GameObject[] highlights = GameObject.FindGameObjectsWithTag("Highlight");
        foreach (var highlight in highlights)
        {
            Destroy(highlight);
        }
        
        // Also clean up selection highlights
        GameObject[] selections = GameObject.FindGameObjectsWithTag("TileSelection");
        foreach (var selection in selections)
        {
            Destroy(selection);
        }
    }

    /// <summary>
    /// Updates the grid when the board size changes.
    /// </summary>
    public void UpdateGridSize()
    {
        // Clean up existing lines
        if (horizontalLines != null)
        {
            foreach (var line in horizontalLines)
            {
                if (line != null) Destroy(line.gameObject);
            }
            horizontalLines = null;
        }

        if (verticalLines != null)
        {
            foreach (var line in verticalLines)
            {
                if (line != null) Destroy(line.gameObject);
            }
            verticalLines = null;
        }

        // Recreate grid with new dimensions
        CreateGridLines();
    }
}
