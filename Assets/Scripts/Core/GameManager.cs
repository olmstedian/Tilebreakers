// Controls game flow, turn sequence, game over condition

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Initialize game (board, UI, etc.)
    }

    public void EndTurn()
    {
        // Handle tile spawn and check for game over
    }
}