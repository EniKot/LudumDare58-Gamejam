using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ColorPicker : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private ColorBag colorBag;
    
    [Header("Detection Settings")]
    [SerializeField] private LayerMask pickableLayerMask = -1;
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private Transform detectionCenter;
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode pickColorKey = KeyCode.E;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGUI = true;
    [SerializeField] private bool showDetectionRange = true;
    #endregion

    #region Private Fields
    private List<IColorPickable<GameObject>> pickableObjects = new List<IColorPickable<GameObject>>();
    private IColorPickable<GameObject> nearestPickable;
    private float nearestDistance = float.MaxValue;
    #endregion

    #region Events
    public event Action<Color, GameObject> OnColorPicked;
    public event Action<IColorPickable<GameObject>> OnPickableObjectFound;
    public event Action OnNoPickableObjectFound;
    #endregion

    #region Unity Lifecycle
    
    private void Awake()
    {
        // 如果没有设置检测中心，使用自身位置
        if (detectionCenter == null)
        {
            detectionCenter = transform;
        }
        
        // 如果没有设置 ColorBag，尝试查找
        if (colorBag == null)
        {
            colorBag = FindObjectOfType<ColorBag>();
            if (colorBag == null)
            {
                Debug.LogError("ColorBag not found! Please assign it in the inspector or add it to the scene.");
            }
        }
    }

    private void Update()
    {
        HandleInput();
        UpdatePickableDetection();
    }

    private void OnGUI()
    {
        if (showDebugGUI)
        {
            DrawDebugGUI();
        }
    }

    private void OnDrawGizmos()
    {
        if (showDetectionRange && detectionCenter != null)
        {
            DrawDetectionRange();
        }
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInput()
    {
        // 检查按键输入
        if (Input.GetKeyDown(pickColorKey))
        {
            TryPickColor();
        }
        
        // 可选：支持新输入系统
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryPickColor();
        }
    }

    #endregion

    #region Color Picking

    /// <summary>
    /// 尝试拾取颜色
    /// </summary>
    private void TryPickColor()
    {
        if (nearestPickable == null)
        {
            OnNoPickableObjectFound?.Invoke();
            Debug.Log("No pickable object in range");
            return;
        }

        // 获取颜色
        Color pickedColor = nearestPickable.GetColor();
        GameObject targetObject = nearestPickable as MonoBehaviour != null ? 
            ((MonoBehaviour)nearestPickable).gameObject : null;

        if (targetObject == null)
        {
            Debug.LogError("Failed to get target object from pickable");
            return;
        }

        // 添加到 ColorBag
        bool success = colorBag.AddColorBullet(pickedColor);
        
        if (success)
        {
            // 通知 IColorPickable 对象颜色被拾取
            nearestPickable.OnColorPicked(pickedColor, targetObject);
            
            // 触发事件
            OnColorPicked?.Invoke(pickedColor, targetObject);
            
            Debug.Log($"Successfully picked color {pickedColor} from {targetObject.name}");
        }
        else
        {
            Debug.LogWarning($"Failed to add color {pickedColor} to ColorBag - no suitable magazine or magazine full");
        }
    }

    /// <summary>
    /// 更新可拾取对象检测（2D版本）
    /// </summary>
    private void UpdatePickableDetection()
    {
        pickableObjects.Clear();
        nearestPickable = null;
        nearestDistance = float.MaxValue;

        // 在检测范围内查找所有2D碰撞体
        Collider2D[] colliders = Physics2D.OverlapCircleAll(detectionCenter.position, detectionRadius, pickableLayerMask);
        
        foreach (Collider2D col in colliders)
        {
            // 查找实现 IColorPickable<GameObject> 的组件
            var pickableComponents = col.GetComponentsInChildren<MonoBehaviour>();
            
            foreach (var component in pickableComponents)
            {
                if (component is IColorPickable<GameObject> pickable)
                {
                    pickableObjects.Add(pickable);
                    
                    // 计算距离，找到最近的对象
                    float distance = Vector2.Distance(detectionCenter.position, col.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPickable = pickable;
                    }
                }
            }
        }

        // 触发本地事件
        if (nearestPickable != null)
        {
            OnPickableObjectFound?.Invoke(nearestPickable);
        }
    }

    #endregion

    #region Debug and Visualization

    /// <summary>
    /// 绘制检测范围（2D版本）
    /// </summary>
    private void DrawDetectionRange()
    {
        Gizmos.color = nearestPickable != null ? Color.green : Color.yellow;
        
        // 绘制2D检测圆圈
        Vector3 center = detectionCenter.position;
        
        // 绘制检测圆圈（在XY平面）
        for (int i = 0; i < 36; i++)
        {
            float angle1 = i * 10f * Mathf.Deg2Rad;
            float angle2 = (i + 1) * 10f * Mathf.Deg2Rad;
            
            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * detectionRadius, Mathf.Sin(angle1) * detectionRadius, 0);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * detectionRadius, Mathf.Sin(angle2) * detectionRadius, 0);
            
            Gizmos.DrawLine(point1, point2);
        }
        
        // 绘制到最近对象的线
        if (nearestPickable != null && nearestPickable is MonoBehaviour nearestComponent)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(detectionCenter.position, nearestComponent.transform.position);
            
            // 在最近对象位置绘制一个小十字标记
            Vector3 targetPos = nearestComponent.transform.position;
            float crossSize = 0.2f;
            Gizmos.DrawLine(targetPos + Vector3.left * crossSize, targetPos + Vector3.right * crossSize);
            Gizmos.DrawLine(targetPos + Vector3.up * crossSize, targetPos + Vector3.down * crossSize);
        }
    }

    /// <summary>
    /// 绘制调试GUI
    /// </summary>
    private void DrawDebugGUI()
    {
        float yOffset = 10f;
        float lineHeight = 25f;
        
        // 显示检测信息
        GUI.color = Color.white;
        GUI.Label(new Rect(10, yOffset, 400, lineHeight), $"Detection Range: {detectionRadius}m");
        yOffset += lineHeight;
        
        GUI.Label(new Rect(10, yOffset, 400, lineHeight), $"Pickable Objects Found: {pickableObjects.Count}");
        yOffset += lineHeight;
        
        if (nearestPickable != null && nearestPickable is MonoBehaviour nearestComponent)
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
        }
        else
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(10, yOffset, 400, lineHeight), "No pickable objects in range");
            yOffset += lineHeight;
        }
        
        // 显示操作提示
        GUI.color = Color.cyan;
        GUI.Label(new Rect(10, yOffset, 400, lineHeight), $"Press [{pickColorKey}] or [E] to pick color");
        yOffset += lineHeight;
        
        // 显示 ColorBag 状态
        if (colorBag != null)
        {
            yOffset += 10f;
            GUI.color = Color.yellow;
            GUI.Label(new Rect(10, yOffset, 400, lineHeight), "=== ColorBag Status ===");
            yOffset += lineHeight;
            
            foreach (var magazine in colorBag.AllMagazines)
            {
                if (magazine != null)
                {
                    GUI.color = magazine.MagazineColor;
                    GUI.Box(new Rect(10, yOffset, 20, 20), "");
                    
                    GUI.color = Color.white;
                    GUI.Label(new Rect(35, yOffset, 300, lineHeight), 
                        $"{magazine.gameObject.name}: {magazine.CurrentBullets}/{magazine.BulletCapacity}");
                    yOffset += lineHeight;
                }
            }
        }
        
        GUI.color = Color.white;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 设置检测半径
    /// </summary>
    public void SetDetectionRadius(float radius)
    {
        detectionRadius = Mathf.Max(0.1f, radius);
    }

    /// <summary>
    /// 设置检测层级
    /// </summary>
    public void SetPickableLayerMask(LayerMask layerMask)
    {
        pickableLayerMask = layerMask;
    }

    /// <summary>
    /// 获取当前检测到的可拾取对象数量
    /// </summary>
    public int GetPickableObjectCount()
    {
        return pickableObjects.Count;
    }

    /// <summary>
    /// 获取最近的可拾取对象
    /// </summary>
    public IColorPickable<GameObject> GetNearestPickable()
    {
        return nearestPickable;
    }

    /// <summary>
    /// 手动触发拾取颜色
    /// </summary>
    public void ManualPickColor()
    {
        TryPickColor();
    }

    /// <summary>
    /// 设置 ColorBag 引用
    /// </summary>
    public void SetColorBag(ColorBag bag)
    {
        colorBag = bag;
    }

    #endregion
}
