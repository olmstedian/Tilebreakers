using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour
{
    public Color tileColor;
    public int number;

    private SpriteRenderer spriteRenderer;
    private TextMeshPro textMeshPro; // Updated to use TextMeshPro

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        textMeshPro = GetComponentInChildren<TextMeshPro>(); // Updated to find TextMeshPro
    }

    public void Initialize(Color color, int value)
    {
        tileColor = color;
        number = value;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        spriteRenderer.color = tileColor;
        textMeshPro.text = number.ToString(); // Updated to set TMP text
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
