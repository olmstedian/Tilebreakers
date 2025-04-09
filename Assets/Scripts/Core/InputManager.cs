using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public Vector2 touchPosition { get; private set; }
    private bool isSelecting;

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
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchPosition = touch.position;

                if (GameStateManager.Instance.IsInState<WaitingForInputState>())
                {
                    DetectTileSelection();
                }
                else if (GameStateManager.Instance.IsInState<SpecialTileActionState>())
                {
                    Debug.Log("InputManager: Waiting for special tile action to complete.");
                    // Optionally, wait for animations or other actions to finish
                }
            }
            else if (touch.phase == TouchPhase.Ended && GameStateManager.Instance.IsInState<WaitingForInputState>())
            {
                ConfirmTileMove();
            }
        }
        else if (Input.GetMouseButtonDown(0)) // For testing in editor
        {
            touchPosition = Input.mousePosition;

            if (GameStateManager.Instance.IsInState<WaitingForInputState>())
            {
                DetectTileSelection();
            }
            else if (GameStateManager.Instance.IsInState<SpecialTileActionState>())
            {
                Debug.Log("InputManager: Waiting for special tile action to complete.");
                // Optionally, wait for animations or other actions to finish
            }
        }
        else if (Input.GetMouseButtonUp(0) && GameStateManager.Instance.IsInState<WaitingForInputState>())
        {
            ConfirmTileMove();
        }
    }

    private void DetectTileSelection()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector2Int gridPosition = BoardManager.Instance.GetGridPositionFromWorldPosition(worldPosition);

        if (BoardManager.Instance.IsWithinBounds(gridPosition))
        {
            // Check if the selected tile is a special tile
            SpecialTile specialTile = SpecialTileManager.Instance.GetSpecialTileAtPosition(gridPosition);
            if (specialTile != null)
            {
                Debug.Log($"InputManager: Special tile detected at {gridPosition}. Activating...");
                // Directly activate the special tile
                specialTile.Activate();
                return;
            }

            // Handle regular tile selection
            if (GameStateManager.Instance?.IsInState<WaitingForInputState>() == true)
            {
                OnTileSelected?.Invoke(gridPosition);
            }
        }
    }

    private void ConfirmTileMove()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector2Int targetPosition = BoardManager.Instance.GetGridPositionFromWorldPosition(worldPosition);

        if (BoardManager.Instance.IsWithinBounds(targetPosition) &&
            GameStateManager.Instance?.IsInState<WaitingForInputState>() == true)
        {
            OnTileMoveConfirmed?.Invoke(targetPosition);
        }
    }
}
