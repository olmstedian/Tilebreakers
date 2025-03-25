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
        // Apply color with slight adjustments based on number value
        Color adjustedColor = tileColor;
        
        // Slightly brighten colors for higher numbers to create visual hierarchy
        float brightnessFactor = Mathf.Min(1.0f, 0.8f + (number * 0.05f));
        adjustedColor = new Color(
            Mathf.Clamp01(adjustedColor.r * brightnessFactor),
            Mathf.Clamp01(adjustedColor.g * brightnessFactor),
            Mathf.Clamp01(adjustedColor.b * brightnessFactor),
            adjustedColor.a
        );
        
        spriteRenderer.color = adjustedColor;

        if (textMeshPro != null)
        {
            textMeshPro.text = number.ToString();
            
            // Adjust text size based on number of digits
            float baseSize = 8f;
            textMeshPro.fontSize = (number >= 10) ? baseSize * 0.8f : baseSize;
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
        // Animate the tile spawning in
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 0.3f).setEaseOutBack();
    }

    public void PlayMergeAnimation()
    {
        // Play a pulse animation when tiles merge
        LeanTween.scale(gameObject, new Vector3(1.2f, 1.2f, 1.2f), 0.1f)
            .setEasePunch()
            .setOnComplete(() => {
                LeanTween.scale(gameObject, Vector3.one, 0.1f);
            });
    }

    public void MoveTo(Vector2 targetPosition, float duration)
    {
        // Smoothly move the tile to the target position
        StartCoroutine(MoveAnimation(targetPosition, duration));
    }

    private System.Collections.IEnumerator MoveAnimation(Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }
}
