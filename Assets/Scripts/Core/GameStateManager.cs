using UnityEngine;
using System.Collections;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    private GameState currentState;

    public delegate void StateChangedHandler(GameState newState);
    public event StateChangedHandler OnStateChanged;

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
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
        OnStateChanged?.Invoke(currentState);
    }

    public void SetStateWithDelay(GameState newState, float delay)
    {
        StartCoroutine(SetStateDelayed(newState, delay));
    }

    private IEnumerator SetStateDelayed(GameState newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetState(newState);
    }

    public void HandleInput(Vector2Int position)
    {
        currentState?.HandleInput(position);
    }

    public void RestartGame()
    {
        BoardManager.Instance?.ClearBoard();
        SetState(new InitState());
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
