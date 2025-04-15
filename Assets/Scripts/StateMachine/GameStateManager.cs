using UnityEngine;
using System.Collections;
// Add this line to import the Special namespace
using Tilebreakers.Special;
using Tilebreakers.Core;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of GameStateManager
    /// </summary>
    
    public static GameStateManager Instance { get; private set; }
    private GameState currentState;

    public delegate void StateChangedHandler(GameState newState);
    public event StateChangedHandler OnStateChanged;

    private Coroutine delayedTransition;

    private void Awake()
    {
        if (Instance == null)
        {
            Debug.Log("GameStateManager: Initializing singleton instance");
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("GameStateManager: Another instance exists. Destroying this instance.");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        currentState?.Update();
    }

    public void SetState(GameState newState)
    {
        if (currentState?.GetType() == newState.GetType())
        {
            Debug.LogWarning($"GameStateManager: Attempted to transition to the same state: {newState.GetType().Name}. Ignoring.");
            return;
        }

        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
        OnStateChanged?.Invoke(currentState);

        Debug.Log($"GameStateManager: Transitioned to {newState.GetType().Name}.");
    }

    public void CancelDelayedTransition()
    {
        if (delayedTransition != null)
        {
            Debug.Log("GameStateManager: Canceling existing delayed transition.");
            StopCoroutine(delayedTransition);
            delayedTransition = null;
        }
    }

    public void SetStateWithDelay(GameState newState, float delay)
    {
        if (currentState?.GetType() == newState.GetType())
        {
            return;
        }

        if (delayedTransition != null)
        {
            StopCoroutine(delayedTransition);
        }

        delayedTransition = StartCoroutine(SetStateDelayed(newState, delay));
    }

    private IEnumerator SetStateDelayed(GameState newState, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Ensure the current state is still valid for the delayed transition
        if (currentState?.GetType() == newState?.GetType())
        {
            Debug.LogWarning($"GameStateManager: Delayed transition to {newState.GetType().Name} canceled because the state changed to {currentState?.GetType().Name ?? "None"}.");
            delayedTransition = null;
            yield break;
        }

        Debug.Log($"GameStateManager: Executing delayed transition to {newState.GetType().Name}.");
        SetState(newState);
        delayedTransition = null; // Clear the reference after execution
    }

    public void HandleInput(Vector2Int position)
    {
        currentState?.HandleInput(position);
    }

    public void RestartGame()
    {
        BoardManager.Instance?.ClearBoard();
        ScoreManager.Instance?.ResetScore();
        
        // Use LevelManager if available
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartCurrentLevel();
        }
        else
        {
            SetState(new InitGameState());
        }
    }

    public void CheckLevelCompletion()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CheckLevelCompletion();
        }
        else
        {
            // Default to checking game over
            SetState(new CheckingGameOverState());
        }
    }

    public void GoToMainMenu()
    {
        BoardManager.Instance?.ClearBoard();
        ScoreManager.Instance?.ResetScore();
        SetState(new MainMenuState());
    }

    public bool IsInState<T>() where T : GameState
    {
        return currentState is T;
    }

    public string GetCurrentStateName()
    {
        return currentState?.GetType().Name ?? "None";
    }

    public void PauseGame()
    {
        Debug.Log("GameStateManager: PauseGame called. Transitioning to PauseState.");
        SetState(new PauseState());
    }

    public void ResumeGame()
    {
        Debug.Log("GameStateManager: ResumeGame called. Transitioning back to WaitingForInputState.");
        SetState(new WaitingForInputState());
    }

    public void LoadLevel()
    {
        Debug.Log("GameStateManager: Loading level...");
        SetState(new LoadingLevelState());
    }

    private void OnDestroy()
    {
        InputManager.OnTileSelected -= HandleInput;
    }

    public void ClearAllSelections()
    {
        BoardManager.Instance?.ClearAllSelectionState();
        foreach (Tile tile in FindObjectsOfType<Tile>())
        {
            tile.ClearSelectionState();
        }
    }

    public void ActivateSpecialTile(Vector2Int gridPosition)
    {
        Debug.Log($"GameStateManager: Activating special tile at position {gridPosition}.");

        SpecialTile specialTile = SpecialTileManager.Instance.GetSpecialTileAtPosition(gridPosition);
        if (specialTile != null)
        {
            specialTile.Activate();
        }
        else
        {
            Debug.LogWarning("GameStateManager: No special tile found at the specified position.");
        }

        // Return to game loop flow
        SetState(new CheckingGameOverState());
    }

    public void SpawnSpecialTile(Vector2Int position, string abilityName)
    {
        if (!BoardManager.Instance.IsWithinBounds(position))
        {
            Debug.LogWarning($"GameStateManager: Cannot spawn special tile at {position}. Position is out of bounds.");
            return;
        }

        Debug.Log($"GameStateManager: Transitioning to SpecialTileSpawningState for '{abilityName}' at {position}.");
        SetState(new SpecialTileSpawningState(position, abilityName));
    }

    public void TriggerSpecialTileActivation()
    {
        Debug.Log("GameStateManager: Triggering special tile activation.");

        // Check if there are any active special tiles
        if (SpecialTileManager.Instance != null && SpecialTileManager.Instance.HasActiveSpecialTiles())
        {
            SetState(new SpecialTileActionState());
        }
        else
        {
            Debug.LogWarning("GameStateManager: No active special tiles to activate.");
            SetState(new CheckingGameOverState());
        }
    }

    public void EndTurn()
    {
        // Reset the merge state of all tiles
        foreach (Tile tile in FindObjectsOfType<Tile>())
        {
            if (tile.HasMerged())
            {
                Debug.Log($"GameStateManager: Resetting merge state for tile at {tile.transform.position}");
            }
            tile.ResetMergeState();
        }

        GameStateManager.Instance?.SetState(new SpawningNewTileState());
    }

    /// <summary>
    /// Checks if the game is currently accepting player input for tile selection.
    /// </summary>
    /// <returns>True if player can select/move tiles, false otherwise.</returns>
    public bool CanProcessTileInput()
    {
        return IsInState<WaitingForInputState>();
    }

    // Add methods to handle all possible states from the StateMachine folder
    
    // These states are already handled:
    // - InitGameState
    // - WaitingForInputState
    // - CheckingGameOverState
    // - GameOverState
    // - PauseState
    // - MainMenuState
    // - LoadingLevelState
    // - LevelCompleteState 
    // - LevelFailedState
    // - GameCompleteState
    // - SpecialTileSpawningState
    // - SpecialTileActionState
    // - SpecialTileActivationState
    
    // Add any missing state transition methods
    
    public void EnterBootState()
    {
        Debug.Log("GameStateManager: Entering BootState");
        SetState(new BootState());
    }
    
    public void EnterInitGameState()
    {
        Debug.Log("GameStateManager: Initializing game");
        SetState(new InitGameState());
    }
    
    public void EnterSpawningNewTileState()
    {
        Debug.Log("GameStateManager: Spawning new tiles");
        SetState(new SpawningNewTileState());
    }
    
    public void EnterGameCompleteState()
    {
        Debug.Log("GameStateManager: Game completed!");
        SetState(new GameCompleteState());
    }
    
    public void EnterGameOverState()
    {
        Debug.Log("GameStateManager: Game over!");
        SetState(new GameOverState());
    }
    
    // Method to get the current state as a readable string with more details
    public string GetDetailedStateInfo()
    {
        if (currentState == null)
            return "No active state";
            
        string stateName = currentState.GetType().Name;
        
        // Add specific details based on state type
        if (currentState is WaitingForInputState)
            return $"{stateName}: Waiting for player to select a tile or make a move";
        else if (currentState is SpawningNewTileState)
            return $"{stateName}: Adding new tile(s) to the board";
        else if (currentState is CheckingGameOverState)
            return $"{stateName}: Verifying if any valid moves remain";
        else if (currentState is SpecialTileActionState)
            return $"{stateName}: Special tile action in progress";
        
        return stateName;
    }
    
    // Additional utility method to check transition validity
    public bool CanTransitionTo<T>() where T : GameState
    {
        // Some transitions might not be valid based on the current state
        // For example, can't go directly from GameOverState to WaitingForInputState
        
        if (currentState is GameOverState && typeof(T) == typeof(WaitingForInputState))
            return false;
            
        if (currentState is PauseState && typeof(T) != typeof(WaitingForInputState) 
            && typeof(T) != typeof(MainMenuState))
            return false;
            
        return true;
    }

    // Add methods for all states from the StateMachine folder
    
    public void EnterAnimatingState()
    {
        Debug.Log("GameStateManager: Entering AnimatingState");
        // Fix: Add the next state parameter to the AnimatingState constructor
        SetState(new AnimatingState(new WaitingForInputState()));
    }
    
    public void EnterCheckingGameOverState()
    {
        Debug.Log("GameStateManager: Entering CheckingGameOverState");
        SetState(new CheckingGameOverState());
    }
    
    public void EnterLevelCompleteState(int nextLevelIndex = -1)
    {
        Debug.Log("GameStateManager: Entering LevelCompleteState");
        SetState(new LevelCompleteState(nextLevelIndex));
    }
    
    public void EnterLevelFailedState()
    {
        Debug.Log("GameStateManager: Entering LevelFailedState");
        SetState(new LevelFailedState());
    }
    
    public void EnterLoadingLevelState()
    {
        Debug.Log("GameStateManager: Entering LoadingLevelState");
        SetState(new LoadingLevelState());
    }
    
    public void EnterMainMenuState()
    {
        Debug.Log("GameStateManager: Entering MainMenuState");
        SetState(new MainMenuState());
    }
    
    public void EnterMergingTilesState()
    {
        Debug.Log("GameStateManager: Entering MergingTilesState");
        SetState(new MergingTilesState());
    }
    
    public void EnterMovingTilesState()
    {
        Debug.Log("GameStateManager: Entering MovingTilesState");
        SetState(new MovingTilesState());
    }
    
    public void EnterPauseState()
    {
        Debug.Log("GameStateManager: Entering PauseState");
        SetState(new PauseState());
    }
    
    public void EnterSpecialTileActionState()
    {
        Debug.Log("GameStateManager: Entering SpecialTileActionState");
        SetState(new SpecialTileActionState());
    }
    
    public void EnterSpecialTileActivationState()
    {
        Debug.Log("GameStateManager: Entering SpecialTileActivationState");
        SetState(new SpecialTileActivationState());
    }
    
    public void EnterSpecialTileSpawningState(Vector2Int position, string abilityName)
    {
        Debug.Log($"GameStateManager: Entering SpecialTileSpawningState for {abilityName} at {position}");
        SetState(new SpecialTileSpawningState(position, abilityName));
    }
    
    public void EnterSplittingTilesState()
    {
        Debug.Log("GameStateManager: Entering SplittingTilesState");
        SetState(new SplittingTilesState());
    }
    
    public void EnterWaitingForInputState()
    {
        Debug.Log("GameStateManager: Entering WaitingForInputState");
        SetState(new WaitingForInputState());
    }

    /// <summary>
    /// Transitions to a specified level
    /// </summary>
    public void GoToLevel(int levelIndex)
    {
        SetState(new LevelTransitionState(levelIndex));
    }

    /// <summary>
    /// Transitions to infinite mode
    /// </summary>
    public void GoToInfiniteMode()
    {
        // Use a level index beyond the total level count to indicate infinite mode
        int infiniteModeIndex = LevelManager.Instance != null ? LevelManager.Instance.TotalLevels : 999;
        SetState(new LevelTransitionState(infiniteModeIndex));
    }

    // Add these three missing state transition methods
    
    /// <summary>
    /// Transitions to the destruction state for handling tile destruction effects.
    /// </summary>
    public void EnterDestructionState()
    {
        Debug.Log("GameStateManager: Entering DestructionState");
        // Fix: Pass an empty list to match the required constructor parameter
        SetState(new DestructionState(new List<(Tile tile, Vector2Int position)>()));
    }

    /// <summary>
    /// Transitions to the level transition state to handle moving between levels.
    /// </summary>
    public void EnterLevelTransitionState(int targetLevelIndex)
    {
        Debug.Log($"GameStateManager: Transitioning to level {targetLevelIndex}");
        SetState(new LevelTransitionState(targetLevelIndex));
    }

    /// <summary>
    /// Transitions to the post-merge evaluation state after merges are completed.
    /// </summary>
    public void EnterPostMergeEvaluationState()
    {
        Debug.Log("GameStateManager: Entering PostMergeEvaluationState");
        SetState(new PostMergeEvaluationState());
    }
}