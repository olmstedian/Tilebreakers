using UnityEngine;
using System.Collections;

public class TileMover : MonoBehaviour
{
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

    public static Vector2 CalculateTargetPosition(Vector2 startPosition, Vector2 direction, float cellSize, int maxSteps)
    {
        Vector2 targetPosition = startPosition;

        for (int step = 1; step <= maxSteps; step++)
        {
            Vector2 nextPosition = startPosition + direction * cellSize * step;
            if (Physics2D.OverlapCircle(nextPosition, cellSize * 0.4f) != null)
            {
                break;
            }
            targetPosition = nextPosition;
        }

        return targetPosition;
    }
}
