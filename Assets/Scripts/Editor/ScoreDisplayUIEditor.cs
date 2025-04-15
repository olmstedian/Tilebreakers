#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using Tilebreakers.UI;
using UnityEngine.UI;

namespace Tilebreakers.Editor
{
    [CustomEditor(typeof(ScoreDisplayUI))]
    public class ScoreDisplayUIEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            ScoreDisplayUI scoreDisplay = (ScoreDisplayUI)target;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Utility Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Find UI References"))
            {
                FindUIReferences(scoreDisplay);
            }
            
            if (GUILayout.Button("Create Score Popup Prefab"))
            {
                CreateScorePopupPrefab(scoreDisplay);
            }
        }
        
        private void FindUIReferences(ScoreDisplayUI scoreDisplay)
        {
            // Find Text components for current score and high score
            var textComponents = scoreDisplay.GetComponentsInChildren<TextMeshProUGUI>();
            
            SerializedObject so = new SerializedObject(scoreDisplay);
            
            foreach (var text in textComponents)
            {
                if (text.name.Contains("CurrentScore") || text.name.Contains("Score"))
                {
                    so.FindProperty("currentScoreText").objectReferenceValue = text;
                    Debug.Log("Found Current Score Text: " + text.name);
                }
                else if (text.name.Contains("HighScore"))
                {
                    so.FindProperty("highScoreText").objectReferenceValue = text;
                    Debug.Log("Found High Score Text: " + text.name);
                }
            }
            
            // Find container for popups (usually the canvas or a panel)
            var parentTransform = scoreDisplay.transform.parent;
            if (parentTransform != null)
            {
                so.FindProperty("scorePopupContainer").objectReferenceValue = parentTransform;
                Debug.Log("Set parent as Score Popup Container: " + parentTransform.name);
            }
            
            so.ApplyModifiedProperties();
        }
        
        private void CreateScorePopupPrefab(ScoreDisplayUI scoreDisplay)
        {
            // Create a new GameObject for the score popup
            GameObject popupObj = new GameObject("ScorePopupPrefab");
            
            // Add Canvas if needed
            Canvas canvas = popupObj.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = popupObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                popupObj.AddComponent<CanvasScaler>();
                popupObj.AddComponent<GraphicRaycaster>();
            }
            
            // Add TextMeshPro component
            TextMeshProUGUI tmpText = popupObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = "+100";
            tmpText.fontSize = 36;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = new Color(0.2f, 1f, 0.2f);
            tmpText.enableAutoSizing = true;
            tmpText.fontSizeMin = 24;
            tmpText.fontSizeMax = 48;
            
            // Add shadow
            Shadow shadow = popupObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);
            
            // Add ScorePopup component
            ScorePopup scorePopup = popupObj.AddComponent<ScorePopup>();
            SerializedObject so = new SerializedObject(scorePopup);
            so.FindProperty("scoreText").objectReferenceValue = tmpText;
            so.ApplyModifiedProperties();
            
            // Set transform properties
            RectTransform rectTransform = popupObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(200, 80);
            
            // Set canvas group for fading
            CanvasGroup canvasGroup = popupObj.AddComponent<CanvasGroup>();
            
            // Create prefab
            string prefabPath = "Assets/Prefabs/UI/ScorePopupPrefab.prefab";
            
            // Ensure directory exists
            string directory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            // Create prefab asset from the GameObject
            GameObject createdPrefab = PrefabUtility.SaveAsPrefabAsset(popupObj, prefabPath);
            
            // Assign to ScoreDisplayUI if successful
            if (createdPrefab != null)
            {
                SerializedObject scoreDisplaySO = new SerializedObject(scoreDisplay);
                scoreDisplaySO.FindProperty("scorePopupPrefab").objectReferenceValue = createdPrefab;
                scoreDisplaySO.ApplyModifiedProperties();
                Debug.Log("ScorePopup prefab created successfully at: " + prefabPath);
            }
            
            // Clean up
            DestroyImmediate(popupObj);
        }
    }
}
#endif
