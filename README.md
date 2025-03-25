# 🧩 Tilebreakers

**Tilebreakers** is a minimalist, single-player, color-and-number merge puzzle game built with **Unity 2D**. Swipe, merge, split, and trigger special abilities to manage the board and rack up the highest score—before it fills up.

---

## 🎮 Gameplay Overview

- **Swipe tiles** to move them up to their number value in any direction
- **Merge same-colored tiles** to add their numbers together
- **Split** tiles when the value exceeds 12 into random tiles that total the original
- **Special tiles** spawn from splits and trigger game-changing abilities
- **New tiles** appear after every move
- **Game ends** when the board is full and no valid moves remain

---

## 🧱 Features

- 6×6 starting board with dynamic expansion (7×7 with Expander tile)
- Randomized tile spawning (numbers 1–5)
- Special characters with unique effects (e.g., Blaster, Doubler, Painter, Expander)
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

2.	Open the project in Unity 2022.3+

3.	Load the Scenes/Game.unity

4.	Press Play to start testing

## 📋 DEVLOG

Track day-to-day development progress in **DEVLOG.md**

## ✅ Roadmap (MVP)

- Board setup & movement
- Merge and split system
- Special tile mechanics
- Game over and score handling
- Combo meter and multipliers
- Challenge mode & unlocks
- Mobile input optimizations

## 📬 License

This is a private project under development. Licensing terms TBD.

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
    - [ ] Create a 6x6 tile grid
        - [ ] Grid cells with proper spacing (15-20px gaps between cells)
        - [ ] Visual cell indicators (light borders or background shading)
        - [ ] Implement grid container with dynamic sizing based on screen dimensions
        - [ ] Add subtle grid background pattern for visual clarity
    - [ ] Position tracking system
        - [ ] Vector2Int coordinates for logical grid positions
        - [ ] Convert between world space and grid coordinates
        - [ ] Implement efficient lookup for tiles at specific positions
        - [ ] Track occupied vs empty cell states
    - [ ] Empty cell detection system
        - [ ] O(1) lookup method for finding empty cells
        - [ ] Queue-based system for prioritizing certain spawn locations
        - [ ] Method to find nearest empty cells to a given position (for splits)
    - [ ] Generate 3–5 random starting tiles
        - [ ] Random number generator (values 1-5)
        - [ ] Color selection from predefined palette (4 base colors)
        - [ ] Strategic initial placement algorithm (avoid immediate merges)
        - [ ] Smooth spawn-in animation
    - [ ] Random number generator with appropriate distribution
    - [ ] Initial color assignment (4 primary colors)
    - [ ] Placement algorithm for starting positions
- [ ] Structure `Tile` prefab (color, number, movement range)
    - [x] SpriteRenderer for tile background and color with rounded corners
    - [x] TextMeshPro component for number display with optimized font settings
    - [ ] Movement range property matching tile number value (1-12)
    - [ ] Basic animation components for transitions (scale, move, fade)
    - [ ] Collision detection for merge interactions
    - [ ] Visual indicator for maximum movement range on hover/selection
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

---

### 🟨 Phase 3: Swipe Input & Tile Movement
- [ ] Detect swipe in 4 directions
- [ ] Move each tile up to `tile.number` cells
- [ ] Block movement when another tile is in the way
- [ ] Animate tile movement smoothly

---

### 🟨 Phase 4: Merge & Split Logic
- [ ] Merge same-colored tiles when they collide
- [ ] Add numbers together on merge
- [ ] If merged value > 12 → split into multiple random new tiles
- [ ] Each split triggers one special tile to spawn nearby

---

### 🟨 Phase 5: Special Tile System
- [ ] Implement `SpecialTile` base class
- [ ] Create 4 common special tiles:
  - Blaster, Painter, Freeze, Doubler
- [ ] Create rare `ExpanderTile` that upgrades board to 7x7
- [ ] Add UI interaction to activate special tiles

---

### 🟨 Phase 6: Tile Spawning, Game Flow, and Game Over
- [ ] Spawn one random tile after each player move
- [ ] Skip spawn if `Freeze` is active
- [ ] Detect game over when board is full and no merges are possible
- [ ] Track and display current score

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