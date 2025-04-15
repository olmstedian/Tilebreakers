#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Tilebreakers.Core
{
    /// <summary>
    /// Documentation for the score system architecture.
    /// This appears in the inspector when the script is selected.
    /// </summary>
    [CreateAssetMenu(fileName = "Score System Documentation", menuName = "Tilebreakers/Documentation/Score System")]
    public class ScoreSystemReadme : ScriptableObject
    {
        public Texture2D logo;
        public string title = "Score System Overview";
        
        [TextArea(15, 30)]
        public string description = @"TILEBREAKERS SCORE SYSTEM
-----------------------

The score system consists of three core components:

1. ScoreManager (Singleton)
   - Handles score tracking, saving, and loading
   - Manages score events and high score
   - Maintains game-specific score rules

2. ScoreUtility (Static)
   - Provides utility methods for score operations
   - Handles score popup creation and animation
   - Acts as a centralized API for other systems

3. UI Components
   - ScoreDisplayUI: Displays score in the UI
   - ScorePopup: Individual score popups

HOW TO USE
----------

1. Adding points:
   ScoreManager.Instance.AddScore(100);
   
2. Showing a score popup:
   ScoreUtility.ShowPopupAtScreenPosition(100, position);
   
3. Adding points with popup:
   ScoreUtility.AddPointsWithPopup(100, worldPosition);
   
4. Testing popups:
   ScoreUtility.ShowTestPopups(5);

ADDITIONAL INFO
--------------
- Score popups always appear at the top of the screen
- Score values are saved automatically
- Popups show only the point value (e.g., +100)";

        [HideInInspector]
        public bool showLogo = true;
    }

    [CustomEditor(typeof(ScoreSystemReadme))]
    public class ScoreSystemReadmeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var readme = (ScoreSystemReadme)target;
            
            if (readme.showLogo && readme.logo != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(readme.logo, GUILayout.MaxWidth(300));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
            
            EditorGUILayout.LabelField(readme.title, EditorStyles.boldLabel);
            GUILayout.Space(5);
            EditorGUILayout.LabelField(readme.description, EditorStyles.wordWrappedLabel);
            
            GUILayout.Space(20);
            if (GUILayout.Button("View ScoreManager", GUILayout.Height(30)))
            {
                var asset = AssetDatabase.FindAssets("t:Script ScoreManager").Length > 0 ? 
                    AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:Script ScoreManager")[0])) : null;
                if (asset != null) EditorGUIUtility.PingObject(asset);
            }
            
            if (GUILayout.Button("View ScoreUtility", GUILayout.Height(30)))
            {
                var asset = AssetDatabase.FindAssets("t:Script ScoreUtility").Length > 0 ? 
                    AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:Script ScoreUtility")[0])) : null;
                if (asset != null) EditorGUIUtility.PingObject(asset);
            }
        }
    }
}
#endif
