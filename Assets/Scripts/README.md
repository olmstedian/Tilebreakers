# Scripts Folder – Tilebreakers

## Structure:
- Core/: Game loop, input, board control
- Tiles/: Base tile behavior, merge, move, spawn
- SpecialTiles/: Special characters and powers
- Grid/: Grid cell logic and board expansion
- UI/: Score UI, menus, special tile interaction
- FX/: VFX, SFX
- Utils/: Helpers, enums, extensions

## Naming Rules:
- CamelCase for classes
- snake_case for files if utility/helper
- Verb-first for components (e.g., TileMover, BoardManager)

## Flow:
GameManager → InputManager → BoardManager → Tile logic → Grid updates → UI/FX