# 🛠️ Development Log – Tilebreakers

## 📅 Recent Updates

### **[Date: 2025-03-25]**
#### **Swipe Enhancements**
- Updated swipe distance calculation to use the grid's cell size instead of screen height.
- Ensured swipe distance is consistent across devices and resolutions.
- Tiles now move based on the swipe distance, limited by their number value.

#### **Tile Movement**
- Implemented dynamic movement logic:
  - Tiles move up to their number value or the swipe distance, whichever is smaller.
  - Movement is smooth and visually satisfying.

### **[Date: 2025-03-26]**
- **Highlight Tag Fix**: Defined the "Highlight" tag in Unity Editor and ensured all highlight objects are correctly tagged. This resolves the error raised by CompareTag("Highlight") and ensures highlights are cleared after moves.

### **[Date: 2025-03-26]**
- **Input Update**: Removed swipe-based input. The input system now uses mouse clicks for tile selection and movement, simplifying controls and testing.

### **[Date: 2025-03-26]**
- **Movement Block Update**: Enhanced valid move highlighting by blocking highlights beyond an occupied cell. If a tile is in the way, no further highlights appear in that direction, ensuring that tile movement is correctly blocked.

### **[Date: 2025-03-26]**
- **Movement Block Updates**: Enhanced move animations and added audio to move. 

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
- [x] Click/Tap input and tile movement.
- [ ] Merge and split logic.
- [ ] Special tile mechanics.
- [ ] Game over handling and UI polish.

