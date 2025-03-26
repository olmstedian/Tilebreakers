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

## 🗂️ Folder Structure (Scripts)
├── Core/           # Game flow, input, board control
├── Tiles/          # Tile logic, merging, movement, animation
├── SpecialTiles/   # Special tile logic
├── Grid/           # Grid utilities and placement logic
├── UI/             # Score display, menus, special UI
├── FX/             # Visual and sound effects
└── Utils/          # Helpers, extensions, enums

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

## ✅ Recent Updates

### Tile Splitting System
- Implemented full tile splitting functionality when merged tiles exceed value 12.
- Added logic for generating multiple new tiles with values that sum to the original.
- Randomized color assignment for split tiles.
- Optimized spawn positions to prioritize non-adjacent cells for better gameplay.

### Tile Number Display
- Fixed issues with TextMeshPro components on spawned tiles.
- Improved font loading and text visibility.
- Enhanced verification processes to ensure numbers display correctly.

### Input System
- Removed swipe-based input and replaced it with mouse click-based selection and movement.
- Simplified controls for better testing and gameplay.

### Movement Enhancements
- Blocked valid move highlights beyond occupied cells.
- Improved animations for tile movement and merging.

---

## 📋 DEVLOG

Track day-to-day development progress in **DEVLOG.md**

---

## ✅ Roadmap (MVP)

- Board setup & movement
- Merge and split system
- Special tile mechanics
- Game over and score handling
- Combo meter and multipliers
- Challenge mode & unlocks
- Mobile input optimizations

---

## 📘 Game Concept Document – Tilebreakers

---

### 1. Game Overview

**Title**: Tilebreakers  
**Genre**: Single-player Puzzle / Merge Board Game  
**Platform**: Mobile (iOS / Android) and Web  
**Style**: Minimalist, clean, color-coded tiles with number-based logic  
**Session Length**: 2–5 minutes per run  
**Target Audience**: Casual puzzle gamers, fans of 2048, Threes!, and strategic match/merge games

---

### 2. Core Gameplay Loop
1. Start with a 6×6 grid and 3–5 random tiles.  
2. Swipe in any of 4 directions to move tiles.  
3. Tiles move up to tile.number spaces in the swipe direction.  
4. Tiles of the same color that collide merge, adding their numbers.  
5. If the resulting tile exceeds 12, it splits into random-colored and -numbered tiles that sum up to the original.  
6. Each split spawns one special tile with unique abilities.  
7. After every move, a new tile spawns in a random empty cell.  
8. Game ends when no moves are possible (board is full and no valid merges).

---

### 3. Tile Properties

**Standard Tiles**  
- Attributes:
  - `color`: Red, Blue, Green, Yellow, etc.
  - `number`: Integer (1–12 max before splitting)
  - `movementRange`: Equal to number

**Spawn Rules**  
- Random color  
- Random number from 1 to 5  
- Spawns after every swipe (unless prevented by a special tile)

---

### 4. Merging Logic
- Tiles must be the same color to merge.  
- Merging adds their numbers.  
- The merged tile appears in the position of the tile moved into.  
- Tiles do not merge if the resulting number would exceed 12 and there’s no room to split.

---

### 5. Splitting Logic
- If a tile becomes >12, it splits:  
  - Total value is split into random smaller numbers that sum to original  
  - Each new tile has a random color  
  - New tiles placed in random empty nearby cells  
  - If not enough space, fill as many as possible

---

### 6. Special Tiles (Characters)

Spawned one per split. They have a color and a unique ability.

**Core Special Tiles**:

| Name      | Effect                             | Trigger |
|-----------|-------------------------------------|---------|
| Blaster   | Destroys 1 adjacent tile            | Tap     |
| Painter   | Converts adjacent tiles to its color| Passive |
| Freeze    | Prevents tile spawn this turn       | Tap     |
| Doubler   | Doubles value of next merge this turn | Passive |

**Rare Special Tile**:

| Name       | Effect                            | Trigger |
|------------|------------------------------------|---------|
| Expander   | Increases board size from 6×6 to 7×7 | Tap     |

Rarity: ~5% chance per split to generate the rare tile

---

### 7. Scoring System
- +1 per successful merge  
- +Split tile total value  
- +10 bonus for using a special ability  
- +Combo/streak bonus if multiple merges/splits occur in a single swipe

---

### 8. Game Over Conditions
- Board is completely full  
- No valid merges in any direction  
- Game ends → show score, high score, and restart option

---

### 9. Monetization & Progression (Post-MVP)
- Cosmetic themes (tile skins, board styles)  
- Undo token (free once per game, or via ad/currency)  
- Challenge modes (timed, limited moves, etc.)  
- Daily mission system  
- Achievements (e.g., “Make 5 splits in one game”)

---

### 10. Visual Design Notes
- Minimalist grid (dark or neutral tone)  
- Color-coded tiles with bold numbers  
- Special tiles use icons or soft glow animations  
- Smooth tile movement + satisfying merge/split animations

---

