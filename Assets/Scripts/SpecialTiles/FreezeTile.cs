using UnityEngine;

public class FreezeTile : SpecialTile
{
    public override void ActivateAbility()
    {
        Debug.Log("FreezeTile: Freezing adjacent tiles.");

        Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);

        // Freeze all adjacent tiles
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
                    adjacentTile.SetState(Tile.TileState.Idle); // Prevent movement for one turn
                }
            }
        }

        // Destroy the FreezeTile itself
        DestroyTile();
    }

    private void DestroyTile()
    {
        BoardManager.Instance.ClearCell(BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position));
        Destroy(gameObject);
    }
}
