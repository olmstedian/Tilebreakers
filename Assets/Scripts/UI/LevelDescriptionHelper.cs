using UnityEngine;
using TMPro;
using Tilebreakers.Core;

namespace Tilebreakers.UI
{
    /// <summary>
    /// Helper component to style and position the level description text.
    /// Attach to the Level Description Text GameObject.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LevelDescriptionHelper : MonoBehaviour
    {
        [Header("Appearance")]
        [SerializeField] private Color textColor = new Color(1f, 0.9f, 0.7f);
        [SerializeField] private bool enableOutline = true;
        [SerializeField] private Color outlineColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] private float outlineWidth = 0.2f;
        
        [Header("Content Formatting")]
        [SerializeField] private string prefix = "Level: ";
        [SerializeField] private string suffix = "";
        [SerializeField] private bool showLevelNumber = true;
        
        private TextMeshProUGUI textComponent;
        
        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            ApplyTextStyling();
        }
        
        private void Start()
        {
            // Update the text initially
            UpdateLevelDescription();
            
            // Optional: Subscribe to level change events if you have them
            // GameEvents.OnLevelChanged += UpdateLevelDescription;
        }
        
        private void OnDestroy()
        {
            // Optional: Unsubscribe from events if you subscribed
            // GameEvents.OnLevelChanged -= UpdateLevelDescription;
        }
        
        public void UpdateLevelDescription()
        {
            if (textComponent == null) return;
            
            string levelName = "Unknown Level";
            int levelNumber = 1;
            
            // Get level info from LevelManager if available
            if (LevelManager.Instance != null)
            {
                levelName = LevelManager.Instance.GetCurrentLevelName();
                levelNumber = LevelManager.Instance.GetCurrentLevelNumber();
                
                // Handle special case for infinite mode
                if (LevelManager.Instance.IsInfiniteMode)
                {
                    levelName = "Infinite Mode";
                    levelNumber = -1;
                }
            }
            
            // Format the text
            string displayText = prefix;
            
            if (showLevelNumber && levelNumber > 0)
            {
                displayText += levelNumber + " - ";
            }
            
            displayText += levelName + suffix;
            
            // Apply to the text component
            textComponent.text = displayText;
        }
        
        private void ApplyTextStyling()
        {
            if (textComponent == null) return;
            
            // Apply basic styling
            textComponent.color = textColor;
            textComponent.fontStyle = FontStyles.Bold;
            
            // Apply outline if enabled
            if (enableOutline)
            {
                textComponent.outlineWidth = outlineWidth;
                textComponent.outlineColor = outlineColor;
            }
        }
    }
}
