# 🛠️ Development Log – Tilebreakers

## 📅 Recent Updates

### **[Date: 2025-03-30]**
#### **Special Tile System**
- Added new special tiles:
  - **FreezeTile**: Freezes adjacent tiles for one turn.
  - **DoublerTile**: Doubles the value of adjacent tiles.
- Improved `SpecialTileManager` to handle spawning and activation of special tiles.
- Enhanced `BlasterTile` logic to destroy adjacent tiles and clear their positions on the board.

#### **Game State System**
- Refactored `GameStateManager` to support delayed transitions and improved state handling.
- Added new states for special tile activation and spawning:
  - `SpecialTileActivationState`
  - `SpecialTileSpawningState`

#### **Tile Splitting Enhancements**
- Centralized all splitting logic into `TileSplitter.cs`.
- Added score calculation for splits based on the total value of resulting tiles.
- Improved randomization of split tile values and positions.

#### **Scoring System**
- Improved `ScoreManager` to handle:
  - Merge score: `+1` point for the merge itself and the merged tile's final number.
  - Split score: Total value of resulting split tiles.
  - Special tile activation bonus: `+10` points.
- Ensured score updates are reflected in the UI.

#### **Game Over System**
- Enhanced `GameOverManager` to detect when the board is full and no valid merges exist.
- Integrated `GameOverState` to handle game over transitions and display the game over screen.

#### **UI Enhancements**
- Updated `UIManager` to handle:
  - Game over screen with final score display.
  - Pause and resume functionality.
  - Resetting the top bar UI (score and move count) on game restart.

---

## 📅 Previous Updates

### **[Date: 2025-03-29]**
#### **Gameplay Enhancements**
- Improved tile splitting logic to prioritize non-adjacent cells for better gameplay.
- Enhanced randomization of split tile values and positions.
- Updated `SpawningNewTileState` to ensure tiles spawn in random locations, avoiding adjacent cells of merged tiles.

#### **Special Tile System**
- Added `ScoreManager.Instance.AddSpecialTileBonus()` to reward players with points when activating special tiles.
- Updated `SpecialTile` base class to handle score addition during activation.
- Improved `BlasterTile` logic to destroy adjacent tiles and clear their positions on the board.

#### **Scoring System**
- Improved `ScoreManager` to handle:
  - Merge score: `+1` point for the merge itself and the merged tile's final number.
  - Split score: Total value of resulting split tiles.
  - Special tile activation bonus: `+10` points.
- Ensured score updates are reflected in the UI.

#### **Game Over System**
- Enhanced `GameOverManager` to detect when the board is full and no valid merges exist.
- Integrated `GameOverState` to handle game over transitions and display the game over screen.

#### **UI Enhancements**
- Updated `UIManager` to handle:
  - Game over screen with final score display.
  - Pause and resume functionality.
  - Resetting the top bar UI (score and move count) on game restart.

---

## 📅 Previous Updates

### **[Date: 2025-03-28]**
#### **Game Over System**
- Added `GameOverManager` to detect when the board is full and no valid moves remain.
- Integrated `GameOverState` to handle game over transitions.
- Updated `UIManager` to display the game over screen with the final score.

#### **Tile Splitting Enhancements**
- Improved tile splitting logic to prioritize non-adjacent cells for better gameplay.
- Added random color assignment for split tiles.
- Fixed issues with split tile spawning in occupied cells.

#### **Special Tile System**
- Added `BlasterTile` to destroy adjacent tiles when activated.
- Created `SpecialTileUI` to handle player interaction with special tiles.

---

## 📅 Previous Updates

### **[Date: 2025-03-27]**
#### **Tile Splitting System**
- Implemented full tile splitting functionality when merged tiles exceed value 12.
- Added logic for generating multiple new tiles with values that sum to the original.
- Randomized color assignment for split tiles.
- Optimized spawn positions to prioritize non-adjacent cells for better gameplay.

#### **Tile Number Display**
- Fixed issues with TextMeshPro components on spawned tiles.
- Improved font loading and text visibility.
- Enhanced verification processes to ensure numbers display correctly.

#### **Input System**
- Removed swipe-based input and replaced it with mouse click-based selection and movement.
- Simplified controls for better testing and gameplay.

#### **Movement Enhancements**
- Blocked valid move highlights beyond occupied cells.
- Improved animations for tile movement and merging.

---

## 📅 Previous Updates

### **[Date: 2025-03-26]**
#### **Tile Enhancements**
- Added dynamic brightness to tiles based on their number for better visual hierarchy.
- Implemented text outlines for improved readability.
- Added animations:
  - Smooth spawn animation for tiles.
  - Pulse animation for merges.
  - Smooth movement animations for tile transitions.

