using UnityEngine;
using Tilebreakers.Core;
using System.Collections;
using TMPro;

public class ScoreDisplayFixer : MonoBehaviour
{
    public static bool fixScorePopupText = true;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (fixScorePopupText)
        {
            // Register to watch for any score popup creation
            Debug.Log("ScoreDisplayFixer: Initialized to monitor score popups");
            
            // Find all existing TextMeshProUGUI components that might be duplicating score values
            StartMonitoring();
        }
    }
    
    private static void StartMonitoring()
    {
        GameObject monitor = new GameObject("ScorePopupMonitor");
        monitor.AddComponent<PopupMonitor>();
        Object.DontDestroyOnLoad(monitor);
    }
    
    private class PopupMonitor : MonoBehaviour
    {
        private void Update()
        {
            // Find any gameobjects that contain "ScorePopup" in their name
            var popups = FindObjectsOfType<TextMeshProUGUI>();
            
            foreach (var text in popups)
            {
                if (text.gameObject.name.Contains("PopupText") || 
                    text.transform.parent.name.Contains("ScorePopup"))
                {
                    // Check if text contains duplicate scores like "Merged +4 4"
                    string content = text.text;
                    
                    // If the text contains more than one occurrence of a number
                    if (ContainsDuplicateScores(content))
                    {
                        Debug.Log($"ScoreDisplayFixer: Found popup with possible duplicated score: '{content}'");
                        
                        // Extract just the number and fix the text
                        int value;
                        if (TryExtractScore(content, out value))
                        {
                            text.text = value >= 0 ? $"+{value}" : value.ToString();
                            Debug.Log($"ScoreDisplayFixer: Fixed popup text to: '{text.text}'");
                        }
                    }
                }
            }
        }
        
        private bool ContainsDuplicateScores(string text)
        {
            // Look for patterns like "Merged +4 4" or "Split +5 5"
            string[] words = text.Split(' ');
            if (words.Length <= 1) return false;
            
            int numberCount = 0;
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i].Trim('+', ' ', ':', ',');
                int num;
                if (int.TryParse(word, out num))
                {
                    numberCount++;
                    if (numberCount > 1) return true;
                }
            }
            
            return false;
        }
        
        private bool TryExtractScore(string text, out int score)
        {
            score = 0;
            
            // First try to find "+N" pattern
            int plusIndex = text.IndexOf('+');
            if (plusIndex >= 0)
            {
                string afterPlus = text.Substring(plusIndex + 1).Trim();
                string[] parts = afterPlus.Split(' ', ',', ':');
                if (parts.Length > 0 && int.TryParse(parts[0], out score))
                {
                    return true;
                }
            }
            
            // If no "+N" pattern, try to find any number
            string[] words = text.Split(' ', ',', ':');
            foreach (string word in words)
            {
                string trimmed = word.Trim();
                if (int.TryParse(trimmed, out score))
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}
