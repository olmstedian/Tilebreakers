using UnityEngine;

public class BlasterTile : SpecialTile
{
    public override void ActivateAbility()
    {
        Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);

        // Destroy all adjacent tiles
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
                    Destroy(adjacentTile.gameObject);
                    BoardManager.Instance.ClearCell(pos);
                }
            }
        }

        // Destroy the BlasterTile itself
        DestroyTile();
    }
}
