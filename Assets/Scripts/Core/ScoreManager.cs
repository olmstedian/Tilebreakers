using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Tilebreakers.UI;

namespace Tilebreakers.Core
{
    [AddComponentMenu("Tilebreakers/Core/Score Manager")]
    /// <summary>
    /// Manages game score tracking, updates, and display
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        // Events for UI to listen to
        public event Action<int> OnScoreChanged;
        public event Action<int> OnHighScoreChanged;

        [Header("Score Settings")]
        [SerializeField] private int initialScore = 0;
        [SerializeField] private bool saveHighScore = true;
        [SerializeField] private string highScoreKey = "HighScore";

        [Header("Score Bonus Settings")]
        [SerializeField] private int specialTileBonus = 50;
        // [SerializeField] private int comboMultiplier = 2; // Remove or comment out this unused field

        [Header("Popup Settings")]
        [SerializeField] private float popupFontSize = 72f;
        [SerializeField] private bool showPopupsOnScoreChanges = true;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = true;

        // Internal score values
        private int score = 0;
        private int highScore = 0;
        private int combo = 0;
        private float lastScoreTime = 0;
        private float comboTimeWindow = 1.5f;

        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Transform scorePopupParent;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadHighScore();
                MigrateScoreData(); 
                score = initialScore;

                // Configure popup text size
                ScoreUtility.SetTextProperties(popupFontSize);

                if (enableDebugLogging)
                    Debug.Log($"ScoreManager: Initialized with score {score} and high score {highScore}");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Public properties to get current scores
        public int Score => score;
        public int HighScore => highScore;
        public int CurrentCombo => combo;

        /// <summary>
        /// Adds points to the current score and updates the high score if necessary.
        /// </summary>
        public void AddScore(int points)
        {
            // Check for combo (repeated scoring within the time window)
            if (Time.time - lastScoreTime < comboTimeWindow)
            {
                combo++;

                // Apply combo multiplier for consecutive actions
                points = Mathf.RoundToInt(points * (1 + (combo * 0.1f)));

                if (enableDebugLogging)
                    Debug.Log($"ScoreManager: Combo x{combo}! Points increased to {points}");
            }
            else
            {
                combo = 0;
            }

            // Update last score time
            lastScoreTime = Time.time;

            int oldScore = score;
            score += points;

            // Update high score if needed
            if (score > highScore)
            {
                int oldHighScore = highScore;
                highScore = score;
                if (saveHighScore)
                    SaveHighScore();

                // Trigger high score changed event
                OnHighScoreChanged?.Invoke(highScore);

                if (enableDebugLogging)
                    Debug.Log($"ScoreManager: New high score! {oldHighScore} -> {highScore}");
            }

            // Trigger score changed event
            OnScoreChanged?.Invoke(score);

            if (enableDebugLogging)
                Debug.Log($"ScoreManager: Added {points} points. New score: {score}");
                
            // IMPORTANT: Removing this section to prevent duplicate popups
            // The popup will be shown by the calling method instead
            /*
            // Show popup if enabled
            if (showPopupsOnScoreChanges)
            {
                // Place popup at top center for better visibility
                Vector2 topCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.85f);

                // Only show +points (or -points) in popup, do not append (xCombo) or duplicate
                string popupText = points >= 0 ? $"+{points}" : points.ToString();

                ScoreUtility.ShowPopupAtScreenPosition(points, topCenter, popupText);
            }
            */
            
            UpdateDisplay();
        }

        /// <summary>
        /// Adds a bonus score for activating a special tile.
        /// </summary>
        public void AddSpecialTileBonus()
        {
            AddScore(specialTileBonus);
            if (enableDebugLogging)
                Debug.Log($"ScoreManager: Added special tile bonus of {specialTileBonus} points.");
        }

        /// <summary>
        /// Resets the current score to zero.
        /// </summary>
        public void ResetScore()
        {
            score = initialScore;
            combo = 0;
            OnScoreChanged?.Invoke(score);
            UpdateDisplay();
            if (enableDebugLogging)
                Debug.Log($"ScoreManager: Score reset to {initialScore}.");
        }

        /// <summary>
        /// Updates the score display. 
        /// </summary>
        private void UpdateDisplay()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        /// <summary>
        /// Saves the high score using PlayerPrefs.
        /// </summary>
        private void SaveHighScore()
        {
            if (!saveHighScore) return;

            PlayerPrefs.SetInt(highScoreKey, highScore);
            PlayerPrefs.Save();
            if (enableDebugLogging)
                Debug.Log($"ScoreManager: High score saved: {highScore}");
        }

        /// <summary>
        /// Loads the high score using PlayerPrefs.
        /// </summary>
        private void LoadHighScore()
        {
            if (!saveHighScore) return;

            highScore = PlayerPrefs.GetInt(highScoreKey, 0);
            if (enableDebugLogging)
                Debug.Log($"ScoreManager: High score loaded: {highScore}");
        }

