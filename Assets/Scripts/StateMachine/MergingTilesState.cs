using UnityEngine;
using System.Collections;

/// <summary>
/// Merging tiles state - handles tile merging logic.
/// </summary>
public class MergingTilesState : GameState
{
    public override void Enter()
    {
        Debug.Log("MergingTilesState: Merging tiles...");

        // Trigger merging logic
        BoardManager.Instance.StartCoroutine(HandleTileMerges(() =>
        {
            // Transition to SplittingTilesState after merges are complete
            GameStateManager.Instance.SetState(new SplittingTilesState());
        }));
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("MergingTilesState: Exiting state.");
    }

    /// <summary>
    /// Handles all tile merges and invokes a callback when complete.
    /// </summary>
    private IEnumerator HandleTileMerges(System.Action onComplete)
    {
        // Simulate merge animations (replace with actual logic if needed)
        yield return new WaitForSeconds(Constants.TILE_MOVE_DURATION);

        // Invoke the callback after merges are complete
        onComplete?.Invoke();
    }
}
