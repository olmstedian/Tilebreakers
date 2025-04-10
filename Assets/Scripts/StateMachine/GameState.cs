using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Abstract base class for all game states
/// </summary>
public abstract class GameState
{
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
    
    public virtual void HandleInput(Vector2Int gridPosition) { }
}

// BootState has been moved to its own file
// MainMenuState has been moved to its own file
// LoadingLevelState has been moved to its own file
// InitGameState has been moved to its own file
// MovingTilesState has been moved to its own file
// MergingTilesState has been moved to its own file
// SplittingTilesState has been moved to its own file
// SpawningNewTileState has been moved to its own file
// SpecialTileActionState has been moved to its own file
// CheckingGameOverState has been moved to its own file
// GameOverState has been moved to its own file
// PauseState has been moved to its own file
// SpecialTileActivationState has been moved to its own file
// SpecialTileSpawningState has been moved to its own file
// LevelCompleteState has been moved to its own file
// LevelFailedState has been moved to its own file
// GameCompleteState has been moved to its own file

