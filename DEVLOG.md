# 🛠️ Development Log – Tilebreakers

## 📅 Recent Updates

### **[Date: 2025-03-29]**
#### **Special Tile System**
- Added `ScoreManager.Instance.AddSpecialTileBonus()` to reward players with points when activating special tiles.
- Updated `SpecialTile` base class to handle score addition during activation.
- Improved `BlasterTile` logic to destroy adjacent tiles and clear their positions on the board.

#### **Tile Splitting Enhancements**
- Centralized all splitting logic into `TileSplitter.cs`.
- Added score calculation for splits based on the total value of resulting tiles.
- Improved randomization of split tile values and positions.

#### **Game State Improvements**
- Enhanced `SplittingTilesState` to use `TileSplitter` for splitting operations.
- Updated `SpawningNewTileState` to avoid spawning tiles adjacent to merged cells.

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

#### **Scoring System**
- Implemented `ScoreManager` to track and update the player's score.
- Added score display to the top bar UI.

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

- **Special Tile System**:
  - Implement additional special tiles with unique abilities (e.g., Painter, Freeze, Doubler).

- **Game Over Handling**:
  - Add restart and main menu buttons to the game over screen.

- **Polish & Optimization**:
  - Add more visual feedback for merging and splitting.
  - Optimize tile object pooling for better performance.
  - Add sound effects for tile interactions.

---

## 🛣️ Development Roadmap
- [x] Core architecture and grid system.
- [x] Tile initialization and animations.
- [x] Ensure starting configuration has valid moves.
- [x] Click/Tap input and tile movement.
- [x] Merge and split logic.
- [x] Special tile mechanics (BlasterTile).
- [x] Game over handling and UI polish.
- [ ] Additional special tiles (Painter, Freeze, Doubler).
- [ ] Sound effects and performance optimization.

