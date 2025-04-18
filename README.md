# Tilebreakers

A strategic board game where players match and merge colorful tiles to earn points.

## Gameplay

Move tiles across a grid, merging matching colors to increase their values. The game combines pattern recognition with strategic planning as players try to maximize their score.

Key mechanics:
- Tiles can only merge with tiles of the same color
- Movement range is determined by the tile's number
- Merge tiles to increase their value
- When a tile's value exceeds the split threshold, it will split into multiple tiles

## Special Tiles
- **Blaster Tile**: Explodes and removes all adjacent tiles
- **Freeze Tile**: Skips the next tile spawn turn
- **Doubler Tile**: Doubles the values of all adjacent tiles
- **Painter Tile**: Changes adjacent tiles to match its color

Special tiles introduce strategic elements and add variety to gameplay with unique abilities.

## How to Play
1. Click on a tile to select it
2. Click on a valid destination to move or merge
3. Special tiles can be activated by clicking on them
4. Manage your board carefully to avoid running out of moves

## Features
- Dynamic grid-based board
- Colorful tile graphics with visual effects
- Score tracking with combo multipliers
- Multiple special tile types with unique abilities
- Level progression system

## Development
This project is built in Unity and follows object-oriented design principles with a focus on component-based architecture.

## Recent Updates
- Added Painter special tile that changes adjacent tiles to its color
- Improved visual highlighting system to distinguish between move targets (blue) and merge targets (gold)
- Enhanced tile splitting logic with better positioning and color assignment
- Added weighted randomization for special tile spawning
- Fixed bugs related to tile merging and board management

# 🧩 Tilebreakers

**Tilebreakers** is a grid-based puzzle game where players merge, split, and activate special tiles to achieve high scores and complete levels.

---

## 🎮 Gameplay Overview

- **Click tiles** to select and move them up to their number value in any direction.
- **Merge same-colored tiles** to add their numbers together.
- **Split** tiles when the value exceeds 12 into random tiles that total the original.
- **Special tiles** spawn from splits and trigger game-changing abilities.
- **New tiles** appear after every move.
- **Game ends** when the board is full and no valid moves remain.

---

## 🧱 Features

- **Tile Merging and Splitting**:
  - Merge tiles of the same color to increase their value.
  - Split tiles with high values into smaller tiles with random values.
  - Click a selected tile again to deselect it.

- **Special Tiles**:
  - **BlasterTile**: Destroys adjacent tiles with explosion effects and sounds.
  - **FreezeTile**: Skips the next tile spawn with satisfying visual feedback.
  - **DoublerTile**: Doubles the value of adjacent tiles.
  - **PainterTile**: Converts adjacent tiles to its own color.

- **Level Progression**:
  - Levels with unique configurations, including grid size, starting tiles, and score targets.
  - Automatic level advancement upon meeting score targets.

- **Game States**:
  - Robust state management for handling game flow, including input, animations, and transitions.
  - Improved state transitions with delayed execution and proper cleanup.

- **UI Enhancements**:
  - Dynamic score, move count, and level display.
  - Pause and resume functionality.
  - Game over screen with final score display.

- **Audiovisual Feedback**:
  - Sound effects for special tile activations.
  - Particle effects for tile destruction and merging.
  - Visual indicators for valid moves and selection.
  - Explosion sounds with pitch variations for BlasterTile.

---

## ✅ Latest Improvements

### **Special Tile Enhancements**
- **BlasterTile Improvements**:
  - Added explosion sound effects with pitch variation for more dynamic feedback.
  - Increased visual presence with larger scale and pulsing effects.
  - Individual "crack" sounds for each destroyed tile.
  - Particle effects for explosions at varying scales.
  - Visual feedback via color pulsing and scaling animations.
  
- **FreezeTile Enhancements**:
  - Added particle effect support for tile activation.
  - Clearer logging and feedback when ability is triggered.
  - Proper cleanup when the tile is used.

