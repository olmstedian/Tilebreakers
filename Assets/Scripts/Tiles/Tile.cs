using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour
{
    public Color tileColor;
    public int number;

    private SpriteRenderer spriteRenderer;
    private TextMeshPro textMeshPro; // Use TextMeshPro for number display


    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        textMeshPro = GetComponentInChildren<TextMeshPro>(); // Find the TextMeshPro component

        if (textMeshPro == null)
        {
            Debug.LogError("TextMeshPro component is missing from the Tile prefab.");
        }
        else
        {
            ConfigureTextMeshPro(); // Ensure TMP is properly configured
        }
    }

    public void Initialize(Color color, int value)
    {
        tileColor = color;
        number = value;
        UpdateVisuals();
        PlaySpawnAnimation();
    }

    private void UpdateVisuals()
    {
        // Apply color with systematic adjustments based on number value
        float brightnessFactor = Mathf.Lerp(0.8f, 1.2f, Mathf.Log10(number + 1) / 3f);
        Color adjustedColor = new Color(
            Mathf.Clamp01(tileColor.r * brightnessFactor),
            Mathf.Clamp01(tileColor.g * brightnessFactor),
            Mathf.Clamp01(tileColor.b * brightnessFactor),
            tileColor.a
        );
        
        spriteRenderer.color = adjustedColor;

        if (textMeshPro != null)
        {
            textMeshPro.text = number.ToString();
            
            // Dynamically adjust text size based on number of digits
            int digitCount = Mathf.FloorToInt(Mathf.Log10(Mathf.Max(1, number))) + 1;
            float baseSize = 8f;
            textMeshPro.fontSize = baseSize * Mathf.Pow(0.85f, digitCount - 1);
            
            // Adjust contrast based on tile brightness
            float luminance = 0.299f * adjustedColor.r + 0.587f * adjustedColor.g + 0.114f * adjustedColor.b;
            textMeshPro.color = luminance > 0.5f ? Color.black : Color.white;
        }
    }

    private void ConfigureTextMeshPro()
    {
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.enableAutoSizing = true;
        textMeshPro.fontSizeMin = 1;
        textMeshPro.fontSizeMax = 10;
        textMeshPro.color = Color.black;

        // Add outline for better readability instead of shadows
        textMeshPro.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f); // Outline width
        textMeshPro.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0, 0, 0, 0.5f)); // Outline color

        textMeshPro.transform.localPosition = new Vector3(0, 0, -0.2f);
        textMeshPro.sortingOrder = 1;
    }

    private void PlaySpawnAnimation()
    {
        // Start with zero scale and slight rotation
        transform.localScale = Vector3.zero;
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
        
        // Sequence: scale up with bounce effect, then subtle rotation correction
        LeanTween.scale(gameObject, Vector3.one, 0.35f).setEaseOutBack().setOvershoot(1.3f);
        LeanTween.rotateZ(gameObject, 0f, 0.4f).setEaseOutElastic().setDelay(0.15f);
        
        // Add a subtle fade-in effect
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color initialColor = sprite.color;
            sprite.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0.6f);
            LeanTween.color(gameObject, initialColor, 0.3f);
        }
    }
}