        /// <summary>
        /// Handles migration or data conversion from previous score files.
        /// </summary>
        private void MigrateScoreData()
        {
            // Check for old player prefs keys and migrate them if needed
            if (PlayerPrefs.HasKey("OldHighScore"))
            {
                int oldHighScore = PlayerPrefs.GetInt("OldHighScore");
                if (PlayerPrefs.HasKey(highScoreKey))
                {
                    int currentHighScore = PlayerPrefs.GetInt(highScoreKey);
                    if (oldHighScore > currentHighScore)
                    {
                        PlayerPrefs.SetInt(highScoreKey, oldHighScore);
                        PlayerPrefs.Save();
                        Debug.Log($"ScoreManager: Migrated old high score: {oldHighScore}");
                    }
                }
                else
                {
                    PlayerPrefs.SetInt(highScoreKey, oldHighScore);
                    PlayerPrefs.Save();
                    Debug.Log($"ScoreManager: Migrated old high score: {oldHighScore}");
                }
                
                // Remove old key after migration
                PlayerPrefs.DeleteKey("OldHighScore");
            }
        }

        /// <summary>
        /// Displays a score popup. Optionally, a custom text can be provided.
        /// </summary>
        public static void ShowScorePopup(int points, string text = null)
        {
            // Show popup at top center of the screen for better visibility
            // Add slight horizontal randomization for more natural appearance
            float randomX = UnityEngine.Random.Range(-50f, 50f);
            Vector2 topCenter = new Vector2(Screen.width * 0.5f + randomX, Screen.height * 0.85f);
            
            // Pass null for text to ensure ScoreUtility uses its own formatting (just +points)
            ScoreUtility.ShowPopupAtScreenPosition(points, topCenter, null);
        }

        /// <summary>
        /// Displays a score popup at a specific world position.
        /// </summary>
        public static void ShowScorePopupAtPosition(int points, Vector2 worldPosition, string text = null)
        {
            // Ignore worldPosition, always show at top center for clarity
            float randomX = UnityEngine.Random.Range(-50f, 50f);
            Vector2 topCenter = new Vector2(Screen.width * 0.5f + randomX, Screen.height * 0.85f);
            
            // Pass null for text to ensure ScoreUtility uses its own formatting (just +points)
            ScoreUtility.ShowPopupAtScreenPosition(points, topCenter, null);
        }

        /// <summary>
        /// Adds score for destroying multiple tiles at once.
        /// </summary>
        public void AddDestructionScore(int baseScore, int tileCount)
        {
            // Apply multiplier based on how many tiles were destroyed at once
            float multiplier = 1.0f;
            if (tileCount > 1)
            {
                // Higher bonus for destroying multiple tiles at once
                multiplier = 1.0f + (tileCount - 1) * 0.5f;
            }

            int scoreToAdd = Mathf.RoundToInt(baseScore * multiplier);

            // CRITICAL FIX: We need to add score WITHOUT showing popup in AddScore
            AddScoreWithoutPopup(scoreToAdd);

            Debug.Log($"ScoreManager: Added {scoreToAdd} points for destroying {tileCount} tiles (base: {baseScore}, multiplier: {multiplier:F1}x)");

            // Only show +points (or -points) in popup, not combo text
            ShowScorePopup(scoreToAdd);
        }

        /// <summary>
        /// Adds score for splitting a high-value tile
        /// </summary>
        public void AddSplitScore(int originalValue)
        {
            // Award bonus points based on the original value of the split tile
            int bonusPoints = Mathf.RoundToInt(originalValue * 0.5f);

            // CRITICAL FIX: We need to add score WITHOUT showing popup in AddScore
            AddScoreWithoutPopup(bonusPoints);

            Debug.Log($"ScoreManager: Added {bonusPoints} bonus points for splitting tile with value {originalValue}");

            // Only show +points (or -points) in popup, not extra text
            ShowScorePopup(bonusPoints);
        }

        /// <summary>
        /// Adds points without showing a popup
        /// </summary>
        public void AddScoreWithoutPopup(int points)
        {
            // Calculate combo bonuses
            if (Time.time - lastScoreTime < comboTimeWindow)
            {
                combo++;
                points = Mathf.RoundToInt(points * (1 + (combo * 0.1f)));
                if (enableDebugLogging)
                    Debug.Log($"ScoreManager: Combo x{combo}! Points increased to {points}");
            }
            else
            {
                combo = 0;
            }

            // Update last score time
            lastScoreTime = Time.time;

            // Update the score
            int oldScore = score;
            score += points;

            // Check high score
            if (score > highScore)
            {
                int oldHighScore = highScore;
                highScore = score;
                if (saveHighScore)
                    SaveHighScore();
                OnHighScoreChanged?.Invoke(highScore);
            }

            // Trigger score changed event
            OnScoreChanged?.Invoke(score);
            
            // Update the display
            UpdateDisplay();
        }

        /// <summary>
        /// Get the current score value (legacy API method)
        /// </summary>
        public int GetCurrentScore()
        {
            return score;
        }

        #if UNITY_EDITOR
        // Add reset high score method for editor only
        [ContextMenu("Reset High Score")]
        public void ResetHighScore()
        {
            highScore = 0;
            SaveHighScore();
            OnHighScoreChanged?.Invoke(highScore);
            Debug.Log("ScoreManager: High score reset to zero.");
        }
        
        [ContextMenu("Show Test Popups")]
        public void ShowTestPopups()
        {
            ScoreUtility.ShowTestPopups(5);
        }
        #endif
    }
}