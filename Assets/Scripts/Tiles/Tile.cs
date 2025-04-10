using UnityEngine;
using TMPro;
using System.Collections;

public class Tile : MonoBehaviour
{
    public enum TileState
    {
        Idle,
        Selected,
        Moving,
        Merging
    }

    [Header("Visual Properties")]
    [SerializeField] private Material tileBaseMaterial;
    [SerializeField] private Material tileSelectedMaterial;
    [SerializeField] private float cornerRadius = 0.15f;
    [SerializeField] private float outlineWidth = 0.05f;
    [SerializeField] private Color outlineColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private float glowIntensity = 0.2f;
    [SerializeField] private float pulseSpeed = 1.5f;

    public TileState CurrentState { get; private set; } = TileState.Idle;

    public Color tileColor;
    public int number;

    private SpriteRenderer spriteRenderer;
    private TextMeshPro textMeshPro;
    private MaterialPropertyBlock propBlock;
    private Color originalOutlineColor;
    private Color textColor;

    public bool hasMerged { get; private set; } = false;

    // Add a getter method for better code readability
    public bool HasMerged()
    {
        return hasMerged;
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        textMeshPro = GetComponentInChildren<TextMeshPro>();
        propBlock = new MaterialPropertyBlock();

        if (textMeshPro == null)
        {
            Debug.LogWarning("Tile: No TextMeshPro component found. Creating one.");
            CreateTextMeshPro();
        }
        else
        {
            ConfigureTextMeshPro();
        }

        originalOutlineColor = outlineColor;

        // Set corner radius in material if possible
        if (spriteRenderer != null && spriteRenderer.sharedMaterial != null)
        {
            propBlock.SetFloat("_CornerRadius", cornerRadius);
            propBlock.SetFloat("_OutlineWidth", outlineWidth);
            propBlock.SetColor("_OutlineColor", outlineColor);
            propBlock.SetFloat("_GlowIntensity", 0f); // Start with no glow
            spriteRenderer.SetPropertyBlock(propBlock);
        }
    }

    public void Initialize(Color color, int value)
    {
        tileColor = color;
        number = value;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (textMeshPro == null)
            CreateTextMeshPro();

        // Calculate text color based on tile color brightness
        textColor = CalculateTextColor(tileColor);
        
        // Ensure the collider is enabled
        Collider2D tileCollider = GetComponent<Collider2D>();
        if (tileCollider != null && !tileCollider.enabled)
        {
            tileCollider.enabled = true;
        }

        UpdateVisuals();
        PlaySpawnAnimation();
    }

    public void UpdateVisuals()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (textMeshPro == null)
            textMeshPro = GetComponentInChildren<TextMeshPro>();

        // Scale brightness based on number value (higher numbers are brighter)
        float brightnessFactor = Mathf.Lerp(0.8f, 1.3f, Mathf.Log10(number + 1) / 3f);
        
        // Make tile colors more vibrant
        Color adjustedColor = new Color(
            Mathf.Clamp01(tileColor.r * brightnessFactor),
            Mathf.Clamp01(tileColor.g * brightnessFactor),
            Mathf.Clamp01(tileColor.b * brightnessFactor),
            tileColor.a
        );

        if (spriteRenderer != null)
        {
            propBlock.SetColor("_Color", adjustedColor);
            spriteRenderer.SetPropertyBlock(propBlock);
        }

