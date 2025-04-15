#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Tilebreakers.Core;

namespace Tilebreakers.Editor
{
    [CustomEditor(typeof(ScoreManager))]
    public class ScoreManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            ScoreManager scoreManager = (ScoreManager)target;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);
            
            // Add test score button
            if (GUILayout.Button("Add 100 Points (Test)"))
            {
                if (Application.isPlaying)
                {
                    scoreManager.AddScore(100);
                    Debug.Log("Test: Added 100 points to score");
                }
                else
                {
                    Debug.LogWarning("Cannot add points in edit mode. Enter play mode first.");
                }
            }
            
            // Reset score button
            if (GUILayout.Button("Reset Score"))
            {
                if (Application.isPlaying)
                {
                    scoreManager.ResetScore();
                    Debug.Log("Reset score to initial value");
                }
                else
                {
                    Debug.LogWarning("Cannot reset score in edit mode. Enter play mode first.");
                }
            }
            
            // Reset high score button
            if (GUILayout.Button("Reset High Score"))
            {
                // This will work in edit mode since we added a special editor method
                scoreManager.ResetHighScore();
            }
            
            // Test score popup
            if (GUILayout.Button("Test Score Popup"))
            {
                if (Application.isPlaying)
                {
                    ScoreManager.ShowScorePopup(100, "Test Popup +100");
                }
                else
                {
                    Debug.LogWarning("Cannot show popups in edit mode. Enter play mode first.");
                }
            }
        }
    }
}
#endif
