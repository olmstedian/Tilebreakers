using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlasterTile : SpecialTile
{
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float explosionDuration = 0.5f;
    [SerializeField] private Color pulseColor = Color.red;
    [SerializeField] private float pulseFrequency = 1.5f;
    [SerializeField] private float tileScale = 1.15f; // Scale factor for the tile
    
    [Header("Audio")]
    [SerializeField] private AudioClip explosionSound;     // Main explosion sound
    [SerializeField] private AudioClip tileCrackSound;     // Sound when adjacent tiles are destroyed
    [SerializeField] [Range(0f, 1f)] private float volume = 0.7f;
    [SerializeField] private bool useRandomPitch = true;
    [SerializeField] [Range(0.8f, 1.2f)] private float minPitch = 0.9f;
    [SerializeField] [Range(0.8f, 1.2f)] private float maxPitch = 1.1f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private BlasterTileVisuals tileVisuals;
    private AudioSource audioSource;

    private void Start()
    {
        specialAbilityName = "Blaster"; // Ensure this matches the ability name used in SpawnSpecialTile
        
        // Set the tile scale larger than normal tiles
        transform.localScale = new Vector3(tileScale, tileScale, 1f);
        
        tileVisuals = GetComponent<BlasterTileVisuals>();
        if (tileVisuals == null)
        {
            // Add the component if it doesn't exist
            tileVisuals = gameObject.AddComponent<BlasterTileVisuals>();
        }
        
        // Initialize audio source
        InitializeAudio();
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            StartCoroutine(PulseAnimation());
        }
        
        // Adjust text size to fit the larger tile
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
            
            // If no explosion sound is assigned, try to load a default one
            if (explosionSound == null)
            {
                explosionSound = Resources.Load<AudioClip>("Sounds/BlasterExplosion");
                tileCrackSound = Resources.Load<AudioClip>("Sounds/TileCrack");
                
                if (explosionSound == null)
                {
                    Debug.LogWarning("BlasterTile: No explosion sound assigned or found in Resources.");
                }
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
            // Increase font size to compensate
            textMesh.fontSize *= 1.1f;
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
        Debug.Log("BlasterTile: Activating Blaster ability.");
        StartCoroutine(BlastSequence());
    }

    private IEnumerator BlastSequence()
    {
        Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);
        
        // First, pulse the tile to indicate activation
        if (spriteRenderer != null)
        {
            // Larger scale effect for the already larger tile
            LeanTween.scale(gameObject, Vector3.one * tileScale * 1.2f, 0.2f).setEaseOutBack();
            LeanTween.color(spriteRenderer.gameObject, Color.red, 0.2f);
            yield return new WaitForSeconds(0.3f);
        }

        // Play the explosion sound with slight randomization
        PlayExplosionSound();

        // Then show explosion effect with larger size
        if (explosionEffect != null)
        {
            ParticleSystem explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            // Scale up the particle system for the larger tile
            ParticleSystem.MainModule main = explosion.main;
            main.startSize = main.startSize.constant * 1.3f;
            explosion.Play();
        }
        else
        {
            // Fallback visual if no particle system is assigned
            GameObject explosionVisual = new GameObject("ExplosionVisual");
            explosionVisual.transform.position = transform.position;
            SpriteRenderer explosionRenderer = explosionVisual.AddComponent<SpriteRenderer>();
            explosionRenderer.sprite = Resources.Load<Sprite>("Effects/ExplosionSprite");
            explosionRenderer.color = Color.red;
            
            LeanTween.scale(explosionVisual, Vector3.one * explosionRadius, explosionDuration)
                .setEaseOutQuad()
                .setOnComplete(() => Destroy(explosionVisual));
            
            LeanTween.alpha(explosionVisual, 0f, explosionDuration);
        }

        // Wait for effect to be visible before destroying tiles
        yield return new WaitForSeconds(0.2f);
        
        // Destroy adjacent tiles with visual effects
        DestroyAdjacentTiles(tilePosition);
        
        // Wait for effects to finish
        yield return new WaitForSeconds(0.3f);
        
        // Destroy the BlasterTile itself
        DestroyTile();
    }
    
    private void PlayExplosionSound()
    {
        if (audioSource != null && explosionSound != null)
        {
            if (useRandomPitch)
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch);
            }
            audioSource.PlayOneShot(explosionSound, volume);
        }
    }
    
    private void PlayTileCrackSound(Vector3 position)
    {
        if (tileCrackSound != null)
        {
            // Create a temporary audio source at the position of the cracking tile
            GameObject audioObj = new GameObject("TileCrack_Audio");
            audioObj.transform.position = position;
            AudioSource tempSource = audioObj.AddComponent<AudioSource>();
            tempSource.clip = tileCrackSound;
            tempSource.spatialBlend = 0f; // 2D sound
            tempSource.volume = volume * 0.6f; // Slightly quieter than the main explosion
            tempSource.pitch = Random.Range(0.95f, 1.05f); // Subtle pitch variation
            tempSource.Play();
            
            // Destroy the temporary audio source when the sound is done
            Destroy(audioObj, tileCrackSound.length + 0.1f);
        }
    }

    private void DestroyAdjacentTiles(Vector2Int tilePosition)
    {
        Debug.Log($"BlasterTile: Destroying adjacent tiles around position {tilePosition}");
        
        // Get all adjacent positions (orthogonal and diagonal)
        List<Vector2Int> adjacentPositions = new List<Vector2Int>
        {
            tilePosition + Vector2Int.up,
            tilePosition + Vector2Int.down,
            tilePosition + Vector2Int.left,
            tilePosition + Vector2Int.right,
            tilePosition + Vector2Int.up + Vector2Int.left,
            tilePosition + Vector2Int.up + Vector2Int.right,
            tilePosition + Vector2Int.down + Vector2Int.left,
            tilePosition + Vector2Int.down + Vector2Int.right
        };

        // Log all positions to check
        Debug.Log($"BlasterTile: Checking {adjacentPositions.Count} adjacent positions");
        
        // Special check for merged cell
        if (BoardManager.Instance.lastMergedCellPosition.HasValue)
        {
            Vector2Int mergedPos = BoardManager.Instance.lastMergedCellPosition.Value;
            Debug.Log($"BlasterTile: Last merged cell was at position {mergedPos}");
            
            // If merged cell is adjacent to the blaster, prioritize destroying it
            if (adjacentPositions.Contains(mergedPos))
            {
                Debug.Log($"BlasterTile: Last merged cell {mergedPos} is adjacent to the blaster!");
                
                Tile mergedTile = BoardManager.Instance.GetTileAtPosition(mergedPos);
                if (mergedTile != null)
                {
                    Debug.Log($"BlasterTile: Found merged tile at position {mergedPos} with value {mergedTile.number}");
                    StartCoroutine(DestroyTileWithEffect(mergedTile, mergedPos));
                }
            }
        }
        
        foreach (Vector2Int pos in adjacentPositions)
        {
            // Skip if this is the last merged cell we already handled
            if (BoardManager.Instance.lastMergedCellPosition.HasValue && 
                BoardManager.Instance.lastMergedCellPosition.Value == pos)
            {
                continue;
            }
            
            Debug.Log($"BlasterTile: Checking position {pos}");
            
            // First check if the position is within board bounds
            if (!BoardManager.Instance.IsWithinBounds(pos))
            {
                Debug.Log($"BlasterTile: Position {pos} is out of bounds");
                continue;
            }
                
            // Directly get any tile at this position (regular or special)
            Tile adjacentTile = BoardManager.Instance.GetTileAtPosition(pos);
            if (adjacentTile != null)
            {
                Debug.Log($"BlasterTile: Found tile at position {pos} with value {adjacentTile.number}");
                StartCoroutine(DestroyTileWithEffect(adjacentTile, pos));
            }
            else
            {
                // Also check for special tiles that might not be registered normally
                SpecialTile specialTile = SpecialTileManager.Instance.GetSpecialTileAtPosition(pos);
                if (specialTile != null)
                {
                    Debug.Log($"BlasterTile: Found special tile '{specialTile.specialAbilityName}' at position {pos}");
                    
                    // Get the tile component if it exists
                    Tile tileComponent = specialTile.GetComponent<Tile>();
                    if (tileComponent != null)
                    {
                        StartCoroutine(DestroyTileWithEffect(tileComponent, pos));
                    }
                    else
                    {
                        // If no tile component, just destroy the special tile directly
                        SpecialTileManager.Instance.UnregisterSpecialTile(specialTile);
                        BoardManager.Instance.ClearCell(pos);
                        Destroy(specialTile.gameObject);
                        Debug.Log($"BlasterTile: Destroyed special tile at {pos}");
                    }
                }
                else
                {
                    Debug.Log($"BlasterTile: No tile found at position {pos}");
                }
            }
        }
    }

    private IEnumerator DestroyTileWithEffect(Tile tile, Vector2Int position)
    {
        if (tile == null)
        {
            Debug.LogWarning("BlasterTile: Attempted to destroy a null tile");
            yield break;
        }
        
        Debug.Log($"BlasterTile: Destroying tile at position {position} with value {tile.number}");
        
        // First mark that we're handling this tile to avoid duplicate destruction
        BoardManager.Instance.SetTileAtPosition(position, null);
        
        // Animate tile destruction
        LeanTween.scale(tile.gameObject, Vector3.one * 1.2f, 0.1f).setEaseOutQuad();
        yield return new WaitForSeconds(0.1f);
        LeanTween.scale(tile.gameObject, Vector3.zero, 0.2f).setEaseInQuad();
        
        // Play the crack sound at the tile's position
        PlayTileCrackSound(tile.transform.position);
        
        // Create a small particle burst at the tile position
        if (explosionEffect != null)
        {
            ParticleSystem miniExplosion = Instantiate(explosionEffect, tile.transform.position, Quaternion.identity);
            ParticleSystem.MainModule main = miniExplosion.main;
            main.startSize = main.startSize.constant * 0.5f; // Make it smaller than the main explosion
            miniExplosion.Play();
        }
        
        // Wait for animation to complete
        yield return new WaitForSeconds(0.2f);
        
        // Ensure the cell is cleared in BoardManager first
        BoardManager.Instance.ClearCell(position);
        
        // Make this cell available for new tile spawns
        BoardManager.Instance.AddToEmptyCells(position);
        
        // Handle case where this is a special tile
        SpecialTile specialTile = tile.GetComponent<SpecialTile>();
        if (specialTile != null)
        {
            SpecialTileManager.Instance.UnregisterSpecialTile(specialTile);
        }
        
        // Clean up
        Destroy(tile.gameObject);
        
        Debug.Log($"BlasterTile: Successfully destroyed tile at {position}");
    }

    private void DestroyTile()
    {
        Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);

        // Unregister from SpecialTileManager
        SpecialTileManager.Instance.UnregisterSpecialTile(this);

        // Clear the cell in BoardManager
        BoardManager.Instance.ClearCell(tilePosition);

        // Destroy the game object
        Destroy(gameObject);

        Debug.Log($"BlasterTile: Destroyed at {tilePosition}.");
    }
    
    private IEnumerator PulseAnimation()
    {
        while (true)
        {
            // Pulse color between original and pulse color
            LeanTween.color(spriteRenderer.gameObject, pulseColor, 0.5f / pulseFrequency).setEaseInOutSine();
            yield return new WaitForSeconds(0.5f / pulseFrequency);
            LeanTween.color(spriteRenderer.gameObject, originalColor, 0.5f / pulseFrequency).setEaseInOutSine();
            yield return new WaitForSeconds(0.5f / pulseFrequency);
        }
    }
}
