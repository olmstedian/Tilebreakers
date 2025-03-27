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
        Debug.Log($"GameStateManager: Transitioning from {currentState?.GetType().Name ?? "None"} to {newState.GetType().Name}");
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
        OnStateChanged?.Invoke(currentState);
    }

    public void SetStateWithDelay(GameState newState, float delay)
    {
        // Prevent scheduling a delayed transition to the same state
        if (delayedTransition != null)
        {
            Debug.LogWarning($"GameStateManager: Canceling existing delayed transition to {newState?.GetType().Name ?? "None"}.");
            StopCoroutine(delayedTransition);
            delayedTransition = null;
        }

        // If newState is null or matches the current state, do not schedule a new transition
        if (newState == null || currentState?.GetType() == newState.GetType())
        {
            Debug.LogWarning($"GameStateManager: Skipping delayed transition to {newState?.GetType().Name ?? "None"} because it matches the current state.");
            return;
        }

        Debug.Log($"GameStateManager: Delaying transition to {newState.GetType().Name} by {delay} seconds.");
        delayedTransition = StartCoroutine(SetStateDelayed(newState, delay));
    }

    private IEnumerator SetStateDelayed(GameState newState, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Ensure the current state is still valid for the delayed transition
        if (currentState?.GetType() != newState?.GetType())
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
        SetState(new InitState());
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

    private void OnDestroy()
    {
        InputManager.OnTileSelected -= HandleInput;
    }

    public void ClearAllSelections()
    {
        PlayerTurnState.ClearAllSelectionState();
        BoardManager.Instance?.ClearSelection();
        foreach (Tile tile in FindObjectsOfType<Tile>())
        {
            tile.ClearSelectionState();
        }
    }
}