### **Tile Interaction Improvements**
- Clicking a selected tile again now deselects it.
- Improved visual feedback for tile selection and valid moves.
- Smoother animations for tile movements and merges.

### **Special Tile Spawning System**
- Improved `FindAlternativePosition` algorithm to find valid positions more reliably.
- Reduced redundant warnings when spawning special tiles.
- Added queue-based priority system for nearby positions.

### **Game Flow Improvements**
- Fixed issues with game state transitions.
- Ensured proper cleanup of resources when tiles are destroyed.
- Improved board reset logic for game restarts.

---

## 🛠️ Tech Stack

- Unity 2022.3 LTS (2D)
- C#
- VSCode (with GitHub Copilot)
- Git + GitHub for version control
- macOS Apple Silicon optimized

---

## 🚀 How to Play (Dev Build)

1. Clone the repo:
   ```bash
   git clone https://github.com/olmstedian/Tilebreakers.git
   ```

2. Open the project in Unity 2022.3+

3. Load the `Scenes/Game.unity`

4. Press Play to start testing

---

## 📋 DEVLOG

Track day-to-day development progress in **DEVLOG.md**

---

## 🛣️ Development Roadmap (MVP)

### ✅ Phase 1: Unity Project Setup
- [x] Create Unity 2D project (Tilebreakers)
- [x] Set up folder structure under `Assets/Scripts/`
- [x] Create `Game.unity` scene in `Scenes/`
- [x] Create `.gitignore`, initialize Git, push to GitHub

---

### ✅ Phase 2: Core Architecture & Grid System
- [x] Implement `GameManager`, `BoardManager`, `InputManager`
- [x] Create a 6×6 tile grid with proper spacing and visual indicators.
- [x] Generate random starting tiles with valid moves.

---

### ✅ Phase 3: Click Input & Tile Movement
- [x] Detect mouse clicks for tile selection and movement.
- [x] Highlight valid movement options based on tile's number value.
- [x] Move tiles to selected destinations with smooth animations.

---

### ✅ Phase 4: Merge & Split Logic
- [x] Merge same-colored tiles and add their numbers.
- [x] Split tiles when their value exceeds 12.
- [x] Spawn special tiles during splits.

---

### ✅ Phase 5: Special Tile System
- [x] Implement `SpecialTile` base class.
- [x] Create `BlasterTile` to destroy adjacent tiles with visual and audio feedback.
- [x] Add additional special tiles (Painter, Freeze, Doubler).
- [x] Add UI interaction for activating special tiles.

---

### ✅ Phase 6: Tile Spawning, Game Flow, and Game Over
- [x] Spawn one random tile after each player move.
- [x] Skip spawn if `FreezeTile` is active.
- [x] Detect game over when the board is full and no valid moves are possible.
- [x] Display game over screen with score and restart options.

---

### 🟨 Phase 7: Polish and UI
- [x] Add visual feedback for merging, splitting, and special abilities.
- [x] Add sound effects for tile interactions.
- [x] Add score UI and game over screen.
- [ ] Add tutorial elements for new players.

---

### 🟩 Phase 8: Extra Features & Optimization (Post-MVP)
- [ ] Add combo meter / multipliers.
- [ ] Challenge levels or endless mode options.
- [ ] Implement undo system.
- [ ] Mobile optimization (touch input, aspect scaling).
- [ ] Performance profiling and pooling for tiles.

---

## 🧩 Level System – LevelManager & LevelData

Even though Tilebreakers is designed as an endless puzzle game, introducing a level system opens the door for:

- Structured challenge levels
- Difficulty progression (e.g., increased color count, limited moves)
- Campaigns or milestones
- Special modifiers (e.g., faster split threshold, spawn blockers)
- Daily rotating objectives

---

### 📁 Files

#### `Core/LevelManager.cs`
Manages current level rules, objectives, and win/loss conditions.

