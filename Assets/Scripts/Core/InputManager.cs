using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public Vector2 touchPosition;
    public Vector2 startTouchPosition { get; private set; } // Add this property
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

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position; // Set startTouchPosition
                    touchPosition = touch.position;
                    isSelecting = true;
                    DetectTileSelection();
                    break;

                case TouchPhase.Ended:
                    isSelecting = false;
                    ConfirmTileMove();
                    break;
            }
        }
        else if (Input.GetMouseButtonDown(0)) // For testing in editor
        {
            startTouchPosition = Input.mousePosition; // Set startTouchPosition
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

        if (BoardManager.Instance.IsWithinBounds(gridPosition))
        {
            OnTileSelected?.Invoke(gridPosition); // Notify listeners about the selected tile
        }
    }

    private void ConfirmTileMove()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector2Int targetPosition = BoardManager.Instance.GetGridPositionFromWorldPosition(worldPosition);

        if (BoardManager.Instance.IsWithinBounds(targetPosition))
        {
            OnTileMoveConfirmed?.Invoke(targetPosition); // Notify listeners about the confirmed move
        }
    }
}
