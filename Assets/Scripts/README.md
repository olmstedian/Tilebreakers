# Scripts Folder – Tilebreakers

## Directory Structure
- **Core/**: Game loop, input handling, and board control systems
- **Tiles/**: Base tile behaviors including merging, movement, and spawn logic
- **SpecialTiles/**: Special character implementations and power-up systems
- **Grid/**: Grid cell management and dynamic board expansion
- **UI/**: Score display, menu systems, and special tile interaction interfaces
- **FX/**: Visual effects and sound management
- **Utils/**: Helper functions, enumerations, and extension methods

## Naming Conventions
- Use **PascalCase** for class names
- Use **snake_case** for utility/helper files
- Prefix component names with verbs (e.g., `TileMover`, `BoardManager`)

## Execution Flow
```
GameManager → InputManager → BoardManager → Tile Logic → Grid Updates → UI/FX
```