### 11. Development Notes
- Developed in Unity 2D  
- Grid managed by a `BoardManager`  
- Each `Tile` is a prefab with:
  - Sprite Renderer (for color)
  - TextMeshPro (for number)  
- Swipe handled by gesture recognition or arrow keys (dev mode)  
- Board can dynamically expand when `Expander` is triggered

---

### Scripts/
```
├── Core/                  # Low-level systems
│   ├── GameManager.cs     # Controls game flow, turn loop, game over, etc.
│   ├── InputManager.cs    # Handles swipe inputs & direction resolution
│   ├── BoardManager.cs    # Controls grid state, tile placement & updates
│   └── Constants.cs       # All global config values (grid size, spawn rates, etc.)
│
├── Tiles/                 # Tile logic (base + types)
│   ├── Tile.cs            # Base tile class: color, number, movement, merge logic
│   ├── TileMover.cs       # Handles movement logic by direction
│   ├── TileMerger.cs      # Handles merge + split logic
│   ├── TileSpawner.cs     # Handles tile creation (random spawn, splits)
│   └── TileAnimator.cs    # Animations: move, merge, split, spawn
│
├── SpecialTiles/          # Special character logic
│   ├── SpecialTile.cs     # Base class for special tile behavior
│   ├── BlasterTile.cs     # Clears adjacent tiles
│   ├── PainterTile.cs     # Color conversion logic
│   ├── FreezeTile.cs      # Delays tile spawn
│   ├── DoublerTile.cs     # Buffs next merge
│   └── ExpanderTile.cs    # Expands grid size
│
├── Grid/                  # Grid-specific utilities and tile placement
│   ├── GridCell.cs        # Represents one cell (position, occupancy, etc.)
│   ├── GridUtils.cs       # Utility functions (e.g., get adjacent, random empty)
│   └── GridResizer.cs     # Dynamically expands grid (e.g. 6x6 → 7x7)
│
├── UI/                    # UI logic & visuals
│   ├── UIManager.cs       # Handles screens, overlays, score panels
│   ├── ScoreManager.cs    # Tracks and displays current score, high score
│   ├── GameOverScreen.cs  # Game over logic
│   └── SpecialTileUI.cs   # Interaction with tappable special tiles
│
├── FX/                    # Sound and visual effects
│   ├── SoundManager.cs    # SoundFX (merge, split, swipe, tap)
│   └── FXManager.cs       # VFX like glow, burst, flash, etc.
│
└── Utils/                 # Helpers and shared logic
    ├── RNGUtils.cs        # Split logic, tile randomization
    ├── Direction.cs       # Enum & helpers for swipe directions
    └── ExtensionMethods.cs # Optional helpful extensions for lists, vectors, etc.
```

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
    - [x] GameManager controls game flow, score tracking, and turn sequence
    - [x] BoardManager maintains grid state and tile positions
    - [x] Create a 6x6 tile grid
        - [x] Grid cells with proper spacing (15-20px gaps between cells)
        - [x] Visual cell indicators (light borders or background shading)
        - [x] Implement grid container with dynamic sizing based on screen dimensions
        - [x] Add subtle grid background pattern for visual clarity
    - [ ] Position tracking system
        - [x] Vector2Int coordinates for logical grid positions
        - [x] Convert between world space and grid coordinates
        - [x] Implement efficient lookup for tiles at specific positions
        - [x] Track occupied vs empty cell states
    - [ ] Empty cell detection system
        - [x] O(1) lookup method for finding empty cells
        - [x] Queue-based system for prioritizing certain spawn locations
        - [x] Method to find random empty cells to a given position (for splits)
    - [ ] Generate 3–5 random starting tiles
        - [x] Random number generator (values 1-5)
        - [x] Color selection from predefined palette (4 base colors)
        - [x] Strategic initial placement algorithm (avoid immediate merges)
        - [x] Smooth spawn-in animation
- [ ] Structure `Tile` prefab (color, number, movement range)
    - [x] SpriteRenderer for tile background and color with rounded corners
    - [x] TextMeshPro component for number display with optimized font settings
    - [x] Movement range property matching tile number value (1-12)
    - [x] Basic animation components for transitions (scale, move, fade)
    - [x] Collision detection for merge interactions
    - [x] Visual indicator for maximum movement range on hover/selection
    - [ ] State machine to handle idle, selected, moving, and merging states
- [ ] Generate Canvas and UI Elements
    - [x] Create main game Canvas with appropriate CanvasScaler settings
    - [x] Implement world-space camera setup for proper rendering and scaling
    - [x] Design game board container with proper anchoring
    - [ ] Add score display and game status panels
    - [ ] Create header area for game title and controls
    - [ ] Implement game over overlay with restart functionality
    - [ ] Set up smooth transitions between UI states
    - [ ] Configure responsive layout for different screen orientations

- [ ] Generate random tiles at the beginning
    - [x] Create 3-5 tiles with random numbers (1-5)
    - [x] Assign random colors from predefined palette
    - [x] Place tiles evenly across the grid
    - [x] Animate tiles appearing on the board
    - [x] Ensure starting configuration has at least one valid move

