using UnityEngine;

public class PainterTile : SpecialTile
{
    private void Start()
    {
        specialAbilityName = "Painter"; // Ensure this matches the ability name used in SpawnSpecialTile
    }

    public override void ActivateAbility()
    {
        Debug.Log("PainterTile: Activating ability to convert adjacent tiles to its own color.");

        Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);
        Tile painterTile = GetComponent<Tile>();

        if (painterTile == null)
        {
            Debug.LogError("PainterTile: Tile component is missing. Cannot activate ability.");
            return;
        }

        // Convert all adjacent tiles to the PainterTile's color
        Vector2Int[] adjacentPositions = new Vector2Int[]
        {
            tilePosition + Vector2Int.up,
            tilePosition + Vector2Int.down,
            tilePosition + Vector2Int.left,
            tilePosition + Vector2Int.right
        };

        foreach (Vector2Int pos in adjacentPositions)
        {
            if (BoardManager.Instance.IsWithinBounds(pos))
            {
                Tile adjacentTile = BoardManager.Instance.GetTileAtPosition(pos);
                if (adjacentTile != null)
                {
                    Debug.Log($"PainterTile: Changing tile at {pos} to color {painterTile.tileColor}.");
                    adjacentTile.tileColor = painterTile.tileColor; // Set the color
                    adjacentTile.UpdateVisuals(); // Update the visuals to reflect the new color
                }
            }
        }

        // Destroy the PainterTile itself
        DestroyTile();
    }

    private void DestroyTile()
    {
        SpecialTileManager.Instance.UnregisterSpecialTile(this);
        BoardManager.Instance.ClearCell(BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position));
        Destroy(gameObject);
    }
}
