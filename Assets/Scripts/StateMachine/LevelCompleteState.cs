using UnityEngine;

/// <summary>
/// Level complete state - shows level complete UI and prepares for the next level.
/// </summary>
public class LevelCompleteState : GameState
{
    private int nextLevelIndex;
    
    public LevelCompleteState(int nextLevelIndex)
    {
        this.nextLevelIndex = nextLevelIndex;
    }
    
    public override void Enter()
    {
        Debug.Log($"LevelCompleteState: Level complete! Next level: {nextLevelIndex}");
        
        // Display level complete screen
        UIManager.Instance.ShowLevelCompleteScreen(ScoreManager.Instance.GetCurrentScore());
        
        // Play success sound
        AudioManager.Instance?.PlayLevelCompleteSound();
    }

    public override void Update() { }

    public override void HandleInput(Vector2Int gridPosition)
    {
        // Progress to the next level when player taps
        LoadNextLevel();
    }

    public override void Exit()
    {
        Debug.Log("LevelCompleteState: Proceeding to next level.");
        UIManager.Instance.HideLevelCompleteScreen();
    }
    
    private void LoadNextLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(nextLevelIndex);
            GameStateManager.Instance.SetState(new WaitingForInputState());
        }
        else
        {
            GameStateManager.Instance.RestartGame();
        }
    }
}
