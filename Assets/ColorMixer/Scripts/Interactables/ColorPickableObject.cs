using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ColorPickableObject : MonoBehaviour, IColorPickable
{
    [SerializeField] private Color color;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] public bool usePredefinedColor = false;
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (!usePredefinedColor)
        {
            color = spriteRenderer.color;
        }
        
    }
    
    public Color GetColor()
    {
        return color;
    }
    
    public void OnColorPicked()
    {
        Debug.Log($"Color picked from {gameObject.name}: {color}");
    }
    
}
