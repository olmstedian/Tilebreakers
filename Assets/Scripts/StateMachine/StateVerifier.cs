using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// Utility class to verify all GameState-derived classes are properly handled in GameStateManager.
/// </summary>
public static class StateVerifier
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void VerifyAllStatesHandled()
    {
        // Only run in editor or development builds
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("StateVerifier: Checking if all states are properly handled...");
        
        // Get all types that inherit from GameState
        var gameStateTypes = GetAllGameStateTypes();
        
        // Check GameStateManager for methods handling these states
        var unhandledStates = FindUnhandledStates(gameStateTypes);
        
        if (unhandledStates.Count > 0)
        {
            Debug.LogWarning($"StateVerifier: Found {unhandledStates.Count} potentially unhandled states:");
            foreach (var state in unhandledStates)
            {
                Debug.LogWarning($" - {state.Name} may not be properly handled in GameStateManager");
            }
            
            // Provide implementation suggestion
            foreach (var state in unhandledStates)
            {
                string methodName = "Enter" + state.Name;
                
                // Check if the state has constructor parameters
                var ctors = state.GetConstructors();
                string constructorParameters = "";
                string methodParameters = "";
                
                foreach (var ctor in ctors)
                {
                    var parameters = ctor.GetParameters();
                    if (parameters.Length > 0)
                    {
                        constructorParameters = string.Join(", ", parameters.Select(p => GetDefaultValueForType(p.ParameterType)));
                        methodParameters = string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name} = {GetDefaultValueForType(p.ParameterType)}"));
                        break;
                    }
                }
                
                Debug.Log($"Suggested implementation:\n" +
                          $"public void {methodName}({methodParameters}) {{\n" +
                          $"    Debug.Log(\"GameStateManager: Entering {state.Name}\");\n" +
                          $"    SetState(new {state.Name}({constructorParameters}));\n" +
                          $"}}");
            }
        }
        else
        {
            Debug.Log("StateVerifier: All game states appear to be properly handled.");
        }
        #endif
    }
    
    private static string GetDefaultValueForType(Type type)
    {
        if (type == typeof(int))
            return "-1";
        if (type == typeof(string))
            return "\"\"";
        if (type == typeof(bool))
            return "false";
        if (type == typeof(Vector2Int))
            return "default(Vector2Int)";
        // Add more types as needed
        return "default";
    }
    
    private static List<Type> GetAllGameStateTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(GameState)))
            .ToList();
    }
    
    private static List<Type> FindUnhandledStates(List<Type> gameStateTypes)
    {
        var unhandledStates = new List<Type>();
        var gameStateManagerType = typeof(GameStateManager);
        
        foreach (var stateType in gameStateTypes)
        {
            // Check for direct method like "EnterXState"
            string expectedMethodName = "Enter" + stateType.Name;
            bool hasDirectMethod = gameStateManagerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Any(m => m.Name == expectedMethodName);
                
            // Check for SetState calls with this state
            bool isHandledInSetState = false;
            
            // Only add to unhandled if both checks fail
            if (!hasDirectMethod && !isHandledInSetState)
            {
                unhandledStates.Add(stateType);
            }
        }
        
        return unhandledStates;
    }
}
