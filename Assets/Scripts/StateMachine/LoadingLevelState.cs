using UnityEngine;

/// <summary>
/// Loading level state - prepares the game level.
/// </summary>
public class LoadingLevelState : GameState
{
    public override void Enter()
    {
        Debug.Log("LoadingLevelState: Preparing data and assets...");
        // Simulate loading process (e.g., load assets, initialize data)
        GameStateManager.Instance.SetStateWithDelay(new InitGameState(), 1.5f); // Delay for demonstration
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("LoadingLevelState: Data and assets prepared.");
    }
}
