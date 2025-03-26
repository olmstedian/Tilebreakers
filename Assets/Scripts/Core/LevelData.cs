using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tilebreakers/LevelData")]
public class LevelData : ScriptableObject
{
    public int gridSizeX = 6;
    public int gridSizeY = 6;
    public int startingTileCount = 4;
    public int scoreTarget = 100;
    public int maxMoves = 0; // 0 = unlimited
    public bool allowSpecialTiles = true;
    public bool isTimed = false;

    [TextArea]
    public string levelDescription;

    public List<Vector2Int> predefinedTilePositions;
}