```csharp
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public LevelData currentLevel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void LoadLevel(LevelData level)
    {
        currentLevel = level;
        // Apply settings: grid size, rules, special tile toggle
    }

    public bool IsLevelComplete()
    {
        // Evaluate score, move count, etc.
        return false;
    }

    public void AdvanceToNextLevel()
    {
        // Load next LevelData or return to menu
    }
}
```

---

#### `Core/LevelData.cs` (ScriptableObject)

Holds data for each level — easily configured in the Unity Inspector.

```csharp
[CreateAssetMenu(menuName = "Tilebreakers/Level")]
public class LevelData : ScriptableObject
{
    public int gridSizeX = 6;
    public int gridSizeY = 6;
    public int startingTileCount = 4;
    public int scoreTarget;
    public int maxMoves;
    public bool allowSpecialTiles = true;
    public bool isTimed;
    public List<Vector2Int> predefinedTilePositions;
}
```

---

### 🛠️ Development Plan

1. Create `LevelData.cs` as a ScriptableObject.
2. Create a few test levels in `Assets/Levels/` (e.g., `Level_01.asset`, `DailyChallenge.asset`).
3. Create `LevelManager.cs` to control level loading and progression.
4. Update `GameManager` to reference `LevelManager.Instance.currentLevel` on start.
5. Pass rules to `BoardManager`, `TileSpawner`, and `GameStateManager` (e.g., disallow special tiles).
6. Add condition evaluation:
   - Score threshold reached
   - Number of moves used
   - Win/loss popup

---

### 🧠 Optional Extensions

- Add "Next Level" button after win
- Tie `LevelData` to a challenge or daily rotation system
- Add `LevelObjective.cs` for complex conditions (e.g., "Merge 5 red tiles")

---

## 🧱 Basic Full Game Build – Checklist (Pre-State System)

Before implementing advanced systems like GameStateManager and LevelManager, the goal is to build a complete, minimal version of the game with all core logic, UI, and interactions functioning. This version can be playtested and expanded safely.

### ✅ Core Gameplay Loop
- [x] Tile movement: click to select and move tile up to its number of spaces
- [x] Merge logic: same-colored tiles merge and add their numbers
- [x] Split logic: if value > 12, split into smaller tiles summing to the original
- [x] Random tile spawn after each move
- [x] Prevent movement into occupied cells
- [ ] Game over check: board full and no valid merges
- [ ] Restart button resets the board

---

### ✅ Basic UI Elements
- [ ] Score display (merges and splits)
- [ ] Game over screen with:
  - Final score
  - Restart button
- [ ] Optional: simple pause button
- [ ] Visual indicator for selected tile
- [ ] Optional: debug UI (current tile state, move log)

---

### ✅ Basic Visuals and Feedback
- [ ] Basic tile color and number rendering
- [ ] Merge animation (e.g. scale bounce)
- [ ] Tile move animation
- [ ] Split animation (simple spawn pop)

---

### 🔁 Once Complete:
- Playtest for game balance, bugs, and feel
- Add GameStateManager to organize logic
- Layer in special tiles, polish, effects, and UI improvements

---

## 🗂️ Scenes & Game States

A full implementation of Tilebreakers will include multiple Unity scenes and a well-defined set of game states to handle transitions and logic flow clearly and scalably.

---

### 🎬 Unity Scenes

#### `MainMenu`
- Entry point of the game
- Buttons: Play, Daily Challenge, Endless Mode, Settings
- Display version and credits

#### `Game`
- Main gameplay scene
- Loads level data and handles all in-game logic
- Includes gameplay UI (score, pause, game over overlay)

#### `Pause`
- Overlay scene that freezes gameplay
- Options to resume, restart, adjust settings, or quit to main menu
- Accessible during active gameplay with pause button

#### `LevelSelect` *(optional/future)*
- List of unlocked or available challenge levels
- Preview objectives and difficulty

