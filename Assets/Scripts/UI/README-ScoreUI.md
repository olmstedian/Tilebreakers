# Score UI System Setup Guide

This guide explains how to set up the score display UI system in your Tilebreakers game scene.

## Prerequisites

- TextMeshPro package installed in your project
- Basic UI Canvas set up in your scene

## Step 1: Add ScoreManager Component

1. Create an empty GameObject named "ScoreManager" in your scene.
2. Add the `ScoreManager` component to it.
3. Configure the initial score, high score settings, and bonus values as needed.

## Step 2: Set Up Score Display UI

1. In your UI Canvas, create a new empty GameObject named "ScoreDisplayUI".
2. Add the `ScoreDisplayUI` component to it.
3. Create two TextMeshPro - Text (UI) components as children of ScoreDisplayUI:
   - Name one "CurrentScoreText" and set its initial text to "Score: 0"
   - Name the other "HighScoreText" and set its initial text to "High Score: 0"
4. Create an empty GameObject named "ScorePopupContainer" as a child of the Canvas.
   - This will serve as the parent for all score popup instances
   - Set its RectTransform to cover the entire screen (stretch anchors)
5. Position these elements where you want them to appear in the UI.

## Step 3: Link UI Components

1. With ScoreDisplayUI selected, in the Inspector click "Find UI References" to automatically find and link the text components.
2. Click "Create Score Popup Prefab" to generate a score popup prefab.

## Step 4: Test the System

1. Enter Play mode.
2. With ScoreManager selected, use the debug buttons in the Inspector to test:
   - Add 100 Points
   - Test Score Popup
   - Reset Score/High Score

## Advanced Configuration

### ScoreDisplayUI Options

- **Animation Settings**: Configure how score changes animate
  - Enable/disable animations
  - Change animation duration and curve
  - Customize positive and negative score colors

### ScorePopup Customization

1. Locate the generated ScorePopupPrefab in Assets/Prefabs/UI/
2. Customize the prefab's appearance by:
   - Changing the font size or style
   - Adjusting the animation curves
   - Modifying the colors and shadow effects

## Runtime Usage

To add points from your game logic:
```csharp
// Add points to the player's score
ScoreManager.Instance.AddScore(100);

// Show a score popup at a specific position
Vector2 worldPosition = tile.transform.position;
ScoreManager.ShowScorePopupAtPosition(100, worldPosition, "+100 Great Move!");
```

## Advanced Integration Tips

- Link ScoreManager events to other game systems
- Use combo mechanics for advanced scoring
- Create different score popups for different events
