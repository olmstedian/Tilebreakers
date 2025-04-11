using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to create and set up prefabs for the game's manager components.
/// This script should be used in the Unity Editor to generate required prefabs.
/// </summary>
#if UNITY_EDITOR
public class GameManagerPrefabSetup : MonoBehaviour
{
    [MenuItem("Tilebreakers/Create Manager Prefabs")]
    public static void CreateManagerPrefabs()
    {
        CreateGameStateManagerPrefab();
        CreateGameOverManagerPrefab();
        CreateTileMovementHandlerPrefab();
        Debug.Log("All manager prefabs created successfully!");
    }

    private static void CreateGameStateManagerPrefab()
    {
        // Create a new game object with the GameStateManager component from StateMachine folder
        GameObject gsm = new GameObject("GameStateManager");
        
        // Make sure to get the GameStateManager type from the StateMachine namespace if applicable
        var stateManagerType = typeof(GameStateManager);
        gsm.AddComponent(stateManagerType);
        
        // Create the prefab
        string prefabPath = "Assets/Prefabs/GameStateManager.prefab";
        CreatePrefabFromGameObject(gsm, prefabPath);
        
        // Clean up the temporary game object
        DestroyImmediate(gsm);
        
        Debug.Log("GameStateManager prefab created at: " + prefabPath);
    }
    
    private static void CreateGameOverManagerPrefab()
    {
        // Create a new game object with the GameOverManager component
        GameObject gom = new GameObject("GameOverManager");
        gom.AddComponent<GameOverManager>();
        
        // Create the prefab
        string prefabPath = "Assets/Prefabs/GameOverManager.prefab";
        CreatePrefabFromGameObject(gom, prefabPath);
        
        // Clean up the temporary game object
        DestroyImmediate(gom);
        
        Debug.Log("GameOverManager prefab created at: " + prefabPath);
    }
    
    private static void CreateTileMovementHandlerPrefab()
    {
        // Create a new game object with the TileMovementHandler component
        GameObject tmh = new GameObject("TileMovementHandler");
        tmh.AddComponent<TileMovementHandler>();
        
        // Create the prefab
        string prefabPath = "Assets/Prefabs/TileMovementHandler.prefab";
        CreatePrefabFromGameObject(tmh, prefabPath);
        
        // Clean up the temporary game object
        DestroyImmediate(tmh);
        
        Debug.Log("TileMovementHandler prefab created at: " + prefabPath);
    }
    
    private static void CreatePrefabFromGameObject(GameObject go, string path)
    {
        // Ensure the directory exists
        string directory = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        
        // Create the prefab
        PrefabUtility.SaveAsPrefabAsset(go, path);
    }
}
#endif
