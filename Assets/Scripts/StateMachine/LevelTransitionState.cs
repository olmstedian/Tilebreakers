using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the transition between different levels or to infinite mode.
/// </summary>
public class LevelTransitionState : GameState
{
    private int targetLevelIndex;
    private float transitionDuration = 0.8f;
    
    public LevelTransitionState(int levelIndex)
    {
        targetLevelIndex = levelIndex;
    }

    public override void Enter()
    {
        Debug.Log($"LevelTransitionState: Beginning transition to level {targetLevelIndex}");
        
        // Create visual transition effect
        UIManager.Instance?.ShowLevelTransition();
        
        // Start the transition sequence
        GameStateManager.Instance.StartCoroutine(TransitionSequence());
    }

    private IEnumerator TransitionSequence()
    {
        // Wait for transition animation
        yield return new WaitForSeconds(transitionDuration);
        
        // Check if we're going to infinite mode
        bool goingToInfiniteMode = targetLevelIndex >= LevelManager.Instance.TotalLevels;
        
        // Handle level loading
        if (goingToInfiniteMode)
        {
            Debug.Log("LevelTransitionState: Transitioning to infinite mode");
            LevelManager.Instance.StartInfiniteMode();
        }
        else 
        {
            Debug.Log($"LevelTransitionState: Loading level {targetLevelIndex}");
            LevelManager.Instance.LoadLevel(targetLevelIndex);
        }
        
        // Transition to WaitingForInputState
        GameStateManager.Instance.SetState(new WaitingForInputState());
    }

    public override void Update()
    {
        // Logic handled in coroutine
    }

    public override void Exit()
    {
        Debug.Log("LevelTransitionState: Transition completed");
    }

    public override void HandleInput(Vector2Int position)
    {
        // No input handling during transition
    }
}