#### **Grid and Cell Improvements**
- Updated grid background to use a uniform light gray color.
- Enhanced cell indicators with subtle scaling and layering for better clarity.
- Fixed gaps between grid cells by slightly increasing the scale of background cells.

#### **Game Initialization**
- Ensured the starting configuration always has at least one valid move:
  - Added a `HasValidMove()` method to check for adjacent tiles with the same color and number.
  - Regenerates the board if no valid moves exist.

### **[Date: 2025-03-25]**
#### **Core Systems**
- Implemented `GameManager` to control game flow, turn sequence, and game over conditions.
- Added `InputManager` to handle swipe gestures and detect directional input.

#### **Board Management**
- Created `BoardManager` to manage grid state, tile placement, and updates.
- Added methods for:
  - Initializing the grid.
  - Generating random starting tiles.
  - Managing empty cells and prioritized spawn locations.

#### **Tile Logic**
- Developed `Tile` class with:
  - Color and number properties.
  - Visual updates based on tile state.
  - Smooth movement and merge animations.

---

## 📅 Upcoming Tasks

### **Core Gameplay**
- [ ] Add additional special tiles with unique abilities (e.g., Painter, Freeze, Doubler).
- [ ] Implement combo mechanics for consecutive merges or splits.
- [ ] Add sound effects for tile interactions and special tile activations.

### **Game Over Handling**
- [ ] Add restart and main menu buttons to the game over screen.
- [ ] Implement a "revive" mechanic to allow players to continue after game over.

### **Polish & Optimization**
- [ ] Add more visual feedback for merging and splitting.
- [ ] Optimize tile object pooling for better performance.
- [ ] Improve animations for smoother transitions and interactions.

### **Level System**
- [ ] Add more levels with increasing difficulty.
- [ ] Implement level objectives (e.g., score targets, limited moves).
- [ ] Add a level select screen to navigate between levels.

---

## 🛣️ Development Roadmap

### ✅ Phase 1: Core Architecture
- [x] Create `GameManager`, `BoardManager`, and `InputManager`.
- [x] Implement a 6×6 tile grid with proper spacing and visual indicators.
- [x] Generate random starting tiles with valid moves.

### ✅ Phase 2: Tile Movement and Merging
- [x] Detect mouse clicks for tile selection and movement.
- [x] Highlight valid movement options based on tile's number value.
- [x] Merge same-colored tiles and add their numbers.

### ✅ Phase 3: Tile Splitting and Special Tiles
- [x] Split tiles when their value exceeds 12.
- [x] Spawn special tiles during splits.
- [x] Implement `BlasterTile` to destroy adjacent tiles.
- [x] Add `FreezeTile` and `DoublerTile` with unique abilities.

### ✅ Phase 4: Scoring and Game Over
- [x] Implement `ScoreManager` to track and update the player's score.
- [x] Add score display to the top bar UI.
- [x] Detect game over when the board is full and no valid moves remain.

### 🟨 Phase 5: Level System
- [x] Create `LevelManager` to manage level progression.
- [x] Add `LevelData` to define level-specific configurations.
- [ ] Add level objectives (e.g., score targets, limited moves).
- [ ] Implement a level select screen.

### 🟨 Phase 6: UI and Visual Feedback
- [x] Add game over screen with final score display.
- [x] Add pause and resume functionality.
- [ ] Add visual feedback for merging, splitting, and special abilities.
- [ ] Add animations for tile spawning and transitions.

### 🟩 Phase 7: Advanced Features
- [ ] Add combo mechanics for consecutive merges or splits.
- [ ] Implement undo functionality for player moves.
- [ ] Add daily challenges or rotating objectives.
- [ ] Optimize performance with tile object pooling.

---

## 🧩 Notes and Ideas
- **Special Tiles**: Consider adding more special tiles with unique abilities, such as:
  - **PainterTile**: Changes the color of adjacent tiles.
  - **FreezeTile**: Prevents adjacent tiles from moving for one turn.
  - **DoublerTile**: Doubles the value of adjacent tiles.

- **Level Objectives**: Introduce objectives like:
  - Merge a specific number of tiles.
  - Reach a score target within a limited number of moves.
  - Use a specific number of special tiles.

- **Daily Challenges**: Add a daily challenge mode with unique objectives and rewards.

- **Combo Mechanics**: Reward players for consecutive merges or splits with score multipliers or special effects.

- **Polish**: Focus on improving animations, sound effects, and overall user experience.

---

## 📋 Summary
This development log provides a clear overview of recent updates, upcoming tasks, and the overall roadmap for Tilebreakers. The focus is on enhancing gameplay, adding new features, and improving the user experience.

