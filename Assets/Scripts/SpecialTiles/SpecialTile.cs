using UnityEngine;

public abstract class SpecialTile : MonoBehaviour
{
    public string specialAbilityName;

    private void OnMouseDown()
    {
        // Ensure activation only occurs in the correct game state
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            Debug.Log($"SpecialTile: {specialAbilityName} clicked. Activating...");
            Activate();
        }
    }

    public void Activate()
    {
        Debug.Log($"SpecialTile: Activating {specialAbilityName} ability.");
        ActivateAbility();

        // Award score bonus for using a special tile
        ScoreManager.Instance.AddSpecialTileBonus();

        // Transition to the next game state
        GameStateManager.Instance?.SetState(new CheckingGameOverState());
    }

    public abstract void ActivateAbility();
}
