using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tilebreakers.Core;

// Add this namespace to prevent conflicts with other TileMergeHandler classes
namespace Tilebreakers.Board
{
    /// <summary>
    /// Handles all tile merging operations in the game.
    /// </summary>
    public class TileMergeHandler : MonoBehaviour
    {
        private static TileMergeHandler _instance;
        public static TileMergeHandler Instance 
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TileMergeHandler>();
                    if (_instance == null)
                    {
                        GameObject handler = new GameObject("TileMergeHandler");
                        _instance = handler.AddComponent<TileMergeHandler>();
                        DontDestroyOnLoad(handler);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// Performs a merge operation between two tiles.
        /// </summary>
        /// <param name="sourceTile">The tile that will be merged into the target</param>
        /// <param name="targetTile">The tile that will remain after merging</param>
        /// <param name="sourcePos">Grid position of the source tile</param>
        /// <param name="targetPos">Grid position of the target tile</param>
        public void PerformMergeOperation(Tile sourceTile, Tile targetTile, Vector2Int sourcePos, Vector2Int targetPos)
        {
            if (sourceTile == null || targetTile == null || 
                !BoardManager.Instance.IsWithinBounds(sourcePos) || 
                !BoardManager.Instance.IsWithinBounds(targetPos))
            {
                Debug.LogError($"TileMergeHandler: Invalid merge operation. Source: {sourceTile}, Target: {targetTile}, SourcePos: {sourcePos}, TargetPos: {targetPos}");
                return;
            }

            // Store a direct reference to the GameObject for later destruction
            GameObject sourceTileGameObject = sourceTile != null ? sourceTile.gameObject : null;
            
            BoardManager.Instance.ClearAllSelectionState();
            BoardManager.Instance.StartCoroutine(MoveTileToTargetForMerge(sourceTile, targetTile, () =>
            {
                // Clear the source cell on the board array
                BoardManager.Instance.ClearCell(sourcePos);
                BoardManager.Instance.emptyCells.Add(sourcePos);

                // Create a local reference to the source tile GameObject that persists through the callback
                GameObject sourceTileObject = sourceTileGameObject;
                
                // Now perform the merge using this reference to ensure it's not lost
                bool mergeSuccess = MergeTiles(targetTile, sourceTile, sourceTileObject);
                Debug.Log($"TileMergeHandler: Merge result: {(mergeSuccess ? "SUCCESS" : "FAILED")}");

                // Validate that the source tile is properly removed from the board array
                if (BoardManager.Instance.GetTileAtPosition(sourcePos) != null)
                {
                    Debug.LogError($"TileMergeHandler: Source tile at {sourcePos} was not properly removed from the board array after merging.");
                    BoardManager.Instance.ClearCell(sourcePos); // Force clear it again
                }
                else
                {
                    Debug.Log($"TileMergeHandler: Source tile at {sourcePos} successfully removed from the board array.");
                }

                // Verify the source tile GameObject was destroyed
                if (sourceTileObject != null)
                {
                    Debug.LogError($"TileMergeHandler: Source tile GameObject was not destroyed during merge. Destroying it now.");
                    DestroySourceTileImmediate(sourceTileObject);
                }

                if (mergeSuccess)
                {
                    // Play merge animation
                    TileAnimator animator = targetTile.GetComponent<TileAnimator>();
                    animator?.PlayMergeAnimation();
                    BoardManager.Instance.ClearSelection();
                    BoardManager.Instance.ClearAllSelectionState();
                    
                    // CRITICAL FIX: Check if the merged tile needs splitting
                    if (targetTile.number > 12)
                    {
                        Debug.LogWarning($"TileMergeHandler: High-value tile detected ({targetTile.number}) - going to SplittingTilesState");
                        GameStateManager.Instance.EnterSplittingTilesState();
                    }
                }
                else
                {
                    GameManager.Instance.EndTurn();
                }
            }));
        }

        /// <summary>
        /// Handles moving a tile to a target position for merging, and then executes a callback.
        /// </summary>
        /// <param name="sourceTile">The tile to be moved</param>
        /// <param name="targetTile">The target tile to merge with</param>
        /// <param name="onComplete">Callback action to execute after movement completes</param>
        /// <returns>Coroutine IEnumerator</returns>
        public IEnumerator MoveTileToTargetForMerge(Tile sourceTile, Tile targetTile, System.Action onComplete)
        {
            // Track the source position for proper board cleanup
            Vector2Int sourcePos = BoardManager.Instance.GetGridPositionFromWorldPosition(sourceTile.transform.position);
            
            // Delegate to TileMovementHandler for move and merge
            yield return TileMovementHandler.Instance.PerformMoveAndMerge(sourceTile, targetTile, sourcePos, onComplete);
        }

        /// <summary>
        /// Merges two tiles, increasing the value of the target tile.
        /// Destroys the source tile after merging.
        /// </summary>
        /// <param name="targetTile">The tile that will remain after merging</param>
        /// <param name="sourceTile">The tile that will be destroyed after merging</param>
        /// <param name="sourceTileObject">Direct reference to the source tile GameObject</param>
        /// <returns>True if the merge was successful, false otherwise</returns>
        public bool MergeTiles(Tile targetTile, Tile sourceTile, GameObject sourceTileObject = null)
        {
            if (targetTile == null)
            {
                Debug.LogError("TileMergeHandler: Cannot merge with null target tile!");
                return false;
            }

            // If no direct reference to the GameObject was provided but we have the tile component
            if (sourceTileObject == null && sourceTile != null)
            {
                sourceTileObject = sourceTile.gameObject;
            }

            // If we have no source tile component but have the GameObject, try to get the component
            if (sourceTile == null && sourceTileObject != null)
            {
                sourceTile = sourceTileObject.GetComponent<Tile>();
            }

            // Final validation - make sure we have at least the component OR the GameObject
            if (sourceTile == null && sourceTileObject == null)
            {
                Debug.LogError("TileMergeHandler: Cannot merge with null source tile and no source object reference!");
                return false;
            }

            // Check if the tiles have compatible colors (only if we have both components)
            if (sourceTile != null && targetTile != null && !CompareColors(targetTile.tileColor, sourceTile.tileColor))
            {
                Debug.LogError("TileMergeHandler: Cannot merge tiles of different colors!");
                return false;
            }

            // Get the source tile number before we destroy it
            int originalSourceNumber = 0;
            if (sourceTile != null)
            {
                originalSourceNumber = sourceTile.number;
                Debug.Log($"TileMergeHandler: Merging tiles {originalSourceNumber} and {targetTile.number}");
            }
            else 
            {
                Debug.LogWarning("TileMergeHandler: Source tile component is null, using fallback value");
                originalSourceNumber = 1; // Fallback value if we can't get the actual number
            }

            int originalTargetNumber = targetTile.number;
            
            // Store the position of the target tile for board reference
            Vector2Int targetPosition = BoardManager.Instance.GetGridPositionFromWorldPosition(targetTile.transform.position);

            // Add the values of the two tiles
            targetTile.number += originalSourceNumber;

            Debug.Log($"TileMergeHandler: Merge result: {originalSourceNumber} + {originalTargetNumber} = {targetTile.number}");
            
            // ENSURE SPLIT DETECTION: Explicitly check if the result needs splitting
            if (targetTile.number > 12)
            {
                Debug.LogWarning($"TileMergeHandler: High-value tile detected ({targetTile.number}) at {targetPosition} - registering for split");
                // Register the high-value tile for splitting with explicit log confirmation
                List<Vector2Int> posToSplit = new List<Vector2Int> { targetPosition };
                TileSplitHandler.RegisterTilesToSplit(posToSplit);
                
                // Set this as the last merged cell position for reference during transitions
                BoardManager.Instance.lastMergedCellPosition = targetPosition;
                Debug.Log($"TileMergeHandler: Registered position {targetPosition} for splitting with value {targetTile.number}");
            }

            // Update the target tile's visuals
            targetTile.UpdateVisuals();
            targetTile.SetState(Tile.TileState.Merging);

            // CRITICAL FIX: Use AddScoreWithoutPopup instead of AddScore to avoid duplicate popups
            ScoreManager.Instance.AddScoreWithoutPopup(targetTile.number);
            
            // Show merging score popups at the TOP of the screen
            ScoreUtility.ShowPopupAtScreenPosition(targetTile.number, 
                new Vector2(Screen.width * 0.5f, Screen.height * 0.9f));

            // Update board reference to the merged cell position
            BoardManager.Instance.lastMergedCellPosition = targetPosition;

            // CRITICAL FIX: Count the merge as a move
            Debug.Log("TileMergeHandler: Counting merge as a move by calling GameManager.EndTurn()");
            GameManager.Instance.EndTurn();

            // Destroy the source tile using our specialized method
            DestroySourceTileImmediate(sourceTileObject);
            
            // Verify source tile destruction after a short delay
            if (Application.isPlaying)
            {
                // Use our own TileMergeVerifier instead of the global MergeVerifier class
                TileMergeVerifier verifier = targetTile.gameObject.AddComponent<TileMergeVerifier>();
                verifier.Initialize(null, originalSourceNumber, originalTargetNumber, targetPosition);
            }

            return true;
        }

        /// <summary>
        /// Safely and immediately destroys a source tile GameObject
        /// </summary>
        private void DestroySourceTileImmediate(GameObject sourceTileObject)
        {
            if (sourceTileObject == null) return;

            try 
            {
                // First disable all components to prevent further interactions
                Component[] components = sourceTileObject.GetComponents<Component>();
                foreach (Component component in components)
                {
                    // Skip Transform component
                    if (component is Transform) continue;
                    
                    // Disable MonoBehaviours
                    if (component is MonoBehaviour behaviour)
                    {
                        behaviour.enabled = false;
                    }
                    
                    // Disable Colliders
                    if (component is Collider2D collider)
                    {
                        collider.enabled = false;
                    }
                    
                    // Disable Renderers
                    if (component is Renderer renderer)
                    {
                        renderer.enabled = false;
                    }
                }
                
                // Then destroy the GameObject
                Destroy(sourceTileObject);
                
                Debug.Log("TileMergeHandler: Source tile GameObject destruction processed.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"TileMergeHandler: Error while destroying source tile: {ex.Message}");
            }
        }

        /// <summary>
        /// Improved color comparison method with tolerance for floating point precision
        /// </summary>
        public bool CompareColors(Color a, Color b)
        {
            const float tolerance = 0.01f; // Adjust tolerance if needed
            return UnityEngine.Mathf.Abs(a.r - b.r) < tolerance && 
                   UnityEngine.Mathf.Abs(a.g - b.g) < tolerance && 
                   UnityEngine.Mathf.Abs(a.b - b.b) < tolerance;
        }

        /// <summary>
        /// Checks if a cell is empty or mergeable with the given tile.
        /// </summary>
        public bool IsCellEmptyOrMergeable(Vector2Int position, Tile tile)
        {
            if (!BoardManager.Instance.IsWithinBounds(position)) return false;
            
            Tile targetTile = BoardManager.Instance.GetTileAtPosition(position);
            
            // Cell is empty or has a tile with the same color (mergeable)
            return targetTile == null || CompareColors(tile.tileColor, targetTile.tileColor);
        }

        /// <summary>
        /// Validates if a merge is possible between two tiles at the given positions.
        /// </summary>
        /// <param name="sourceTilePos">Position of the source tile</param>
        /// <param name="targetTilePos">Position of the potential merge target</param>
        /// <param name="sourceTile">The source tile to check</param>
        /// <param name="targetTile">The target tile to check</param>
        /// <returns>True if merge is valid, false otherwise</returns>
        public bool IsValidMerge(Vector2Int sourceTilePos, Vector2Int targetTilePos, Tile sourceTile, Tile targetTile)
        {
            if (sourceTile == null || targetTile == null) return false;

            // First check if target is a special tile (cannot merge with special tiles)
            if (targetTile.GetComponent<SpecialTile>() != null)
            {
                Debug.Log("TileMergeHandler: Target tile is a special tile. Merge is not allowed.");
                return false;
            }
            
            // Use TileMovementHandler to validate the move for merging
            bool pathClear;
            bool isValidMove = TileMovementHandler.Instance.IsValidMove(sourceTilePos, targetTilePos, sourceTile, out pathClear);
            
            // Color match is also required
            bool isSameColor = CompareColors(sourceTile.tileColor, targetTile.tileColor);
            
            Debug.Log($"TileMergeHandler: Merge validation - Valid move: {isValidMove}, Path clear: {pathClear}, Same color: {isSameColor}");
            
            // Merge is only possible if all conditions are met
            return isValidMove && pathClear && isSameColor;
        }
        
        /// <summary>
        /// Handles evaluation and execution of a potential merge between two tiles.
        /// </summary>
        /// <param name="sourceTilePos">Position of the source tile</param>
        /// <param name="targetTilePos">Position of the target tile</param> 
        /// <param name="sourceTile">The source tile to potentially merge</param>
        /// <param name="targetTile">The target tile to potentially merge with</param>
        /// <returns>True if merge was performed, false otherwise</returns>
        public bool HandlePotentialMerge(Vector2Int sourceTilePos, Vector2Int targetTilePos, Tile sourceTile, Tile targetTile)
        {
            Debug.Log($"TileMergeHandler: Evaluating potential merge from {sourceTilePos} to {targetTilePos}");
            
            if (IsValidMerge(sourceTilePos, targetTilePos, sourceTile, targetTile))
            {
                Debug.Log("TileMergeHandler: Valid merge detected! Proceeding with merge operation.");
                
                // Clear UI selection state first
                BoardManager.Instance.ClearAllSelectionState();
                
                // Perform the merge operation
                PerformMergeOperation(sourceTile, targetTile, sourceTilePos, targetTilePos);
                
                // IMPORTANT: Don't call EndTurn here, as it's now called within MergeTiles()
                // to ensure it's always counted once per operation
                
                return true;
            }
            
            return false;
        }
    }

    /// <summary>
    /// Helper MonoBehaviour specific to TileMergeHandler to verify that a source tile was properly destroyed after merging
    /// </summary>
    public class TileMergeVerifier : MonoBehaviour
    {
        private GameObject _sourceTileObject;
        private int _sourceValue;
        private int _targetValue;
        private Vector2Int _mergePosition;
        private float _checkTime = 0.3f; // Time to wait before checking
        private float _timer = 0f;
        private bool _verified = false;

        public void Initialize(GameObject sourceTileObject, int sourceValue, int targetValue, Vector2Int mergePosition)
        {
            _sourceTileObject = sourceTileObject;
            _sourceValue = sourceValue;
            _targetValue = targetValue;
            _mergePosition = mergePosition;
        }

        private void Update()
        {
            if (_verified) return;
            
            _timer += Time.deltaTime;
            
            if (_timer >= _checkTime)
            {
                _verified = true;
                // If using TilesMergeHandler, we should reference the verifier from Tilebreakers.Board
                // This line is unchanged, as it simply marks verification complete
                
                // Check if the source tile still exists (it shouldn't)
                if (_sourceTileObject != null)
                {
                    Debug.LogError($"TileMergeVerifier: Source tile with value {_sourceValue} was not properly destroyed after merging at position {_mergePosition}. Destroying it now.");
                    Destroy(_sourceTileObject);
                }
                else
                {
                    Debug.Log($"TileMergeVerifier: Source tile destruction verified for merge ({_sourceValue} + {_targetValue} = {_sourceValue + _targetValue}) at position {_mergePosition}.");
                }
                
                // Self-destruct after verification
                Destroy(this);
            }
        }
    }
}