        if (textMeshPro != null)
        {
            textMeshPro.text = number.ToString();
            textMeshPro.color = textColor;
            
            // Scale text size based on number digits
            int digitCount = Mathf.FloorToInt(Mathf.Log10(number) + 1);
            textMeshPro.fontSize = Mathf.Lerp(8, 4, Mathf.Clamp01((digitCount - 1) / 3.0f));
            
            textMeshPro.ForceMeshUpdate();
        }
    }

    private Color CalculateTextColor(Color backgroundColor)
    {
        // Calculate brightness using perceptual luminance formula
        float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;
        
        // Use white text on dark backgrounds, black text on light backgrounds
        return luminance > 0.5f ? Color.black : Color.white;
    }

    public void SetState(TileState newState)
    {
        if (CurrentState == newState) return;

        ExitState(CurrentState);
        CurrentState = newState;
        EnterState(CurrentState);
        
        // Ensure collider is enabled when changing state
        Collider2D tileCollider = GetComponent<Collider2D>();
        if (tileCollider != null && !tileCollider.enabled)
        {
            tileCollider.enabled = true;
        }
        
        // Add highlight around the tile when selected
        if (newState == TileState.Selected)
        {
            CreateSelectionHighlight();
        }
        else
        {
            RemoveSelectionHighlight();
        }
    }

    private void EnterState(TileState state)
    {
        switch (state)
        {
            case TileState.Idle:
                transform.localScale = Vector3.one;
                StopAllCoroutines();
                StartCoroutine(SubtleIdleAnimation());
                break;
                
            case TileState.Selected:
                StopAllCoroutines();
                LeanTween.scale(gameObject, Vector3.one * 1.1f, 0.2f).setEaseOutBack();
                StartCoroutine(PulseOutline());
                
                // Set glow intensity
                if (spriteRenderer != null)
                {
                    propBlock.SetFloat("_GlowIntensity", glowIntensity);
                    spriteRenderer.SetPropertyBlock(propBlock);
                }
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
        if (state == TileState.Selected || state == TileState.Moving || state == TileState.Merging)
        {
            LeanTween.cancel(gameObject);
            
            // Reset glow intensity
            if (spriteRenderer != null)
            {
                propBlock.SetFloat("_GlowIntensity", 0f);
                propBlock.SetColor("_OutlineColor", originalOutlineColor);
                spriteRenderer.SetPropertyBlock(propBlock);
            }
        }
        transform.localScale = Vector3.one;
    }

    public void ClearSelectionState()
    {
        // Reset the tile's state visually and logically
        LeanTween.cancel(gameObject);
        StopAllCoroutines();
        UpdateVisuals();
        transform.localScale = Vector3.one;
        CurrentState = TileState.Idle;
        
        // Reset material properties
        if (spriteRenderer != null)
        {
            propBlock.SetFloat("_GlowIntensity", 0f);
            propBlock.SetColor("_OutlineColor", originalOutlineColor);
            spriteRenderer.SetPropertyBlock(propBlock);
        }
        
        // Remove any selection highlight
        RemoveSelectionHighlight();
        
        // Ensure collider is enabled
        Collider2D tileCollider = GetComponent<Collider2D>();
        if (tileCollider != null && !tileCollider.enabled)
        {
            tileCollider.enabled = true;
        }
        
        StartCoroutine(SubtleIdleAnimation());
    }

    public void ResetState()
    {
        if (gameObject == null)
        {
            Debug.LogError("Tile: Attempted to reset state on a null GameObject.");
            return;
        }

        number = 0;
        tileColor = Color.white;
        UpdateVisuals();
        ClearSelectionState();
        hasMerged = false;

        Debug.Log($"Tile: Reset state for tile at position {transform.position}.");
    }

    public void MarkAsMerged()
    {
        hasMerged = true;
        Debug.Log($"Tile: Marked tile at {transform.position} as merged.");
        
        // Make sure the collider remains enabled for selection
        Collider2D tileCollider = GetComponent<Collider2D>();
        if (tileCollider != null && !tileCollider.enabled)
        {
            tileCollider.enabled = true;
        }
    }

    public void ResetMergeState()
    {
        hasMerged = false;
        
        // Ensure collider is enabled
        Collider2D tileCollider = GetComponent<Collider2D>();
        if (tileCollider != null && !tileCollider.enabled)
        {
            tileCollider.enabled = true;
        }
    }

    // Subtle continuous animation for idle tiles
    private IEnumerator SubtleIdleAnimation()
    {
        Vector3 originalScale = Vector3.one;
        float time = 0;
        
        while (true)
        {
            time += Time.deltaTime;
            float scale = 1 + Mathf.Sin(time * 0.5f) * 0.01f; // Very subtle 1% size oscillation
            transform.localScale = originalScale * scale;
            yield return null;
        }
    }

    // Pulse outline color for selected state
    private IEnumerator PulseOutline()
    {
        Color brightOutline = new Color(1f, 1f, 1f, 0.8f);
        float time = 0;
        
        while (true)
        {
            time += Time.deltaTime * pulseSpeed;
            float t = (Mathf.Sin(time * 6) + 1) * 0.5f; // Oscillate between 0 and 1
            Color pulseColor = Color.Lerp(originalOutlineColor, brightOutline, t);
            
            if (spriteRenderer != null)
            {
                propBlock.SetColor("_OutlineColor", pulseColor);
                spriteRenderer.SetPropertyBlock(propBlock);
            }
            
            yield return null;
        }
    }

    private void CreateTextMeshPro()
    {
        GameObject textObj = new GameObject("NumberText");
        textObj.transform.SetParent(transform, false);
        textObj.transform.localPosition = new Vector3(0, 0, -0.1f);

        textMeshPro = textObj.AddComponent<TextMeshPro>();
        ConfigureTextMeshPro();
    }

    private void ConfigureTextMeshPro()
    {
        if (textMeshPro == null) return;

        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.enableAutoSizing = true;
        textMeshPro.fontSizeMin = 1;
        textMeshPro.fontSizeMax = 10;
        textMeshPro.color = Color.black;

        // Ensure the TextMeshPro is rendered above the tile
        MeshRenderer textRenderer = textMeshPro.GetComponent<MeshRenderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingLayerName = "UI"; // Ensure this matches your sorting layer
            textRenderer.sortingOrder = 10; // Set a high order to render above tiles
        }
    }

    private void PlaySpawnAnimation()
    {
        transform.localScale = Vector3.zero;
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));

        LeanTween.scale(gameObject, Vector3.one, 0.35f).setEaseOutBack().setOvershoot(1.3f);
        LeanTween.rotateZ(gameObject, 0f, 0.4f).setEaseOutElastic().setDelay(0.15f);
    }
    
    private GameObject selectionHighlight;
    
    private void CreateSelectionHighlight()
    {
        // Remove any existing highlight first
        RemoveSelectionHighlight();
        
        // Create a new highlight object
        selectionHighlight = new GameObject("SelectionHighlight");
        selectionHighlight.transform.SetParent(transform);
        selectionHighlight.transform.localPosition = Vector3.zero;
        selectionHighlight.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        
        // Add our identifier component
        selectionHighlight.AddComponent<SelectionHighlightIdentifier>();
        
        // Add a sprite renderer component
        SpriteRenderer highlightRenderer = selectionHighlight.AddComponent<SpriteRenderer>();
        highlightRenderer.sprite = spriteRenderer.sprite;
        highlightRenderer.color = new Color(1f, 1f, 0.5f, 0.5f); // Yellow-ish highlight
        highlightRenderer.sortingOrder = spriteRenderer.sortingOrder - 1; // Behind the tile
        
        // Animate the highlight
        LeanTween.scale(selectionHighlight, new Vector3(1.3f, 1.3f, 1f), 0.5f).setLoopPingPong().setEaseInOutSine();
        LeanTween.rotateZ(selectionHighlight, 5f, 1.2f).setLoopPingPong().setEaseInOutSine();
    }
    
    private void RemoveSelectionHighlight()
    {
        if (selectionHighlight != null)
        {
            LeanTween.cancel(selectionHighlight);
            Destroy(selectionHighlight);
            selectionHighlight = null;
        }
    }

    private void OnMouseDown()
    {
        // Only process input when in the correct game state
        if (GameStateManager.Instance == null || !GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            Debug.Log($"Tile: Ignored OnMouseDown - not in WaitingForInputState. Current state: {GameStateManager.Instance?.GetCurrentStateName() ?? "Unknown"}");
            return;
        }

        // Convert world position to grid position
        Vector2Int gridPosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);
        
        // Instead of directly invoking the event, send the selection to the BoardManager
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.HandleTileSelection(gridPosition);
        }
    }
}
