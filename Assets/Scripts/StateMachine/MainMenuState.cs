using UnityEngine;

/// <summary>
/// Main menu state - displays the main menu and waits for player input to start the game.
/// </summary>
public class MainMenuState : GameState
{
    public override void Enter()
    {
        Debug.Log("MainMenuState: Entering MainMenuState...");
        UIManager.Instance.ShowMainMenu();
    }

    public override void Update() { }

    public override void HandleInput(Vector2Int gridPosition)
    {
        GameStateManager.Instance.SetState(new LoadingLevelState());
    }

    public override void Exit()
    {
        Debug.Log("MainMenuState: Exiting MainMenuState...");
        UIManager.Instance.HideMainMenu();
    }
}
