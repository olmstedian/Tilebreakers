using UnityEngine;

/// <summary>
/// Boot state - handles app initialization and splash screen display.
/// </summary>
public class BootState : GameState
{
    public override void Enter()
    {
        Debug.Log("BootState: Entering BootState...");
        UIManager.Instance.ShowSplashScreen();

        // Schedule transition to MainMenuState
        Debug.Log("BootState: Scheduling transition to MainMenuState.");
        GameStateManager.Instance.SetStateWithDelay(new MainMenuState(), 2.0f);
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("BootState: Exiting BootState...");
        UIManager.Instance.HideSplashScreen();
    }
}
