using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Tilebreakers.Core
{
    /// <summary>
    /// Bridge class for backward compatibility with code that uses ScorePopupBridge
    /// Forwards calls to ScoreUtility
    /// </summary>
    public static class ScorePopupBridge
    {
        // Initialization flag for compatibility
        private static bool isInitialized = false;

        /// <summary>
        /// Checks if the bridge has been initialized
        /// </summary>
        public static bool IsInitialized()
        {
            return isInitialized;
        }

        /// <summary>
        /// Initializes the popup system
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized) return;
            
            Debug.Log("ScorePopupBridge: Initializing the system");
            
            // Forward to ScoreUtility
            ScoreUtility.Initialize();
            
            isInitialized = true;
            
            Debug.Log("ScorePopupBridge: System initialized successfully");
        }

        /// <summary>
        /// Shows a score popup at a screen position.
        /// Places the popup near the top center of the screen for better visibility.
        /// </summary>
        public static void ShowPopupAtScreenPosition(int points, Vector2 screenPosition, string text = null)
        {
            // Place popup at top center, offset down a bit
            Vector2 topCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.85f);
            
            // Always force the text to be just the point value - ignore any custom text completely
            string pointsStr = points >= 0 ? $"+{points}" : points.ToString();
            ScoreUtility.ShowPopupAtScreenPosition(points, topCenter, pointsStr);
        }

        /// <summary>
        /// Shows a score popup at a world position.
        /// Places the popup near the top center of the screen for better visibility.
        /// </summary>
        public static void ShowPopup(int points, Vector3 worldPosition, string text = null)
        {
            // Ignore worldPosition, always show at top center for clarity
            Vector2 topCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.85f);
            
            // Always force the text to be just the point value - ignore any custom text completely
            string pointsStr = points >= 0 ? $"+{points}" : points.ToString();
            ScoreUtility.ShowPopupAtScreenPosition(points, topCenter, pointsStr);
        }
        
        /// <summary>
        /// Set text appearance properties for score popups
        /// </summary>
        public static void SetTextProperties(float size, bool bold = true, bool outline = true, bool shadow = true)
        {
            // Forward to ScoreUtility
            ScoreUtility.SetTextProperties(size, bold, outline, shadow);
        }
        
        /// <summary>
        /// Shows a test popup to verify system is working
        /// </summary>
        public static void ShowTestPopup()
        {
            Vector2 topCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.85f);
            ScoreUtility.ShowPopupAtScreenPosition(100, topCenter, "Test Popup");
            Debug.Log("ScorePopupBridge: Showed test popup");
        }
    }
}
