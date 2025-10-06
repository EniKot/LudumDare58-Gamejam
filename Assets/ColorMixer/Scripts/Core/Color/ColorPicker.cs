using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ColorPicker : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private ColorBag colorBag;
    [SerializeField] private PlayerController playerController;
    
    [Header("Detection Settings")]
    [SerializeField] private LayerMask pickableLayerMask = -1;
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private Transform detectionCenter;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGUI = true;
    [SerializeField] private bool showDetectionRange = true;
    #endregion

    #region Private Fields
    private List<IColorPickable> pickableObjects = new List<IColorPickable>();
    private IColorPickable nearestPickable;
    private float nearestDistance = float.MaxValue;
    private InputService inputService;
    #endregion

    #region Events
    public event Action<Color, GameObject> OnColorPicked;
    public event Action<IColorPickable> OnPickableObjectFound;
    public event Action OnNoPickableObjectFound;
    #endregion

    #region Unity Lifecycle
    
    private void Awake()
    {
        if (detectionCenter == null)
            detectionCenter = transform;
        
        if (colorBag == null)
            colorBag = FindObjectOfType<ColorBag>();
        
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
            
        inputService = InputService.Instance;
    }

    private void Start()
    {
        // 订阅 Fire 输入
        if (inputService?.inputMap?.Player != null && inputService.inputMap.Player.Fire != null)
            inputService.inputMap.Player.Fire.started += OnFireInput;
    }

    private void OnDestroy()
    {
        if (inputService?.inputMap?.Player != null && inputService.inputMap.Player.Fire != null)
            inputService.inputMap.Player.Fire.started -= OnFireInput;
    }

    private void OnGUI()
    {
        if (showDebugGUI)
            DrawDebugGUI();
    }

    private void OnDrawGizmos()
    {
        if (showDetectionRange && detectionCenter != null)
            DrawDetectionRange();
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// 处理 Fire 输入 - 只处理颜色拾取
    /// </summary>
    private void OnFireInput(InputAction.CallbackContext context)
    {
        UpdatePickableDetection();
        
        if (nearestPickable != null)
        {
            TryPickColor();
        }
        // 射击逻辑已移至ColorShooter，这里不再处理
    }

    #endregion

    #region Color Picking

    /// <summary>
    /// 尝试拾取颜色
    /// </summary>
    private void TryPickColor()
    {
        if (nearestPickable == null) return;

        Color pickedColor = nearestPickable.GetColor();
        GameObject targetObject = (nearestPickable as MonoBehaviour)?.gameObject;

        if (targetObject == null) return;

        nearestPickable.OnColorPicked();
        OnColorPicked?.Invoke(pickedColor, targetObject);
        Debug.Log($"Picked color {pickedColor} from {targetObject.name}");

        colorBag.AddColorBullet(pickedColor);
    }

    /// <summary>
    /// 更新可拾取对象检测（2D版本）
    /// </summary>
    private void UpdatePickableDetection()
    {
        pickableObjects.Clear();
        nearestPickable = null;
        nearestDistance = float.MaxValue;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(detectionCenter.position, detectionRadius, pickableLayerMask);
        
        foreach (var col in colliders)
        {
            foreach (var component in col.GetComponentsInChildren<MonoBehaviour>())
            {
                if (component is IColorPickable pickable)
                {
                    pickableObjects.Add(pickable);
                    
                    float distance = Vector2.Distance(detectionCenter.position, col.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPickable = pickable;
                    }
                }
            }
        }

        if (nearestPickable != null)
            OnPickableObjectFound?.Invoke(nearestPickable);
        else
            OnNoPickableObjectFound?.Invoke();
    }

    #endregion

    #region Debug and Visualization

    private void DrawDetectionRange()
    {
        // 绘制检测范围
        Gizmos.color = nearestPickable != null ? Color.green : Color.yellow;
        
        Vector3 center = detectionCenter.position;
        for (int i = 0; i < 36; i++)
        {
            float angle1 = i * 10f * Mathf.Deg2Rad;
            float angle2 = (i + 1) * 10f * Mathf.Deg2Rad;
            
            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * detectionRadius, Mathf.Sin(angle1) * detectionRadius, 0);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * detectionRadius, Mathf.Sin(angle2) * detectionRadius, 0);
            
            Gizmos.DrawLine(point1, point2);
        }
        
        // 绘制到最近对象的连线和标记
        if (nearestPickable is MonoBehaviour nearestComponent)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(detectionCenter.position, nearestComponent.transform.position);
            
            Vector3 targetPos = nearestComponent.transform.position;
            float crossSize = 0.2f;
            Gizmos.DrawLine(targetPos + Vector3.left * crossSize, targetPos + Vector3.right * crossSize);
            Gizmos.DrawLine(targetPos + Vector3.up * crossSize, targetPos + Vector3.down * crossSize);
        }
    }

    private void DrawDebugGUI()
    {
        float yOffset = 10f;
        float lineHeight = 25f;
        
        GUI.color = Color.white;
        GUI.Label(new Rect(10, yOffset, 400, lineHeight), $"Detection Range: {detectionRadius}m");
        yOffset += lineHeight;
        
        GUI.Label(new Rect(10, yOffset, 400, lineHeight), $"Pickable Objects: {pickableObjects.Count}");
        yOffset += lineHeight;
        
        if (nearestPickable is MonoBehaviour nearestComponent)
        {
            GUI.color = Color.green;
            GUI.Label(new Rect(10, yOffset, 400, lineHeight), $"Nearest: {nearestComponent.gameObject.name} ({nearestDistance:F1}m)");
            yOffset += lineHeight;
            
            Color nearestColor = nearestPickable.GetColor();
            GUI.color = nearestColor;
            GUI.Box(new Rect(10, yOffset, 50, 20), "");
            GUI.color = Color.white;
            GUI.Label(new Rect(70, yOffset, 300, lineHeight), $"Color: {nearestColor}");
            yOffset += lineHeight;
            
            GUI.color = Color.cyan;
            GUI.Label(new Rect(10, yOffset, 400, lineHeight), "Press [Fire] to pick color");
        }
        else
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(10, yOffset, 400, lineHeight), "No pickable objects");
            yOffset += lineHeight;
            
            GUI.color = Color.yellow;
            GUI.Label(new Rect(10, yOffset, 400, lineHeight), "Shooting handled by ColorShooter");
        }
        
        yOffset += 10f;
        
        // ColorBag 状态
        if (colorBag != null)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(10, yOffset, 400, lineHeight), "=== ColorBag Status ===");
            yOffset += lineHeight;
            
            foreach (var magazine in colorBag.AllMagazines)
            {
                if (magazine != null)
                {
                    GUI.color = magazine.MagazineColor;
                    GUI.Box(new Rect(10, yOffset, 20, 20), "");
                    
                    // 高亮当前选中的弹匣
                    bool isCurrent = magazine == colorBag.CurrentColorMagazine;
                    GUI.color = isCurrent ? Color.green : Color.white;
                    
                    string prefix = isCurrent ? ">>> " : "    ";
                    GUI.Label(new Rect(35, yOffset, 300, lineHeight), 
                        $"{prefix}{magazine.MagazineName}: {magazine.CurrentBullets}/{magazine.BulletCapacity}");
                    yOffset += lineHeight;
                }
            }
            
            // 显示混色模式状态
            if (colorBag.IsMixMode)
            {
                yOffset += 5f;
                GUI.color = Color.magenta;
                GUI.Label(new Rect(10, yOffset, 400, lineHeight), "*** MIX MODE ACTIVE ***");
                yOffset += lineHeight;
                
                if (colorBag.FirstMixMagazine != null)
                {
                    GUI.color = colorBag.FirstMixMagazine.MagazineColor;
                    GUI.Box(new Rect(10, yOffset, 20, 20), "");
                    GUI.color = Color.white;
                    GUI.Label(new Rect(35, yOffset, 300, lineHeight), 
                        $"First: {colorBag.FirstMixMagazine.MagazineName}");
                }
            }
        }
        
        GUI.color = Color.white;
    }

    #endregion

    #region Public Methods

    public void SetDetectionRadius(float radius) => detectionRadius = Mathf.Max(0.1f, radius);
    public void SetPickableLayerMask(LayerMask layerMask) => pickableLayerMask = layerMask;
    public int GetPickableObjectCount() => pickableObjects.Count;
    public IColorPickable GetNearestPickable() => nearestPickable;
    public void SetColorBag(ColorBag bag) => colorBag = bag;
    public bool HasPickableInRange() => nearestPickable != null;

    /// <summary>
    /// 手动触发颜色拾取
    /// </summary>
    public void ManualPickColor()
    {
        UpdatePickableDetection();
        TryPickColor();
    }

    /// <summary>
    /// 强制更新可拾取对象检测
    /// </summary>
    public void ForceUpdateDetection()
    {
        UpdatePickableDetection();
    }

    #endregion
}
