using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

namespace Tilebreakers.UI
{
    /// <summary>
    /// Handles score popup animations and display with enhanced visual effects
    /// </summary>
    [AddComponentMenu("Tilebreakers/UI/Score Popup")]
    public class ScorePopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private float duration = 1.8f; // Slightly longer for enhanced animation
        [SerializeField] private float moveDistance = 120f; // Increased for more visible motion
        
        // Enhanced animation curves
        [SerializeField] private AnimationCurve fadeCurve = new AnimationCurve(
            new Keyframe(0, 0),
            new Keyframe(0.1f, 1),
            new Keyframe(0.7f, 1),
            new Keyframe(1.0f, 0)
        );
        
        [SerializeField] private AnimationCurve scaleCurve = new AnimationCurve(
            new Keyframe(0, 0.3f),
            new Keyframe(0.15f, 1.4f), // Bigger pop effect
            new Keyframe(0.3f, 1.1f),
            new Keyframe(0.7f, 1.1f),
            new Keyframe(1.0f, 0.7f)
        );
        
        // Improved text visibility settings
        [SerializeField] private float fontSize = 86f; // Larger text for better visibility
        [SerializeField] private bool enableOutline = true;
        [SerializeField] private Color outlineColor = new Color(0.1f, 0.1f, 0.1f, 1f); // Darker outline
        [SerializeField] private float outlineThickness = 0.25f; // Thicker outline
        [SerializeField] private bool enableShadow = true;
        [SerializeField] private Color shadowColor = new Color(0, 0, 0, 0.6f); // Darker shadow
        [SerializeField] private Vector2 shadowOffset = new Vector2(2, -2); // More visible shadow
        
        // Animation enhancement
        [SerializeField] private bool useRotationEffect = true;
        [SerializeField] private float rotationAmount = 5f; // Small rotation for added visual interest
        
