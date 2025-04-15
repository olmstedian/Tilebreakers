using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Tilebreakers.UI;

namespace Tilebreakers.Core
{
    /// <summary>
    /// Consolidated utility for score operations and popup display
    /// Combines functionality from ScoreFacade, ScorePopupBridge, and other helpers
    /// </summary>
    public static class ScoreUtility
    {
        #region Properties and Fields
        
        // Canvas and prefab for popups
        private static Canvas popupCanvas;
        private static bool initialized = false;
        
        // Text appearance properties
        private static float fontSize = 72f;
        private static bool useBoldText = true;
        private static bool useOutline = true;
        private static bool useShadow = true;
        private static Color outlineColor = Color.black;
        private static float outlineThickness = 0.2f;
        private static Color shadowColor = new Color(0, 0, 0, 0.5f);
        private static Vector2 shadowOffset = new Vector2(1f, -1f);
        
        #endregion

        #region Initialization
        
        // Static constructor to ensure system is initialized
        static ScoreUtility()
        {
            Initialize();
        }
        
        /// <summary>
        /// Initializes the score utility system
        /// </summary>
        public static void Initialize()
        {
            if (initialized) return;
            
            Debug.Log("ScoreUtility: Initializing system");
            
            // Create canvas for popups
            CreatePopupCanvas();
            
            initialized = true;
        }
        
        private static void CreatePopupCanvas()
        {
            if (popupCanvas != null) return;
            
            // Check for existing canvas
            Canvas[] existingCanvases = Object.FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in existingCanvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    popupCanvas = canvas;
                    Debug.Log("ScoreUtility: Using existing canvas");
                    return;
                }
            }
            
            // Create canvas if none exists
            GameObject canvasObj = new GameObject("ScorePopupCanvas");
            Object.DontDestroyOnLoad(canvasObj);
            
            popupCanvas = canvasObj.AddComponent<Canvas>();
            popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            popupCanvas.sortingOrder = 32767; // Ensure it's on top
            
