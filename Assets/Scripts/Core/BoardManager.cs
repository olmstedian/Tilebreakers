// Manages the grid state, tiles, and board updates

using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public int width = 6;
    public int height = 6;

    private Tile[,] board;

    void Start()
    {
        board = new Tile[width, height];
        // TODO: Initialize board with starting tiles
    }
}