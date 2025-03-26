# 🧩 Tilebreakers

**Tilebreakers** is a minimalist, single-player, color-and-number merge puzzle game built with **Unity 2D**. Swipe, merge, split, and trigger special abilities to manage the board and rack up the highest score—before it fills up.

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

- 6×6 starting board with dynamic expansion (7×7 with Expander tile)
- Randomized tile spawning (numbers 1–5)
- Special tiles with unique effects (e.g., Blaster, Doubler, Painter, Expander)
- Smooth animations for tile spawning, merging, and splitting
- Enhanced tile visuals with dynamic brightness and outlines
- Subtle grid background and cell indicators for better clarity
- Simple scoring system with combo bonuses
- Clean, modern visual style
- Designed for infinite play and challenge modes

---

## 🛠️ Tech Stack

- Unity 2022.3 LTS (2D)
- C#
- VSCode (with GitHub Copilot)
- Git + GitHub for version control
- macOS Apple Silicon optimized

---

## ✅ Completed Development Plan

### Core Systems
- **GameManager**: Controls game flow, turn sequence, and game over conditions.
- **GameStateManager**: Manages game states (e.g., InitState, PlayerTurnState, PostTurnState, GameOverState).
- **InputManager**: Handles mouse and touch input for tile selection and movement.

### Board and Tile Management
- **BoardManager**: Manages the grid, tile placement, and updates.
- **Tile**: Implements tile behavior, including color, number, and animations.
- **TileMover**: Handles smooth tile movement.
- **TileAnimator**: Adds animations for merging and splitting.
- **TileMerger**: Implements merging and splitting logic.

### Special Tiles
- **SpecialTile Base Class**: Defines common behavior for all special tiles.
- **BlasterTile**: Destroys adjacent tiles when activated.
- **SpecialTileUI**: Handles player interaction with special tiles.

### Visual and UI Enhancements
- **Dynamic Brightness**: Tiles brighten slightly as their numbers increase.
- **Text Outlines**: Improved text readability with subtle outlines.
- **Animations**:
  - Smooth spawn animation for tiles.
  - Pulse animation for merges.
  - Smooth movement animations for tile transitions.
- **Grid Background**: Uniform light gray background for all cells.
- **Cell Indicators**: Subtle scaling and layering for better visual clarity.

---

## 🟨 Upcoming Development Plan

### Special Tile System
- **Implement Additional Special Tiles**:
  - **PainterTile**: Converts adjacent tiles to its color.
  - **FreezeTile**: Prevents tile spawn for one turn.
  - **DoublerTile**: Doubles the value of the next merge.
  - **ExpanderTile**: Expands the board from 6×6 to 7×7.

### Game Over Handling
- **Game Over Screen**:
  - Display final score and high score.
  - Add restart and main menu buttons.

### Scoring System
- **Implement Scoring**:
  - Add points for merges and splits.
  - Introduce combo bonuses for consecutive merges.

### Polish and Optimization
- **Visual Feedback**:
  - Add effects for merging, splitting, and special tile activation.
- **Sound Effects**:
  - Add sounds for tile movement, merging, and splitting.
- **Performance Optimization**:
  - Implement object pooling for tiles to reduce instantiation overhead.

### Challenge Modes
- **Daily Challenges**:
  - Introduce predefined levels with unique objectives.
- **Endless Mode**:
  - Allow players to play indefinitely with increasing difficulty.

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
- [ ] Add additional special tiles (Painter, Freeze, Doubler, Expander).
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