---

### 🟨 Phase 3: Click Input & Tile Movement
- [x] Detect swipe gestures in 4 directions.
- [x] Move tiles up to their number value.
- [x] Select a tile with mouse click.
- [x] Show valid movement options based on tile's number value.
- [x] Move tile to selected destination with second click.
- [x] Block movement when another tile is in the way.

---

### 🟨 Phase 4: Merge & Split Logic
- [x] Merge same-colored tiles when they collide.
- [x] Add numbers together on merge.
- [x] Split tiles when their value exceeds 12.
- [ ] Add UI interaction to activate special tiles

---

### 🟨 Phase 6: Tile Spawning, Game Flow, and Game Over
- [x] Spawn one random tile after each player move
- [x] Skip spawn if `Freeze` is active
- [x] Detect game over when board is full and no merges are possible
- [x] Track and display current score

### 🟨 Phase 5: Special Tile System
- [ ] Implement `SpecialTile` base class
- [ ] Create 4 common special tiles:
  - Blaster, Painter, Freeze, Doubler
- [ ] Create rare `ExpanderTile` that upgrades board to 7x7
- [ ] Add UI interaction to activate special tiles

---

### 🟨 Phase 6: Tile Spawning, Game Flow, and Game Over
- [x] Spawn one random tile after each player move
- [x] Skip spawn if `Freeze` is active
- [x] Detect game over when board is full and no merges are possible
- [x] Track and display current score

---

### 🟨 Phase 7: Polish and UI
- [ ] Add visual feedback for:
  - Merging, splitting, special abilities
- [ ] Add SFX and minimal VFX
- [ ] Add score UI, game over screen
- [ ] Optional: Add pause/restart buttons

---

### 🟩 Phase 8: Extra Features & Optimization (Post-MVP)
- [ ] Add combo meter / multipliers
- [ ] Challenge levels or endless mode options
- [ ] Implement undo system
- [ ] Mobile optimization (touch input, aspect scaling)
- [ ] Performance profiling and pooling for tiles

---

## 🎯 GameStateManager Integration Plan

To ensure clean, modular, and scalable game logic, `GameStateManager` is introduced as the central authority for managing the game’s state transitions.

---

### 🔗 Integration Targets for `GameStateManager`

---

#### 1. `GameManager.cs`

**Role**: Orchestrates the game loop  
**Integration**:
- Sets initial state to `Init`, then `WaitingForInput`
- Controls the turn cycle:  
  `WaitingForInput → MovingTiles → Merging → Splitting → Spawning → WaitingForInput`
- Triggers the `GameOver` state when the board is full and no valid moves remain

```csharp
GameStateManager.Instance.SetState(GameState.WaitingForInput);
```

---

#### 2. `InputManager.cs`

**Role**: Captures and processes swipe or click input  
**Integration**:
- Only processes input when the state is `WaitingForInput`

```csharp
if (GameStateManager.Instance.Is(GameState.WaitingForInput)) {
    // Allow input and start tile movement
}
```

---

#### 3. `BoardManager.cs`

**Role**: Manages tile movement, merging, and spawning  
**Integration**:
- Sets state transitions based on logic flow:
  - `MovingTiles` when a swipe is applied
  - `Merging` when merge is occurring
  - `Splitting` when a split is triggered
  - `Spawning` after all merges and splits
- Notifies `GameManager` or `GameStateManager` when a phase completes

```csharp
GameStateManager.Instance.SetState(GameState.Merging);
```

---

#### 4. `TileMerger.cs` & `TileSpawner.cs`

**Role**: Handles merging and spawning of tiles  
**Integration**:
- Respect the active state of the game but **do not directly change it**
- Only perform actions when in proper state (e.g., `Spawning`, `Merging`)

---

#### 5. `SpecialTileUI.cs` (or other interactive UI)

**Role**: Allowing players to tap/use special abilities  
**Integration**:
- Only activate special tiles when state allows it

```csharp
if (GameStateManager.Instance.Is(GameState.SpecialTileAction)) {
    // allow activation
}
```

---

### 🧩 Optional Later Integrations

- **`UIManager.cs`**:
  - Show/hide overlays based on state (e.g., Game Over, Pause, Combo Chain)
- **`ScoreManager.cs`**:
  - Adjust scoring logic depending on state (e.g., apply multipliers during `Splitting`)

---

### 🧠 Summary Table

| File               | Uses `GameStateManager` to…                                  |
|--------------------|--------------------------------------------------------------|
| `GameManager.cs`   | Set global state at each step                                |
| `InputManager.cs`  | Only allow input in `WaitingForInput` state                  |
| `BoardManager.cs`  | Transition states for movement, merging, splitting, spawning |
| `TileMerger.cs` & `TileSpawner.cs` | Respect current game state (read-only logic)         |
| `SpecialTileUI.cs` | Limit interaction to valid game states                       |

---