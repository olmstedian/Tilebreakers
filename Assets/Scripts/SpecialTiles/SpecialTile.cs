using UnityEngine;

public abstract class SpecialTile : MonoBehaviour
{
    public Color tileColor;
    public string specialAbilityName;

    private SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpecialTile requires a SpriteRenderer component.");
        }
    }

    public void Initialize(Color color, string abilityName)
    {
        tileColor = color;
        specialAbilityName = abilityName;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = tileColor;
        }
    }

    public abstract void ActivateAbility();

    protected void DestroyTile()
    {
        Destroy(gameObject);
    }
}
