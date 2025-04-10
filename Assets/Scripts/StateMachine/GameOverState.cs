using UnityEngine;

/// <summary>
/// Game over state - the player has no more valid moves.
/// </summary>
public class GameOverState : GameState
{
    public override void Enter()
    {
        Debug.Log("GameOverState: Game over.");
        UIManager.Instance.ShowGameOverScreen(ScoreManager.Instance.GetCurrentScore());
    }

    public override void Update() { }

    public override void HandleInput(Vector2Int gridPosition)
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartCurrentLevel();
        }
        else
        {
            GameStateManager.Instance.RestartGame();
        }
    }

    public override void Exit()
    {
        Debug.Log("GameOverState: Exiting game over.");
        UIManager.Instance.HideGameOverScreen();
    }
}
