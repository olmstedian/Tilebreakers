using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tilebreakers.Core;
using System.Reflection;
using System;
using System.Collections;

namespace Tilebreakers.UI
{
    /// <summary>
    /// Handles the visual display of scores in the UI
    /// </summary>
    [AddComponentMenu("Tilebreakers/UI/Score Display")]
    public class ScoreDisplayUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI currentScoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private Transform scorePopupContainer; // This is the "ScorePopup Parent"
        [SerializeField] private GameObject scorePopupPrefab;

        [Header("Animation Settings")]
        [SerializeField] private bool animateScoreChanges = true;
        [SerializeField] private float scoreAnimationDuration = 0.5f;
        [SerializeField] private AnimationCurve scoreAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private Color positiveScoreColor = new Color(0.2f, 1f, 0.2f);
        [SerializeField] private Color negativeScoreColor = new Color(1f, 0.2f, 0.2f);

        // Tracking variables for score animations
        private int displayedScore = 0;
        private int targetScore = 0;
        private float animationTimer = 0f;
        private bool animatingScore = false;

        private void Start()
        {
            // Initialize score display
            UpdateHighScoreDisplay();
            
            if (ScoreManager.Instance != null)
            {
                // Initialize with current score
                displayedScore = ScoreManager.Instance.Score;
                targetScore = displayedScore;
                UpdateScoreDisplay(displayedScore);
                
                // Subscribe to score change events
                ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
                ScoreManager.Instance.OnHighScoreChanged += HandleHighScoreChanged;
            }
            else
            {
                Debug.LogError("ScoreDisplayUI: ScoreManager instance not found!");
            }

            // Ensure we have a container for popups
            if (scorePopupContainer == null)
            {
                Debug.LogWarning("ScoreDisplayUI: No popup container assigned. Creating one.");
                GameObject container = new GameObject("ScorePopupContainer");
                container.transform.SetParent(transform);
                RectTransform rt = container.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                scorePopupContainer = container.transform;
            }

            // Log setup completion for debugging
            Debug.Log("ScoreDisplayUI: Initialization complete. Ready to show score updates.");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
                ScoreManager.Instance.OnHighScoreChanged -= HandleHighScoreChanged;
            }
        }

        private void Update()
        {
            // Animate score changes
            if (animatingScore)
            {
                animationTimer += Time.deltaTime;
                float progress = animationTimer / scoreAnimationDuration;
                
                if (progress >= 1f)
                {
                    // Animation complete
                    animatingScore = false;
                    UpdateScoreDisplay(targetScore);
                }
                else
                {
                    // Interpolate between displayed score and target score
                    float curveValue = scoreAnimationCurve.Evaluate(progress);
                    int currentValue = Mathf.RoundToInt(Mathf.Lerp(displayedScore, targetScore, curveValue));
                    UpdateScoreDisplay(currentValue);
                }
            }
        }

        /// <summary>
        /// Handles score change events from ScoreManager
        /// </summary>
        public void HandleScoreChanged(int newScore)
        {
            if (animateScoreChanges && gameObject.activeInHierarchy)
            {
                // Start new animation
                displayedScore = currentScoreText != null ? 
                    int.TryParse(currentScoreText.text.Replace("Score: ", ""), out int current) ? current : 0 : 0;
                targetScore = newScore;
                animationTimer = 0f;
                animatingScore = true;
            }
            else
            {
                // Update immediately without animation
                UpdateScoreDisplay(newScore);
            }
        }

        /// <summary>
        /// Handles high score change events from ScoreManager
        /// </summary>
        public void HandleHighScoreChanged(int newHighScore)
        {
            UpdateHighScoreDisplay();
        }

        /// <summary>
        /// Updates the current score display
        /// </summary>
        public void UpdateScoreDisplay(int score)
        {
            if (currentScoreText != null)
            {
                currentScoreText.text = $"Score: {score}";
            }
        }

        /// <summary>
        /// Updates the high score display
        /// </summary>
        public void UpdateHighScoreDisplay()
        {
            if (highScoreText != null && ScoreManager.Instance != null)
            {
                highScoreText.text = $"High Score: {ScoreManager.Instance.HighScore}";
            }
        }

        /// <summary>
        /// Spawns a score popup at the specified position
        /// </summary>
        public void SpawnScorePopup(int points, Vector2 worldPosition, string text = null)
        {
            // Debug logs to track calls to this method
            Debug.Log($"ScoreDisplayUI: Attempting to spawn score popup for {points} points at {worldPosition}");
            
            if (scorePopupPrefab == null)
            {
                Debug.LogError("ScoreDisplayUI: Score popup prefab is not assigned!");
                CreateDefaultScorePopupPrefab(); // Try to create a default prefab
                if (scorePopupPrefab == null) return; // Exit if still null
            }
            
            if (scorePopupContainer == null)
            {
                Debug.LogWarning("ScoreDisplayUI: Score popup container is not assigned! Creating one.");
                GameObject container = new GameObject("ScorePopupContainer");
                container.transform.SetParent(transform, false);
                RectTransform rt = container.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                scorePopupContainer = container.transform;
            }

            // Additional validation
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("ScoreDisplayUI: GameObject is inactive, cannot spawn popup");
                return;
            }

            // ALWAYS show only the points value, ignore any custom text
            text = points >= 0 ? $"+{points}" : points.ToString();

            // Convert world position to screen position
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            
            // Create score popup
            GameObject popupObject = Instantiate(scorePopupPrefab, scorePopupContainer);
            popupObject.SetActive(true); // Ensure the popup is active
            
            // Set the position
            RectTransform popupRect = popupObject.GetComponent<RectTransform>();
            if (popupRect != null)
            {
                popupRect.position = screenPosition;
            }
            else
            {
                Debug.LogWarning("ScoreDisplayUI: Popup object has no RectTransform, adding one");
                popupRect = popupObject.AddComponent<RectTransform>();
                popupRect.position = screenPosition;
            }
            
            // Get the ScorePopup component
            ScorePopup popup = popupObject.GetComponent<ScorePopup>();
            
            if (popup != null)
            {
                // Set popup text, value, and color
                Color textColor = points >= 0 ? positiveScoreColor : negativeScoreColor;
                
                // Try to call SetValue using reflection to avoid compile errors
                try
                {
                    // First try direct method call
                    popup.SetValue(points, text, textColor);
                    Debug.Log($"ScoreDisplayUI: Spawned popup with text: {text}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"ScoreDisplayUI: Could not call SetValue directly: {e.Message}");
                    
                    // Fallback to setting properties directly
                    ConfigurePopupManually(popup, points, text, textColor);
                }
            }
            else
            {
                Debug.LogError("ScoreDisplayUI: Instantiated prefab does not have a ScorePopup component!");
                // As a fallback, try to set text directly if there's a TextMeshProUGUI component
                TextMeshProUGUI textComponent = popupObject.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = string.IsNullOrEmpty(text) ? 
                        (points >= 0 ? $"+{points}" : points.ToString()) : text;
                    textComponent.color = points >= 0 ? positiveScoreColor : negativeScoreColor;
                    
                    // Start animation if there's a canvas group
                    CanvasGroup canvasGroup = popupObject.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = popupObject.AddComponent<CanvasGroup>();
                    }
                    
                    // Animate the popup
                    StartCoroutine(AnimatePopupFallback(popupObject, canvasGroup));
                    
                    Debug.Log("ScoreDisplayUI: Set text directly on TextMeshProUGUI component as fallback");
                }
            }
        }
        
        /// <summary>
        /// Configure a popup manually when SetValue method isn't available
        /// </summary>
        private void ConfigurePopupManually(ScorePopup popup, int points, string text, Color textColor)
        {
            // Try to find and set the scoreText field using reflection
            GameObject popupObj = popup.gameObject;
            TextMeshProUGUI textComponent = popupObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                // ALWAYS show only the points value, ignore any custom text
                textComponent.text = points >= 0 ? $"+{points}" : points.ToString();
                
                // Set color
                textComponent.color = textColor;
                
                Debug.Log($"ScoreDisplayUI: Manually configured popup with text: {textComponent.text}");
                
                // Try to start animation through reflection
                try
                {
                    var methodInfo = popup.GetType().GetMethod("AnimatePopup", 
                        BindingFlags.Instance | BindingFlags.NonPublic);
                        
                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(popup, null);
                    }
                    else
                    {
                        // Use our own animation as fallback
                        CanvasGroup canvasGroup = popupObj.GetComponent<CanvasGroup>();
                        if (canvasGroup == null)
                        {
                            canvasGroup = popupObj.AddComponent<CanvasGroup>();
                        }
                        StartCoroutine(AnimatePopupFallback(popupObj, canvasGroup));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"ScoreDisplayUI: Could not start animation via reflection: {e.Message}");
                    
                    // Use our own animation as fallback
                    CanvasGroup canvasGroup = popupObj.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = popupObj.AddComponent<CanvasGroup>();
                    }
                    StartCoroutine(AnimatePopupFallback(popupObj, canvasGroup));
                }
            }
            else
            {
                Debug.LogError("ScoreDisplayUI: Could not find TextMeshProUGUI component in popup");
            }
        }
        
        /// <summary>
        /// Fallback animation coroutine for when we can't use ScorePopup's built-in animation
        /// </summary>
        private IEnumerator AnimatePopupFallback(GameObject popupObject, CanvasGroup canvasGroup)
        {
            float duration = 1.5f;
            float elapsed = 0f;
            float moveDistance = 100f;
            
            RectTransform rectTransform = popupObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = popupObject.AddComponent<RectTransform>();
            }
            
            Vector2 startPosition = rectTransform.anchoredPosition;
            Vector2 targetPosition = startPosition + Vector2.up * moveDistance;
            
            // Ensure popup is visible at the start
            canvasGroup.alpha = 1.0f;
            
            while (elapsed < duration)
            {
                float normalizedTime = elapsed / duration;
                
                // Update position
                rectTransform.anchoredPosition = Vector2.Lerp(
                    startPosition, targetPosition, normalizedTime);
                
                // Update scale - simple scale up then down
                float scale = 1.0f;
                if (normalizedTime < 0.15f)
                    scale = Mathf.Lerp(0.5f, 1.2f, normalizedTime / 0.15f);
                else if (normalizedTime < 0.3f)
                    scale = Mathf.Lerp(1.2f, 1.0f, (normalizedTime - 0.15f) / 0.15f);
                else if (normalizedTime > 0.7f)
                    scale = Mathf.Lerp(1.0f, 0.5f, (normalizedTime - 0.7f) / 0.3f);
                
                popupObject.transform.localScale = new Vector3(scale, scale, scale);
                
                // Update opacity - fade in then out
                if (normalizedTime < 0.3f)
                    canvasGroup.alpha = normalizedTime / 0.3f;
                else
                    canvasGroup.alpha = 1.0f - ((normalizedTime - 0.3f) / 0.7f);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Animation complete, destroy the popup
            Destroy(popupObject);
        }

        /// <summary>
        /// Creates a default score popup prefab if none exists
        /// </summary>
        public void CreateDefaultScorePopupPrefab()
        {
            // This can be called from the editor to create a default prefab
            GameObject popupObj = new GameObject("ScorePopupPrefab");
            
            // Add canvas components if needed
            popupObj.AddComponent<CanvasGroup>();
            popupObj.AddComponent<RectTransform>();
            
            // Add TextMeshPro component
            GameObject textObj = new GameObject("ScoreText");
            textObj.transform.SetParent(popupObj.transform, false);
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = "+100";
            tmpText.fontSize = 36;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = positiveScoreColor;
            
            // Size the text object
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(200, 80);
            
            // Add the ScorePopup component
            ScorePopup popup = popupObj.AddComponent<ScorePopup>();
            
            // Try to initialize the popup if SetValue exists
            try
            {
                popup.SetValue(100); // Initialize with default value
            }
            catch (Exception e)
            {
                Debug.LogWarning($"ScoreDisplayUI: Could not call SetValue on new popup: {e.Message}");
                // We'll initialize it when it's actually used
            }
            
            // Set as prefab
            scorePopupPrefab = popupObj;
            
            Debug.Log("ScoreDisplayUI: Created default score popup prefab");
        }

        #if UNITY_EDITOR
        // Helper method for editor script to find missing references
        public bool ValidateReferences()
        {
            bool valid = true;
            
            if (currentScoreText == null)
            {
                Debug.LogError("ScoreDisplayUI: Missing Current Score Text reference");
                valid = false;
            }
            
            if (highScoreText == null)
            {
                Debug.LogError("ScoreDisplayUI: Missing High Score Text reference");
                valid = false;
            }
            
            if (scorePopupContainer == null)
            {
                Debug.LogError("ScoreDisplayUI: Missing Score Popup Container reference");
                valid = false;
            }
            
            if (scorePopupPrefab == null)
            {
                Debug.LogError("ScoreDisplayUI: Missing Score Popup Prefab reference");
                valid = false;
            }
            
            return valid;
        }
        #endif
    }
}
