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

    // Special spawn chance (e.g., Expander tile chance, increased to 50% expressed as 0.5f)
    public const float SPECIAL_TILE_CHANCE = 0.5f; // Temporarily increase for testing
    // Special spawn chance (e.g., Painter tile chance, increased to 20% expressed as 0.2f)
    public const float PAINTER_TILE_CHANCE = 0.2f;
}
