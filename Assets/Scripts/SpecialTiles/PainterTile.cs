using UnityEngine;
using System.Collections;

public class PainterTile : SpecialTile
{
    [SerializeField] private ParticleSystem colorBurstEffect;
    [SerializeField] private float effectDuration = 0.5f;
    [SerializeField] private Color pulseColor = new Color(0.5f, 0.8f, 1f); // Light blue pulse
    [SerializeField] private float pulseFrequency = 1.3f;
    [SerializeField] private float tileScale = 1.1f; // Scale factor for the tile
    
    [Header("Audio")]
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private AudioClip colorChangeSound;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.7f;
    [SerializeField] private bool useRandomPitch = true;
    [SerializeField] [Range(0.8f, 1.2f)] private float minPitch = 0.95f;
    [SerializeField] [Range(0.8f, 1.2f)] private float maxPitch = 1.05f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private AudioSource audioSource;
    private PainterTileVisuals tileVisuals;

    private void Start()
    {
        specialAbilityName = "Painter"; // Ensure this matches the ability name used in SpawnSpecialTile
        
        // Set the tile scale to make it visually distinct
        transform.localScale = new Vector3(tileScale, tileScale, 1f);
        
        // Initialize sprite renderer and colors
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            StartCoroutine(PulseAnimation());
        }
        
        // Add visuals component if not present
        tileVisuals = GetComponent<PainterTileVisuals>();
        if (tileVisuals == null)
        {
            tileVisuals = gameObject.AddComponent<PainterTileVisuals>();
        }
        
        // Initialize audio source
        InitializeAudio();
        
