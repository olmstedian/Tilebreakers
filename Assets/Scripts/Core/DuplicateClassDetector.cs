using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

/// <summary>
/// A utility class to detect duplicate class definitions in the project.
/// This is especially useful for finding ambiguous method calls.
/// </summary>
public class DuplicateClassDetector : MonoBehaviour
{
    [SerializeField] private bool runOnStart = false;
    [SerializeField] private bool includeUnityClasses = false;
    
    private Dictionary<string, List<Type>> typeMap = new Dictionary<string, List<Type>>();
    
    private void Start()
    {
        if (runOnStart)
        {
            DetectDuplicateClasses();
        }
    }
    
    [ContextMenu("Detect Duplicate Classes")]
    public void DetectDuplicateClasses()
    {
        Debug.Log("Starting duplicate class detection...");
        typeMap.Clear();
        
        // Get all loaded assemblies
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (Assembly assembly in assemblies)
        {
            // Skip Unity assemblies unless specifically requested
            if (!includeUnityClasses && (assembly.FullName.Contains("UnityEngine") || assembly.FullName.Contains("Unity.")))
                continue;
                
            try
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    // Skip compiler-generated types
                    if (type.Name.Contains("<") || type.Name.Contains("+"))
                        continue;
                        
                    // Add the type to our map
                    if (!typeMap.ContainsKey(type.Name))
                    {
                        typeMap[type.Name] = new List<Type>();
                    }
                    typeMap[type.Name].Add(type);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.LogWarning($"Could not load types from assembly {assembly.FullName}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing assembly {assembly.FullName}: {ex.Message}");
            }
        }
        
        // Report duplicates
        int duplicatesFound = 0;
        foreach (var pair in typeMap)
        {
            if (pair.Value.Count > 1)
            {
                duplicatesFound++;
                Debug.LogWarning($"Found duplicate class: {pair.Key} ({pair.Value.Count} instances)");
                
                foreach (Type type in pair.Value)
                {
                    Debug.LogWarning($"  - {type.FullName} in assembly {type.Assembly.GetName().Name}");
                }
            }
        }
        
        Debug.Log($"Duplicate class detection complete. Found {duplicatesFound} duplicates.");
    }
    
    [ContextMenu("Check Ambiguous Method Calls")]
    public void CheckAmbiguousMethods()
    {
        Debug.Log("Checking for potentially ambiguous method calls...");
        int ambiguousMethodCount = 0;
        
        // First ensure we have the type map populated
        if (typeMap.Count == 0)
            DetectDuplicateClasses();
            
        // Look for types with multiple definitions
        foreach (var pair in typeMap.Where(p => p.Value.Count > 1))
        {
            List<Type> duplicateTypes = pair.Value;
            
            // For each duplicate type, check its methods
            HashSet<string> methodSignatures = new HashSet<string>();
            Dictionary<string, List<string>> ambiguousMethods = new Dictionary<string, List<string>>();
            
            foreach (Type type in duplicateTypes)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | 
                                                       BindingFlags.Static | BindingFlags.Instance |
                                                       BindingFlags.DeclaredOnly);
                                                       
                foreach (MethodInfo method in methods)
                {
                    string signature = GetMethodSignature(method);
                    
                    if (methodSignatures.Contains(signature))
                    {
                        // This is a potentially ambiguous method
                        if (!ambiguousMethods.ContainsKey(method.Name))
                        {
                            ambiguousMethods[method.Name] = new List<string>();
                        }
                        if (!ambiguousMethods[method.Name].Contains(signature))
                        {
                            ambiguousMethods[method.Name].Add(signature);
                        }
                    }
                    else
                    {
                        methodSignatures.Add(signature);
                    }
                }
            }
            
            // Report ambiguous methods
            foreach (var methodPair in ambiguousMethods)
            {
                ambiguousMethodCount += methodPair.Value.Count;
                Debug.LogWarning($"Potentially ambiguous method: {pair.Key}.{methodPair.Key}");
                foreach (string signature in methodPair.Value)
                {
                    Debug.LogWarning($"  - {signature}");
                }
            }
        }
        
        Debug.Log($"Ambiguous method check complete. Found {ambiguousMethodCount} potentially ambiguous methods.");
    }
    
    private string GetMethodSignature(MethodInfo method)
    {
        string parameters = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
        return $"{method.ReturnType.Name} {method.Name}({parameters})";
    }
}
