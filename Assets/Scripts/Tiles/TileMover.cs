using UnityEngine;
using System.Collections;

public class TileMover : MonoBehaviour
{
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool useUnscaledTime = false;
    private int originalSortingOrder = 0; // Store the original sorting order

    private void Awake()
    {
        // Store the original sorting order when component initializes
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalSortingOrder = sr.sortingOrder;
        }
    }

    public IEnumerator MoveTile(Vector2 targetPosition, float duration)
    {
        // Store original sorting order at the beginning of the move
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalSortingOrder = sr.sortingOrder;
        }

        Vector2 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Use either regular or unscaled time based on configuration
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += deltaTime;

            // Calculate progress with curve for smoother movement
            float t = elapsed / duration;
            float curvedT = movementCurve.Evaluate(t);
            
            // Update position
            transform.position = Vector2.Lerp(startPosition, targetPosition, curvedT);
            
            // Set state to Moving if it's a Tile
            Tile tile = GetComponent<Tile>();
            if (tile != null)
            {
                tile.SetState(Tile.TileState.Moving);
            }
            
            yield return null;
        }
        
        // Ensure we end exactly at the target position
        transform.position = targetPosition;
        
        // Safety check - ensure sorting order is restored
        if (sr != null && sr.sortingOrder < 0)
        {
            sr.sortingOrder = originalSortingOrder;
        }
    }

    public static Vector2 CalculateTargetPosition(Vector2 startPosition, Vector2 direction, float cellSize, int maxSteps)
    {
        // Normalize direction to ensure consistent movement
        Vector2 normalizedDirection = direction.normalized;
        
        // Calculate the target position based on the direction and max steps
        Vector2 targetPosition = startPosition + normalizedDirection * cellSize * maxSteps;
        
        return targetPosition;
    }
}
