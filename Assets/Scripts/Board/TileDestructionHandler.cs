using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tilebreakers.Core; // Add namespace reference for ScoreFacade
// Add the namespace import for SpecialTileManager
using Tilebreakers.Special;

/// <summary>
/// Handles all tile destruction operations in the game.
/// </summary>
public class TileDestructionHandler : MonoBehaviour
{
    private static TileDestructionHandler _instance;
    public static TileDestructionHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TileDestructionHandler>();
                if (_instance == null)
                {
                    GameObject handler = new GameObject("TileDestructionHandler");
                    _instance = handler.AddComponent<TileDestructionHandler>();
                    DontDestroyOnLoad(handler);
                }
            }
            return _instance;
        }
    }

    [SerializeField] private ParticleSystem destructionParticlesPrefab;
    [SerializeField] private AudioClip destroySound;
    [Range(0f, 1f)] [SerializeField] private float destroySoundVolume = 0.7f;
    [SerializeField] private float destroyAnimationDuration = 0.3f;

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
    /// Safely destroy a tile and clean up all references in the game board.
    /// </summary>
    public void DestroyTile(GameObject tileObject, Vector2Int position)
    {
        if (tileObject == null)
        {
            Debug.LogWarning($"TileDestructionHandler: Attempted to destroy null tile object at position {position}");
            
            // Still clean up the board position
            BoardManager.Instance.ClearCell(position);
            BoardManager.Instance.AddToEmptyCells(position);
            
            return;
        }
        
        // Play destruction effects
        PlayDestructionEffects(tileObject.transform.position);
        
        // Check for SpecialTile component
        SpecialTile specialTile = tileObject.GetComponent<SpecialTile>();
        if (specialTile != null && Tilebreakers.Special.SpecialTileManager.Instance != null)
        {
            Debug.Log($"TileDestructionHandler: Found and removing special tile at {position}");
            // Use the public UnregisterSpecialTile method instead of directly accessing the private collection
            Tilebreakers.Special.SpecialTileManager.Instance.UnregisterSpecialTile(specialTile);
        }
        
        // Update board state
        BoardManager.Instance.ClearCell(position);
        BoardManager.Instance.AddToEmptyCells(position);
        
        // Destroy the GameObject
        Object.Destroy(tileObject);
        
        Debug.Log($"TileDestructionHandler: Destroyed tile at {position}");
        
        // Verify destruction after a slight delay
        StartCoroutine(VerifyDestructionAfterDelay(position, 0.1f));
    }
    
    /// <summary>
    /// Safely destroy a tile while ensuring all references are cleaned up.
    /// </summary>
    public void DestroyTile(Tile tile)
    {
        if (tile == null)
        {
            Debug.LogWarning("TileDestructionHandler: Attempted to destroy null tile");
            return;
        }
        
        Vector2Int position = BoardManager.Instance.GetGridPositionFromWorldPosition(tile.transform.position);
        DestroyTile(tile.gameObject, position);
    }
    
    /// <summary>
    /// Destroys a tile with animation and particle effects.
    /// </summary>
    public IEnumerator DestroyTileWithAnimation(Tile tile, Vector2Int position, System.Action onComplete = null)
    {
        if (tile == null || tile.gameObject == null)
        {
            Debug.LogWarning($"TileDestructionHandler: Cannot animate destruction of null tile at {position}");
            
            // Still clean up the board position
            BoardManager.Instance.ClearCell(position);
            BoardManager.Instance.AddToEmptyCells(position);
            
            onComplete?.Invoke();
            yield break;
        }
        
        // Get and store necessary components before we start destroying the tile
        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
        Transform tileTransform = tile.transform;
        Vector3 tilePosition = tileTransform.position;
        Vector3 originalScale = tileTransform.localScale;
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        
        // Disable collisions during animation
        Collider2D tileCollider = tile.GetComponent<Collider2D>();
        if (tileCollider != null)
        {
            tileCollider.enabled = false;
        }
        
        // Clear the board reference immediately to prevent issues during animation
        BoardManager.Instance.ClearCell(position);
        
        // Animation: Shrink and fade out
        float elapsed = 0;
        while (elapsed < destroyAnimationDuration && tileTransform != null)
        {
            float t = elapsed / destroyAnimationDuration;
            if (tileTransform != null)
            {
                tileTransform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                
                if (spriteRenderer != null)
                {
                    Color fadeColor = originalColor;
                    fadeColor.a = UnityEngine.Mathf.Lerp(1, 0, t);
                    spriteRenderer.color = fadeColor;
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Play destruction effects at the tile's position before destroying it
        PlayDestructionEffects(tilePosition);
        
        // Destroy the tile GameObject
        if (tile != null && tile.gameObject != null) 
        {
            Destroy(tile.gameObject);
        }
        
        // Ensure the cell is marked as empty in the board data
        BoardManager.Instance.AddToEmptyCells(position);
        
        // Wait a small amount of time for effects to play
        yield return new WaitForSeconds(0.05f);
        
        // Invoke completion callback
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Destroys a group of tiles with cascading animation.
    /// </summary>
    public IEnumerator DestroyTilesWithCascadingAnimation(List<(Tile tile, Vector2Int position)> tilesToDestroy, float delay = 0.05f)
    {
        int count = 0;
        foreach (var (tile, position) in tilesToDestroy)
        {
            // Slight delay between each destruction for cascading effect
            yield return new WaitForSeconds(delay);
            
            // Use the individual tile destruction method
            yield return DestroyTileWithAnimation(tile, position);
            
            count++;
        }
        
        Debug.Log($"TileDestructionHandler: Finished destroying {count} tiles in cascade");
        
        // After all tiles are destroyed, validate the board state
        BoardManager.Instance.ValidateEmptyCellsAfterStateChange();
    }
    
    /// <summary>
    /// Play particle effects and sound at the specified position.
    /// </summary>
    private void PlayDestructionEffects(Vector3 position)
    {
        // Spawn particle effects
        if (destructionParticlesPrefab != null)
        {
            ParticleSystem particles = Instantiate(destructionParticlesPrefab, position, Quaternion.identity);
            Destroy(particles.gameObject, particles.main.duration + 0.5f);
        }
        
        // Play sound effect
        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, position, destroySoundVolume);
        }
    }
    
    /// <summary>
    /// Verifies that a tile has been properly destroyed and performs cleanup if necessary.
    /// </summary>
    private IEnumerator VerifyDestructionAfterDelay(Vector2Int position, float delay)
    {
        yield return new WaitForSeconds(delay);
        VerifyTileDestruction(position);
    }
    
    /// <summary>
    /// Verifies that a tile has been properly destroyed and performs cleanup if necessary.
    /// </summary>
    public void VerifyTileDestruction(Vector2Int position)
    {
        // Check if the position is within board bounds
        if (!BoardManager.Instance.IsWithinBounds(position))
        {
            Debug.LogWarning($"TileDestructionHandler: Position {position} is out of bounds. Cannot verify destruction.");
            return;
        }
        
        // Check if there's still a tile at this position in the board array
        Tile tileAtPosition = BoardManager.Instance.GetTileAtPosition(position);
        if (tileAtPosition != null)
        {
            Debug.LogError($"TileDestructionHandler: Found tile at {position} after destruction! Cleaning up.");
            BoardManager.Instance.ClearCell(position);
            
            // If the GameObject still exists, destroy it
            if (tileAtPosition.gameObject != null)
            {
                Object.Destroy(tileAtPosition.gameObject);
            }
        }
        
        // Ensure the position is marked as empty
        bool empty = BoardManager.Instance.IsCellEmpty(position);
        if (!empty)
        {
            Debug.LogWarning($"TileDestructionHandler: Cell at {position} is not marked as empty after destruction. Fixing...");
            BoardManager.Instance.AddToEmptyCells(position);
        }
        
        // Check for physical objects at this position
        Vector2 worldPos = BoardManager.Instance.GetWorldPosition(position);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.4f);
        
        foreach (var collider in colliders)
        {
            // Skip triggers and UI elements
            if (collider.isTrigger || collider.CompareTag("Highlight")) continue;
            
            Tile remainingTile = collider.GetComponent<Tile>();
            SpecialTile remainingSpecialTile = collider.GetComponent<SpecialTile>();
            
            if (remainingTile != null || remainingSpecialTile != null)
            {
                Debug.LogError($"TileDestructionHandler: Found physical {(remainingTile != null ? "Tile" : "SpecialTile")} at {position} after destruction! Cleaning up.");
                Object.Destroy(collider.gameObject);
            }
        }
    }
    
    /// <summary>
    /// Queue multiple tiles for destruction with potential scoring and effects.
    /// </summary>
    public void QueueTilesForDestruction(List<Vector2Int> positions, bool applyScoring = true)
    {
        if (positions == null || positions.Count == 0) return;
        
        List<(Tile tile, Vector2Int position)> tilesToDestroy = new List<(Tile, Vector2Int)>();
        int totalScore = 0;
        
        foreach (Vector2Int pos in positions)
        {
            Tile tile = BoardManager.Instance.GetTileAtPosition(pos);
            if (tile != null)
            {
                tilesToDestroy.Add((tile, pos));
                totalScore += tile.number;
            }
        }
        
        if (tilesToDestroy.Count > 0)
        {
            // Add score if scoring is enabled
            if (applyScoring)
            {
                // Use the ScoreFacade to avoid ambiguity issues
                Tilebreakers.Core.ScoreManager.Instance.AddScore(totalScore);
                Debug.Log($"TileDestructionHandler: Added {totalScore} points for destroying {tilesToDestroy.Count} tiles");
            }
            
            // Transition to destruction state to handle the animations
            GameStateManager.Instance.SetState(new DestructionState(tilesToDestroy));
        }
    }
}
