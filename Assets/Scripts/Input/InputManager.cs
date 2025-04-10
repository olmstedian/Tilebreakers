using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public Vector2 touchPosition { get; private set; }
    private bool isSelecting;
    private bool tileWasSelected = false;  // New tracking variable

    public delegate void TileSelectedAction(Vector2Int gridPosition);
    public static event TileSelectedAction OnTileSelected;

    public delegate void TileMoveConfirmedAction(Vector2Int targetPosition);
    public static event TileMoveConfirmedAction OnTileMoveConfirmed;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // First check if we're in the correct state before processing any input
        bool isInWaitingState = GameStateManager.Instance?.IsInState<WaitingForInputState>() == true;
        
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchPosition = touch.position;

                if (isInWaitingState)
                {
                    DetectTileSelection();
                }
                else
                {
                    Debug.Log($"InputManager: Touch input ignored - not in WaitingForInputState. Current state: {GameStateManager.Instance?.GetCurrentStateName() ?? "Unknown"}");
                }
            }
            else if (touch.phase == TouchPhase.Ended && isInWaitingState)
            {
                // For touch input, we handle clicks in a single touch, no separate confirm needed
            }
        }
        else if (Input.GetMouseButtonDown(0)) // For testing in editor
        {
            touchPosition = Input.mousePosition;
            isSelecting = true;

            if (isInWaitingState)
            {
                DetectTileSelection();
            }
            else
            {
                Debug.Log($"InputManager: Mouse input ignored - not in WaitingForInputState. Current state: {GameStateManager.Instance?.GetCurrentStateName() ?? "Unknown"}");
            }
        }
        else if (Input.GetMouseButtonUp(0) && isInWaitingState && isSelecting)
        {
            isSelecting = false;
            // Mouse input is already fully handled in DetectTileSelection
        }
    }

    private void DetectTileSelection()
    {
        // We already checked the state in HandleInput, but let's double-check for safety
        if (GameStateManager.Instance == null || !GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            Debug.LogWarning("InputManager: DetectTileSelection called outside of WaitingForInputState.");
            return;
        }

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector2Int gridPosition = BoardManager.Instance.GetGridPositionFromWorldPosition(worldPosition);

        // Check if the position is within board bounds
        if (BoardManager.Instance.IsWithinBounds(gridPosition))
        {
            // Invoke the event to select or move tiles
            OnTileSelected?.Invoke(gridPosition);
            
            // We don't need to call OnTileMoveConfirmed separately, as HandleTileSelection will
            // handle both selection and movement based on context
        }
        else
        {
            Debug.Log($"InputManager: Position {gridPosition} is out of bounds");
        }
    }

    // This method is no longer needed as HandleTileSelection in BoardManager now properly handles both
    // tile selection and movement to empty cells
    private void ConfirmTileMove()
    {
        // Double-check state again
        if (GameStateManager.Instance == null || !GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            Debug.LogWarning("InputManager: ConfirmTileMove called outside of WaitingForInputState.");
            return;
        }

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector2Int targetPosition = BoardManager.Instance.GetGridPositionFromWorldPosition(worldPosition);

        if (BoardManager.Instance.IsWithinBounds(targetPosition))
        {
            Debug.Log($"InputManager: Confirming tile move to position {targetPosition}");
            OnTileMoveConfirmed?.Invoke(targetPosition);
        }
    }
}
