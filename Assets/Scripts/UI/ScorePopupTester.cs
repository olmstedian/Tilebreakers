using UnityEngine;
using Tilebreakers.Core;

namespace Tilebreakers.UI
{
    /// <summary>
    /// Simple component to test score popups - add to any GameObject
    /// </summary>
    [AddComponentMenu("Tilebreakers/UI/Score Popup Tester")]
    public class ScorePopupTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private KeyCode showPopupKey = KeyCode.P;
        [SerializeField] private KeyCode showMultipleKey = KeyCode.M;
        [SerializeField] private KeyCode addScoreKey = KeyCode.S;
        [SerializeField] private int testPoints = 100;
        [SerializeField] private string testText = "+100";
        [SerializeField] private bool showOnStart = true;
        
        [Header("Appearance")]
        [SerializeField] private float fontSize = 72f;
        [SerializeField] private bool useBold = true;
        [SerializeField] private bool useOutline = true;
        [SerializeField] private bool useShadow = true;
        
        private void Start()
        {
            // Apply text settings
            ScoreUtility.SetTextProperties(fontSize, useBold, useOutline, useShadow);
            
            if (showOnStart)
            {
                Invoke(nameof(ShowTestPopup), 0.5f); // Delay to ensure UI is ready
            }
            
            Debug.Log("ScorePopupTester: Press P to show popup, M for multiple, S to add score");
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(showPopupKey))
            {
                ShowTestPopup();
            }
            
            if (Input.GetKeyDown(showMultipleKey))
            {
                ScoreUtility.ShowTestPopups(5);
            }
            
            if (Input.GetKeyDown(addScoreKey))
            {
                ScoreUtility.AddPointsWithPopupAtInput(testPoints, testText);
            }
        }
        
        [ContextMenu("Show Test Popup")]
        public void ShowTestPopup()
        {
            // Show at center of screen
            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            ScoreUtility.ShowPopupAtScreenPosition(testPoints, center, testText);
        }
    }
}
