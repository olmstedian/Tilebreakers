using UnityEngine;

/// <summary>
/// Simple component used to identify and manage selection highlight objects.
/// </summary>
public class SelectionHighlightIdentifier : MonoBehaviour
{
    private void Start()
    {
        // Verify that the highlight should exist in the current state
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            Debug.LogWarning($"SelectionHighlightIdentifier: Highlight created outside of WaitingForInputState. Destroying.");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Continuously check if we're still in the correct state
        // If the game state changes, destroy this highlight
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            Debug.Log($"SelectionHighlightIdentifier: Game state changed from WaitingForInputState. Removing highlight.");
            Destroy(gameObject);
        }
    }

    public void RemoveHighlight()
    {
        // Animate out the highlight before destroying it
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.zero, 0.2f).setEaseInBack().setOnComplete(() => {
            Destroy(gameObject);
        });
    }
}
