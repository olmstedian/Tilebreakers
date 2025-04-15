using UnityEngine;
using System.Collections;
using Tilebreakers.Core; // Add this namespace to resolve ScoreManager

public class FreezeTile : SpecialTile
{
    [SerializeField] private ParticleSystem activationEffect;
    [SerializeField] private float effectDuration = 0.5f;
    [SerializeField] private Color pulseColor = new Color(0.5f, 0.8f, 1f); // Light blue
    [SerializeField] private float pulseFrequency = 1.2f;
    [SerializeField] private float tileScale = 1.1f; // Scale factor for the tile
    
    [Header("Audio")]
    [SerializeField] private AudioClip activationSound;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.7f;
    [SerializeField] private bool useRandomPitch = true;
    [SerializeField] [Range(0.8f, 1.2f)] private float minPitch = 0.95f;
    [SerializeField] [Range(0.8f, 1.2f)] private float maxPitch = 1.05f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private AudioSource audioSource;

    private void Start()
    {
        specialAbilityName = "Freeze"; // Ensure this matches the ability name used in SpawnSpecialTile
        
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
                activationSound = Resources.Load<AudioClip>("Sounds/FreezeActivation");
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
        Debug.Log("FreezeTile: Activating ability to skip the next tile spawn.");
        StartCoroutine(FreezeSequence());
    }

    private IEnumerator FreezeSequence()
    {
        // Play activation sound
        if (audioSource != null && activationSound != null)
        {
            if (useRandomPitch)
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch);
            }
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
        
        // Create an ice crystal effect that expands from the tile
        CreateIceCrystalEffect();
        
        yield return new WaitForSeconds(effectDuration);
        
        // Skip the next tile spawn
        BoardManager.Instance.SkipNextTileSpawn();

        // Add score bonus
        ScoreManager.Instance.AddSpecialTileBonus();
        
        // Show feedback text
        ShowFeedbackText("NEXT SPAWN SKIPPED!");
        
        // Wait a bit more for the player to see the effect
        yield return new WaitForSeconds(0.5f);
        
        // Destroy the FreezeTile itself
        DestroyTile();
    }
    
    private void CreateIceCrystalEffect()
    {
        // Create multiple ice crystal sprites that expand outward
        for (int i = 0; i < 8; i++)
        {
            GameObject crystal = new GameObject($"IceCrystal_{i}");
            crystal.transform.position = transform.position;
            
            SpriteRenderer crystalRenderer = crystal.AddComponent<SpriteRenderer>();
            // Try to load a crystal sprite, or create a simple diamond shape
            crystalRenderer.sprite = Resources.Load<Sprite>("Effects/IceCrystal");
            if (crystalRenderer.sprite == null)
            {
                // Create a simple diamond sprite
                Texture2D tex = new Texture2D(32, 32);
                for (int x = 0; x < 32; x++)
                {
                    for (int y = 0; y < 32; y++)
                    {
                        float distX = Mathf.Abs(x - 16);
                        float distY = Mathf.Abs(y - 16);
                        float dist = distX + distY;
                        float alpha = Mathf.Clamp01(1 - dist / 16f);
                        tex.SetPixel(x, y, new Color(0.8f, 0.9f, 1f, alpha));
                    }
                }
                tex.Apply();
                crystalRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            }
            
            // Calculate direction for this crystal
            float angle = i * (360f / 8);
            Vector3 direction = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0
            );
            
            // Set initial scale and color
            crystal.transform.localScale = Vector3.zero;
            crystalRenderer.color = new Color(0.8f, 0.9f, 1f, 0.8f);
            
            // Animate the crystal
            float randomDistance = Random.Range(1.5f, 2.5f);
            float randomDuration = Random.Range(0.8f, 1.2f);
            
            LeanTween.moveLocal(crystal, direction * randomDistance, randomDuration).setEaseOutQuad();
            LeanTween.scale(crystal, new Vector3(0.5f, 0.5f, 1f), randomDuration * 0.3f).setEaseOutQuad();
            LeanTween.scale(crystal, Vector3.zero, randomDuration * 0.7f).setEaseInQuad().setDelay(randomDuration * 0.3f);
            LeanTween.alpha(crystal, 0f, randomDuration * 0.5f).setDelay(randomDuration * 0.5f).setOnComplete(() => {
                Destroy(crystal);
            });
            
            // Add a slight rotation for more dynamic effect
            LeanTween.rotateZ(crystal, Random.Range(-180f, 180f), randomDuration).setEaseInOutQuad();
        }
        
        // Create a central frost burst effect
        GameObject frostBurst = new GameObject("FrostBurst");
        frostBurst.transform.position = transform.position;
        
        SpriteRenderer burstRenderer = frostBurst.AddComponent<SpriteRenderer>();
        burstRenderer.sprite = spriteRenderer.sprite;
        burstRenderer.color = new Color(0.7f, 0.9f, 1f, 0.7f);
        burstRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        
        LeanTween.scale(frostBurst, Vector3.one * 2f, 0.5f).setEaseOutCubic();
        LeanTween.alpha(frostBurst, 0f, 0.5f).setEaseOutCubic().setOnComplete(() => {
            Destroy(frostBurst);
        });
    }
    
    private void ShowFeedbackText(string message)
    {
        GameObject feedbackText = new GameObject("FreezeFeedback");
        feedbackText.transform.position = transform.position + new Vector3(0, 0, -0.1f);
        
        TMPro.TextMeshPro text = feedbackText.AddComponent<TMPro.TextMeshPro>();
        text.text = message;
        text.fontSize = 4f;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = new Color(0.7f, 0.9f, 1f); // Light blue text
        text.fontStyle = TMPro.FontStyles.Bold;
        
        // Animate the text
        LeanTween.moveY(feedbackText, transform.position.y + 1.5f, 1f).setEaseOutQuad();
        LeanTween.scale(feedbackText, Vector3.one * 1.5f, 0.3f).setEaseOutBack().setLoopPingPong(1);
        LeanTween.alpha(feedbackText, 0f, 0.8f).setEaseInQuad().setDelay(0.7f).setOnComplete(() => {
            Destroy(feedbackText);
        });
    }

    private void DestroyTile()
    {
        Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);
        
        Debug.Log($"FreezeTile: Destroying tile at {tilePosition}.");
        
        // Use our centralized destruction utility instead of manual destruction
        TileDestructionUtility.DestroyTile(gameObject, tilePosition);
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
