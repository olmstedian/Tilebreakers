using UnityEngine;
using System.Collections;

/// <summary>
/// Abstract base class for all game states
/// </summary>
public abstract class GameState
{
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
    
    public virtual void HandleInput(Vector2Int gridPosition) { }
}

/// <summary>
/// Boot state - handles app initialization and splash screen display.
/// </summary>
public class BootState : GameState
{
    public override void Enter()
    {
        Debug.Log("BootState: Entering BootState...");
        UIManager.Instance.ShowSplashScreen();

        // Schedule transition to MainMenuState
        Debug.Log("BootState: Scheduling transition to MainMenuState.");
        GameStateManager.Instance.SetStateWithDelay(new MainMenuState(), 2.0f);
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("BootState: Exiting BootState...");
        UIManager.Instance.HideSplashScreen();
    }
}

/// <summary>
/// Main menu state - displays the main menu and waits for player input to start the game.
/// </summary>
public class MainMenuState : GameState
{
    public override void Enter()
    {
        Debug.Log("MainMenuState: Entering MainMenuState...");
        UIManager.Instance.ShowMainMenu();
    }

    public override void Update() { }

    public override void HandleInput(Vector2Int gridPosition)
    {
        GameStateManager.Instance.SetState(new LoadingLevelState());
    }

    public override void Exit()
    {
        Debug.Log("MainMenuState: Exiting MainMenuState...");
        UIManager.Instance.HideMainMenu();
    }
}

/// <summary>
/// Loading level state - prepares the game level.
/// </summary>
public class LoadingLevelState : GameState
{
    public override void Enter()
    {
        Debug.Log("LoadingLevelState: Preparing data and assets...");
        // Simulate loading process (e.g., load assets, initialize data)
        GameStateManager.Instance.SetStateWithDelay(new InitGameState(), 1.5f); // Delay for demonstration
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("LoadingLevelState: Data and assets prepared.");
    }
}

/// <summary>
/// Init game state - initializes the game board and spawns starting tiles.
/// </summary>
public class InitGameState : GameState
{
    public override void Enter()
    {
        Debug.Log("InitGameState: Initializing game...");

        // Check if BoardManager is initialized
        if (BoardManager.Instance == null)
        {
            Debug.LogError("InitGameState: BoardManager.Instance is null. Ensure BoardManager is properly initialized.");
            return;
        }

        // Check if ScoreManager is initialized
        if (ScoreManager.Instance == null)
        {
            Debug.LogError("InitGameState: ScoreManager.Instance is null. Ensure ScoreManager is properly initialized.");
            return;
        }

        // Check if UIManager is initialized
        if (UIManager.Instance == null)
        {
            Debug.LogError("InitGameState: UIManager.Instance is null. Ensure UIManager is properly initialized.");
            return;
        }

        // Clear the board and initialize it
        BoardManager.Instance.ClearBoard();
        BoardManager.Instance.InitializeBoard();

        // Spawn starting tiles
        BoardManager.Instance.GenerateRandomStartingTiles(Constants.MIN_START_TILES, Constants.MAX_START_TILES);

        // Reset the score and UI
        ScoreManager.Instance.ResetScore();
        UIManager.Instance.ResetTopBar();

        // Transition to the next state
        GameStateManager.Instance.SetState(new WaitingForInputState());
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("InitGameState: Game initialized.");
    }
}

/// <summary>
/// Waiting for input state - waits for player input.
/// </summary>
public class WaitingForInputState : GameState
{
    public override void Enter()
    {
        Debug.Log("WaitingForInputState: Waiting for player input...");
    }

    public override void Update() { }

    public override void HandleInput(Vector2Int gridPosition)
    {
        Debug.Log($"WaitingForInputState: Player selected grid position {gridPosition}.");

        // Transition to MovingTilesState after input is handled
        GameStateManager.Instance.SetState(new MovingTilesState());
    }

    public override void Exit()
    {
        Debug.Log("WaitingForInputState: Exiting state.");
    }
}

/// <summary>
/// Moving tiles state - handles tile movement animations.
/// </summary>
public class MovingTilesState : GameState
{
    public override void Enter()
    {
        Debug.Log("MovingTilesState: Moving tiles...");

        // Trigger tile movement animations
        BoardManager.Instance.StartCoroutine(AnimateTileMovements(() =>
        {
            // Transition to the next state after animations are complete
            GameStateManager.Instance.SetState(new MergingTilesState());
        }));
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("MovingTilesState: Exiting state.");
    }

    /// <summary>
    /// Animates all tile movements and invokes a callback when complete.
    /// </summary>
    private IEnumerator AnimateTileMovements(System.Action onComplete)
    {
        // Simulate tile movement animations (replace with actual logic if needed)
        yield return new WaitForSeconds(Constants.TILE_MOVE_DURATION);

        // Invoke the callback after animations are complete
        onComplete?.Invoke();
    }
}

