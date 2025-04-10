using UnityEngine;

/// <summary>
/// Pause state - pauses the game and displays the pause menu.
/// </summary>
public class PauseState : GameState
{
    public override void Enter()
    {
        Debug.Log("PauseState: Entering PauseState. Game is now paused.");
        Time.timeScale = 0f; // Freeze the game
    }

    public override void Update()
    {
        Debug.Log("PauseState: Game is paused. No updates are processed.");
    }

    public override void Exit()
    {
        Debug.Log("PauseState: Exiting PauseState. Game is resuming.");
        Time.timeScale = 1f; // Resume the game
    }
}
