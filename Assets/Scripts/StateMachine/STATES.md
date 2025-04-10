# Game State System Documentation

## Overview

The game state system in Tilebreakers uses a state pattern implementation to manage different game states. Each state is implemented as a separate class that inherits from the abstract `GameState` base class. This approach allows for clean separation of logic between different phases of gameplay.

## State Hierarchy

All game states inherit from the abstract `GameState` class which defines the common interface:

```csharp
public abstract class GameState
{
    public abstract void Enter();  // Called when entering the state
    public abstract void Update();  // Called every frame while in the state
    public abstract void Exit();    // Called when leaving the state
    
    public virtual void HandleInput(Vector2Int gridPosition) { }  // Optional input handling
}
```

## Folder Structure

The state classes are organized in the `StateMachine` folder:

/Assets/Scripts/StateMachine/
├── GameState.cs                   // Base abstract class
├── BootState.cs                   // Game initialization
├── MainMenuState.cs               // Main menu
├── LoadingLevelState.cs           // Level loading
├── InitGameState.cs               // Game initialization
├── WaitingForInputState.cs        // Waiting for player input
├── MovingTilesState.cs            // Animating tile movements
├── MergingTilesState.cs           // Merging tiles
├── SplittingTilesState.cs         // Splitting tiles
├── SpawningNewTileState.cs        // Spawning new tiles
├── SpecialTileActionState.cs      // Special tile activation
├── CheckingGameOverState.cs       // Checking for game over
├── GameOverState.cs               // Game over state
├── PauseState.cs                  // Game paused
├── SpecialTileActivationState.cs  // Activating special tiles
├── SpecialTileSpawningState.cs    // Spawning special tiles
├── LevelCompleteState.cs          // Level completion
├── LevelFailedState.cs            // Level failure
├── GameCompleteState.cs           // Game completion
└── STATES.md                      // This documentation file

## State Transition Diagram (Updated)

```
                        ┌───────────────┐
                        │   BootState   │
                        └───────┬───────┘
                                ▼
                        ┌───────────────┐
                        │ MainMenuState │
                        └───────┬───────┘
                                ▼
                      ┌─────────────────┐
                      │LoadingLevelState│
                      └────────┬────────┘
                               ▼
                       ┌───────────────┐
                       │ InitGameState │
                       └───────┬───────┘
                               ▼
┌─────────────┐      ┌───────────────────┐      ┌──────────────────┐
│  PauseState ◄──────┤WaitingForInputState├─────►SpecialTileAction  │
└─────┬───────┘      └─────────┬─────────┘      └─────────┬────────┘
      │                        │                          │
      │                        ▼                          │
      │              ┌───────────────────┐                │
      │              │  MovingTilesState │                │
      │              └─────────┬─────────┘                │
      │                        ▼                          │
      │              ┌───────────────────┐                │
      │              │ MergingTilesState │                │
      │              └─────────┬─────────┘                │
      │                        ▼                          │
      │              ┌───────────────────┐                │
      │              │SplittingTilesState│                │
      │              └─────────┬─────────┘                │
      │                        ▼                          │
      │              ┌───────────────────┐                │
      │              │SpawningNewTileState│               │
      │              └─────────┬─────────┘                │
      │                        ▼                          │
      └──────────────►┌───────────────────┐◄──────────────┘
                      │CheckingGameOverState│
                      └──┬──────────────┬──┘
                         │              │
                         ▼              ▼
                 ┌──────────────┐    ┌──────────────────┐
                 │ GameOverState│    │WaitingForInputState│
                 └──────┬───────┘    └──────────────────┘
                        │
                        ▼
                 ┌──────────────┐
                 │ MainMenuState│
                 └──────────────┘
```

## State Manager

The `GameStateManager` class is responsible for managing state transitions:

- It maintains a reference to the current active state.
- It handles state transitions through `SetState(GameState newState)`.
- It provides delayed state transitions through `SetStateWithDelay(GameState newState, float delay)`.
- It distributes input to the current active state.

## States Description

### BootState
- Initial state when the game starts.
- Displays splash screen.
- Transitions to `MainMenuState` after a delay.

### MainMenuState
- Displays the main menu UI.
- Handles menu interactions.
- Transitions to `LoadingLevelState` when game starts.

### LoadingLevelState
- Prepares game data and assets.
- Shows loading indicators.
- Transitions to `InitGameState` when loading completes.

### InitGameState
- Initializes the game board.
- Spawns starting tiles.
- Resets score and UI.
- Transitions to `WaitingForInputState`.

### WaitingForInputState
- Waits for player input.
- Handles tile selection.
- Transitions to `MovingTilesState` after player moves.

### MovingTilesState
- Handles tile movement animations.
- Transitions to `MergingTilesState` after movements are complete.

### MergingTilesState
- Handles tile merging logic.
- Updates score.
- Transitions to `SplittingTilesState`.

### SplittingTilesState
- Handles tile splitting logic.
- Transitions to `SpawningNewTileState`.

### SpawningNewTileState
- Spawns new tiles on the board.
- Transitions to `CheckingGameOverState`.

### SpecialTileActionState
- Activates special tiles.
- Transitions to `CheckingGameOverState`.

### CheckingGameOverState
- Checks if the game is over.
- Transitions to `GameOverState` or back to `WaitingForInputState`.

### GameOverState
- Shows game over screen.
- Handles restart logic.
- Transitions to `InitGameState` or `MainMenuState`.

### PauseState
- Pauses the game.
- Shows pause menu.
- Transitions back to previous state when resumed.

### SpecialTileActivationState
- Handles player interaction with special tiles.
- Transitions to other states based on special tile actions.

### SpecialTileSpawningState
- Handles spawning of special tiles.
- Transitions to `WaitingForInputState`.

### LevelCompleteState
- Shows level completion UI.
- Handles progression to next level.
- Transitions to `LoadingLevelState` or `WaitingForInputState`.

### LevelFailedState
- Shows level failure UI.
- Handles level restart or menu return.
- Transitions to `LoadingLevelState` or `MainMenuState`.

### GameCompleteState
- Shows game completion screen.
- Handles return to main menu.
- Transitions to `MainMenuState`.

## State Transition Rules

- States should clean up after themselves in the `Exit()` method.
- States should initialize what they need in the `Enter()` method.
- State transitions should occur at logical points in gameplay.
- Avoid circular state dependencies.
- Each state should have a single responsibility.

## Implementation Tips

When adding a new state:

1. Create a new class inheriting from `GameState`.
2. Implement the required methods: `Enter()`, `Update()`, `Exit()`.
3. Add appropriate state transitions.
4. Register any new state in the `GameStateManager`.

For debugging:

- States log their actions through `Debug.Log` statements.
- The current state name can be retrieved through `GameStateManager.GetCurrentStateName()`.
- Use breakpoints in state methods to track execution flow

For special cases:

- Use `IsInState<T>()` to check the current state type.
- Use `OnStateChanged` event to subscribe to state changes.
- Store additional context data in state constructors for more complex states

## Best Practices:
- Keep states focused on a single responsibility
- Avoid direct references between states
- Use the GameStateManager for all transitions
- Handle edge cases like interrupted transitions