        // Optional background for better visibility
        [SerializeField] private bool useBackground = true;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.2f);
        private Image backgroundImage;

        private void Awake()
        {
            // Auto-find or create the text component if not assigned
            if (scoreText == null) 
            {
                scoreText = GetComponentInChildren<TextMeshProUGUI>();
                
                if (scoreText == null)
                {
                    // Create a background panel first if enabled
                    if (useBackground)
                    {
                        GameObject bgObj = new GameObject("Background");
                        bgObj.transform.SetParent(transform, false);
                        backgroundImage = bgObj.AddComponent<Image>();
                        backgroundImage.color = backgroundColor;
                        
                        // Make the background slightly larger than the text
                        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
                        bgRt.anchorMin = new Vector2(0.5f, 0.5f);
                        bgRt.anchorMax = new Vector2(0.5f, 0.5f);
                        bgRt.sizeDelta = new Vector2(340, 140);
                        
                        // Add rounded corners
                        backgroundImage.sprite = Resources.Load<Sprite>("UI/RoundedRect");
                        if (backgroundImage.sprite == null)
                        {
                            // If sprite not found, try to use a built-in sprite or create one
                            backgroundImage.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                            backgroundImage.type = Image.Type.Sliced;
                        }
                    }
                    
                    // Create text object
                    GameObject textObj = new GameObject("ScoreText");
                    textObj.transform.SetParent(transform, false);
                    scoreText = textObj.AddComponent<TextMeshProUGUI>();
                    scoreText.text = "+100";
                    scoreText.fontSize = fontSize;
                    scoreText.alignment = TextAlignmentOptions.Center;
                    scoreText.color = new Color(0.2f, 1f, 0.2f);
                    
                    // Make text bold
                    scoreText.fontStyle = FontStyles.Bold;
                    
                    // Add outline for better visibility
                    if (enableOutline)
                    {
                        scoreText.outlineWidth = outlineThickness;
                        scoreText.outlineColor = outlineColor;
                    }
                    
                    // Add drop shadow if enabled
                    if (enableShadow)
                    {
                        Shadow shadow = textObj.AddComponent<Shadow>();
                        shadow.effectColor = shadowColor;
                        shadow.effectDistance = shadowOffset;
                    }
                    
                    // Set up the rect transform
                    RectTransform textRect = textObj.GetComponent<RectTransform>();
                    textRect.anchorMin = new Vector2(0.5f, 0.5f);
                    textRect.anchorMax = new Vector2(0.5f, 0.5f);
                    textRect.sizeDelta = new Vector2(320, 140); // Larger size to fit larger text
                }
            }
            else
            {
                // Ensure existing text is properly sized and styled
                scoreText.fontSize = fontSize;
                scoreText.fontStyle = FontStyles.Bold;
                
                if (enableOutline)
                {
                    scoreText.outlineWidth = outlineThickness;
                    scoreText.outlineColor = outlineColor;
                }
                
                // Add shadow if not present
                if (enableShadow && !scoreText.gameObject.GetComponent<Shadow>())
                {
                    Shadow shadow = scoreText.gameObject.AddComponent<Shadow>();
                    shadow.effectColor = shadowColor;
                    shadow.effectDistance = shadowOffset;
                }
            }
            
            // Add a canvas group if not present
            if (GetComponent<CanvasGroup>() == null)
            {
                gameObject.AddComponent<CanvasGroup>();
            }
            
            // Make sure we have a RectTransform
            if (GetComponent<RectTransform>() == null)
            {
                gameObject.AddComponent<RectTransform>();
            }
            
            Debug.Log("ScorePopup: Component initialized with enhanced visuals");
        }

        /// <summary>
        /// Sets the value and appearance of the score popup
        /// </summary>
        public void SetValue(int points, string customText = null, Color? color = null)
        {
            Debug.Log($"ScorePopup: Setting value to {points}");
            
            // Ensure we have a text component
            if (scoreText == null)
            {
                Awake(); // This will create required components
            }
            
            // Set text content
            if (scoreText != null)
            {
                // ALWAYS show only the plain +points format
                // Completely ignore any custom text that may duplicate the points
                scoreText.text = points >= 0 ? $"+{points}" : points.ToString();
                
                // Enhanced colors based on point value
                Color textColor;
                if (color.HasValue)
                {
                    textColor = color.Value;
                }
                else if (points > 0)
                {
                    // Positive value - green to yellow gradient based on value
                    if (points > 500)
                        textColor = new Color(1f, 0.9f, 0.2f); // Gold for high values
                    else if (points > 100)
                        textColor = new Color(0.3f, 1f, 0.3f); // Brighter green for medium values
                    else
                        textColor = new Color(0.2f, 0.9f, 0.2f); // Standard green for normal values
                }
                else if (points < 0)
                {
                    textColor = new Color(1f, 0.3f, 0.3f); // Red for negative values
                }
                else
                {
                    textColor = new Color(1f, 1f, 1f); // White for zero
                }
                
                scoreText.color = textColor;
                
                // Set font size with a slight random variation for visual interest
                float randomSizeVariation = Random.Range(-4f, 8f);
                scoreText.fontSize = fontSize + randomSizeVariation;
                
                // Apply font style - always bold for readability
                scoreText.fontStyle = FontStyles.Bold;
                
                // Apply outline for better visibility
                if (enableOutline)
                {
                    scoreText.outlineWidth = outlineThickness;
                    scoreText.outlineColor = outlineColor;
                }
            }
            else
            {
                Debug.LogError("ScorePopup: scoreText is null even after attempting creation!");
                return; // Exit if we still don't have a text component
            }
            
            // Start the enhanced animation
            StartCoroutine(AnimatePopup());
        }

        private IEnumerator AnimatePopup()
        {
            // Make sure we have a RectTransform to animate
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError("ScorePopup: Missing RectTransform for animation");
                Destroy(gameObject);
                yield break;
            }
            
            float elapsed = 0f;
            Vector2 startPosition = rectTransform.anchoredPosition;
            Vector2 targetPosition = startPosition + Vector2.up * moveDistance;
            
            // Get canvas group
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0; // Start invisible
            
            // Enhanced animation variables
            float startRotation = useRotationEffect ? Random.Range(-rotationAmount, rotationAmount) : 0f;
            float targetRotation = useRotationEffect ? Random.Range(-rotationAmount, rotationAmount) : 0f;
            
            // Create a small delay before starting to stagger multiple popups
            yield return new WaitForSeconds(0.02f);
            
            while (elapsed < duration)
            {
                float normalizedTime = elapsed / duration;
                
                // Position - add slight horizontal movement for more dynamic feel
                float horizontalOffset = Mathf.Sin(normalizedTime * 6) * 5f; // Subtle side-to-side motion
                Vector2 currentPos = Vector2.Lerp(startPosition, targetPosition, normalizedTime);
                currentPos.x += horizontalOffset;
                rectTransform.anchoredPosition = currentPos;
                
                // Scale with bounce effect
                float scale = scaleCurve.Evaluate(normalizedTime);
                transform.localScale = new Vector3(scale, scale, scale);
                
                // Opacity with hold time in the middle
                canvasGroup.alpha = fadeCurve.Evaluate(normalizedTime);
                
                // Rotation effect
                if (useRotationEffect)
                {
                    float rotation = Mathf.Lerp(startRotation, targetRotation, normalizedTime);
                    transform.rotation = Quaternion.Euler(0, 0, rotation);
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Animation complete, destroy the popup
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Creates a score popup at the specified position
        /// </summary>
        public static ScorePopup Create(Transform parent, Vector2 position, int points, string text = null, Color? color = null)
        {
            GameObject popupObject = new GameObject("ScorePopup");
            popupObject.transform.SetParent(parent, false);
            
            RectTransform rectTransform = popupObject.AddComponent<RectTransform>();
            rectTransform.position = position;
            
            ScorePopup popup = popupObject.AddComponent<ScorePopup>();
            popup.SetValue(points, text, color);
            
            return popup;
        }
    }
}
