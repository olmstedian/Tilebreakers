public static class Constants
{
    // Grid configuration
    public const int DEFAULT_WIDTH = 6;
    public const int DEFAULT_HEIGHT = 6;
    public const float DEFAULT_CELL_SIZE = 1.5f;

    // Starting tiles configuration
    public const int MIN_START_TILES = 3;
    public const int MAX_START_TILES = 5;

    // Tile movement
    public const float TILE_MOVE_DURATION = 0.2f;

    // Tile number range for spawning
    public const int MIN_TILE_NUMBER = 1;
    public const int MAX_TILE_NUMBER = 5;

    // Special spawn chance (e.g., Expander tile chance, increased to 70% expressed as 0.7f)
    public const float SPECIAL_TILE_CHANCE = 0.7f; // Increased for testing
    // Special spawn chance (e.g., Painter tile chance, increased to 20% expressed as 0.2f)
    public const float PAINTER_TILE_CHANCE = 0.2f;
    
    // Testing mode - forces specific special tiles to appear more frequently
    public const bool TESTING_MODE = true; // Set to true for testing
    
    // Specific special tile weights for testing (higher = more frequent)
    public const float BLASTER_WEIGHT = 1.0f;
    public const float FREEZE_WEIGHT = 1.0f;
    public const float DOUBLER_WEIGHT = 3.0f; // Higher weight for Doubler during testing
    public const float PAINTER_WEIGHT = 2.0f; // Medium-high weight for Painter during testing
}
