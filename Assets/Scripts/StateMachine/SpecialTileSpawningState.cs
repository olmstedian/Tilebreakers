using UnityEngine;
using Tilebreakers.Special; // Add this namespace to access SpecialTileManager

/// <summary>
/// Special tile spawning state - handles spawning of special tiles.
/// </summary>
public class SpecialTileSpawningState : GameState
{
    private Vector2Int spawnPosition;
    private string specialAbilityName;

    public SpecialTileSpawningState(Vector2Int spawnPosition, string specialAbilityName)
    {
        this.spawnPosition = spawnPosition;
        this.specialAbilityName = specialAbilityName;
    }

    public override void Enter()
    {
        Debug.Log($"SpecialTileSpawningState: Spawning special tile '{specialAbilityName}' at {spawnPosition}.");
        SpecialTileManager.Instance.SpawnSpecialTile(spawnPosition, specialAbilityName);
        GameStateManager.Instance.SetState(new WaitingForInputState());
    }

    public override void Update()
    {
        // No specific update logic for this state
    }

    public override void Exit()
    {
        Debug.Log("SpecialTileSpawningState: Exiting state.");
    }
}
