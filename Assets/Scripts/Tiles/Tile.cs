using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour
{
    public enum TileState
    {
        Idle,
        Selected,
        Moving,
        Merging
    }

    public TileState CurrentState { get; private set; } = TileState.Idle;

    public Color tileColor;
    public int number;

    private SpriteRenderer spriteRenderer;
    private TextMeshPro textMeshPro; // Use TextMeshPro for number display

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Try to find TextMeshPro or TextMeshProUGUI component
        textMeshPro = GetComponentInChildren<TextMeshPro>();
        
        if (textMeshPro == null)
        {
            // Try to find TextMeshProUGUI (2D version) if the 3D version isn't found
            var ugui = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (ugui != null) 
            {
                // We need to create a TextMeshPro for consistency
                CreateTextMeshPro();
            }
            else
            {
                Debug.LogError("No TextMeshPro or TextMeshProUGUI component found on the Tile prefab.");
                CreateTextMeshPro();
            }
        }
        else
        {
            ConfigureTextMeshPro();
        }
    }

    public void Initialize(Color color, int value)
    {
        // Store the values
        tileColor = color;
        number = value;
        
        // Ensure we have a sprite renderer
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Apply the color
        if (spriteRenderer != null)
            spriteRenderer.color = tileColor;
        
        // Get or create TextMeshPro component
        if (textMeshPro == null)
        {
            // Look for existing TextMeshPro in children
            textMeshPro = GetComponentInChildren<TextMeshPro>();
            
            // If still null, create a new one
            if (textMeshPro == null)
            {
                CreateTextMeshPro();
            }
        }
        
        if (textMeshPro != null)
        {
            // Ensure the text matches the number
            textMeshPro.text = number.ToString();
            textMeshPro.ForceMeshUpdate();
            
            // Explicitly make the TextMeshPro active
            textMeshPro.gameObject.SetActive(true);
        }
        
        // Update visuals with our number
        UpdateVisuals();
        
        // Play the spawn animation
        PlaySpawnAnimation();
    }

    public void UpdateVisuals()
    {
        // First ensure we have required components
        if (spriteRenderer == null) 
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (textMeshPro == null)
            textMeshPro = GetComponentInChildren<TextMeshPro>();
        
        // If we still don't have TextMeshPro, create it
        if (textMeshPro == null)
            CreateTextMeshPro();
        
        // Calculate adjusted color based on tile number
        float brightnessFactor = Mathf.Lerp(0.8f, 1.2f, Mathf.Log10(number + 1) / 3f);
        Color adjustedColor = new Color(
            Mathf.Clamp01(tileColor.r * brightnessFactor),
            Mathf.Clamp01(tileColor.g * brightnessFactor),
            Mathf.Clamp01(tileColor.b * brightnessFactor),
            tileColor.a
        );
        
        // Apply the adjusted color to the sprite
        if (spriteRenderer != null)
            spriteRenderer.color = adjustedColor;
        
        // Update the text display
        if (textMeshPro != null)
        {
            // Make sure the GameObject is active
            textMeshPro.gameObject.SetActive(true);
            
            // CRITICAL: Ensure the text reflects the current number value
            textMeshPro.text = number.ToString();
            
            // Adjust text size based on digit count
            int digitCount = Mathf.FloorToInt(Mathf.Log10(Mathf.Max(1, number))) + 1;
            float baseSize = 8f; 
            textMeshPro.fontSize = baseSize * Mathf.Pow(0.85f, digitCount - 1);
            
            // Set color based on background brightness for better contrast
            float luminance = 0.299f * adjustedColor.r + 0.587f * adjustedColor.g + 0.114f * adjustedColor.b;
            textMeshPro.color = luminance > 0.5f ? Color.black : Color.white;
            
            // Force mesh update to ensure text renders properly
            textMeshPro.ForceMeshUpdate();            
        }
    }

    public void SetState(TileState newState)
    {
        if (CurrentState == newState) return;

        ExitState(CurrentState);
        CurrentState = newState;
        EnterState(CurrentState);
    }

    private void EnterState(TileState state)
    {
        switch (state)
        {
            case TileState.Idle:
                transform.localScale = Vector3.one;
                break;

            case TileState.Selected:
                LeanTween.scale(gameObject, Vector3.one * 1.1f, 0.2f).setEaseOutBack();
                break;

            case TileState.Moving:
                LeanTween.scale(gameObject, Vector3.one, 0.2f).setEaseInBack();
                break;

            case TileState.Merging:
                LeanTween.scale(gameObject, Vector3.one * 1.2f, 0.2f).setEaseOutBack().setOnComplete(() =>
                {
                    LeanTween.scale(gameObject, Vector3.one, 0.2f).setEaseInBack();
                });
                break;
        }
    }

    private void ExitState(TileState state)
    {
        switch (state)
        {
            case TileState.Selected:
                LeanTween.cancel(gameObject);
                transform.localScale = Vector3.one;
                break;

            case TileState.Moving:
            case TileState.Merging:
                LeanTween.cancel(gameObject);
                break;
        }
    }

    public void MergeWith(Tile movingTile)
    {
        // Before merging, ensure no tiles remain selected
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ClearAllSelections();
        }
        
        // Also explicitly clear our own visual selection state
        ClearSelectionState();
        
        TileMerger.MergeTiles(this, movingTile);
    }

    // Add this method to help with selection state management
    public void ClearSelectionState()
    {
        LeanTween.cancel(gameObject);
        UpdateVisuals();
        transform.localScale = Vector3.one;
    }

    private void ConfigureTextMeshPro()
    {
        if (textMeshPro == null) return;
        
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
        
        // Enable mesh rendering (if somehow disabled)
        MeshRenderer meshRenderer = textMeshPro.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.enabled = true;
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

    // Creates a new TextMeshPro component if it doesn't exist
    private void CreateTextMeshPro()
    {
        // Create a new GameObject for the text
        GameObject textObj = new GameObject("NumberText");
        textObj.transform.SetParent(transform, false);
        textObj.transform.localPosition = new Vector3(0, 0, -0.1f);
        
        // Add TextMeshPro component
        textMeshPro = textObj.AddComponent<TextMeshPro>();
        
        // Basic configuration
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.fontSize = 8f;
        textMeshPro.enableAutoSizing = true;
        textMeshPro.fontSizeMin = 1;
        textMeshPro.fontSizeMax = 10;
        textMeshPro.color = Color.black;
        
        // Critical: set the text to show the number
        textMeshPro.text = number.ToString();
        
        // Add necessary components
        if (textObj.GetComponent<MeshRenderer>() == null)
            textObj.AddComponent<MeshRenderer>();
        
        // Try to find a default font asset (important for text rendering)
        TMP_FontAsset defaultFont = null;
        
        // First look for an asset in Resources folder
        defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        
        // If not found, try to find from existing TextMeshPro components
        if (defaultFont == null)
        {
            TextMeshPro[] existingTexts = FindObjectsOfType<TextMeshPro>();
            foreach (TextMeshPro tmp in existingTexts)
            {
                if (tmp != textMeshPro && tmp.font != null)
                {
                    defaultFont = tmp.font;
                    break;
                }
            }
        }
        
        // Apply the font if found
        if (defaultFont != null)
        {
            textMeshPro.font = defaultFont;
        }
        
        // Force an update to ensure text is visible
        textMeshPro.ForceMeshUpdate();
    }
}
