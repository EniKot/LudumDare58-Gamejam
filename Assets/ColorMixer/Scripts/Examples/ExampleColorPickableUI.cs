using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Example implementation of IColorPickable for UI Image components
/// </summary>
[RequireComponent(typeof(Image))]
public class ExampleColorPickableUI : MonoBehaviour, IColorPickable<Image>
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public Color GetColor()
    {
        if (image == null)
            return Color.white;

        return image.color;
    }

    public void OnColorPicked(Color color, Image target)
    {
        Debug.Log($"{gameObject.name} UI color was picked: {color}");
        // You can add visual feedback here
    }

    /// <summary>
    /// Helper method to set the color of this UI element
    /// </summary>
    public void SetColor(Color color)
    {
        if (image != null)
        {
            image.color = color;
        }
    }
}
