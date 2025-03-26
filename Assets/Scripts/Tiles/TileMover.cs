using UnityEngine;
using System.Collections;

public class TileMover : MonoBehaviour
{
    // Smoothly moves the tile to the given target position over the specified duration.
    public IEnumerator MoveTile(Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }

    // Calculate the target position based on start position, direction, cell size, and maximum steps.
    public static Vector2 CalculateTargetPosition(Vector2 startPosition, Vector2 direction, float cellSize, int maxSteps)
    {
        Vector2 targetPosition = startPosition;
        for (int step = 1; step <= maxSteps; step++)
        {
            // Calculate next potential position.
            Vector2 nextPosition = startPosition + direction * cellSize * step;
            // Check for collisions at the next position.
            Collider2D hit = Physics2D.OverlapCircle(nextPosition, cellSize * 0.4f);
            if (hit != null && hit.gameObject != null)
            {
                // Stop at the current position before collision.
                return targetPosition;
            }
            targetPosition = nextPosition;
        }
        return targetPosition;
    }
}
