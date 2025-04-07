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

- **Special Tiles**:
  - **BlasterTile**: Destroys adjacent tiles.
  - **FreezeTile**: Skips the next tile spawn.
  - **DoublerTile**: Doubles the value of adjacent tiles.
  - **PainterTile**: Converts adjacent tiles to its own color.

- **Level Progression**:
  - Levels with unique configurations, including grid size, starting tiles, and score targets.
  - Automatic level advancement upon meeting score targets.

- **Game States**:
  - Robust state management for handling game flow, including input, animations, and transitions.

- **UI Enhancements**:
  - Dynamic score, move count, and level display.
  - Pause and resume functionality.
  - Game over screen with final score display.

- **Debugging and Logging**:
  - Improved logging for debugging tile interactions and special tile activations.

---

## ✅ Latest Improvements

### **Special Tile System**
- Added new special tiles:
  - **FreezeTile**: Freezes adjacent tiles for one turn.
  - **DoublerTile**: Doubles the value of adjacent tiles.
- Improved `SpecialTileManager` to handle spawning and activation of special tiles.
- Enhanced `BlasterTile` logic to destroy adjacent tiles and clear their positions on the board.

### **Game State System**
- Refactored `GameStateManager` to support delayed transitions and improved state handling.
- Added new states for special tile activation and spawning:
  - `SpecialTileActivationState`
  - `SpecialTileSpawningState`

### **Tile Splitting Enhancements**
- Centralized all splitting logic into `TileSplitter.cs`.
- Added score calculation for splits based on the total value of resulting tiles.
- Improved randomization of split tile values and positions.

### **Scoring System**
- Improved `ScoreManager` to handle:
  - Merge score: `+1` point for the merge itself and the merged tile's final number.
  - Split score: Total value of resulting split tiles.
  - Special tile activation bonus: `+10` points.
- Ensured score updates are reflected in the UI.

### **Game Over System**
- Enhanced `GameOverManager` to detect when the board is full and no valid merges exist.
- Integrated `GameOverState` to handle game over transitions and display the game over screen.

### **UI Enhancements**
- Updated `UIManager` to handle:
  - Game over screen with final score display.
  - Pause and resume functionality.
  - Resetting the top bar UI (score and move count) on game restart.

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

### 🟨 Phase 5: Special Tile System
- [x] Implement `SpecialTile` base class.
- [x] Create `BlasterTile` to destroy adjacent tiles.
- [x] Add additional special tiles (Painter, Freeze, Doubler, Expander).
- [ ] Add UI interaction for activating special tiles.

---

### 🟨 Phase 6: Tile Spawning, Game Flow, and Game Over
- [x] Spawn one random tile after each player move.
- [ ] Skip spawn if `FreezeTile` is active.
- [ ] Detect game over when the board is full and no valid moves are possible.
- [ ] Display game over screen with score and restart options.

---

### 🟨 Phase 7: Polish and UI
- [ ] Add visual feedback for merging, splitting, and special abilities.
- [ ] Add sound effects for tile interactions.
- [ ] Add score UI and game over screen.

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