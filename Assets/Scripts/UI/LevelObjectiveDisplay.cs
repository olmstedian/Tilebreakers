using UnityEngine;
using TMPro;
using Tilebreakers.Core;

namespace Tilebreakers.UI
{
    /// <summary>
    /// Component that displays level objective text
    /// Attach to a TextMeshProUGUI object that will show level objectives
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LevelObjectiveDisplay : MonoBehaviour
    {
        [Header("Update Settings")]
        [SerializeField] private bool updateOnEnable = true;
        [SerializeField] private bool updateOnLevelChange = true;
        
        [Header("Text Appearance")]
        [SerializeField] private bool applyColorGradient = true;
        [SerializeField] private Color objectiveColor = new Color(0.2f, 0.8f, 1f);
        [SerializeField] private Color valueColor = new Color(1f, 0.8f, 0.2f);
        
        private TextMeshProUGUI textComponent;
        
        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.richText = true; // Ensure rich text is enabled
            }
        }
        
        private void OnEnable()
        {
            if (updateOnEnable)
            {
                UpdateObjectiveText();
            }
        }
        
        private void Start()
        {
            // Initial update
            UpdateObjectiveText();
            
            // Subscribe to events if needed
            if (updateOnLevelChange)
            {
                // Example: Subscribe to level change event if your game has one
                // GameEvents.OnLevelChanged.AddListener(UpdateObjectiveText);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (updateOnLevelChange)
            {
                // Example: Unsubscribe from level change event if needed
                // GameEvents.OnLevelChanged.RemoveListener(UpdateObjectiveText);
            }
        }
        
        /// <summary>
        /// Updates the objective text based on current level data
        /// </summary>
        [ContextMenu("Update Objective Text")]
        public void UpdateObjectiveText()
        {
            if (textComponent == null) return;
            
            LevelData currentLevelData = null;
            if (LevelManager.Instance != null)
            {
                currentLevelData = LevelManager.Instance.GetCurrentLevelData();
            }
            
            // If we have level data, use its objective text
            if (currentLevelData != null)
            {
                // Check if there's a custom objective text set
                if (!string.IsNullOrEmpty(currentLevelData.levelObjectiveText))
                {
                    // Use the custom text as-is (it might already contain rich text tags)
                    textComponent.text = currentLevelData.levelObjectiveText;
                }
                else
                {
                    // Generate a default objective text with formatting
                    if (applyColorGradient)
                    {
                        string objectiveHtml = ColorUtility.ToHtmlStringRGB(objectiveColor);
                        string valueHtml = ColorUtility.ToHtmlStringRGB(valueColor);
                        
                        if (currentLevelData.scoreObjective)
                        {
                            textComponent.text = $"<color=#{objectiveHtml}>Score</color> <color=#{valueHtml}>{currentLevelData.scoreTarget}</color> <color=#{objectiveHtml}>points</color>";
                            
                            if (currentLevelData.movesTarget > 0)
                            {
                                textComponent.text += $" <color=#{objectiveHtml}>in</color> <color=#{valueHtml}>{currentLevelData.movesTarget}</color> <color=#{objectiveHtml}>moves</color>";
                            }
                        }
                        else if (currentLevelData.movesTarget > 0)
                        {
                            textComponent.text = $"<color=#{objectiveHtml}>Survive</color> <color=#{valueHtml}>{currentLevelData.movesTarget}</color> <color=#{objectiveHtml}>moves</color>";
                        }
                    }
                    else
                    {
                        // Plain text version
                        textComponent.text = currentLevelData.GetObjectiveText();
                    }
                }
            }
            else if (LevelManager.Instance != null && LevelManager.Instance.IsInfiniteMode)
            {
                // Infinite mode
                textComponent.text = "Survive as long as possible!";
            }
            else
            {
                // Fallback text
                textComponent.text = "Complete the level objective!";
            }
            
            Debug.Log($"LevelObjectiveDisplay: Updated text to: {textComponent.text}");
        }
        
        /// <summary>
        /// Sets custom objective text directly
        /// </summary>
        public void SetObjectiveText(string text)
        {
            if (textComponent != null)
            {
                textComponent.text = text;
            }
        }
    }
}
