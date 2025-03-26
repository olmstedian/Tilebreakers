using UnityEngine;

public class SpecialTileUI : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detect left mouse click
        {
            HandleSpecialTileClick();
        }
    }

    private void HandleSpecialTileClick()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePosition2D, Vector2.zero);

        if (hit.collider != null)
        {
            SpecialTile specialTile = hit.collider.GetComponent<SpecialTile>();
            if (specialTile != null)
            {
                // Activate the special tile's ability
                specialTile.ActivateAbility();
            }
        }
    }
}
