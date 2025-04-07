using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class BlasterTileVisuals : MonoBehaviour
{
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float minScale = 1.10f;  // Larger minimum scale
    [SerializeField] private float maxScale = 1.20f;  // Larger maximum scale
    
    [SerializeField] private Color baseColor = new Color(1f, 0.3f, 0.2f);
    [SerializeField] private Color pulseColor = new Color(1f, 0.6f, 0.2f);
    
    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Start()
    {
        // Set the sprite renderer's material to a brighter shade to make it stand out
        if (spriteRenderer != null)
        {
            spriteRenderer.material = new Material(spriteRenderer.material);
            spriteRenderer.material.SetFloat("_Brightness", 1.2f);
        }
        
        StartCoroutine(PulseAnimation());
        StartCoroutine(ColorPulse());
    }
    
    private IEnumerator PulseAnimation()
    {
        Vector3 originalScale = transform.localScale;
        
        while (true)
        {
            float t = Mathf.PingPong(Time.time * pulseSpeed, 1);
            float scale = Mathf.Lerp(originalScale.x * minScale, originalScale.x * maxScale, t);
            transform.localScale = new Vector3(scale, scale, originalScale.z);
            yield return null;
        }
    }
    
    private IEnumerator ColorPulse()
    {
        while (true)
        {
            float t = Mathf.PingPong(Time.time * pulseSpeed * 0.7f, 1);
            spriteRenderer.color = Color.Lerp(baseColor, pulseColor, t);
            yield return null;
        }
    }
}
