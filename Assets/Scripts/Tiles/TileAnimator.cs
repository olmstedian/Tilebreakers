using UnityEngine;
using System.Collections;

public class TileAnimator : MonoBehaviour
{
    // Smoothly moves the tile to the given target position over the specified duration.
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

    // Plays an enhanced move animation with optional easing, sound, and a completion callback.
    public void PlayMoveAnimation(Vector2 targetPosition, float duration, AnimationCurve easingCurve = null, bool playSound = false, System.Action onComplete = null)
    {
        if (easingCurve == null)
        {
            easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
        StartCoroutine(MoveWithEffects(targetPosition, duration, easingCurve, playSound, onComplete));
    }
    
    private IEnumerator MoveWithEffects(Vector2 targetPosition, float duration, AnimationCurve easingCurve, bool playSound, System.Action onComplete)
    {
        if (playSound)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }
        }
        
        Vector2 startPosition = transform.position;
        float elapsed = 0f;
        Vector2 velocity = Vector2.zero;
        float smoothTime = duration * 0.3f;
        float overShoot = 0.1f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = easingCurve.Evaluate(t);
            Vector2 targetPos;
            if (t < 0.7f)
            {
                // Accelerate toward target with slight overshoot.
                targetPos = Vector2.Lerp(startPosition, targetPosition + (targetPosition - startPosition).normalized * overShoot, easedT);
            }
            else
            {
                // Settle into final position.
                targetPos = Vector2.Lerp(targetPosition + (targetPosition - startPosition).normalized * overShoot, targetPosition, (t - 0.7f) / 0.3f);
            }
            
            transform.position = Vector2.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime, Mathf.Infinity, Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
        onComplete?.Invoke();
    }
}
