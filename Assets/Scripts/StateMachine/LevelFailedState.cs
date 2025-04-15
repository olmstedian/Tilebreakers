using UnityEngine;
using Tilebreakers.Core;

/// <summary>
/// Level failed state - shows level failed UI and allows restarting or returning to menu.
/// </summary>
public class LevelFailedState : GameState
{
    public override void Enter()
    {
        Debug.Log("LevelFailedState: Level failed!");
        UIManager.Instance.ShowLevelFailedScreen(ScoreManager.Instance.GetCurrentScore());
        
        // Play level failed sound
        AudioManager.Instance?.PlayLevelFailedSound();
    }

    public override void Update() { }

    public override void HandleInput(Vector2Int gridPosition)
    {
        // Do nothing - let the UI buttons handle actions
    }

    public override void Exit()
    {
        Debug.Log("LevelFailedState: Exiting level failed state.");
        UIManager.Instance.HideLevelFailedScreen();
    }
}
