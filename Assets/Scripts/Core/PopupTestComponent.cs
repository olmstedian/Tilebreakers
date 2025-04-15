using UnityEngine;

namespace Tilebreakers.Core
{
    /// <summary>
    /// Test component for score popups - add to any GameObject to test
    /// </summary>
    [AddComponentMenu("Tilebreakers/Core/Popup Test Component")]
    public class PopupTestComponent : MonoBehaviour
    {
        [Header("Text Settings")]
        [SerializeField] private float fontSize = 72f;
        [SerializeField] private bool useBoldText = true;
        [SerializeField] private bool useOutlineEffect = true;
        [SerializeField] private bool useShadowEffect = true;
        
        [Header("Test Keys")]
        [SerializeField] private KeyCode testPopupKey = KeyCode.P;
        [SerializeField] private KeyCode testMultipleKey = KeyCode.M;
        
        [Header("Test Settings")]
        [SerializeField] private bool showPopupOnStart = true;
        [SerializeField] private bool showPopupOnClick = true;
        [SerializeField] private bool showPositiveScores = true;
        [SerializeField] private int minPoints = 10;
        [SerializeField] private int maxPoints = 1000;
        
        [Header("Auto Setup")]
        [SerializeField] private bool initializeSystem = true;
        
        private void Start()
        {
            if (initializeSystem)
            {
                // Initialize the system
                ScorePopupBridge.Initialize();
                Debug.Log("PopupTestComponent: Initialized popup system");
            }
            
            // Apply text settings
            ScorePopupBridge.SetTextProperties(fontSize, useBoldText, useOutlineEffect, useShadowEffect);
            
            Debug.Log("PopupTestComponent: Press P to show a test popup or M to show multiple popups");
            
            if (showPopupOnStart)
            {
                Invoke("ShowRandomPopup", 0.5f); // Slight delay to ensure UI is ready
            }
        }
        
        private void Update()
        {
            if (showPopupOnClick && Input.GetMouseButtonDown(0))
            {
                ShowPopupAtMouse();
            }
            
            // Show test popup
            if (Input.GetKeyDown(testPopupKey))
            {
                ShowTestPopup();
            }
            
            // Show multiple popups
            if (Input.GetKeyDown(testMultipleKey))
            {
                ShowMultiplePopups();
            }
        }
        
        /// <summary>
        /// Shows a popup at the mouse position
        /// </summary>
        [ContextMenu("Show Popup At Mouse")]
        public void ShowPopupAtMouse()
        {
            int points = showPositiveScores ? 
                Random.Range(minPoints, maxPoints) : 
                Random.Range(-maxPoints, maxPoints);
                
            ScorePopupBridge.ShowPopupAtScreenPosition(points, Input.mousePosition);
            Debug.Log($"PopupTestComponent: Showed popup with {points} points at mouse");
        }
        
        /// <summary>
        /// Shows a random popup on screen
        /// </summary>
        [ContextMenu("Show Random Popup")]
        public void ShowRandomPopup()
        {
            float x = Screen.width * 0.5f + Random.Range(-200f, 200f);
            float y = Screen.height * 0.5f + Random.Range(-100f, 100f);
            Vector2 pos = new Vector2(x, y);
            
            int points = showPositiveScores ? 
                Random.Range(minPoints, maxPoints) : 
                Random.Range(-maxPoints, maxPoints);
                
            ScorePopupBridge.ShowPopupAtScreenPosition(points, pos);
            Debug.Log($"PopupTestComponent: Showed random popup with {points} points");
        }
        
        /// <summary>
        /// Shows a popup at this GameObject's position
        /// </summary>
        [ContextMenu("Show Popup At This Object")]
        public void ShowPopupAtThisObject()
        {
            if (Camera.main == null)
            {
                Debug.LogError("PopupTestComponent: No main camera found!");
                return;
            }
            
            int points = showPositiveScores ? 
                Random.Range(minPoints, maxPoints) : 
                Random.Range(-maxPoints, maxPoints);
                
            ScorePopupBridge.ShowPopup(points, transform.position);
            Debug.Log($"PopupTestComponent: Showed popup with {points} points at this object");
        }
        
        [ContextMenu("Show Test Popup")]
        private void ShowTestPopup()
        {
            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            ScorePopupBridge.ShowPopupAtScreenPosition(100, center, "+100");
            Debug.Log("PopupTestComponent: Showed test popup at center screen");
        }
        
        [ContextMenu("Show Multiple Popups")]
        private void ShowMultiplePopups()
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = new Vector2(
                    Screen.width * 0.5f + Random.Range(-200f, 200f),
                    Screen.height * 0.5f + Random.Range(-100f, 100f));
                
                int points = Random.Range(10, 1000);
                ScorePopupBridge.ShowPopupAtScreenPosition(points, pos);
            }
            Debug.Log("PopupTestComponent: Showed multiple popups");
        }
    }
}
