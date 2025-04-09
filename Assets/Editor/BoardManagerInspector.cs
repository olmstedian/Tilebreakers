using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BoardManager))]
public class BoardManagerInspector : Editor
{
    SerializedProperty gridBackgroundColor;
    SerializedProperty gridLineColor;
    SerializedProperty gridLineWidth;
    SerializedProperty useRoundedCorners;
    SerializedProperty cornerRadius;
    SerializedProperty cellSpacing;
    SerializedProperty gridCellMaterial;
    
    // Tile properties
    SerializedProperty width;
    SerializedProperty height;
    SerializedProperty cellSize;
    SerializedProperty tilePrefab;
    SerializedProperty cellIndicatorPrefab;
    SerializedProperty gridBackgroundPrefab;

    void OnEnable()
    {
        // Visual properties
        gridBackgroundColor = serializedObject.FindProperty("gridBackgroundColor");
        gridLineColor = serializedObject.FindProperty("gridLineColor");
        gridLineWidth = serializedObject.FindProperty("gridLineWidth");
        useRoundedCorners = serializedObject.FindProperty("useRoundedCorners");
        cornerRadius = serializedObject.FindProperty("cornerRadius");
        cellSpacing = serializedObject.FindProperty("cellSpacing");
        gridCellMaterial = serializedObject.FindProperty("gridCellMaterial");
        
        // Board properties
        width = serializedObject.FindProperty("width");
        height = serializedObject.FindProperty("height");
        cellSize = serializedObject.FindProperty("cellSize");
        tilePrefab = serializedObject.FindProperty("tilePrefab");
        cellIndicatorPrefab = serializedObject.FindProperty("cellIndicatorPrefab");
        gridBackgroundPrefab = serializedObject.FindProperty("gridBackgroundPrefab");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("Board Dimensions", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(width);
        EditorGUILayout.PropertyField(height);
        EditorGUILayout.PropertyField(cellSize);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tilePrefab);
        EditorGUILayout.PropertyField(cellIndicatorPrefab);
        EditorGUILayout.PropertyField(gridBackgroundPrefab);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(gridBackgroundColor);
        EditorGUILayout.PropertyField(gridLineColor);
        EditorGUILayout.PropertyField(gridLineWidth);
        EditorGUILayout.PropertyField(useRoundedCorners);
        
        if (useRoundedCorners.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(cornerRadius);
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.PropertyField(cellSpacing);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Materials", EditorStyles.boldLabel);
        
        // Material field with Create button
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(gridCellMaterial);
        
        if (GUILayout.Button("Create", GUILayout.Width(60)))
        {
            CreateGridCellMaterial();
        }
        EditorGUILayout.EndHorizontal();
        
        if (gridCellMaterial.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("A Grid Cell Material is required for properly rendering grid cells. Click 'Create' to generate a default material.", MessageType.Info);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void CreateGridCellMaterial()
    {
        // Check if the shader exists
        Shader gridCellShader = Shader.Find("Custom/GridCell");
        if (gridCellShader == null)
        {
            EditorUtility.DisplayDialog("Shader Missing", 
                "The Custom/GridCell shader could not be found. Make sure you have created the GridCellShader.shader file first.", 
                "OK");
            return;
        }
        
        // Create the material
        Material material = new Material(gridCellShader);
        material.SetColor("_Color", new Color(0.4f, 0.4f, 0.5f, 1.0f));
        material.SetColor("_OutlineColor", new Color(0.2f, 0.2f, 0.3f, 1.0f));
        material.SetFloat("_OutlineWidth", 0.02f);
        material.SetFloat("_CornerRadius", 0.1f);
        material.SetFloat("_GlowIntensity", 0f);
        material.SetColor("_GlowColor", new Color(0.5f, 0.8f, 1.0f, 1.0f));
        
        // Save the material as an asset
        string path = "Assets/Materials";
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
        
        string assetPath = EditorUtility.SaveFilePanelInProject(
            "Save Grid Cell Material", 
            "GridCellMaterial", 
            "mat", 
            "Choose a location to save the grid cell material", 
            path);
            
        if (!string.IsNullOrEmpty(assetPath))
        {
            AssetDatabase.CreateAsset(material, assetPath);
            AssetDatabase.SaveAssets();
            
            // Assign the new material to the property
            gridCellMaterial.objectReferenceValue = material;
            serializedObject.ApplyModifiedProperties();
            
            EditorUtility.DisplayDialog("Material Created", 
                "Grid Cell Material was created successfully.", 
                "OK");
        }
    }
}
