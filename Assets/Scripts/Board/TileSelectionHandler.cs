using UnityEngine;
using System.Collections.Generic;
// Add the namespace for SpecialTileManager
using Tilebreakers.Special;
using Tilebreakers.Board; // Add this namespace for TileMergeHandler

/// <summary>
/// Handles tile selection logic, highlight creation, and selection state management.
/// </summary>
public class TileSelectionHandler : MonoBehaviour
{
    private static TileSelectionHandler _instance;
    public static TileSelectionHandler Instance 
    {
        get 
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TileSelectionHandler>();
                if (_instance == null)
                {
                    GameObject handler = new GameObject("TileSelectionHandler");
                    _instance = handler.AddComponent<TileSelectionHandler>();
                    DontDestroyOnLoad(handler);
                }
            }
            return _instance;
        }
    }

    // Selection state
    private Tile selectedTile;
    private Vector2Int selectedTilePosition;
    private Vector2Int targetTilePosition;

    // Color settings for different highlight types
    [Header("Highlight Colors")]
    [SerializeField] private Color moveHighlightColor = new Color(0.4f, 0.8f, 1f, 0.6f);
    [SerializeField] private Color mergeHighlightColor = new Color(1f, 0.7f, 0.2f, 0.7f);
    [SerializeField] private Color selectionHighlightColor = new Color(1f, 0.8f, 0.2f, 0.6f);
    [SerializeField] private Color invalidTargetHighlightColor = new Color(1f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color blockingTileHighlightColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);

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
    /// Gets the currently selected tile.
    /// </summary>
    public Tile GetSelectedTile()
    {
        return selectedTile;
    }

    /// <summary>
    /// Gets the position of the currently selected tile.
    /// </summary>
    public Vector2Int GetSelectedTilePosition()
    {
        return selectedTilePosition;
    }

    /// <summary>
    /// Gets the target position for the selected tile.
    /// </summary>
    public Vector2Int GetTargetTilePosition()
    {
        // If no target position has been set, return an invalid position or selectedTilePosition
        if (targetTilePosition == default(Vector2Int))
        {
            Debug.LogWarning("TileSelectionHandler: targetTilePosition has not been set yet.");
            return selectedTilePosition; // Return selected position as fallback
        }
        return targetTilePosition;
    }

    /// <summary>
    /// Handles the selection of a tile at the specified grid position.
    /// </summary>
    public void HandleTileSelection(Vector2Int gridPosition)
    {
        // First, verify that we're in WaitingForInputState
        if (GameStateManager.Instance == null || !GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            Debug.Log("TileSelectionHandler: HandleTileSelection aborted - not in WaitingForInputState");
            return;
        }

        Debug.Log($"TileSelectionHandler: HandleTileSelection called for position {gridPosition}");
        
        // First check if this is a special tile and delegate to SpecialTileManager if needed
        SpecialTile specialTile = SpecialTileManager.Instance?.GetSpecialTileAtPosition(gridPosition);
        if (specialTile != null)
        {
            Debug.Log($"TileSelectionHandler: Found special tile '{specialTile.specialAbilityName}' at {gridPosition}. Activating...");
            specialTile.Activate();
            return;
        }

        // Check if we already have a selected tile
        if (selectedTile != null)
        {
            // We have a selected tile, so this click is either selecting another tile or moving to an empty space
            Tile clickedTile = BoardManager.Instance.GetTileAtPosition(gridPosition);
            
            if (clickedTile != null)
            {
                // Clicking on another tile
                if (clickedTile == selectedTile)
                {
                    // Clicking on the same tile - deselect it
                    Debug.Log("TileSelectionHandler: Deselecting currently selected tile");
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
                Debug.Log($"TileSelectionHandler: Empty cell clicked at {gridPosition}. Attempting move from {selectedTilePosition}.");
                HandleTileMoveConfirmation(gridPosition);
            }
            return;
        }

        // No tile is currently selected, so try to select the clicked tile
        Tile tile = BoardManager.Instance.GetTileAtPosition(gridPosition);
        if (tile != null)
        {
            // Select this tile
            Debug.Log($"TileSelectionHandler: Selecting tile at {gridPosition} with number {tile.number} and color {tile.tileColor}");
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
            Debug.Log($"TileSelectionHandler: Clicked on empty cell at {gridPosition} with no tile selected.");
            // Clear any lingering selection state just to be safe
            ClearSelection();
        }
    }

    /// <summary>
    /// Handles the confirmation of a tile move.
    /// </summary>
    public void HandleTileMoveConfirmation(Vector2Int targetPosition)
    {
        if (selectedTile == null)
        {
            Debug.LogWarning("TileSelectionHandler: No tile selected for move confirmation.");
            return;
        }

        Vector2Int startPosition = BoardManager.Instance.GetGridPositionFromWorldPosition(selectedTile.transform.position);

        if (!BoardManager.Instance.IsValidMove(startPosition, targetPosition, selectedTile, out bool pathClear) || !pathClear)
        {
            Debug.LogWarning($"TileSelectionHandler: Invalid move from {startPosition} to {targetPosition}.");
            return;
        }

        Debug.Log($"TileSelectionHandler: Moving tile from {startPosition} to {targetPosition}.");
        BoardManager.Instance.MoveTile(selectedTile, startPosition, targetPosition);

        // Clear selection
        ClearSelection();
        
        // IMPORTANT: Call EndTurn to increment move count
        Debug.Log("TileSelectionHandler: Move completed, calling GameManager.EndTurn()");
        GameManager.Instance.EndTurn();
        
        // Handle post-move logic
        BoardManager.Instance.HandlePostMove();
    }

    /// <summary>
    /// Helper method to handle potential merges between tiles.
    /// </summary>
    private void HandlePotentialMerge(Vector2Int gridPosition, Tile targetTile)
    {
        Debug.Log($"TileSelectionHandler: Already have selected tile at {selectedTilePosition} with number {selectedTile.number}");
        
        // Make sure TileMergeHandler is instantiated
        if (TileMergeHandler.Instance == null)
        {
            Debug.LogError("TileSelectionHandler: TileMergeHandler.Instance is null! Cannot handle merge.");
            return;
        }
        
        // Delegate merge handling to TileMergeHandler
        bool mergeSuccessful = TileMergeHandler.Instance.HandlePotentialMerge(
            selectedTilePosition, gridPosition, selectedTile, targetTile);

        // Set the target position
        targetTilePosition = gridPosition;

        // If merge was not successful, select the new tile instead
        if (!mergeSuccessful)
        {
            Debug.Log("TileSelectionHandler: Cannot merge - selecting new tile instead");
            ClearSelection();
            selectedTile = targetTile;
            selectedTilePosition = gridPosition;
            targetTile.SetState(Tile.TileState.Selected);
            CreateSelectionHighlight(gridPosition);
            HighlightValidMoves(gridPosition, targetTile.number);
        }
    }

    /// <summary>
    /// Highlights valid moves for the selected tile.
    /// </summary>
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
                if (!BoardManager.Instance.IsWithinBounds(targetPosition)) break;
                if (BoardManager.Instance.IsCellOccupied(targetPosition)) 
                {
                    // Found a tile - check if it can be merged
                    Tile targetTile = BoardManager.Instance.GetTileAtPosition(targetPosition);
                    Tile sourceTile = BoardManager.Instance.GetTileAtPosition(startPosition);
                    
                    // Skip special tiles as merge targets
                    if (targetTile != null && targetTile.GetComponent<SpecialTile>() != null)
                    {
                        // Mark this as an invalid target with a red highlight
                        HighlightCellAsInvalidTarget(targetPosition);
                        break; // Stop highlighting in this direction
                    }
                    
                    // Highlight if same color (mergeable)
                    if (targetTile != null && sourceTile != null && 
                        TileMergeHandler.Instance.CompareColors(sourceTile.tileColor, targetTile.tileColor))
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

    /// <summary>
    /// Highlights a cell as a valid move target.
    /// </summary>
    private void HighlightCellAsMoveTarget(Vector2Int position)
    {
        GameObject cellIndicator = Instantiate(BoardManager.Instance.cellIndicatorPrefab, BoardManager.Instance.GetWorldPosition(position), Quaternion.identity, transform);
        cellIndicator.tag = "Highlight";
        SpriteRenderer highlightRenderer = cellIndicator.GetComponent<SpriteRenderer>();
        if (highlightRenderer != null)
        {
            highlightRenderer.color = moveHighlightColor;
            highlightRenderer.material = new Material(Shader.Find("Sprites/Default"));
            highlightRenderer.material.SetColor("_Color", highlightRenderer.color);
            // Make it slightly smaller than the cell to create a nice border effect
            float cellSize = BoardManager.Instance.cellSize;
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

    /// <summary>
    /// Highlights a cell as a valid merge target.
    /// </summary>
    private void HighlightCellAsMergeTarget(Vector2Int position)
    {
        GameObject cellIndicator = Instantiate(BoardManager.Instance.cellIndicatorPrefab, BoardManager.Instance.GetWorldPosition(position), Quaternion.identity, transform);
        cellIndicator.tag = "Highlight";
        SpriteRenderer highlightRenderer = cellIndicator.GetComponent<SpriteRenderer>();
        if (highlightRenderer != null)
        {
            highlightRenderer.color = mergeHighlightColor;
            highlightRenderer.material = new Material(Shader.Find("Sprites/Default"));
            highlightRenderer.material.SetColor("_Color", highlightRenderer.color);
            // Make it slightly larger and with more distinct animation for merge targets
            float cellSize = BoardManager.Instance.cellSize;
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

    /// <summary>
    /// Highlights a cell as an invalid target (e.g., special tile that can't be merged).
    /// </summary>
    private void HighlightCellAsInvalidTarget(Vector2Int position)
    {
        GameObject cellIndicator = Instantiate(BoardManager.Instance.cellIndicatorPrefab, BoardManager.Instance.GetWorldPosition(position), Quaternion.identity, transform);
        cellIndicator.tag = "Highlight";
        
        SpriteRenderer highlightRenderer = cellIndicator.GetComponent<SpriteRenderer>();
        if (highlightRenderer != null)
        {
            highlightRenderer.color = invalidTargetHighlightColor;
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
            
            float cellSize = BoardManager.Instance.cellSize;
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

    /// <summary>
    /// Highlights a cell as a blocking tile (non-matching color).
    /// </summary>
    private void HighlightCellAsBlockingTile(Vector2Int position)
    {
        GameObject cellIndicator = Instantiate(BoardManager.Instance.cellIndicatorPrefab, BoardManager.Instance.GetWorldPosition(position), Quaternion.identity, transform);
        cellIndicator.tag = "Highlight";
        
        SpriteRenderer highlightRenderer = cellIndicator.GetComponent<SpriteRenderer>();
        if (highlightRenderer != null)
        {
            highlightRenderer.color = blockingTileHighlightColor;
            highlightRenderer.material = new Material(Shader.Find("Sprites/Default"));
            highlightRenderer.material.SetColor("_Color", highlightRenderer.color);
            
            float cellSize = BoardManager.Instance.cellSize;
            // Slightly smaller outline around the tile
            cellIndicator.transform.localScale = new Vector3(cellSize * 0.85f, cellSize * 0.85f, 1f);
            
            // No animation needed for blocking tiles - just static highlight
        }
    }

    /// <summary>
    /// Creates a visual highlight around a selected tile.
    /// </summary>
    private void CreateSelectionHighlight(Vector2Int position)
    {
        // Only create highlights when in WaitingForInputState
        if (GameStateManager.Instance == null || !GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            Debug.Log("TileSelectionHandler: CreateSelectionHighlight aborted - not in WaitingForInputState");
            return;
        }

        GameObject highlight = new GameObject($"Selection_{position.x}_{position.y}");
        // Change the tag to "Highlight" which should already be defined in your project
        highlight.tag = "Highlight";
        highlight.transform.position = BoardManager.Instance.GetWorldPosition(position);
        highlight.transform.SetParent(transform);

        // Add a component we can use to identify it as a selection highlight
        highlight.AddComponent<SelectionHighlightIdentifier>();
        SpriteRenderer renderer = highlight.AddComponent<SpriteRenderer>();
        renderer.sprite = selectedTile.GetComponent<SpriteRenderer>().sprite;
        renderer.color = selectionHighlightColor; // Golden highlight
        renderer.sortingOrder = -1; // Just behind the tile

        float cellSize = BoardManager.Instance.cellSize;
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
    /// Explicitly clears the selection state (selected tile and position)
    /// </summary>
    public void ClearSelection()
    {
        Debug.Log("TileSelectionHandler: Clearing tile selection state");
        
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

    /// <summary>
    /// Clears all highlights from the board.
    /// </summary>
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

    /// <summary>
    /// Checks if there's a currently selected tile
    /// </summary>
    public bool HasSelectedTile()
    {
        return selectedTile != null;
    }
}

/// <summary>
/// Simple component to identify objects that are selection highlights.
/// </summary>
public class SelectionHighlightIdentifier : MonoBehaviour
{
    // This is an empty marker component
}
