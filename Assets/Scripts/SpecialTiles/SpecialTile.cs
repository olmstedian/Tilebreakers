using UnityEngine;

public abstract class SpecialTile : MonoBehaviour
{
    public string specialAbilityName;

    private void OnMouseDown()
    {
        // Route special tile activation through GameStateManager
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsInState<SpecialTileActivationState>())
        {
            GameStateManager.Instance.ActivateSpecialTile(BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position));
        }
    }

    public void Activate()
    {
        Debug.Log($"SpecialTile: Activating {specialAbilityName} ability.");
        ActivateAbility();

        // Return to game loop flow after activation
        GameStateManager.Instance?.SetState(new CheckingGameOverState());
    }

    public abstract void ActivateAbility();
}
