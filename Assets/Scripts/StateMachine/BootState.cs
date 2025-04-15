using UnityEngine;
using Tilebreakers.Core;

/// <summary>
/// Boot state - handles app initialization and splash screen display.
/// </summary>
public class BootState : GameState
{
    public override void Enter()
    {
        Debug.Log("BootState: Initializing game...");

        // Ensure the board is cleared and no tiles are spawned
        BoardManager.Instance?.ClearBoard();

        // Initialize other game systems if necessary
        GameManager.Instance.ResetMoves();
        ScoreManager.Instance?.ResetScore();

        // Transition to the InitGameState after booting
        GameStateManager.Instance.SetState(new InitGameState());
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("BootState: Exiting BootState...");
        UIManager.Instance.HideSplashScreen();
    }
}
