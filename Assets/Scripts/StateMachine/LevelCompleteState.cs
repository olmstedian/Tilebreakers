using UnityEngine;
using Tilebreakers.Core; // Add this to resolve ScoreManager reference

/// <summary>
/// State when a level is completed
/// </summary>
public class LevelCompleteState : GameState
{
    private readonly int nextLevelIndex;
    private bool transitionRequested = false;

    public LevelCompleteState(int nextLevelIndex = -1)
    {
        this.nextLevelIndex = nextLevelIndex;
    }

    public override void Enter()
    {
        Debug.Log("LevelCompleteState: Entering level complete state.");

        // Use ScoreManager.Instance to reset the score
        ScoreManager.Instance?.ResetScore();

        // Fix: Call the static method using the class name
        ScoreManager.ShowScorePopup(ScoreManager.Instance.Score, "Level Complete!");

        // Notify UIManager to display level completion info
        UIManager.Instance.ShowLevelCompletePanel(nextLevelIndex);

        // Example: Reset score for the next level
        ScoreManager.Instance?.ResetScore();

        // Example: Prepare the next level
        LevelManager.Instance?.LoadLevel(nextLevelIndex);
    }

    public override void Update()
    {
        // Check if a transition to next level has been requested by another component
        if (transitionRequested)
        {
            Debug.Log("LevelCompleteState: Processing transition request to next level");
            GameStateManager.Instance.SetState(new LoadingLevelState());
            transitionRequested = false;
        }
    }

    public override void Exit()
    {
        Debug.Log("LevelCompleteState: Exited state");
        
        // Ensure any level complete UI is hidden
        UIManager.Instance?.HideLevelCompleteScreen();
    }

    /// <summary>
    /// Requests a transition to the next level
    /// </summary>
    public void RequestNextLevelTransition()
    {
        transitionRequested = true;
        Debug.Log("LevelCompleteState: Next level transition requested");
    }

    public override void HandleInput(Vector2Int position)
    {
        // No grid input handling in this state
        // Only UI buttons should be interactive
    }
}
