using UnityEngine;

public class BlasterTile : SpecialTile
{
    private void Start()
    {
        specialAbilityName = "Blaster"; // Ensure this matches the ability name used in SpawnSpecialTile
    }

    private void OnMouseDown()
    {
        // Trigger activation when the player taps the tile
        Activate();
    }

    public override void ActivateAbility()
    {
        Debug.Log("BlasterTile: Activating Blaster ability.");

        Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);

        // Destroy all adjacent tiles (orthogonal and diagonal)
        Vector2Int[] adjacentPositions = new Vector2Int[]
        {
            tilePosition + Vector2Int.up,
            tilePosition + Vector2Int.down,
            tilePosition + Vector2Int.left,
            tilePosition + Vector2Int.right,
            tilePosition + Vector2Int.up + Vector2Int.left,
            tilePosition + Vector2Int.up + Vector2Int.right,
            tilePosition + Vector2Int.down + Vector2Int.left,
            tilePosition + Vector2Int.down + Vector2Int.right
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

        // Award score for using the Blaster
        ScoreManager.Instance.AddSpecialTileBonus();

        // Destroy the BlasterTile itself
        DestroyTile();
    }

    private void DestroyTile()
    {
        SpecialTileManager.Instance.UnregisterSpecialTile(this);
        BoardManager.Instance.ClearCell(BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position));
        Destroy(gameObject);
    }
}
