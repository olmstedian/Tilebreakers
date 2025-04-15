using UnityEngine;
using TMPro;
using Tilebreakers.Core;

namespace Tilebreakers.UI
{
    /// <summary>
    /// Manages the display of the current level description.
    /// Attach to a TextMeshProUGUI component that will show the level description.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LevelDescriptionDisplay : MonoBehaviour
    {
        [Header("Text Display")]
        [SerializeField] private bool showLevelNumber = true;
        [SerializeField] private bool showLevelName = true;
        [SerializeField] private bool showDescription = true;
        
        [Header("Formatting")]
        [SerializeField] private string formatString = "Level {0}: {1}\n{2}";
        [SerializeField] private Color levelNumberColor = new Color(1f, 0.8f, 0.4f); // Gold
        [SerializeField] private Color levelNameColor = new Color(1f, 1f, 0.8f); // Light yellow
        [SerializeField] private Color descriptionColor = new Color(0.9f, 0.9f, 0.9f); // Light gray
        
        // Reference to the text component
        private TextMeshProUGUI textComponent;
        
        private void Awake()
        {
            // Get the text component
            textComponent = GetComponent<TextMeshProUGUI>();
            
            // Set rich text to true to allow colored text
            if (textComponent)
            {
                textComponent.richText = true;
            }
        }
        
        private void Start()
        {
            // Update the text on start
            UpdateDescription();
            
            // Subscribe to level change events if your game has them
            // Example: GameEvents.OnLevelChanged.AddListener(UpdateDescription);
        }
        
        private void OnEnable()
        {
            // Update whenever this becomes visible
            UpdateDescription();
        }
        
        /// <summary>
        /// Updates the description text based on the current level
        /// </summary>
        public void UpdateDescription()
        {
            if (!textComponent) return;
            
            LevelData currentLevel = null;
            
            // Try to get the current level data from LevelManager
            if (LevelManager.Instance)
            {
                currentLevel = LevelManager.Instance.GetCurrentLevelData();
            }
            
            // If we have level data, format and display it
            if (currentLevel != null)
            {
                string formattedText = "";
                
                if (showLevelNumber && showLevelName && showDescription)
                {
                    formattedText = string.Format(formatString, 
                        ColorText(currentLevel.levelNumber.ToString(), levelNumberColor),
                        ColorText(currentLevel.levelName, levelNameColor),
                        ColorText(currentLevel.levelDescription, descriptionColor));
                }
                else
                {
                    // Create custom format based on what's enabled
                    if (showLevelNumber)
                    {
                        formattedText += ColorText("Level " + currentLevel.levelNumber, levelNumberColor);
                        
                        if (showLevelName)
                            formattedText += ": ";
                        else if (showDescription)
                            formattedText += "\n";
                    }
                    
                    if (showLevelName)
                    {
                        formattedText += ColorText(currentLevel.levelName, levelNameColor);
                        
                        if (showDescription)
                            formattedText += "\n";
                    }
                    
                    if (showDescription)
                    {
                        formattedText += ColorText(currentLevel.levelDescription, descriptionColor);
                    }
                }
                
                textComponent.text = formattedText;
            }
            else
            {
                // Fallback for when level data isn't available
                if (LevelManager.Instance?.IsInfiniteMode ?? false)
                {
                    textComponent.text = "Infinite Mode\nPlay until you run out of moves!";
                }
                else
                {
                    textComponent.text = "Level description not available";
                }
            }
        }
        
        /// <summary>
        /// Helper method to color text using rich text tags
        /// </summary>
        private string ColorText(string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }
        
        /// <summary>
        /// Force update the description - can be called after level changes
        /// </summary>
        [ContextMenu("Update Description")]
        public void ForceUpdateDescription()
        {
            UpdateDescription();
        }
    }
}
