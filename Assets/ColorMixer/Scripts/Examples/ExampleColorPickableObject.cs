using UnityEngine;

/// <summary>
/// Example implementation of IColorPickable for 3D objects with MeshRenderer
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class ExampleColorPickableObject : MonoBehaviour, IColorPickable<MeshRenderer>
{
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    public Color GetColor()
    {
        if (meshRenderer == null || meshRenderer.sharedMaterial == null)
            return Color.white;

        // Get color from material property block or material
        meshRenderer.GetPropertyBlock(propertyBlock);
        
        if (propertyBlock.isEmpty)
        {
            // Fall back to material color
            return meshRenderer.sharedMaterial.HasProperty("_Color") 
                ? meshRenderer.sharedMaterial.GetColor("_Color") 
                : Color.white;
        }
        
        return propertyBlock.GetColor("_Color");
    }

    public void OnColorPicked(Color color, MeshRenderer target)
    {
        Debug.Log($"{gameObject.name} color was picked: {color}");
        // You can add visual feedback here, like a highlight effect
    }

    /// <summary>
    /// Helper method to set the color of this object
    /// </summary>
    public void SetColor(Color color)
    {
        if (meshRenderer == null) return;

        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", color);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}