/// <summary>
/// Merging tiles state - handles tile merging logic.
/// </summary>
public class MergingTilesState : GameState
{
    public override void Enter()
    {
        Debug.Log("MergingTilesState: Merging tiles...");

        // Trigger merging logic
        BoardManager.Instance.StartCoroutine(HandleTileMerges(() =>
        {
            // Transition to SplittingTilesState after merges are complete
            GameStateManager.Instance.SetState(new SplittingTilesState());
        }));
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("MergingTilesState: Exiting state.");
    }

    /// <summary>
    /// Handles all tile merges and invokes a callback when complete.
    /// </summary>
    private IEnumerator HandleTileMerges(System.Action onComplete)
    {
        // Simulate merge animations (replace with actual logic if needed)
        yield return new WaitForSeconds(Constants.TILE_MOVE_DURATION);

        // Invoke the callback after merges are complete
        onComplete?.Invoke();
    }
}

/// <summary>
/// Splitting tiles state - handles tile splitting logic.
/// </summary>
public class SplittingTilesState : GameState
{
    public override void Enter()
    {
        Debug.Log("SplittingTilesState: Splitting tiles...");

        // Trigger splitting logic
        BoardManager.Instance.StartCoroutine(HandleTileSplits(() =>
        {
            // Transition to SpawningNewTileState after splits are complete
            GameStateManager.Instance.SetState(new SpawningNewTileState(BoardManager.Instance.lastMergedCellPosition));
        }));
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("SplittingTilesState: Exiting state.");
    }

    /// <summary>
    /// Handles all tile splits and invokes a callback when complete.
    /// </summary>
    private IEnumerator HandleTileSplits(System.Action onComplete)
    {
        // Simulate split animations (replace with actual logic if needed)
        yield return new WaitForSeconds(Constants.TILE_MOVE_DURATION);

        // Example: Use TileSplitter for splitting logic
        Tile tileToSplit = BoardManager.Instance.GetTileAtPosition(BoardManager.Instance.lastMergedCellPosition.Value);
        if (tileToSplit != null)
        {
            TileSplitter.SplitTile(tileToSplit, BoardManager.Instance.lastMergedCellPosition.Value);
        }

        // Invoke the callback after splits are complete
        onComplete?.Invoke();
    }
}

/// <summary>
/// Spawning new tile state - spawns a random new tile on the board.
/// </summary>
public class SpawningNewTileState : GameState
{
    private Vector2Int? mergedCellPosition;

    public SpawningNewTileState(Vector2Int? mergedCellPosition = null)
    {
        this.mergedCellPosition = mergedCellPosition;
    }

    public override void Enter()
    {
        Debug.Log("SpawningNewTileState: Spawning a new tile...");

        // Spawn a random new tile, avoiding adjacent cells of the merged cell if applicable
        BoardManager.Instance.GenerateRandomStartingTiles(1, 1, mergedCellPosition);

        // Transition to CheckingGameOverState after spawning the tile
        GameStateManager.Instance.SetState(new CheckingGameOverState());
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("SpawningNewTileState: Exiting state.");
    }
}

/// <summary>
/// Checking game over state - checks if the game is over.
/// </summary>
public class CheckingGameOverState : GameState
{
    public override void Enter()
    {
        Debug.Log("CheckingGameOverState: Checking game over...");
        if (!BoardManager.Instance.HasValidMove())
        {
            GameStateManager.Instance.SetState(new GameOverState());
        }
        else
        {
            GameStateManager.Instance.SetState(new WaitingForInputState());
        }
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("CheckingGameOverState: Game over check complete.");
    }
}

/// <summary>
/// Game over state - the player has no more valid moves.
/// </summary>
public class GameOverState : GameState
{
    public override void Enter()
    {
        Debug.Log("GameOverState: Game over.");
        UIManager.Instance.ShowGameOverScreen(ScoreManager.Instance.GetCurrentScore());
    }

    public override void Update() { }

    public override void HandleInput(Vector2Int gridPosition)
    {
        GameStateManager.Instance.RestartGame();
    }

    public override void Exit()
    {
        Debug.Log("GameOverState: Exiting game over.");
        UIManager.Instance.HideGameOverScreen();
    }
}

/// <summary>
/// Pause state - pauses the game and displays the pause menu.
/// </summary>
public class PauseState : GameState
{
    public override void Enter()
    {
        Debug.Log("PauseState: Entering PauseState. Game is now paused.");
        Time.timeScale = 0f; // Freeze the game
    }

    public override void Update()
    {
        Debug.Log("PauseState: Game is paused. No updates are processed.");
    }

    public override void Exit()
    {
        Debug.Log("PauseState: Exiting PauseState. Game is resuming.");
        Time.timeScale = 1f; // Resume the game
    }
}

