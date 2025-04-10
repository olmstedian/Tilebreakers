using UnityEngine;

/// <summary>
/// Checking game over state - checks if the game is over or transitions to SpecialTileAction.
/// </summary>
public class CheckingGameOverState : GameState
{
    public override void Enter()
    {
        Debug.Log("CheckingGameOverState: Checking game over...");

        if (GameOverManager.Instance == null)
        {
            Debug.LogError("CheckingGameOverState: GameOverManager.Instance is null. Ensure it is properly instantiated.");
            GameStateManager.Instance.SetState(new WaitingForInputState());
            return;
        }

        GameOverManager.Instance.CheckGameOver();
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("CheckingGameOverState: Game over check complete.");
    }
}
