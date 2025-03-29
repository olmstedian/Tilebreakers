using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private int currentScore;
    private int highScore;
    private float comboMultiplier = 1.0f; // Optional multiplier for combos/streaks

    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI highScoreText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Load high score from PlayerPrefs
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateHighScoreUI(); // Ensure high score UI is updated on load
    }

    public void AddScore(int points)
    {
        currentScore += Mathf.RoundToInt(points * comboMultiplier);
        UpdateScoreUI();

        // Update high score if necessary
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
            UpdateHighScoreUI();
        }
    }

    public void AddMergeScore(int mergedTileValue)
    {
        // +1 point for the merge itself
        // + merged tile's final number
        int points = 1 + mergedTileValue;
        AddScore(points);
    }

    public void AddSplitScore(int totalSplitValue)
    {
        // + total value of resulting split tiles
        AddScore(totalSplitValue);
    }

    public void AddSpecialTileBonus()
    {
        // +10 bonus for using a special tile
        AddScore(10);
    }

    public void SetComboMultiplier(float multiplier)
    {
        // Optional: Set a multiplier for combos/streaks
        comboMultiplier = multiplier;
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public int GetHighScore()
    {
        return highScore;
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore}";
        }
    }

    public void ResetScore()
    {
        currentScore = 0;
        comboMultiplier = 1.0f; // Reset combo multiplier
        UpdateScoreUI();
    }
}
