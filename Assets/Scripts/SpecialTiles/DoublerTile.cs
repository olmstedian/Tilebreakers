using UnityEngine;
using System.Collections;
using Tilebreakers.Core;

public class DoublerTile : SpecialTile
{
    [SerializeField] private ParticleSystem activationEffect;
    [SerializeField] private float effectDuration = 0.5f;
    [SerializeField] private Color pulseColor = Color.yellow;
    [SerializeField] private float pulseFrequency = 1.2f;
    [SerializeField] private float tileScale = 1.1f; // Scale factor for the tile
    
    [Header("Audio")]
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private AudioClip tileDoubleSound;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.7f;
    [SerializeField] private bool useRandomPitch = true;
    [SerializeField] [Range(0.8f, 1.2f)] private float minPitch = 0.95f;
    [SerializeField] [Range(0.8f, 1.2f)] private float maxPitch = 1.05f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private AudioSource audioSource;

    private void Start()
    {
        specialAbilityName = "Doubler"; // Ensure this matches the ability name used in SpawnSpecialTile
        
        // Set the tile scale to make it visually distinct
        transform.localScale = new Vector3(tileScale, tileScale, 1f);
        
        // Initialize sprite renderer and colors
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            StartCoroutine(PulseAnimation());
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
                activationSound = Resources.Load<AudioClip>("Sounds/DoublerActivation");
            }
            if (tileDoubleSound == null)
            {
                tileDoubleSound = Resources.Load<AudioClip>("Sounds/TileDouble");
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

    public override void ActivateAbility()
    {
        Debug.Log("DoublerTile: Activating ability to double adjacent tiles.");
        StartCoroutine(DoublerSequence());
    }

    private IEnumerator DoublerSequence()
    {
        Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);
        
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
        if (activationEffect != null)
        {
            ParticleSystem effect = Instantiate(activationEffect, transform.position, Quaternion.identity);
            effect.Play();
        }
        
        yield return new WaitForSeconds(0.1f);
        
        // Get ALL adjacent positions to double their values (including diagonals)
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

        int doubledTileCount = 0;
        bool doubledMergedTile = false;
        
        Debug.Log($"DoublerTile: Doubling tiles adjacent to position {tilePosition} (including diagonals)");
        
        // Special check for merged cell first - prioritize doubling it
        if (BoardManager.Instance.lastMergedCellPosition.HasValue)
        {
            Vector2Int mergedPos = BoardManager.Instance.lastMergedCellPosition.Value;
            
            // Check if the merged tile is adjacent to the doubler (including diagonals)
            if (System.Array.IndexOf(adjacentPositions, mergedPos) >= 0)
            {
                Debug.Log($"DoublerTile: Found recently merged tile at {mergedPos}!");
                Tile mergedTile = BoardManager.Instance.GetTileAtPosition(mergedPos);
                if (mergedTile != null)
                {
                    // Store original value for feedback and verification
                    int originalValue = mergedTile.number;
                    
                    // Double the recently merged tile
                    mergedTile.number *= 2;
                    mergedTile.UpdateVisuals();
                    
                    Debug.Log($"DoublerTile: Doubled recently merged tile from {originalValue} to {mergedTile.number}");
                    
                    // Create more impressive visual effect for doubling a merged tile
                    StartCoroutine(AnimateMergedTileDoubling(mergedTile));
                    
                    doubledTileCount++;
                    doubledMergedTile = true;
                    
                    if (audioSource != null && tileDoubleSound != null)
                    {
                        // Play a higher pitch for doubled merged tiles
                        audioSource.pitch = Mathf.Min(maxPitch * 1.2f, 1.5f);
                        audioSource.PlayOneShot(tileDoubleSound, volume);
                    }
                    
                    yield return new WaitForSeconds(0.2f); // Longer pause for emphasis
                }
            }
        }
        
        // Process all adjacent positions (including diagonals)
        foreach (Vector2Int pos in adjacentPositions)
        {
            // Skip if this is the last merged cell we already processed
            if (BoardManager.Instance.lastMergedCellPosition.HasValue && 
                BoardManager.Instance.lastMergedCellPosition.Value == pos && 
                doubledMergedTile)
            {
                continue;
            }
            
            if (BoardManager.Instance.IsWithinBounds(pos))
            {
                Tile adjacentTile = BoardManager.Instance.GetTileAtPosition(pos);
                if (adjacentTile != null)
                {
                    // Store the original value for logging and verification
                    int originalValue = adjacentTile.number;
                    
                    // Check if this tile was previously doubled
                    bool wasPreviouslyDoubled = adjacentTile.GetComponent<DoublingTracker>() != null;
                    
                    // Double the tile value
                    adjacentTile.number *= 2;
                    adjacentTile.UpdateVisuals();
                    
                    // Mark the tile as doubled for future reference
                    if (!wasPreviouslyDoubled)
                    {
                        DoublingTracker tracker = adjacentTile.gameObject.AddComponent<DoublingTracker>();
                        tracker.doubledAt = Time.time;
                    }
                    else
                    {
                        // Update the doubling timestamp
                        DoublingTracker tracker = adjacentTile.GetComponent<DoublingTracker>();
                        if (tracker != null)
                        {
                            tracker.doubledAt = Time.time;
                            tracker.doublingCount++;
                        }
                    }
                    
                    doubledTileCount++;
                    
                    if (wasPreviouslyDoubled)
                    {
                        Debug.Log($"DoublerTile: Re-doubled previously doubled tile at {pos} from {originalValue} to {adjacentTile.number}!");
                        StartCoroutine(AnimateRedoubledTile(adjacentTile));
                    }
                    else
                    {
                        Debug.Log($"DoublerTile: Doubled tile at {pos} from {originalValue} to {adjacentTile.number}");
                        StartCoroutine(AnimateTileDoubling(adjacentTile));
                    }
                    
                    // Play sound effect with slight delay between each tile
                    if (audioSource != null && tileDoubleSound != null)
                    {
                        if (useRandomPitch)
                        {
                            audioSource.pitch = wasPreviouslyDoubled ? 
                                Random.Range(minPitch * 1.1f, maxPitch * 1.1f) : // Slightly higher pitch for re-doubled tiles
                                Random.Range(minPitch, maxPitch);
                        }
                        audioSource.PlayOneShot(tileDoubleSound, volume * 0.8f);
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                else
                {
                    Debug.Log($"DoublerTile: No tile found at adjacent position {pos}");
                }
            }
            else
            {
                Debug.Log($"DoublerTile: Position {pos} is out of bounds");
            }
        }
        
        // Wait for effects to complete
        yield return new WaitForSeconds(effectDuration);
        
        // Add bonus score based on how many tiles were doubled
        if (doubledTileCount > 0)
        {
            ScoreManager.Instance?.AddSpecialTileBonus();
            
            // Add extra score for multiple tiles doubled (5 points per tile)
            if (doubledTileCount > 1)
            {
                ScoreManager.Instance?.AddScore(doubledTileCount * 5);
            }
            
            // Bonus points for doubling a merged tile
            if (doubledMergedTile)
            {
                ScoreManager.Instance?.AddScore(10);
                Debug.Log("DoublerTile: Bonus points awarded for doubling a merged tile!");
            }
            
            Debug.Log($"DoublerTile: Successfully doubled {doubledTileCount} tiles");
        }
        else
        {
            Debug.Log("DoublerTile: No adjacent tiles were doubled");
        }
        
        // Destroy the DoublerTile itself
        DestroyTile();
    }

    private IEnumerator AnimateTileDoubling(Tile tile)
    {
        if (tile == null) yield break;
        
        // Save original scale
        Vector3 originalScale = tile.transform.localScale;
        
        // Quick pulse animation
        LeanTween.cancel(tile.gameObject);
        LeanTween.scale(tile.gameObject, originalScale * 1.3f, 0.2f).setEaseOutBack();
        
        // Add a brief glow effect
        SpriteRenderer tileRenderer = tile.GetComponent<SpriteRenderer>();
        if (tileRenderer != null)
        {
            // Create glow overlay
            GameObject glowObj = new GameObject("DoubleGlow");
            glowObj.transform.position = tile.transform.position;
            glowObj.transform.SetParent(tile.transform);
            
            SpriteRenderer glowRenderer = glowObj.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = tileRenderer.sprite;
            glowRenderer.color = new Color(1f, 1f, 0.5f, 0.8f);
            glowRenderer.sortingOrder = tileRenderer.sortingOrder + 1;
            
            // Animate glow fade out
            LeanTween.alpha(glowObj, 0f, 0.5f).setEaseOutExpo().setOnComplete(() => {
                Destroy(glowObj);
            });
            LeanTween.scale(glowObj, Vector3.one * 1.5f, 0.5f).setEaseOutExpo();
        }
        
        yield return new WaitForSeconds(0.25f);
        
        // Return to normal scale with a slight bounce
        LeanTween.scale(tile.gameObject, originalScale, 0.3f).setEaseInOutBack();
    }

    private IEnumerator AnimateRedoubledTile(Tile tile)
    {
        if (tile == null) yield break;
        
        // Save original scale
        Vector3 originalScale = tile.transform.localScale;
        
        // More dramatic pulse animation
        LeanTween.cancel(tile.gameObject);
        LeanTween.scale(tile.gameObject, originalScale * 1.5f, 0.3f).setEaseOutBack();
        
        // Add a stronger glow effect
        SpriteRenderer tileRenderer = tile.GetComponent<SpriteRenderer>();
        if (tileRenderer != null)
        {
            // Create glow overlay with different color
            GameObject glowObj = new GameObject("DoubleGlow");
            glowObj.transform.position = tile.transform.position;
            glowObj.transform.SetParent(tile.transform);
            
            SpriteRenderer glowRenderer = glowObj.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = tileRenderer.sprite;
            glowRenderer.color = new Color(0.9f, 0.5f, 0.9f, 0.9f); // Purple-ish for re-doubled tiles
            glowRenderer.sortingOrder = tileRenderer.sortingOrder + 1;
            
            // Animate glow fade out with spinning
            LeanTween.alpha(glowObj, 0f, 0.8f).setEaseOutExpo().setOnComplete(() => {
                Destroy(glowObj);
            });
            LeanTween.scale(glowObj, Vector3.one * 2f, 0.8f).setEaseOutExpo();
            LeanTween.rotateZ(glowObj, 180f, 0.8f).setEaseOutCubic();
        }
        
        // Create text feedback
        GameObject feedbackText = new GameObject("RedoubleText");
        feedbackText.transform.position = tile.transform.position + new Vector3(0, 0, -0.1f);
        TMPro.TextMeshPro text = feedbackText.AddComponent<TMPro.TextMeshPro>();
        text.text = "×4!";
        text.fontSize = 6;
        text.color = new Color(1f, 0.5f, 1f); // Purple text
        text.alignment = TMPro.TextAlignmentOptions.Center;
        
        // Animate the text up and fade out
        LeanTween.moveY(feedbackText, tile.transform.position.y + 1f, 1f).setEaseOutQuad();
        LeanTween.alpha(feedbackText, 0f, 1f).setEaseInQuad().setOnComplete(() => {
            Destroy(feedbackText);
        });
        
        yield return new WaitForSeconds(0.3f);
        
        // Return to normal scale with a slight bounce
        LeanTween.scale(tile.gameObject, originalScale, 0.4f).setEaseInOutBack();
    }

    private IEnumerator AnimateMergedTileDoubling(Tile tile)
    {
        if (tile == null) yield break;
        
        // Save original scale
        Vector3 originalScale = tile.transform.localScale;
        
        // Special effect for doubling a merged tile - very flashy!
        LeanTween.cancel(tile.gameObject);
        LeanTween.scale(tile.gameObject, originalScale * 1.6f, 0.4f).setEaseOutElastic();
        
        // Create a burst of particles
        for (int i = 0; i < 8; i++)
        {
            GameObject particle = new GameObject($"BurstParticle_{i}");
            particle.transform.position = tile.transform.position;
            
            SpriteRenderer particleRenderer = particle.AddComponent<SpriteRenderer>();
            particleRenderer.sprite = Resources.Load<Sprite>("Effects/GlowParticle");
            if (particleRenderer.sprite == null)
            {
                // Create a simple circle sprite
                Texture2D tex = new Texture2D(32, 32);
                for (int x = 0; x < 32; x++)
                {
                    for (int y = 0; y < 32; y++)
                    {
                        float distSq = Mathf.Pow(x - 16, 2) + Mathf.Pow(y - 16, 2);
                        float alpha = Mathf.Clamp01(1 - distSq / 256f);
                        tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                    }
                }
                tex.Apply();
                particleRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            }
            
            float angle = (i / 8f) * 2 * Mathf.PI;
            float distance = 2f;
            Vector3 targetPos = tile.transform.position + new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                0
            );
            
            // Randomly color the particle
            Color[] colors = new Color[] {
                new Color(1f, 0.5f, 0.5f), // red
                new Color(0.5f, 1f, 0.5f), // green
                new Color(0.5f, 0.5f, 1f), // blue
                new Color(1f, 1f, 0.5f)    // yellow
            };
            particleRenderer.color = colors[Random.Range(0, colors.Length)];
            
            // Animate the particle
            LeanTween.scale(particle, Vector3.one * 0.5f, 0f); // Start small
            LeanTween.scale(particle, Vector3.zero, 0.8f).setEaseInQuad();
            LeanTween.move(particle, targetPos, 0.8f).setEaseOutQuad().setOnComplete(() => {
                Destroy(particle);
            });
        }
        
        // Create bold text feedback
        GameObject feedbackText = new GameObject("MergeDoubleText");
        feedbackText.transform.position = tile.transform.position + new Vector3(0, 0, -0.1f);
        TMPro.TextMeshPro text = feedbackText.AddComponent<TMPro.TextMeshPro>();
        text.text = "DOUBLE BONUS!";
        text.fontSize = 5f;
        text.fontStyle = TMPro.FontStyles.Bold;
        text.color = Color.yellow;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        
        // Animate the text
        LeanTween.scale(feedbackText, Vector3.one * 1.5f, 0.4f).setEaseOutBack();
        LeanTween.moveY(feedbackText, tile.transform.position.y + 1.5f, 1.2f).setEaseOutQuad();
        LeanTween.alpha(feedbackText, 0f, 1f).setEaseInQuad().setDelay(0.3f).setOnComplete(() => {
            Destroy(feedbackText);
        });
        
        yield return new WaitForSeconds(0.5f);
        
        // Return to normal scale with a strong bounce
        LeanTween.scale(tile.gameObject, originalScale, 0.5f).setEaseOutElastic();
    }

    private void DestroyTile()
    {
        Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);
        
        // Use our centralized destruction utility
        TileDestructionUtility.DestroyTile(gameObject, tilePosition);
        
        Debug.Log($"DoublerTile: Destroyed at {tilePosition}.");
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

    // Tracking component to identify tiles that have been doubled before
    public class DoublingTracker : MonoBehaviour
    {
        public float doubledAt;
        public int doublingCount = 1;
    }
}
