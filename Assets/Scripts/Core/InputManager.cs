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
                isSelecting = true;
                DetectTileSelection();
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isSelecting = false;
                ConfirmTileMove();
            }
        }
        else if (Input.GetMouseButtonDown(0)) // For testing in editor
        {
            touchPosition = Input.mousePosition;
            isSelecting = true;
            DetectTileSelection();
        }
        else if (Input.GetMouseButtonUp(0) && isSelecting)
        {
            isSelecting = false;
            ConfirmTileMove();
        }
    }

    private void DetectTileSelection()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector2Int gridPosition = BoardManager.Instance.GetGridPositionFromWorldPosition(worldPosition);

        if (BoardManager.Instance.IsWithinBounds(gridPosition) &&
            GameStateManager.Instance?.IsInState<PlayerTurnState>() == true)
        {
            OnTileSelected?.Invoke(gridPosition);
        }
    }

    private void ConfirmTileMove()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector2Int targetPosition = BoardManager.Instance.GetGridPositionFromWorldPosition(worldPosition);

        if (BoardManager.Instance.IsWithinBounds(targetPosition) &&
            GameStateManager.Instance?.IsInState<PlayerTurnState>() == true)
        {
            OnTileMoveConfirmed?.Invoke(targetPosition);
        }
    }
}
