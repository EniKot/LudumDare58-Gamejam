using UnityEngine;

/// <summary>
/// Example usage of ColorPicker - demonstrates how to use the ColorPicker events and methods
/// </summary>
public class ExampleColorPickerUsage : MonoBehaviour
{
    [SerializeField] private ColorPicker colorPicker;
    
    [Header("Objects to Apply Colors")]
    [SerializeField] private ExampleColorPickableObject targetObject;
    [SerializeField] private ExampleColorPickableUI targetUI;

    private void OnEnable()
    {
        if (colorPicker != null)
        {
            // Subscribe to events
            colorPicker.OnColorPicked += HandleColorPicked;
            colorPicker.OnColorsMixed += HandleColorsMixed;
            colorPicker.OnColorSlotsCleared += HandleColorSlotsCleared;
        }
    }

    private void OnDisable()
    {
        if (colorPicker != null)
        {
            // Unsubscribe from events
            colorPicker.OnColorPicked -= HandleColorPicked;
            colorPicker.OnColorsMixed -= HandleColorsMixed;
            colorPicker.OnColorSlotsCleared -= HandleColorSlotsCleared;
        }
    }

    private void HandleColorPicked(Color color, int slotIndex)
    {
        Debug.Log($"Color picked and stored in slot {slotIndex}: {color}");
        
        // Example: Apply color to target object based on slot
        if (slotIndex == 0 && targetObject != null)
        {
            targetObject.SetColor(color);
        }
    }

    private void HandleColorsMixed(Color mixedColor)
    {
        Debug.Log($"Colors mixed! Result: {mixedColor}");
        
        // Example: Apply mixed color to target UI
        if (targetUI != null)
        {
            targetUI.SetColor(mixedColor);
        }
        
        // Example: Apply mixed color to target object
        if (targetObject != null)
        {
            targetObject.SetColor(mixedColor);
        }
    }

    private void HandleColorSlotsCleared()
    {
        Debug.Log("Color slots cleared!");
        
        // Reset target colors if needed
        if (targetObject != null)
        {
            targetObject.SetColor(Color.white);
        }
        if (targetUI != null)
        {
            targetUI.SetColor(Color.white);
        }
    }

    // Example: Manual operations
    private void Update()
    {
        if (colorPicker == null) return;

        // Press 'M' to manually mix colors
        if (Input.GetKeyDown(KeyCode.M))
        {
            colorPicker.MixCurrentColors();
        }

        // Press 'C' to clear color slots
        if (Input.GetKeyDown(KeyCode.C))
        {
            colorPicker.ClearColorSlots();
        }

        // Press '1' to get color from slot 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Color color = colorPicker.GetColorFromSlot(0);
            Debug.Log($"Color in slot 1: {color}");
        }

        // Press '2' to set a random color to slot 2
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Color randomColor = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f)
            );
            colorPicker.SetColorToSlot(1, randomColor);
        }
    }

    // Example: Apply current mixed color to an object
    public void ApplyMixedColorToObject(GameObject targetObj)
    {
        if (colorPicker == null || targetObj == null) return;

        var pickable = targetObj.GetComponent<ExampleColorPickableObject>();
        if (pickable != null)
        {
            pickable.SetColor(colorPicker.MixedColor);
        }
    }

    // Example: Get current color slots info
    public void PrintColorSlotsInfo()
    {
        if (colorPicker == null) return;

        Debug.Log($"=== Color Picker Status ===");
        Debug.Log($"Pick Count: {colorPicker.PickCount}");
        Debug.Log($"Mixing Complete: {colorPicker.IsMixingComplete}");
        Debug.Log($"Mixed Color: {colorPicker.MixedColor}");
        
        for (int i = 0; i < colorPicker.ColorSlots.Length; i++)
        {
            Debug.Log($"Slot {i}: {colorPicker.ColorSlots[i]}");
        }
    }
}
