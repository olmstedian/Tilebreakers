using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Scriptable object containing all data for a level.
/// </summary>
[CreateAssetMenu(fileName = "New Level", menuName = "Tilebreakers/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    [SerializeField] public int levelNumber = 1;
    [SerializeField] public string levelName = "Level 1";
    [SerializeField] [TextArea(2, 5)] public string levelDescription = "Break some tiles!";
    [SerializeField] [TextArea(1, 2)] public string levelObjectiveText = ""; // Added for custom objectives
    
    public enum Difficulty
    {
        Tutorial,
        Easy,
        Medium,
        Hard,
        Expert
    }
    
    [SerializeField] public Difficulty difficulty = Difficulty.Easy;
    
    [Header("Level Objectives")]
    [SerializeField] public int movesTarget = 20;
    [SerializeField] public int scoreTarget = 500;
    [SerializeField] public int timerInSeconds = 0; // 0 means no timer
    [Tooltip("If true, player must reach scoreTarget within movesTarget. If false, must survive movesTarget moves")]
    [SerializeField] public bool scoreObjective = true;
    
    [Header("Board Setup")]
    [SerializeField] [Range(3, 8)] public int boardWidth = Constants.DEFAULT_WIDTH;
    [SerializeField] [Range(3, 8)] public int boardHeight = Constants.DEFAULT_HEIGHT;
    
    [Header("Starting Tiles")]
    [SerializeField] [Range(1, 12)] public int minStartingTiles = Constants.MIN_START_TILES;
    [SerializeField] [Range(1, 12)] public int maxStartingTiles = Constants.MAX_START_TILES;
    [SerializeField] [Range(1, 4)] public int minTileValue = Constants.MIN_TILE_NUMBER;
    [SerializeField] [Range(1, 12)] public int maxTileValue = Constants.MAX_TILE_NUMBER;
    
    [Header("Special Tiles")]
    [SerializeField] [Range(0f, 1f)] public float specialTileChance = Constants.SPECIAL_TILE_CHANCE;
    [SerializeField] public bool enableBlasterTile = true;
    [SerializeField] public bool enableFreezerTile = true;
    [SerializeField] public bool enableDoublerTile = true;
    [SerializeField] public bool enablePainterTile = true;
    
    [Header("Special Configuration")]
    [SerializeField] public bool hasPresetBoard = false;
    [SerializeField] public List<TilePreset> presetTiles = new List<TilePreset>();
    [SerializeField] public List<Vector2Int> blockedTilePositions = new List<Vector2Int>();
    [SerializeField] public List<SpecialTilePreset> presetSpecialTiles = new List<SpecialTilePreset>();
    
    [Header("Tutorial Settings")]
    [SerializeField] public bool isTutorialLevel = false;
    [SerializeField] public List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    /// <summary>
    /// Represents a preset tile for level design
    /// </summary>
    [System.Serializable]
    public class TilePreset
    {
        public Vector2Int position;
        public int number;
        public Color color; // Using a Color instead of enum for more flexibility

        public TilePreset(Vector2Int pos, int num, Color col)
        {
            position = pos;
            number = num;
            color = col;
        }
    }

    /// <summary>
    /// Represents a preset special tile for level design
    /// </summary>
    [System.Serializable]
    public class SpecialTilePreset
    {
        public Vector2Int position;
        public string specialTileType; // "Blaster", "Freezer", "Doubler", "Painter"

        public SpecialTilePreset(Vector2Int pos, string type)
        {
            position = pos;
            specialTileType = type;
        }
    }

    /// <summary>
    /// Represents a tutorial step with instructions and triggered elements
    /// </summary>
    [System.Serializable]
    public class TutorialStep
    {
        public string instructionText;
        public Vector2Int highlightPosition;
        public bool waitForTileSelection;
        public bool waitForTileMove;
        public bool waitForMerge;

        public TutorialStep(string text)
        {
            instructionText = text;
            highlightPosition = new Vector2Int(-1, -1); // -1, -1 means no highlight
            waitForTileSelection = false;
            waitForTileMove = false;
            waitForMerge = false;
        }
    }

    /// <summary>
    /// Returns a clone of the current level data object
    /// </summary>
    public LevelData Clone()
    {
        LevelData clone = CreateInstance<LevelData>();
        
        // Copy basic level info
        clone.levelNumber = this.levelNumber;
        clone.levelName = this.levelName;
        clone.levelDescription = this.levelDescription;
        clone.difficulty = this.difficulty;
        
        // Copy objectives
        clone.movesTarget = this.movesTarget;
        clone.scoreTarget = this.scoreTarget;
        clone.timerInSeconds = this.timerInSeconds;
        clone.scoreObjective = this.scoreObjective;
        
        // Copy board setup
        clone.boardWidth = this.boardWidth;
        clone.boardHeight = this.boardHeight;
        
        // Copy starting tiles
        clone.minStartingTiles = this.minStartingTiles;
        clone.maxStartingTiles = this.maxStartingTiles;
        clone.minTileValue = this.minTileValue;
        clone.maxTileValue = this.maxTileValue;
        
        // Copy special tiles
        clone.specialTileChance = this.specialTileChance;
        clone.enableBlasterTile = this.enableBlasterTile;
        clone.enableFreezerTile = this.enableFreezerTile;
        clone.enableDoublerTile = this.enableDoublerTile;
        clone.enablePainterTile = this.enablePainterTile;
        
        // Copy special configuration
        clone.hasPresetBoard = this.hasPresetBoard;
        
        // Copy preset tiles
        clone.presetTiles = new List<TilePreset>();
        foreach (var preset in this.presetTiles)
        {
            clone.presetTiles.Add(new TilePreset(preset.position, preset.number, preset.color));
        }
        
        // Copy blocked tile positions
        clone.blockedTilePositions = new List<Vector2Int>(this.blockedTilePositions);
        
        // Copy preset special tiles
        clone.presetSpecialTiles = new List<SpecialTilePreset>();
        foreach (var special in this.presetSpecialTiles)
        {
            clone.presetSpecialTiles.Add(new SpecialTilePreset(special.position, special.specialTileType));
        }
        
        // Copy tutorial settings
        clone.isTutorialLevel = this.isTutorialLevel;
        clone.tutorialSteps = new List<TutorialStep>();
        foreach (var step in this.tutorialSteps)
        {
            TutorialStep newStep = new TutorialStep(step.instructionText);
            newStep.highlightPosition = step.highlightPosition;
            newStep.waitForTileSelection = step.waitForTileSelection;
            newStep.waitForTileMove = step.waitForTileMove;
            newStep.waitForMerge = step.waitForMerge;
            clone.tutorialSteps.Add(newStep);
        }
        
        return clone;
    }
    
    /// <summary>
    /// Gets a string describing the level objective
    /// </summary>
    public string GetObjectiveText()
    {
        // Use custom objective text if available
        if (!string.IsNullOrEmpty(levelObjectiveText))
        {
            return levelObjectiveText;
        }
        
        // Otherwise generate default objective text
        if (scoreObjective)
        {
            return $"Score {scoreTarget} points in {movesTarget} moves or less";
        }
        else
        {
            return $"Survive for {movesTarget} moves";
        }
    }
    
    /// <summary>
    /// Returns a descriptive name for this level
    /// </summary>
    public override string ToString()
    {
        return $"Level {levelNumber}: {levelName} ({difficulty})";
    }
}

/// <summary>
/// Represents a single tile to be pre-placed on the board
/// </summary>
[System.Serializable]
public class TileSpawnData
{
    public Vector2Int position;
    public int tileValue;
    public TileColorType colorType; // Use an enum for preset colors
    [HideInInspector]
    public Color customColor; // Only used if colorType is Custom
    public SpecialTileType specialTileType = SpecialTileType.None;
    
    public Color GetTileColor()
    {
        switch (colorType)
        {
            case TileColorType.Red:
                return new Color(1f, 0.5f, 0.5f);
            case TileColorType.Blue:
                return new Color(0.5f, 0.5f, 1f);
            case TileColorType.Green:
                return new Color(0.5f, 1f, 0.5f);
            case TileColorType.Yellow:
                return new Color(1f, 1f, 0.5f);
            case TileColorType.Custom:
                return customColor;
            default:
                return Color.white;
        }
    }
}

/// <summary>
/// Level difficulty categories
/// </summary>
public enum LevelDifficulty
{
    Easy,
    Medium,
    Hard,
    Expert,
    Infinite
}

/// <summary>
/// Types of level objectives
/// </summary>
public enum LevelObjectiveType
{
    None,
    ReachScore,
    CreateSpecificTile,
    MergeTilesCount,
    UseSpecialTilesCount,
    ClearColorTilesCount,
    SplitTilesCount,
    SurvivalTurns
}

/// <summary>
/// Predefined tile colors
/// </summary>
public enum TileColorType
{
    Random,
    Red,
    Blue,
    Green,
    Yellow,
    Custom
}

/// <summary>
/// Types of special tiles
/// </summary>
public enum SpecialTileType
{
    None,
    Blaster,
    Freezer,
    Doubler,
    Painter
}