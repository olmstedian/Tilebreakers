using UnityEngine;

/// <summary>
/// Spawning new tile state - spawns a random new tile on the board.
/// </summary>
public class SpawningNewTileState : GameState
{
    private Vector2Int? mergedCellPosition;

    public SpawningNewTileState(Vector2Int? mergedCellPosition = null)
    {
        this.mergedCellPosition = mergedCellPosition;
    }

    public override void Enter()
    {
        Debug.Log("SpawningNewTileState: Spawning a new tile...");

        bool tileSpawned = BoardManager.Instance.GenerateRandomStartingTiles(1, 1, mergedCellPosition);

        if (tileSpawned)
        {
            Debug.Log("SpawningNewTileState: Tile spawned successfully. Transitioning to CheckingGameOverState.");
            GameStateManager.Instance.SetState(new CheckingGameOverState());
        }
        else
        {
            Debug.LogWarning("SpawningNewTileState: No valid positions to spawn a new tile. Checking game over.");
            GameOverManager.Instance.CheckGameOver();
        }
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("SpawningNewTileState: Exiting state.");
    }
}
