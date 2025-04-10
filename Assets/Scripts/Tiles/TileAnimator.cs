using UnityEngine;
using System.Collections;

/// <summary>
/// Handles visual animations for tiles such as movement, merging, and other effects.
/// </summary>
public class TileAnimator : MonoBehaviour
{
    [SerializeField] private float mergeAnimationDuration = 0.3f;
    [SerializeField] private float moveAnimationDuration = 0.2f;
    
    private Tile tile;
    private SpriteRenderer spriteRenderer;
    private bool isPlayingMergeAnimation = false;
    private bool isMoveAnimationPlaying = false;
    
    private void Awake()
    {
        tile = GetComponent<Tile>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    /// <summary>
    /// Plays the merge animation for when two tiles combine.
    /// </summary>
    public void PlayMergeAnimation()
    {
        // If the animation is already playing, don't restart it
        if (isPlayingMergeAnimation) return;
        
        // Set the tile state to Merging
        if (tile != null)
        {
            tile.SetState(Tile.TileState.Merging);
        }
        
        // Start the merge animation coroutine
        StartCoroutine(MergeAnimationSequence());
        
        // Play merge sound if available
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }
    
    /// <summary>
    /// Plays the movement animation for when a tile moves to a new position.
    /// </summary>
    public void PlayMoveAnimation(Vector2 targetPosition, float duration)
    {
        // If a merge animation is playing, don't start a move animation
        if (isPlayingMergeAnimation) return;
        
        // Set the tile state to Moving
        if (tile != null)
        {
            tile.SetState(Tile.TileState.Moving);
        }
        
        isMoveAnimationPlaying = true;
        
        // Lower the sorting order during movement
        if (spriteRenderer != null)
        {
            TileSortingManager.StoreOriginalSortingOrder(gameObject);
            TileSortingManager.SetAnimationSortingOrder(gameObject);
        }
        
        // Start the movement animation
        StartCoroutine(MoveAnimationSequence(duration));
    }
    
    private IEnumerator MergeAnimationSequence()
    {
        isPlayingMergeAnimation = true;
        
        // Store the original scale
        Vector3 originalScale = transform.localScale;
        
        // First, quickly scale up
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, originalScale * 1.3f, mergeAnimationDuration * 0.5f).setEaseOutQuad();
        
        // Create a particle effect for merging
        CreateMergeEffect();
        
        yield return new WaitForSeconds(mergeAnimationDuration * 0.5f);
        
        // Then scale back to normal size with a bounce effect
        LeanTween.scale(gameObject, originalScale, mergeAnimationDuration * 0.5f).setEaseInOutBack();
        
        // Wait for the animation to finish
        yield return new WaitForSeconds(mergeAnimationDuration * 0.5f);
        
        isPlayingMergeAnimation = false;
        
        // Set the tile state back to Idle
        if (tile != null)
        {
            tile.SetState(Tile.TileState.Idle);
        }
    }
    
    private IEnumerator MoveAnimationSequence(float duration)
    {
        // Wait for the specified duration (the actual movement is handled by TileMover)
        yield return new WaitForSeconds(duration);
        
        isMoveAnimationPlaying = false;
        
        // Restore original sorting order
        if (spriteRenderer != null)
        {
            TileSortingManager.RestoreSortingOrder(gameObject);
        }
        
        // Set the tile state back to Idle
        if (tile != null && tile.CurrentState == Tile.TileState.Moving)
        {
            tile.SetState(Tile.TileState.Idle);
        }
    }
    
    private void CreateMergeEffect()
    {
        // Create a simple particle burst effect
        for (int i = 0; i < 8; i++)
        {
            GameObject particle = new GameObject($"MergeParticle_{i}");
            particle.transform.position = transform.position;
            
            // Add a sprite renderer component
            SpriteRenderer particleRenderer = particle.AddComponent<SpriteRenderer>();
            
            // Try to load a particle sprite, or use a simple circle
            particleRenderer.sprite = Resources.Load<Sprite>("Effects/StarParticle");
            if (particleRenderer.sprite == null)
            {
                // Create a simple circle texture
                Texture2D tex = new Texture2D(16, 16);
                for (int x = 0; x < 16; x++)
                {
                    for (int y = 0; y < 16; y++)
                    {
                        float distSq = Mathf.Pow(x - 8, 2) + Mathf.Pow(y - 8, 2);
                        float alpha = Mathf.Clamp01(1 - distSq / 64f);
                        tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                    }
                }
                tex.Apply();
                particleRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f));
            }
            
            // Use the tile's color for the particle
            if (tile != null)
            {
                particleRenderer.color = tile.tileColor;
            }
            else
            {
                particleRenderer.color = new Color(1f, 1f, 0.5f); // Default to a yellow-ish color
            }
            
            // Make the particle appear in front of the tile
            particleRenderer.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 1 : 1;
            
            // Calculate a random direction
            float angle = i * 45f; // Distribute evenly in 8 directions
            Vector3 direction = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0
            ) * 0.7f; // Distance to travel
            
            // Animate the particle
            LeanTween.move(particle, transform.position + direction, 0.5f).setEaseOutQuad();
            LeanTween.scale(particle, Vector3.one * 0.3f, 0.1f).setEaseOutQuad(); // Start small
            LeanTween.scale(particle, Vector3.zero, 0.4f).setEaseInQuad().setDelay(0.1f); // Fade out
            LeanTween.alpha(particle, 0f, 0.4f).setEaseInQuad().setDelay(0.1f).setOnComplete(() => {
                Destroy(particle);
            });
        }
    }
    
    /// <summary>
    /// Checks if a merge animation is currently playing.
    /// </summary>
    public bool IsPlayingMergeAnimation()
    {
        return isPlayingMergeAnimation;
    }
    
    /// <summary>
    /// Checks if a move animation is currently playing.
    /// </summary>
    public bool IsMoveAnimationPlaying()
    {
        return isMoveAnimationPlaying;
    }
    
    /// <summary>
    /// Checks if any animation is currently playing.
    /// </summary>
    public bool IsAnimating()
    {
        return isPlayingMergeAnimation || isMoveAnimationPlaying;
    }
    
    /// <summary>
    /// Stops all animations immediately and sets the tile to Idle state.
    /// </summary>
    public void StopAllAnimations()
    {
        StopAllCoroutines();
        LeanTween.cancel(gameObject);
        
        isPlayingMergeAnimation = false;
        isMoveAnimationPlaying = false;
        
        // Return to original scale
        transform.localScale = Vector3.one;
        
        // Restore original sorting order
        TileSortingManager.RestoreSortingOrder(gameObject);
        
        // Return to Idle state
        if (tile != null)
        {
            tile.SetState(Tile.TileState.Idle);
        }
    }
}
