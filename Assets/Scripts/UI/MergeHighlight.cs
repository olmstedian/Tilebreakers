using UnityEngine;

public class MergeHighlight : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float pulseFrequency = 1.5f;
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;
    [SerializeField] private Color startColor = new Color(1f, 0.7f, 0.2f, 0.7f);
    [SerializeField] private Color endColor = new Color(1f, 0.5f, 0f, 0.9f);
    
    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Set up appearance
        spriteRenderer.color = startColor;
        
        // Start animations
        StartAnimations();
    }
    
    private void StartAnimations()
    {
        // Rotation animation
        LeanTween.rotateZ(gameObject, 360f, rotationSpeed).setLoopClamp();
        
        // Scale pulsing
        LeanTween.scale(gameObject, Vector3.one * maxScale, pulseFrequency / 2f)
            .setEaseInOutSine()
            .setLoopPingPong();
            
        // Color pulsing
        LeanTween.color(gameObject, endColor, pulseFrequency / 2f)
            .setEaseInOutSine()
            .setLoopPingPong();
    }
}
