using UnityEngine;

/// <summary>
/// Represents a blocked tile on the board that cannot be moved or merged with
/// </summary>
public class BlockedTile : MonoBehaviour
{
    private void Start()
    {
        // Optional: Add visual effects like subtle pulsing
        LeanTween.scale(gameObject, Vector3.one * 0.95f, 1.5f)
            .setEaseInOutSine()
            .setLoopPingPong();
    }

    // Blocked tiles cannot be destroyed by normal means
    public bool CanBeDestroyed() 
    {
        return false;
    }

    // But they can be destroyed by special tiles
    public void DestroyBySpecialTile()
    {
        // Get the position before destroying
        Vector2Int gridPos = BoardManager.Instance.GetGridPositionFromWorldPosition(transform.position);
        
        // Play destruction effect
        ParticleSystem destructionEffect = GetComponentInChildren<ParticleSystem>();
        if (destructionEffect != null)
        {
            destructionEffect.transform.SetParent(null);
            destructionEffect.Play();
            Destroy(destructionEffect.gameObject, destructionEffect.main.duration);
        }
        
        // Clear the cell in board manager
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ClearCell(gridPos);
            BoardManager.Instance.AddToEmptyCells(gridPos);
        }
        
        // Destroy the GameObject
        Destroy(gameObject);
    }
}
