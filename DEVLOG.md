# 🛠️ Development Log – Tilebreakers

## 📅 Recent Updates

### **[Date: 2025-03-26]**
#### **Swipe Enhancements**
- Updated swipe distance calculation to use the grid's cell size instead of screen height.
- Ensured swipe distance is consistent across devices and resolutions.
- Tiles now move based on the swipe distance, limited by their number value.

#### **Tile Movement**
- Implemented dynamic movement logic:
  - Tiles move up to their number value or the swipe distance, whichever is smaller.
  - Movement is smooth and visually satisfying.

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
- **Swipe Input & Tile Movement**:
  - Detect swipe gestures in 4 directions.
  - Move tiles up to their number value.
  - Block movement when another tile is in the way.

- **Merge & Split Logic**:
  - Merge same-colored tiles when they collide.
  - Add numbers together on merge.
  - Split tiles when their value exceeds 12.

- **Special Tile System**:
  - Implement special tiles with unique abilities (e.g., Blaster, Painter, Freeze, Doubler).

- **Game Over Handling**:
  - Detect when the board is full and no valid moves remain.
  - Display game over screen with score and restart options.

---

## 🛣️ Development Roadmap
- [x] Core architecture and grid system.
- [x] Tile initialization and animations.
- [x] Ensure starting configuration has valid moves.
- [x] Fixed gaps between grid cells.
- [ ] Swipe input and tile movement.
- [ ] Merge and split logic.
- [ ] Special tile mechanics.
- [ ] Game over handling and UI polish.

