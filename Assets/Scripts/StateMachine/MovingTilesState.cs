using UnityEngine;
using System.Collections;

/// <summary>
/// Moving tiles state - handles tile movement animations.
/// </summary>
public class MovingTilesState : GameState
{
    public override void Enter()
    {
        Debug.Log("MovingTilesState: Moving tiles...");

        // Trigger tile movement animations
        BoardManager.Instance.StartCoroutine(AnimateTileMovements(() =>
        {
            // Transition to the next state after animations are complete
            GameStateManager.Instance.SetState(new MergingTilesState());
        }));
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("MovingTilesState: Exiting state.");
    }

    /// <summary>
    /// Animates all tile movements and invokes a callback when complete.
    /// </summary>
    private IEnumerator AnimateTileMovements(System.Action onComplete)
    {
        // Simulate tile movement animations (replace with actual logic if needed)
        yield return new WaitForSeconds(Constants.TILE_MOVE_DURATION);

        // Invoke the callback after animations are complete
        onComplete?.Invoke();
    }
}
