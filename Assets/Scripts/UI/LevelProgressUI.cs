using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelProgressUI : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI targetScoreText;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    
    private void Update()
    {
        if (LevelManager.Instance == null || LevelManager.Instance.CurrentLevel == null) return;
        
        // Update progress bar
        if (progressBar != null)
        {
            progressBar.fillAmount = LevelManager.Instance.GetLevelProgress();
        }
        
        // Update progress percentage text
        if (progressText != null)
        {
            float progressPercentage = LevelManager.Instance.GetLevelProgress() * 100f;
            progressText.text = $"{Mathf.RoundToInt(progressPercentage)}%";
        }
        
        // Update target score text
        if (targetScoreText != null)
        {
            targetScoreText.text = $"Target: {LevelManager.Instance.CurrentLevel.scoreTarget}";
        }
        
        // Update current score text
        if (currentScoreText != null && ScoreManager.Instance != null)
        {
            currentScoreText.text = $"Score: {ScoreManager.Instance.GetCurrentScore()}";
        }
    }
}
