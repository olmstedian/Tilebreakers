using UnityEngine;
using Tilebreakers.Core;

/// <summary>
/// Game complete state - the player has finished all levels.
/// </summary>
public class GameCompleteState : GameState
{
    public override void Enter()
    {
        Debug.Log("GameCompleteState: Game complete! All levels finished.");
        UIManager.Instance.ShowGameCompleteScreen(ScoreManager.Instance.GetCurrentScore());
        
        // Play game complete fanfare
        AudioManager.Instance?.PlayGameCompleteSound();
    }

    public override void Update() { }

    public override void HandleInput(Vector2Int gridPosition)
    {
        // Do nothing - let the UI buttons handle actions
    }

    public override void Exit()
    {
        Debug.Log("GameCompleteState: Exiting game complete state.");
        UIManager.Instance.HideGameCompleteScreen();
    }
}
