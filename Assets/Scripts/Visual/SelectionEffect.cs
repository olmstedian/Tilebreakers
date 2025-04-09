using UnityEngine;

/// <summary>
/// Handles visual effects for tile selection. Attach to a GameObject that will be used as a selection indicator.
/// </summary>
public class SelectionEffect : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float pulseMagnitude = 0.1f;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private Color highlightColor = new Color(1f, 0.8f, 0.2f, 0.6f);
    
    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.color = highlightColor;
            spriteRenderer.sortingOrder = -1; // Behind the tile
        }
    }
    
    private void Start()
    {
        // Start animations
        StartPulseAnimation();
        StartRotationAnimation();
    }
    
    private void StartPulseAnimation()
    {
        LeanTween.scale(gameObject, originalScale * (1 + pulseMagnitude), pulseSpeed)
            .setEaseInOutSine()
            .setLoopPingPong();
    }
    
    private void StartRotationAnimation()
    {
        LeanTween.rotateZ(gameObject, 360f, rotationSpeed)
            .setEaseLinear()
            .setLoopClamp();
    }
    
    /// <summary>
    /// Updates the base sprite used for the selection effect.
    /// </summary>
    public void UpdateSprite(Sprite tileSprite)
    {
        if (spriteRenderer != null && tileSprite != null)
        {
            spriteRenderer.sprite = tileSprite;
        }
    }
    
    /// <summary>
    /// Updates the color of the selection effect.
    /// </summary>
    public void UpdateColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(color.r, color.g, color.b, highlightColor.a);
        }
    }
    
    /// <summary>
    /// Called when the GameObject is destroyed to cancel any active tweens.
    /// </summary>
    private void OnDestroy()
    {
        LeanTween.cancel(gameObject);
    }
}
