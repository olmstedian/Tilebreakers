using UnityEngine;
using System.Collections;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    private GameState currentState;

    public delegate void StateChangedHandler(GameState newState);
    public event StateChangedHandler OnStateChanged;

    private Coroutine delayedTransition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
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
}