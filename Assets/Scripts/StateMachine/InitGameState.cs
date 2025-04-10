using UnityEngine;

/// <summary>
/// Init game state - initializes the game board and spawns starting tiles.
/// </summary>
public class InitGameState : GameState
{
    public override void Enter()
    {
        Debug.Log("InitGameState: Initializing game...");

        // Check if BoardManager is initialized
        if (BoardManager.Instance == null)
        {
            Debug.LogError("InitGameState: BoardManager.Instance is null. Ensure BoardManager is properly initialized.");
            return;
        }

        // Check if ScoreManager is initialized
        if (ScoreManager.Instance == null)
        {
            Debug.LogError("InitGameState: ScoreManager.Instance is null. Ensure ScoreManager is properly initialized.");
            return;
        }

        // Check if UIManager is initialized
        if (UIManager.Instance == null)
        {
            Debug.LogError("InitGameState: UIManager.Instance is null. Ensure UIManager is properly initialized.");
            return;
        }

        // Clear the board and initialize it
        BoardManager.Instance.ClearBoard();
        BoardManager.Instance.InitializeBoard();

        // Spawn starting tiles
        BoardManager.Instance.GenerateRandomStartingTiles(Constants.MIN_START_TILES, Constants.MAX_START_TILES);

        // Reset the score and UI
        ScoreManager.Instance.ResetScore();
        UIManager.Instance.ResetTopBar();

        // Transition to the next state
        GameStateManager.Instance.SetState(new WaitingForInputState());
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("InitGameState: Game initialized.");
    }
}