        // Adjust text size for better visibility
        AdjustTextSize();
    }
    
    private void InitializeAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.volume = volume;
            
            // If no sounds are assigned, try to load default ones
            if (activationSound == null)
            {
                activationSound = Resources.Load<AudioClip>("Sounds/PainterActivation");
            }
            if (colorChangeSound == null)
            {
                colorChangeSound = Resources.Load<AudioClip>("Sounds/ColorChange");
            }
        }
    }
    
    private void AdjustTextSize()
    {
        TMPro.TextMeshPro textMesh = GetComponentInChildren<TMPro.TextMeshPro>();
        if (textMesh != null)
        {
            // Scale down the text to fit within the larger tile
            textMesh.transform.localScale = new Vector3(1f / tileScale, 1f / tileScale, 1f);
            // Increase font size and make it bold for better visibility
            textMesh.fontSize *= 1.1f;
            textMesh.fontStyle = TMPro.FontStyles.Bold;
            textMesh.ForceMeshUpdate();
        }
    }

    private void OnMouseDown()
    {
        // Trigger activation when the player taps the tile
        Activate();
    }

    public override void ActivateAbility()
    {
        Debug.Log("PainterTile: Activating ability to convert adjacent tiles to its own color.");
        StartCoroutine(PainterSequence());
    }

    private IEnumerator PainterSequence()
    {
        Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);
        Tile painterTile = GetComponent<Tile>();

        if (painterTile == null)
        {
            Debug.LogError("PainterTile: Tile component is missing. Cannot activate ability.");
            DestroyTile();
            yield break;
        }
        
        // Play activation sound
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound, volume);
        }
        
        // Visual feedback for activation
        if (spriteRenderer != null)
        {
            LeanTween.scale(gameObject, Vector3.one * tileScale * 1.3f, 0.2f).setEaseOutBack();
            LeanTween.color(spriteRenderer.gameObject, pulseColor, 0.2f);
            yield return new WaitForSeconds(0.2f);
        }
        
        // Play particle effect if assigned
        if (colorBurstEffect != null)
        {
            ParticleSystem effect = Instantiate(colorBurstEffect, transform.position, Quaternion.identity);
            effect.Play();
        }
        
        yield return new WaitForSeconds(0.1f);
        
        // Convert all adjacent tiles to the PainterTile's color (including diagonals)
        Vector2Int[] adjacentPositions = new Vector2Int[]
        {
            tilePosition + Vector2Int.up,                 // North
            tilePosition + Vector2Int.right,              // East
            tilePosition + Vector2Int.down,               // South
            tilePosition + Vector2Int.left,               // West
            tilePosition + Vector2Int.up + Vector2Int.right,      // Northeast
            tilePosition + Vector2Int.up + Vector2Int.left,       // Northwest
            tilePosition + Vector2Int.down + Vector2Int.right,    // Southeast
            tilePosition + Vector2Int.down + Vector2Int.left      // Southwest
        };

        int changedTileCount = 0;

        foreach (Vector2Int pos in adjacentPositions)
        {
            if (BoardManager.Instance.IsWithinBounds(pos))
            {
                Tile adjacentTile = BoardManager.Instance.GetTileAtPosition(pos);
                if (adjacentTile != null)
                {
                    // Store original color for animation
                    Color originalColor = adjacentTile.tileColor;
                    
                    // If the tile already has the same color, skip it
                    if (BoardManager.Instance.CompareColors(originalColor, painterTile.tileColor))
                    {
                        Debug.Log($"PainterTile: Tile at {pos} already has the same color, skipping.");
                        continue;
                    }
                    
                    Debug.Log($"PainterTile: Changing tile at {pos} from {originalColor} to {painterTile.tileColor}.");
                    
                    // Change the tile's color
                    adjacentTile.tileColor = painterTile.tileColor;
                    
                    // IMPORTANT: Ensure the tile is still properly registered in the board
                    BoardManager.Instance.ReregisterTileAtPosition(pos, adjacentTile);
                    
                    // Animate the color change
                    StartCoroutine(AnimateColorChange(adjacentTile, originalColor, painterTile.tileColor));
                    changedTileCount++;
                    
                    // Play color change sound with slight delay between each tile
                    if (audioSource != null && colorChangeSound != null)
                    {
                        if (useRandomPitch)
                        {
                            audioSource.pitch = Random.Range(minPitch, maxPitch);
                        }
                        audioSource.PlayOneShot(colorChangeSound, volume * 0.8f);
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
        
        // Wait for effects to complete
        yield return new WaitForSeconds(effectDuration);
        
        // Add bonus score based on how many tiles were changed
        if (changedTileCount > 0)
        {
            ScoreManager.Instance?.AddSpecialTileBonus();
            
            // Add extra score for multiple tiles changed (3 points per tile)
            if (changedTileCount > 1)
            {
                ScoreManager.Instance?.AddScore(changedTileCount * 3);
            }
            
            Debug.Log($"PainterTile: Successfully changed {changedTileCount} tiles");
        }
        else
        {
            Debug.Log("PainterTile: No tiles were changed");
        }

        // Destroy the PainterTile itself
        DestroyTile();
    }
    
    private IEnumerator AnimateColorChange(Tile tile, Color startColor, Color targetColor)
    {
        if (tile == null) yield break;
        
        // Save original scale
        Vector3 originalScale = tile.transform.localScale;
        
        // Quick pulse animation
        LeanTween.cancel(tile.gameObject);
        LeanTween.scale(tile.gameObject, originalScale * 1.2f, 0.2f).setEaseOutBack();
        
        // Create color swirl effect
        GameObject colorEffect = new GameObject("ColorChangeEffect");
        colorEffect.transform.position = tile.transform.position;
        
        SpriteRenderer effectRenderer = colorEffect.AddComponent<SpriteRenderer>();
        effectRenderer.sprite = tile.GetComponent<SpriteRenderer>().sprite;
        effectRenderer.color = targetColor;
        effectRenderer.sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder + 1;
        
        // Start with zero scale and transparent
        colorEffect.transform.localScale = Vector3.zero;
        effectRenderer.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0);
        
        // Apply color to the tile itself
        tile.tileColor = targetColor;
        tile.UpdateVisuals();
        
        // Animate the swirl effect
        LeanTween.scale(colorEffect, originalScale * 1.5f, 0.4f).setEaseOutQuad();
        LeanTween.alpha(colorEffect, 0.7f, 0.2f).setEaseInOutSine();
        LeanTween.alpha(colorEffect, 0f, 0.3f).setEaseInOutSine().setDelay(0.2f);
        LeanTween.rotateZ(colorEffect, 180f, 0.5f).setEaseInOutSine().setOnComplete(() => {
            Destroy(colorEffect);
        });
        
        yield return new WaitForSeconds(0.3f);
        
        // Return to normal scale with a slight bounce
        LeanTween.scale(tile.gameObject, originalScale, 0.2f).setEaseInOutBack();
    }

    private void DestroyTile()
    {
        Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);
        
        // Use our centralized destruction utility
        TileDestructionUtility.DestroyTile(gameObject, tilePosition);
        
        Debug.Log($"PainterTile: Destroyed at {tilePosition}.");
    }
    
    private IEnumerator PulseAnimation()
    {
        while (gameObject != null && spriteRenderer != null)
        {
            // Pulse color between original and pulse color
            LeanTween.color(spriteRenderer.gameObject, pulseColor, 0.5f / pulseFrequency).setEaseInOutSine();
            yield return new WaitForSeconds(0.5f / pulseFrequency);
            LeanTween.color(spriteRenderer.gameObject, originalColor, 0.5f / pulseFrequency).setEaseInOutSine();
            yield return new WaitForSeconds(0.5f / pulseFrequency);
        }
    }
}
