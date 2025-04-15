using UnityEngine;
using Tilebreakers.Special; // Add this namespace to access SpecialTileManager

/// <summary>
/// Special tile action state - handles the activation of special tiles.
/// </summary>
public class SpecialTileActionState : GameState
{
    public override void Enter()
    {
        Debug.Log("SpecialTileActionState: Activating all special tiles...");
        SpecialTileManager.Instance.ActivateAllSpecialTiles();

        // Transition to CheckingGameOverState after activation
        GameStateManager.Instance.SetState(new CheckingGameOverState());
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("SpecialTileActionState: Exiting special tile action state.");
    }

    public override void HandleInput(Vector2Int gridPosition)
    {
        // No input is handled in this state
    }
}
