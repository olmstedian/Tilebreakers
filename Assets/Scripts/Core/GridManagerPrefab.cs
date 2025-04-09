using UnityEngine;

/// <summary>
/// Helper script to create a GridManager prefab.
/// Attach this script to a GameObject and create a prefab from it.
/// </summary>
[RequireComponent(typeof(GridManager))]
public class GridManagerPrefab : MonoBehaviour
{
    // This class is intentionally empty
    // It serves as a container for the GridManager component
    // and ensures the GridManager component is always added

    private void Awake()
    {
        // Ensure GridManager is properly initialized
        GetComponent<GridManager>().Initialize();
    }
}
