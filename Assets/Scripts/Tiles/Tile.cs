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
    private TextMeshPro textMeshPro;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        textMeshPro = GetComponentInChildren<TextMeshPro>();

        if (textMeshPro == null)
        {
            Debug.LogWarning("Tile: No TextMeshPro component found. Creating one.");
            CreateTextMeshPro();
        }
        else
        {
            ConfigureTextMeshPro();
        }
    }

    public void Initialize(Color color, int value)
    {
        tileColor = color;
        number = value;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.color = tileColor;

        if (textMeshPro == null)
            CreateTextMeshPro();

        if (textMeshPro != null)
        {
            textMeshPro.text = number.ToString();
            textMeshPro.ForceMeshUpdate();
            textMeshPro.gameObject.SetActive(true);
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

        float brightnessFactor = Mathf.Lerp(0.8f, 1.2f, Mathf.Log10(number + 1) / 3f);
        Color adjustedColor = new Color(
            Mathf.Clamp01(tileColor.r * brightnessFactor),
            Mathf.Clamp01(tileColor.g * brightnessFactor),
            Mathf.Clamp01(tileColor.b * brightnessFactor),
            tileColor.a
        );

        if (spriteRenderer != null)
            spriteRenderer.color = adjustedColor;

        if (textMeshPro != null)
        {
            textMeshPro.text = number.ToString();
            textMeshPro.color = adjustedColor.grayscale > 0.5f ? Color.black : Color.white;
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
        if (state == TileState.Selected || state == TileState.Moving || state == TileState.Merging)
        {
            LeanTween.cancel(gameObject);
            transform.localScale = Vector3.one;
        }
    }

    public void ClearSelectionState()
    {
        // Reset the tile's state visually and logically
        LeanTween.cancel(gameObject);
        UpdateVisuals();
        transform.localScale = Vector3.one;
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
}
