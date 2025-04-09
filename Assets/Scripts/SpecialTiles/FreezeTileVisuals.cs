using UnityEngine;
using TMPro;

/// <summary>
/// Handles the visual elements of the FreezeTile, creates a dynamic sprite with a snowflake icon.
/// </summary>
public class FreezeTileVisuals : MonoBehaviour
{
    [SerializeField] private Color tileColor = new Color(0.7f, 0.9f, 1f); // Light ice blue
    [SerializeField] private Color symbolColor = new Color(1f, 1f, 1f);   // White for the snowflake icon
    [SerializeField] private float cornerRadius = 0.15f;
    
    private SpriteRenderer spriteRenderer;
    private TextMeshPro symbolText;
    
    private void Awake()
    {
        // Set up sprite renderer with a rounded rectangle
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Create the tile sprite
        spriteRenderer.sprite = CreateRoundedRectSprite(1f, 1f, cornerRadius);
        spriteRenderer.color = tileColor;
        
        // Create the snowflake symbol
        CreateSymbolText();
    }
    
    private void CreateSymbolText()
    {
        // Create a child GameObject for the text
        GameObject textObj = new GameObject("FreezeSymbol");
        textObj.transform.SetParent(transform, false);
        textObj.transform.localPosition = new Vector3(0, 0, -0.05f); // Slightly in front
        
        // Add TextMeshPro component
        symbolText = textObj.AddComponent<TextMeshPro>();
        symbolText.text = "❄"; // Unicode snowflake character
        symbolText.fontSize = 5f;
        symbolText.alignment = TextAlignmentOptions.Center;
        symbolText.color = symbolColor;
        symbolText.fontStyle = FontStyles.Bold;
        
        // Make sure the text renders above the tile
        MeshRenderer textRenderer = symbolText.GetComponent<MeshRenderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingLayerName = "UI";
            textRenderer.sortingOrder = 1;
        }
        
        // Set the proper size
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(1.5f, 1.5f);
        }
    }
    
    private Sprite CreateRoundedRectSprite(float width, float height, float radius)
    {
        int textureSize = 128;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        Color transparentColor = new Color(1f, 1f, 1f, 0f);
        Color whiteColor = Color.white;
        
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                // Normalize coordinates to -0.5...0.5
                float nx = (x / (float)textureSize) - 0.5f;
                float ny = (y / (float)textureSize) - 0.5f;
                
                // Scale to match target width/height
                float scaledX = nx * width;
                float scaledY = ny * height;
                
                // Calculate distance from nearest edge
                float dx = Mathf.Max(Mathf.Abs(scaledX) - width/2f + radius, 0);
                float dy = Mathf.Max(Mathf.Abs(scaledY) - height/2f + radius, 0);
                
                // If in corner region, calculate distance from corner
                float distanceFromCorner = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (distanceFromCorner <= radius)
                    texture.SetPixel(x, y, whiteColor);
                else if (Mathf.Abs(scaledX) <= width/2f && Mathf.Abs(scaledY) <= height/2f)
                    texture.SetPixel(x, y, whiteColor);
                else
                    texture.SetPixel(x, y, transparentColor);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), 100);
    }
    
    /// <summary>
    /// Updates the visuals with specified colors.
    /// </summary>
    public void UpdateVisuals(Color tileColor, Color symbolColor)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = tileColor;
        }
        
        if (symbolText != null)
        {
            symbolText.color = symbolColor;
        }
    }
    
    /// <summary>
    /// Performs a highlight animation for when the freeze tile is activated.
    /// </summary>
    public void PlayActivationAnimation()
    {
        // Scale up and then back down
        transform.LeanScale(Vector3.one * 1.2f, 0.3f).setEaseOutBack()
            .setOnComplete(() => transform.LeanScale(Vector3.one, 0.2f).setEaseInOutQuad());
            
        // Flash the symbol text
        if (symbolText != null)
        {
            LeanTween.value(gameObject, 1f, 2f, 0.3f)
                .setOnUpdate((float val) => {
                    symbolText.fontSize = val * 5f;
                })
                .setLoopPingPong(2)
                .setEaseInOutSine();
        }
    }
}