            // Add required components
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("ScoreUtility: Created popup canvas");
        }
        
        #endregion

        #region Score Operations
        
        /// <summary>
        /// Add points to the player's score
        /// </summary>
        public static void AddPoints(int points)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(points);
            }
            else
            {
                Debug.LogError("ScoreUtility: ScoreManager instance not found!");
            }
        }
        
        /// <summary>
        /// Gets the current score
        /// </summary>
        public static int GetScore()
        {
            return ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        }
        
        /// <summary>
        /// Gets the current high score
        /// </summary>
        public static int GetHighScore()
        {
            return ScoreManager.Instance != null ? ScoreManager.Instance.HighScore : 0;
        }
        
        #endregion

        #region Popup Display
        
        /// <summary>
        /// Configure the appearance settings for popups
        /// </summary>
        public static void SetTextProperties(float size, bool bold = true, bool outline = true, bool shadow = true)
        {
            fontSize = size;
            useBoldText = bold;
            useOutline = outline;
            useShadow = shadow;
            Debug.Log($"ScoreUtility: Text properties set - Size: {size}, Bold: {bold}, Outline: {outline}, Shadow: {shadow}");
        }
        
        /// <summary>
        /// Show a score popup at a specific screen position
        /// </summary>
        public static void ShowPopupAtScreenPosition(int points, Vector2 screenPosition, string text = null)
        {
            // Make sure we're initialized
            if (!initialized) Initialize();
            
            if (popupCanvas == null)
            {
                Debug.LogError("ScoreUtility: Canvas is null. Cannot show popup.");
                return;
            }
            
            // Create popup
            GameObject popup = new GameObject("ScorePopup");
            popup.transform.SetParent(popupCanvas.transform, false);
            
            // Setup transform
            RectTransform rt = popup.AddComponent<RectTransform>();
            rt.position = screenPosition;
            
            // Add canvas group for fading
            CanvasGroup cg = popup.AddComponent<CanvasGroup>();
            
            // Create text object
            GameObject textObj = new GameObject("PopupText");
            textObj.transform.SetParent(popup.transform, false);
            
            // Setup text
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.fontSize = fontSize;
            tmpText.alignment = TextAlignmentOptions.Center;
            
            // CRITICAL FIX: Always use a simple format for the score
            // COMPLETELY IGNORE the text parameter - only show the score value with + sign
            tmpText.text = points >= 0 ? $"+{points}" : points.ToString();
            
            // Apply bold if needed
            if (useBoldText)
            {
                tmpText.fontStyle = FontStyles.Bold;
            }
            
            // Apply outline if needed
            if (useOutline)
            {
                tmpText.outlineWidth = outlineThickness;
                tmpText.outlineColor = outlineColor;
            }
            
            // Apply shadow if needed
            if (useShadow)
            {
                Shadow shadow = textObj.AddComponent<Shadow>();
                shadow.effectColor = shadowColor;
                shadow.effectDistance = shadowOffset;
            }
            
            Color textColor = points >= 0 ? 
                new Color(0.2f, 1f, 0.2f) : // Positive color
                new Color(1f, 0.2f, 0.2f);  // Negative color
            
            tmpText.color = textColor;
            
            // Setup text rect transform
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0.5f, 0.5f);
            textRt.anchorMax = new Vector2(0.5f, 0.5f);
            textRt.sizeDelta = new Vector2(300, 120); // Larger size for better readability
            
            // Start animation
            Debug.Log($"ScoreUtility: Created popup with text '{text}' at {screenPosition}");
            PopupAnimator animator = popup.AddComponent<PopupAnimator>();
            animator.StartAnimation();
        }
        
        /// <summary>
        /// Show a score popup at a world position
        /// </summary>
        public static void ShowPopup(int points, Vector3 worldPosition, string text = null)
        {
            if (Camera.main == null)
            {
                Debug.LogError("ScoreUtility: No main camera found!");
                return;
            }
            
            // Convert to screen position
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            ShowPopupAtScreenPosition(points, screenPosition, text);
        }
        
        /// <summary>
        /// Show a score popup at the current mouse/touch position
        /// </summary>
        public static void ShowPopupAtInput(int points, string text = null)
        {
            // Get screen position from input
            Vector2 screenPos = Input.mousePosition;
            if (Input.touchCount > 0)
            {
                screenPos = Input.GetTouch(0).position;
            }
            
            // Show popup at screen position
            ShowPopupAtScreenPosition(points, screenPos, text);
        }
        
        /// <summary>
        /// Add points and show a popup at a world position
        /// </summary>
        public static void AddPointsWithPopup(int points, Vector3 worldPosition, string text = null)
        {
            AddPoints(points);
            ShowPopup(points, worldPosition, text);
        }
        
        /// <summary>
        /// Add points and show a popup at the input position
        /// </summary>
        public static void AddPointsWithPopupAtInput(int points, string text = null)
        {
            AddPoints(points);
            ShowPopupAtInput(points, text);
        }
        
        /// <summary>
        /// Shows multiple test popups for debugging
        /// </summary>
        public static void ShowTestPopups(int count = 5)
        {
            for (int i = 0; i < count; i++)
            {
                float x = Screen.width * 0.5f + Random.Range(-200f, 200f);
                float y = Screen.height * 0.5f + Random.Range(-100f, 100f);
                Vector2 pos = new Vector2(x, y);
                
                int points = Random.Range(10, 1000);
                ShowPopupAtScreenPosition(points, pos);
            }
            
            Debug.Log($"ScoreUtility: Showed {count} test popups");
        }
        
        #endregion
        
        #region Helper Classes
        
        /// <summary>
        /// Controller for popup animation
        /// </summary>
        private class PopupAnimator : MonoBehaviour
        {
            private float duration = 1.5f;
            private float moveDistance = 100f;
            private Vector2 startPos;
            private CanvasGroup canvasGroup;
            
            public void StartAnimation()
            {
                canvasGroup = GetComponent<CanvasGroup>();
                RectTransform rt = GetComponent<RectTransform>();
                startPos = rt != null ? rt.anchoredPosition : Vector2.zero;
                StartCoroutine(AnimatePopup());
            }
            
            private IEnumerator AnimatePopup()
            {
                float elapsed = 0f;
                RectTransform rt = GetComponent<RectTransform>();
                Vector2 targetPos = startPos + new Vector2(0, moveDistance);
                
                // Scale animation keyframes
                Keyframe[] scaleKeys = new Keyframe[] {
                    new Keyframe(0, 0.5f),
                    new Keyframe(0.15f, 1.2f),
                    new Keyframe(0.3f, 1.0f),
                    new Keyframe(0.7f, 1.0f),
                    new Keyframe(1.0f, 0.5f)
                };
                
                AnimationCurve scaleCurve = new AnimationCurve(scaleKeys);
                AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
                
                while (elapsed < duration)
                {
                    // Update position, scale, and alpha
                    float t = elapsed / duration;
                    
                    // Position
                    rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                    
                    // Scale
                    float scale = scaleCurve.Evaluate(t);
                    transform.localScale = new Vector3(scale, scale, scale);
                    
                    // Alpha
                    canvasGroup.alpha = fadeCurve.Evaluate(t);
                    
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                
                // Clean up
                Destroy(gameObject);
            }
        }
        
        #endregion
    }
}
