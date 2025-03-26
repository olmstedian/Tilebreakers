using UnityEngine;
using System.Collections;

public class TileAnimator : MonoBehaviour
{
    public void PlayMoveAnimation(Vector2 targetPosition, float duration)
    {
        StartCoroutine(MoveTileAnimation(targetPosition, duration));
    }

    public IEnumerator MoveTileAnimation(Vector2 targetPosition, float duration)
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

    public void PlayMergeAnimation()
    {
        LeanTween.scale(gameObject, Vector3.one * 1.2f, 0.2f).setEaseOutBack().setOnComplete(() =>
        {
            LeanTween.scale(gameObject, Vector3.one, 0.2f).setEaseInBack();
        });
    }
}