#### `GameOver` *(optional as separate scene or overlay)*
- Final score display
- Buttons: Play Again, Main Menu

#### `Settings` *(overlay or modal preferred)*
- Toggle music/SFX
- Theme / colorblind options
- Optionally accessible from Main Menu and Game scenes

---

### 🎮 GameState Enum & Flow

Use this structure for `GameState.cs`:

```csharp
public enum GameState
{
    Boot,               // App initialization - OK
    MainMenu,           // Main menu screen - OK
    LoadingLevel,       // Preparing data / assets - OK
    InitGame,           // Spawning board and tiles - OK
    WaitingForInput,    // Awaiting player swipe/tap - OK
    MovingTiles,        // Tiles animating their movement - OK
    MergingTiles,       // Handling merges - OK
    SplittingTiles,     // Handling tile splits - OK
    SpawningNewTile,    // Dropping a random new tile - OK
    SpecialTileAction,  // Waiting for special tile use
    CheckingGameOver,   // Verifying end condition
    GameOver,           // Showing result
    Paused              // Frozen state (pause menu)
}
```

---

### 🔁 Game Flow Overview

```
MainMenu → LoadingLevel → InitGame → WaitingForInput
→ MovingTiles → MergingTiles → SplittingTiles
→ SpawningNewTile → CheckingGameOver
→ GameOver or back to WaitingForInput

At any point during gameplay:
WaitingForInput → Paused → WaitingForInput
```

---

### 🧩 Optional Advanced States (Future Growth)

| State               | Description |
|----------------------|-------------|
| `UndoingMove`        | Handling undo system |
| `ChallengeComplete`  | Level-based victory |
| `Reviving`           | Ad-based revive |
| `VictoryAnimation`   | Extra feedback after winning |

---

## Project Audit: Step-by-Step Overview

1. **Project Structure & Architecture**  
   - Review the folder layout (Core, UI, Tiles, SpecialTiles, Visual, etc.).  
   - Confirm that key managers—BoardManager, GameStateManager, LevelManager, GameManager—are singletons and correctly instantiated.

2. **Game Flow & States**  
   - Audit the GameState subclasses (BootState, MainMenuState, WaitingForInputState, etc.) to ensure smooth transitions.  
   - Verify that delayed transitions and state checks (e.g., in GameStateManager) perform as expected.

3. **Board and Tile Management**  
   - Confirm that BoardManager synchronizes the board array with the emptyCells collection.  
   - Audit tile operations (Move, Merge, Split, and destruction) to handle edge cases consistently.

4. **Special Tiles & Visual Effects**  
   - Verify the functionality of special tiles (Blaster, Doubler, Freeze, Painter) and their feedback (sound, particles, animations).  
   - Cross-check that associated utility functions (e.g., TileDestructionUtility) handle cleanup correctly.

5. **Input & UI Components**  
   - Check that InputManager captures touch/mouse events and interacts with the GameStateManager to trigger expected responses.  
   - Review UI updates in UIManager when game state changes (score updates, level transitions, etc.).

6. **Auditing the Logs**  
   - Validate that debug logs provide sufficient insight into data inconsistencies (e.g., emptyCells sync issues) and state transitions.  
   - Ensure error logging in critical components (BoardManager, GameOverManager) helps identify runtime issues.

7. **Testing and Improvement Areas**  
   - List known issues or data discrepancies (from logs) and ensure they are addressed by recent code changes.  
   - Plan further tests using Unity’s Editor Play Mode and review console logs for warnings or errors.

---

## Installation

1. Clone the repository.
2. Open the project in Unity.
3. Assign required prefabs (e.g., `TilePrefab`, `SpecialTilePrefabs`) in the Unity Editor.
4. Play the game in the Unity Editor or build it for your desired platform.

---

## Contributing

Contributions are welcome! Please follow the coding standards and submit pull requests for review.

---

## License

This project is licensed under the MIT License.