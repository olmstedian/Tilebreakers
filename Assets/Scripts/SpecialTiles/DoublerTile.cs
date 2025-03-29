using UnityEngine;

public class DoublerTile : SpecialTile
{
    public override void ActivateAbility()
    {
        Debug.Log("DoublerTile: Doubling adjacent tiles.");

        Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);

        // Double the value of all adjacent tiles
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
                    adjacentTile.number *= 2;
                    adjacentTile.UpdateVisuals();
                }
            }
        }

        // Destroy the DoublerTile itself
        DestroyTile();
    }

    private void DestroyTile()
    {
        BoardManager.Instance.ClearCell(BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position));
        Destroy(gameObject);
    }
}
