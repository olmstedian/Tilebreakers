using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private bool showPresetTilesSettings = true;
    private bool showTutorialSettings = true;
    private bool showPreviewSettings = true;
    private Texture2D previewTexture;
    private int previewSize = 300;

    public override void OnInspectorGUI()
    {
        LevelData levelData = (LevelData)target;
        SerializedObject serializedObject = new SerializedObject(target);
        serializedObject.Update();

        // Add quick access buttons at the top
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Test Level", GUILayout.Height(30)))
        {
            // Code to test the level in play mode
            TestLevel(levelData);
        }
        
        if (GUILayout.Button("Create Copy", GUILayout.Height(30)))
        {
            // Create a copy of this level
            CreateLevelCopy(levelData);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // ADD LEVEL DESCRIPTION - Enhanced editor
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Level Description", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);
        
        // Level name and number in one row
        EditorGUILayout.BeginHorizontal();
        
        // Level Number with label in front
        EditorGUILayout.LabelField("Level #:", GUILayout.Width(60));
        levelData.levelNumber = EditorGUILayout.IntField(levelData.levelNumber, GUILayout.Width(40));
        
        EditorGUILayout.Space(10);
        
        // Level Name with label in front
        EditorGUILayout.LabelField("Name:", GUILayout.Width(45));
        levelData.levelName = EditorGUILayout.TextField(levelData.levelName);
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Level Description as a text area with scroll
        EditorGUILayout.LabelField("Description (shown in UI):");
        
        // Use a larger text area with rich text preview
        GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.wordWrap = true;
        
        levelData.levelDescription = EditorGUILayout.TextArea(levelData.levelDescription, textAreaStyle, GUILayout.Height(60));
        
        // Level Objective Text
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Level Objective:", EditorStyles.boldLabel);
        
        // Generate default objective text based on level settings
        string defaultObjective = "";
        if (levelData.scoreObjective && levelData.scoreTarget > 0)
        {
            defaultObjective = $"Score {levelData.scoreTarget} points";
            if (levelData.movesTarget > 0)
            {
                defaultObjective += $" within {levelData.movesTarget} moves";
            }
        }
        else if (levelData.movesTarget > 0)
        {
            defaultObjective = $"Survive for {levelData.movesTarget} moves";
        }
        
        // Add a property for the objective text if it doesn't exist
        SerializedProperty objectiveTextProp = serializedObject.FindProperty("levelObjectiveText");
        if (objectiveTextProp == null)
        {
            // If the property doesn't exist yet, show a message and use the default text
            EditorGUILayout.HelpBox("Level objective text property not found in LevelData. Using auto-generated objective.", MessageType.Info);
            EditorGUILayout.LabelField("Auto-generated objective:", defaultObjective);
        }
        else
        {
            // Show field for custom objective text
            EditorGUILayout.PropertyField(objectiveTextProp, new GUIContent("Custom Objective Text"));
            
            // Show auto-generate button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button("Auto-generate Objective"))
            {
                objectiveTextProp.stringValue = defaultObjective;
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();
            
            // Show preview
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
            
            // Create a preview style similar to in-game
            GUIStyle objectivePreviewStyle = new GUIStyle(EditorStyles.label);
            objectivePreviewStyle.wordWrap = true;
            objectivePreviewStyle.richText = true;
            objectivePreviewStyle.fontStyle = FontStyle.Bold;
            objectivePreviewStyle.fontSize = 12;
            objectivePreviewStyle.normal.textColor = new Color(0.2f, 0.8f, 1f); // Bright blue for objectives
            
            string objectiveToShow = string.IsNullOrEmpty(objectiveTextProp.stringValue) ? 
                defaultObjective : objectiveTextProp.stringValue;
            
            // Show the preview
            EditorGUILayout.LabelField(objectiveToShow, objectivePreviewStyle);
        }
        
        // Rich text support section
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Formatting Tools:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // Create formatting buttons
        if (GUILayout.Button("Bold", EditorStyles.miniButton))
        {
            levelData.levelDescription = WrapSelectedText(textAreaStyle, "<b>", "</b>");
        }
        
        if (GUILayout.Button("Italic", EditorStyles.miniButton))
        {
            levelData.levelDescription = WrapSelectedText(textAreaStyle, "<i>", "</i>");
        }
        
        if (GUILayout.Button("Color", EditorStyles.miniButton))
        {
            levelData.levelDescription = WrapSelectedText(textAreaStyle, "<color=#FFCC00>", "</color>");
        }
        
        if (GUILayout.Button("Size+", EditorStyles.miniButton))
        {
            levelData.levelDescription = WrapSelectedText(textAreaStyle, "<size=120%>", "</size>");
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Second row of formatting buttons
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Clear Format", EditorStyles.miniButton))
        {
            // Remove all rich text tags
            string cleanText = levelData.levelDescription;
            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, "<.*?>", string.Empty);
            levelData.levelDescription = cleanText;
        }
        
        if (GUILayout.Button("Sample Text", EditorStyles.miniButton))
        {
            // Insert sample description text
            levelData.levelDescription = "Merge <color=#FFCC00>colored tiles</color> to create <b>higher values</b>!\n" +
                                        "Reach <color=#00CCFF><b>" + levelData.scoreTarget + "</b></color> points to complete the level.";
        }
        
        // Add objective text sample button
        if (GUILayout.Button("Sample Objective", EditorStyles.miniButton))
        {
            // Use the already declared objectiveTextProp variable from above
            // instead of redeclaring it
            if (objectiveTextProp != null)
            {
                // Insert sample objective text with formatting
                objectiveTextProp.stringValue = $"<color=#00FFFF>Score {levelData.scoreTarget} points</color> in <color=#FFAA00>{levelData.movesTarget} moves</color> or less!";
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("LevelDataEditor: Could not find levelObjectiveText property");
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Preview the description as it will appear in game
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Description Preview:", EditorStyles.boldLabel);
        
        // Create a preview style that mimics the in-game look
        GUIStyle previewStyle = new GUIStyle(EditorStyles.label);
        previewStyle.wordWrap = true;
        previewStyle.richText = true;
        previewStyle.fontStyle = FontStyle.Bold;
        previewStyle.fontSize = 12;
        previewStyle.normal.textColor = new Color(1f, 0.9f, 0.7f); // Warm gold color
        
        // Show the preview
        EditorGUILayout.LabelField(levelData.levelDescription, previewStyle, GUILayout.Height(60));
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Draw the default inspector for the rest of the properties
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        
        // Skip past the properties we've already handled manually
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            
            // Skip the properties we've handled above
            if (iterator.name == "m_Script" || 
                iterator.name == "levelNumber" || 
                iterator.name == "levelName" || 
                iterator.name == "levelDescription" ||
                iterator.name == "levelObjectiveText")
            {
                continue;
            }
            
            EditorGUILayout.PropertyField(iterator, true);
        }
        
        serializedObject.ApplyModifiedProperties();
        
        // Add preview visualization
        DrawLevelPreview(levelData);
        
        // Add button to verify level data
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Verify Level Data", GUILayout.Height(30)))
        {
            VerifyLevelData(levelData);
        }
    }

    private void DrawLevelPreview(LevelData levelData)
    {
        showPreviewSettings = EditorGUILayout.Foldout(showPreviewSettings, "Level Preview");
        
        if (showPreviewSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Calculate grid size based on board dimensions
            int cellSize = previewSize / Mathf.Max(levelData.boardWidth, levelData.boardHeight);
            int previewWidth = levelData.boardWidth * cellSize;
            int previewHeight = levelData.boardHeight * cellSize;
            
            // Create or update the preview texture
            if (previewTexture == null || 
                previewTexture.width != previewWidth || 
                previewTexture.height != previewHeight)
            {
                previewTexture = new Texture2D(previewWidth, previewHeight);
            }
            
            // Draw the board grid
            for (int x = 0; x < previewWidth; x++)
            {
                for (int y = 0; y < previewHeight; y++)
                {
                    int gridX = x / cellSize;
                    int gridY = y / cellSize;
                    
                    // Calculate checkerboard pattern
                    bool isEvenCell = (gridX + gridY) % 2 == 0;
                    Color cellColor = isEvenCell ? new Color(0.9f, 0.9f, 0.9f) : new Color(0.8f, 0.8f, 0.8f);
                    
                    // Mark blocked positions
                    Vector2Int pos = new Vector2Int(gridX, gridY);
                    if (levelData.blockedTilePositions.Contains(pos))
                    {
                        cellColor = new Color(0.3f, 0.3f, 0.3f);
                    }
                    
                    // Set the pixel color
                    previewTexture.SetPixel(x, y, cellColor);
                }
            }
            
            // Draw preset tiles
            foreach (var tilePreset in levelData.presetTiles)
            {
                int startX = tilePreset.position.x * cellSize;
                int startY = tilePreset.position.y * cellSize;
                
                // Draw the tile color
                for (int x = 0; x < cellSize; x++)
                {
                    for (int y = 0; y < cellSize; y++)
                    {
                        if (startX + x < previewWidth && startY + y < previewHeight)
                        {
                            previewTexture.SetPixel(startX + x, startY + y, tilePreset.color);
                        }
                    }
                }
            }
            
            // Draw special tiles
            foreach (var specialTile in levelData.presetSpecialTiles)
            {
                int startX = specialTile.position.x * cellSize;
                int startY = specialTile.position.y * cellSize;
                
                // Use different colors for different special tile types
                Color specialTileColor = Color.white;
                switch (specialTile.specialTileType)
                {
                    case "Blaster": specialTileColor = new Color(1f, 0.5f, 0f); break;
                    case "Freezer": specialTileColor = new Color(0f, 0.8f, 1f); break;
                    case "Doubler": specialTileColor = new Color(1f, 0.8f, 0f); break;
                    case "Painter": specialTileColor = new Color(0.7f, 0.3f, 0.7f); break;
                    default: specialTileColor = new Color(0.5f, 0.5f, 0.5f); break;
                }
                
                // Draw diagonal lines to indicate special tile
                for (int i = 0; i < cellSize; i++)
                {
                    // Diagonal lines
                    if (startX + i < previewWidth && startY + i < previewHeight)
                    {
                        previewTexture.SetPixel(startX + i, startY + i, specialTileColor);
                    }
                    if (startX + i < previewWidth && startY + cellSize - i - 1 < previewHeight)
                    {
                        previewTexture.SetPixel(startX + i, startY + cellSize - i - 1, specialTileColor);
                    }
                }
            }
            
            // Apply the texture
            previewTexture.Apply();
            
            // Display the preview
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(previewTexture, GUILayout.Width(previewWidth), GUILayout.Height(previewHeight));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
    }

    private void TestLevel(LevelData levelData)
    {
        // This would be implemented to initiate a test of this specific level
        if (EditorApplication.isPlaying)
        {
            Debug.Log($"Testing level: {levelData.levelName}");
            if (LevelManager.Instance != null)
            {
                // Set the current level to test and reload
                // This assumes LevelManager has a method to test a specific level
                // You would need to implement a TestLevel method in LevelManager
                // LevelManager.Instance.TestLevel(levelData);
                Debug.Log("This would test the level if TestLevel method was implemented in LevelManager");
            }
            else
            {
                Debug.LogWarning("Cannot test level - LevelManager instance not found");
            }
        }
        else
        {
            Debug.LogWarning("Cannot test level - Enter Play Mode first");
        }
    }

    private void CreateLevelCopy(LevelData levelData)
    {
        // Create a new copy of the level data
        LevelData newLevel = Instantiate(levelData);
        
        // Set a new name
        newLevel.levelName = $"{levelData.levelName} (Copy)";
        newLevel.levelNumber = levelData.levelNumber + 1;
        
        // Save the new asset with a modified name
        string path = AssetDatabase.GetAssetPath(levelData);
        string directory = System.IO.Path.GetDirectoryName(path);
        string filename = System.IO.Path.GetFileNameWithoutExtension(path);
        string newPath = $"{directory}/{filename}_Copy.asset";
        
        // Ensure the path is unique
        newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);
        
        // Create the asset and save it
        AssetDatabase.CreateAsset(newLevel, newPath);
        AssetDatabase.SaveAssets();
        
        // Select the new asset
        Selection.activeObject = newLevel;
        
        Debug.Log($"Created a copy of level {levelData.levelName} at {newPath}");
    }

    private void VerifyLevelData(LevelData levelData)
    {
        bool isValid = true;
        List<string> issues = new List<string>();
        
        // Check basic level info
        if (string.IsNullOrEmpty(levelData.levelName))
        {
            issues.Add("Level name is empty");
            isValid = false;
        }
        
        // Check level description
        if (string.IsNullOrEmpty(levelData.levelDescription))
        {
            issues.Add("Level description is empty");
            isValid = false;
        }
        else if (levelData.levelDescription.Length < 10)
        {
            issues.Add("Level description is too short (less than 10 characters)");
            isValid = false;
        }
        
        // Check level objectives
        if (levelData.scoreTarget <= 0 && levelData.scoreObjective)
        {
            issues.Add("Score objective is enabled but no score target is set");
            isValid = false;
        }
        
        if (levelData.movesTarget <= 0)
        {
            issues.Add("Moves target is not set");
            isValid = false;
        }
        
        // Check board dimensions
        if (levelData.boardWidth < 3 || levelData.boardWidth > 10)
        {
            issues.Add($"Board width ({levelData.boardWidth}) is outside the recommended range (3-10)");
            isValid = false;
        }
        
        if (levelData.boardHeight < 3 || levelData.boardHeight > 10)
        {
            issues.Add($"Board height ({levelData.boardHeight}) is outside the recommended range (3-10)");
            isValid = false;
        }
        
        // Check preset board positions
        if (levelData.hasPresetBoard)
        {
            foreach (var preset in levelData.presetTiles)
            {
                if (preset.position.x < 0 || preset.position.x >= levelData.boardWidth || 
                    preset.position.y < 0 || preset.position.y >= levelData.boardHeight)
                {
                    issues.Add($"Preset tile at position {preset.position} is outside the board bounds");
                    isValid = false;
                }
                
                if (preset.number <= 0 || preset.number > 12)
                {
                    issues.Add($"Preset tile at position {preset.position} has an invalid number: {preset.number}");
                    isValid = false;
                }
                
                // Check if position is blocked
                if (levelData.blockedTilePositions.Contains(preset.position))
                {
                    issues.Add($"Preset tile at position {preset.position} overlaps with a blocked position");
                    isValid = false;
                }
            }
            
            // Check special tiles
            foreach (var special in levelData.presetSpecialTiles)
            {
                if (special.position.x < 0 || special.position.x >= levelData.boardWidth || 
                    special.position.y < 0 || special.position.y >= levelData.boardHeight)
                {
                    issues.Add($"Special tile at position {special.position} is outside the board bounds");
                    isValid = false;
                }
                
                // Check if position is blocked
                if (levelData.blockedTilePositions.Contains(special.position))
                {
                    issues.Add($"Special tile at position {special.position} overlaps with a blocked position");
                    isValid = false;
                }
                
                // Check for overlap with preset tiles
                bool overlapDetected = false;
                foreach (var preset in levelData.presetTiles)
                {
                    if (preset.position == special.position)
                    {
                        overlapDetected = true;
                        break;
                    }
                }
                
                if (overlapDetected)
                {
                    issues.Add($"Special tile at position {special.position} overlaps with a preset tile");
                    isValid = false;
                }
                
                // Check special tile type
                if (string.IsNullOrEmpty(special.specialTileType))
                {
                    issues.Add($"Special tile at position {special.position} has no type specified");
                    isValid = false;
                }
                else
                {
                    string type = special.specialTileType.ToLower();
                    if (type != "blaster" && type != "freezer" && type != "doubler" && type != "painter")
                    {
                        issues.Add($"Special tile at position {special.position} has invalid type: {special.specialTileType}");
                        isValid = false;
                    }
                }
            }
        }
        
        // Display the results
        if (isValid)
        {
            EditorUtility.DisplayDialog("Level Validation", "Level data is valid!", "OK");
        }
        else
        {
            string message = "The following issues were found:\n\n";
            foreach (var issue in issues)
            {
                message += "• " + issue + "\n";
            }
            
            EditorUtility.DisplayDialog("Level Validation", message, "OK");
        }
    }

    // Helper method to wrap selected text in formatting tags
    private string WrapSelectedText(GUIStyle textAreaStyle, string openTag, string closeTag)
    {
        TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
        string text = ((LevelData)target).levelDescription;
        
        if (textEditor != null && textEditor.hasSelection)
        {
            int selectionStart = textEditor.cursorIndex;
            int selectionEnd = textEditor.selectIndex;
            
            if (selectionStart > selectionEnd)
            {
                int temp = selectionStart;
                selectionStart = selectionEnd;
                selectionEnd = temp;
            }
            
            // Insert tags around selection
            text = text.Substring(0, selectionStart) + openTag + 
                   text.Substring(selectionStart, selectionEnd - selectionStart) + 
                   closeTag + text.Substring(selectionEnd);
        }
        else
        {
            // If no selection, add empty tags at cursor position
            // First try to get cursor position
            if (GUIUtility.keyboardControl == GUIUtility.GetControlID(FocusType.Keyboard) && 
                EditorGUIUtility.editingTextField)
            {
                int cursorPosition = textEditor.cursorIndex;
                text = text.Substring(0, cursorPosition) + openTag + closeTag + text.Substring(cursorPosition);
            }
            else
            {
                // If we can't determine cursor position, just append at the end
                text += openTag + closeTag;
            }
        }
        
        return text;
    }
}
