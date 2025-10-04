using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    
    [Header("Color Mixing Settings")]
    [SerializeField] private int maxColorSlots = 2;
    [SerializeField] private bool autoMixOnSecondPick = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGUI = true;
    #endregion

    #region Public Properties
    public Color[] ColorSlots => colorSlots;
    public Color MixedColor => mixedColor;
    public int PickCount => pickCount;
    public bool IsMixingComplete => mixedDone;
    #endregion

    #region Private Fields
    private Color[] colorSlots;
    private Color mixedColor;
    private int pickCount = 0;
    private bool mixedDone = false;
    #endregion

    #region Events
    public event Action<Color, int> OnColorPicked;
    public event Action<Color> OnColorsMixed;
    public event Action OnColorSlotsCleared;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        colorSlots = new Color[maxColorSlots];
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        HandleInput();
    }

    private void OnGUI()
    {
        if (showDebugGUI)
        {
            DrawDebugColorSlots();
        }
    }
    #endregion

    #region Input Handling
    private void HandleInput()
    {
        // Check for right mouse button click
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            TryPickColor();
        }
    }
    #endregion

    #region Color Picking
    private void TryPickColor()
    {
        Color? pickedColor = TryPickColorFrom3DObject() ?? TryPickColorFromUI();

        if (pickedColor == null)
        {
            Debug.Log("No color pickable object found at mouse position");
            return;
        }

        Debug.Log($"Picked Color: {pickedColor.Value}");
        ProcessPickedColor(pickedColor.Value);
    }

    private Color? TryPickColorFrom3DObject()
    {
        if (mainCamera == null || Mouse.current == null)
            return null;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return null;

        return GetColorFromPickableComponents(hit.collider.gameObject);
    }

    private Color? TryPickColorFromUI()
    {
        if (EventSystem.current == null || Mouse.current == null)
            return null;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            Color? color = GetColorFromPickableComponents(result.gameObject);
            if (color.HasValue)
                return color;
        }

        return null;
    }

    private Color? GetColorFromPickableComponents(GameObject targetObject)
    {
        // Try to find any component that implements IColorPickable with any target type
        var components = targetObject.GetComponents<MonoBehaviour>();
        
        foreach (var comp in components)
        {
            if (comp == null) continue;

            // Get the component's type
            var compType = comp.GetType();
            
            // Check all interfaces implemented by this component
            foreach (var interfaceType in compType.GetInterfaces())
            {
                // Check if it's IColorPickable<T>
                if (interfaceType.IsGenericType && 
                    interfaceType.GetGenericTypeDefinition() == typeof(IColorPickable<>))
                {
                    // Call GetColor method via reflection
                    var getColorMethod = interfaceType.GetMethod("GetColor");
                    if (getColorMethod != null)
                    {
                        try
                        {
                            Color color = (Color)getColorMethod.Invoke(comp, null);
                            return color;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"Failed to get color from {compType.Name}: {ex.Message}");
                        }
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Color Processing
    private void ProcessPickedColor(Color color)
    {
        if (!mixedDone)
        {
            ProcessColorBeforeMixing(color);
        }
        else
        {
            ProcessColorAfterMixing(color);
        }
    }

    private void ProcessColorBeforeMixing(Color color)
    {
        if (pickCount == 0)
        {
            // First color pick
            colorSlots[0] = color;
            pickCount++;
            OnColorPicked?.Invoke(color, 0);
            Debug.Log($"First color stored: {color}");
        }
        else if (pickCount == 1 && autoMixOnSecondPick)
        {
            // Second color pick - auto mix
            colorSlots[1] = color;
            OnColorPicked?.Invoke(color, 1);
            Debug.Log($"Second color stored: {color}");
            
            PerformColorMixing();
        }
    }

    private void ProcessColorAfterMixing(Color color)
    {
        // After first mixing is done, new color goes to slot 2
        colorSlots[1] = color;
        OnColorPicked?.Invoke(color, 1);
        Debug.Log($"New color stored in slot 2: {color}");
    }

    private void PerformColorMixing()
    {
        mixedColor = MixColors(colorSlots[0], colorSlots[1]);
        colorSlots[0] = mixedColor;
        colorSlots[1] = Color.clear;
        pickCount++;
        mixedDone = true;
        
        OnColorsMixed?.Invoke(mixedColor);
        Debug.Log($"Colors mixed! Result: {mixedColor}");
    }
    #endregion

    #region Color Mixing
    private Color MixColors(Color c1, Color c2)
    {
        // Average color mixing
        return new Color(
            (c1.r + c2.r) / 2f,
            (c1.g + c2.g) / 2f,
            (c1.b + c2.b) / 2f,
            (c1.a + c2.a) / 2f
        );
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Manually trigger a color mix between the two slots
    /// </summary>
    public void MixCurrentColors()
    {
        if (pickCount < 2)
        {
            Debug.LogWarning("Need at least 2 colors to mix!");
            return;
        }

        PerformColorMixing();
    }

    /// <summary>
    /// Clear all color slots and reset state
    /// </summary>
    public void ClearColorSlots()
    {
        for (int i = 0; i < colorSlots.Length; i++)
        {
            colorSlots[i] = Color.clear;
        }
        
        mixedColor = Color.clear;
        pickCount = 0;
        mixedDone = false;
        
        OnColorSlotsCleared?.Invoke();
        Debug.Log("Color slots cleared");
    }

    /// <summary>
    /// Get color from a specific slot
    /// </summary>
    public Color GetColorFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= colorSlots.Length)
        {
            Debug.LogError($"Invalid slot index: {slotIndex}");
            return Color.clear;
        }

        return colorSlots[slotIndex];
    }

    /// <summary>
    /// Set color to a specific slot
    /// </summary>
    public void SetColorToSlot(int slotIndex, Color color)
    {
        if (slotIndex < 0 || slotIndex >= colorSlots.Length)
        {
            Debug.LogError($"Invalid slot index: {slotIndex}");
            return;
        }

        colorSlots[slotIndex] = color;
        OnColorPicked?.Invoke(color, slotIndex);
    }
    #endregion

    #region Debug GUI
    private void DrawDebugColorSlots()
    {
        float slotSize = 50f;
        float spacing = 10f;
        float startX = 10f;
        float startY = 10f;

        for (int i = 0; i < colorSlots.Length; i++)
        {
            Color slotColor = colorSlots[i];
            
            // Use black background for empty slots
            GUI.color = slotColor == default || slotColor.a < 0.01f 
                ? new Color(0.2f, 0.2f, 0.2f, 1f) 
                : slotColor;
            
            float xPos = startX + (slotSize + spacing) * i;
            GUI.Box(new Rect(xPos, startY, slotSize, slotSize), $"Slot {i + 1}");
        }

        // Reset GUI color
        GUI.color = Color.white;

        // Display status text
        string statusText = $"Picks: {pickCount} | Mixed: {mixedDone}";
        GUI.Label(new Rect(startX, startY + slotSize + 10f, 300f, 30f), statusText);
    }
    #endregion
}
