using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool isSwiping;

    public delegate void SwipeAction(Vector2Int direction, int swipeDistance);
    public static event SwipeAction OnSwipe;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        HandleSwipeInput();
    }

    private void HandleSwipeInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    isSwiping = true;
                    break;

                case TouchPhase.Moved:
                    if (isSwiping)
                    {
                        endTouchPosition = touch.position;
                        DetectSwipe();
                    }
                    break;

                case TouchPhase.Ended:
                    isSwiping = false;
                    break;
            }
        }
        else if (Input.GetMouseButtonDown(0)) // For testing in editor
        {
            startTouchPosition = Input.mousePosition;
            isSwiping = true;
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            endTouchPosition = Input.mousePosition;
            isSwiping = false;
            DetectSwipe();
        }
    }

    private void DetectSwipe()
    {
        Vector2 swipeDelta = endTouchPosition - startTouchPosition;

        if (swipeDelta.magnitude > 50) // Minimum swipe distance
        {
            swipeDelta.Normalize();

            Vector2Int direction = Vector2Int.zero;

            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                direction = swipeDelta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                direction = swipeDelta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }

            // Calculate swipe distance in grid units
            int swipeDistance = Mathf.FloorToInt((endTouchPosition - startTouchPosition).magnitude / (Screen.height / 6f));

            OnSwipe?.Invoke(direction, swipeDistance); // Notify listeners about the swipe direction and distance
        }
    }
}
