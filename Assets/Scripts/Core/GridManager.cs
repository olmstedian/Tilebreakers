using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private float gridIntroAnimationDuration = 1.2f;
    [SerializeField] private float cellAnimationDelay = 0.03f;
    [SerializeField] private ParticleSystem gridBurstEffect;
    [SerializeField] private Color gridHighlightColor = new Color(0.4f, 0.8f, 1f, 0.6f);
    
    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    
    private List<GameObject> gridCells = new List<GameObject>();
    
    private void Start()
    {
        if (boardManager == null)
            boardManager = FindObjectOfType<BoardManager>();
            
        // Find all grid cells
        GameObject gridParent = transform.Find("GridBackground")?.gameObject;
        if (gridParent != null)
        {
            foreach (Transform child in gridParent.transform)
            {
                if (child.name.StartsWith("GridCell"))
                {
                    gridCells.Add(child.gameObject);
                }
            }
        }
        
        PlayGridIntroAnimation();
    }
    
    public void PlayGridIntroAnimation()
    {
        // Hide all cells initially
        foreach (GameObject cell in gridCells)
        {
            cell.transform.localScale = Vector3.zero;
        }
        
        // Animate them appearing one by one
        StartCoroutine(AnimateCellsIntro());
    }
    
    private IEnumerator AnimateCellsIntro()
    {
        // Shuffle the grid cells for a random appearance order
        List<GameObject> shuffledCells = new List<GameObject>(gridCells);
        ShuffleList(shuffledCells);
        
        // Wait a moment before starting
        yield return new WaitForSeconds(0.2f);
        
        // Play a burst effect at the center of the grid
        if (gridBurstEffect != null)
        {
            Instantiate(gridBurstEffect, Vector3.zero, Quaternion.identity);
        }
        
        // Animate each cell
        foreach (GameObject cell in shuffledCells)
        {
            LeanTween.scale(cell, Vector3.one, 0.3f).setEaseOutBack();
            yield return new WaitForSeconds(cellAnimationDelay);
        }
        
        // After all cells appear, add a finishing touch
        yield return new WaitForSeconds(0.3f);
        AnimateGridFlash();
    }
    
    public void AnimateGridFlash()
    {
        foreach (GameObject cell in gridCells)
        {
            SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                Color originalColor = renderer.color;
                LeanTween.value(cell, 0f, 1f, 0.5f)
                    .setEaseInOutSine()
                    .setOnUpdate((float val) => {
                        renderer.color = Color.Lerp(gridHighlightColor, originalColor, val);
                    });
            }
        }
    }
    
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
