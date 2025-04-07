using UnityEngine;

public class FreezeTile : SpecialTile
{
    [SerializeField] private ParticleSystem activationEffect; // Optional visual effect

    public override void ActivateAbility()
    {
        Debug.Log("FreezeTile: Activating ability to skip the next tile spawn.");

        // Skip the next tile spawn
        BoardManager.Instance.SkipNextTileSpawn();

        // Add score bonus
        ScoreManager.Instance.AddSpecialTileBonus();

        // Play activation effect if assigned
        if (activationEffect != null)
        {
            Instantiate(activationEffect, transform.position, Quaternion.identity);
        }

        // Destroy the FreezeTile itself
        DestroyTile();
    }

    private void DestroyTile()
    {
        Vector2Int tilePosition = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);

        // Unregister from SpecialTileManager
        SpecialTileManager.Instance.UnregisterSpecialTile(this);

        // Clear the cell in BoardManager
        BoardManager.Instance.ClearCell(tilePosition);

        // Destroy the game object
        Destroy(gameObject);

        Debug.Log($"FreezeTile: Destroyed at {tilePosition}.");
    }
}
