using UnityEngine;

/// <summary>
/// 新 ColorPicker 系统的使用示例
/// 演示如何设置和使用基于碰撞检测的颜色拾取系统
/// </summary>
public class NewColorPickerExample : MonoBehaviour
{
    #region Serialized Fields
    [Header("System References")]
    [SerializeField] private ColorPicker colorPicker;
    [SerializeField] private ColorBag colorBag;
    [SerializeField] private ColorMixController mixController;
    
    [Header("Example Objects")]
    [SerializeField] private ExampleColorPickableObject[] colorObjects;
    
    [Header("Demo Settings")]
    [SerializeField] private bool autoCreateTestObjects = true;
    [SerializeField] private int testObjectCount = 5;
    [SerializeField] private float spawnRadius = 5f;
    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        SetupSystem();
        
        if (autoCreateTestObjects)
        {
            CreateTestObjects();
        }
        
        SubscribeToEvents();
        
        Debug.Log("=== New ColorPicker System Example Started ===");
        Debug.Log("Controls:");
        Debug.Log("- E: Pick color from nearby objects");
        Debug.Log("- 1: Mix Red + Green");
        Debug.Log("- 2: Mix Red + Blue");
        Debug.Log("- 3: Mix Green + Blue");
        Debug.Log("- 4: Clear Mix Magazine");
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region System Setup

    /// <summary>
    /// 设置系统组件
    /// </summary>
    private void SetupSystem()
    {
        // 如果没有找到组件，尝试自动查找
        if (colorPicker == null)
            colorPicker = FindObjectOfType<ColorPicker>();
        
        if (colorBag == null)
            colorBag = FindObjectOfType<ColorBag>();
        
        if (mixController == null)
            mixController = FindObjectOfType<ColorMixController>();

        // 确保系统组件存在
        if (colorPicker == null)
        {
            Debug.LogError("ColorPicker not found! Please add ColorPicker component to the scene.");
        }

        if (colorBag == null)
        {
            Debug.LogError("ColorBag not found! Please add ColorBag component to the scene.");
        }

        if (mixController == null)
        {
            Debug.LogError("ColorMixController not found! Please add ColorMixController component to the scene.");
        }

        // 设置组件间的引用
        if (colorPicker != null && colorBag != null)
        {
            colorPicker.SetColorBag(colorBag);
        }

        if (mixController != null && colorBag != null)
        {
            mixController.SetColorBag(colorBag);
        }
    }

    /// <summary>
    /// 创建测试对象
    /// </summary>
    private void CreateTestObjects()
    {
        Color[] testColors = {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            new Color(1f, 0.5f, 0f), // Orange
            new Color(0.5f, 0f, 0.5f), // Purple
        };

        for (int i = 0; i < testObjectCount; i++)
        {
            // 创建测试对象
            GameObject testObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObj.name = $"TestColorObject_{i}";
            
            // 随机位置
            Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
            randomPos.y = Mathf.Abs(randomPos.y) + 1f; // 确保在地面上方
            testObj.transform.position = transform.position + randomPos;
            
            // 随机旋转
            testObj.transform.rotation = Random.rotation;
            
            // 添加颜色拾取组件
            var pickable = testObj.AddComponent<ExampleColorPickableObject>();
            
            // 设置随机颜色
            Color randomColor = testColors[Random.Range(0, testColors.Length)];
            pickable.SetColor(randomColor);
            
            // 设置层级（可选）
            testObj.layer = LayerMask.NameToLayer("Default");
        }
        
        Debug.Log($"Created {testObjectCount} test objects for color picking demonstration.");
    }

    #endregion

    #region Event Handling

    /// <summary>
    /// 订阅系统事件
    /// </summary>
    private void SubscribeToEvents()
    {
        if (colorPicker != null)
        {
            colorPicker.OnColorPicked += HandleColorPicked;
            colorPicker.OnPickableObjectFound += HandlePickableObjectFound;
            colorPicker.OnNoPickableObjectFound += HandleNoPickableObjectFound;
        }

        if (colorBag != null)
        {
            colorBag.OnColorsMixed += HandleColorsMixed;
            colorBag.OnMagazineUpdated += HandleMagazineUpdated;
        }
    }

    /// <summary>
    /// 取消订阅系统事件
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (colorPicker != null)
        {
            colorPicker.OnColorPicked -= HandleColorPicked;
            colorPicker.OnPickableObjectFound -= HandlePickableObjectFound;
            colorPicker.OnNoPickableObjectFound -= HandleNoPickableObjectFound;
        }

        if (colorBag != null)
        {
            colorBag.OnColorsMixed -= HandleColorsMixed;
            colorBag.OnMagazineUpdated -= HandleMagazineUpdated;
        }
    }

    /// <summary>
    /// 处理颜色拾取事件
    /// </summary>
    private void HandleColorPicked(Color color, GameObject target)
    {
        Debug.Log($"[Example] Color picked: {color} from {target.name}");
        
        // 可以在这里添加拾取效果，比如：
        // - 播放音效
        // - 显示UI提示
        // - 增加分数等
    }

    /// <summary>
    /// 处理找到可拾取对象事件
    /// </summary>
    private void HandlePickableObjectFound(IColorPickable<GameObject> pickable)
    {
        // 可以在这里添加UI提示，显示可以拾取的对象
        if (pickable is MonoBehaviour component)
        {
            // Debug.Log($"[Example] Pickable object in range: {component.gameObject.name}");
        }
    }

    /// <summary>
    /// 处理没有可拾取对象事件
    /// </summary>
    private void HandleNoPickableObjectFound()
    {
        Debug.Log("[Example] No pickable objects in range");
    }

    /// <summary>
    /// 处理颜色混合事件
    /// </summary>
    private void HandleColorsMixed(ColorMagazine mag1, ColorMagazine mag2, Color mixedColor)
    {
        Debug.Log($"[Example] Colors mixed: {mag1.gameObject.name} + {mag2.gameObject.name} = {mixedColor}");
        
        // 可以在这里添加混合效果，比如：
        // - 播放混合音效
        // - 显示特效
        // - 更新UI等
    }

    /// <summary>
    /// 处理弹匣更新事件
    /// </summary>
    private void HandleMagazineUpdated(ColorMagazine magazine)
    {
        Debug.Log($"[Example] Magazine updated: {magazine.gameObject.name} - {magazine.CurrentBullets}/{magazine.BulletCapacity}");
    }

    #endregion

    #region Public Methods (可用于UI按钮等)

    /// <summary>
    /// 手动拾取颜色（可绑定到UI按钮）
    /// </summary>
    public void ManualPickColor()
    {
        if (colorPicker != null)
        {
            colorPicker.ManualPickColor();
        }
    }

    /// <summary>
    /// 清空所有弹匣（调试用）
    /// </summary>
    public void ClearAllMagazines()
    {
        if (colorBag != null)
        {
            colorBag.ClearAllMagazines();
            Debug.Log("[Example] All magazines cleared");
        }
    }

    /// <summary>
    /// 填满所有基础弹匣（调试用）
    /// </summary>
    public void FillAllBaseMagazines()
    {
        if (colorBag != null)
        {
            colorBag.RedMagazine?.FillMagazine();
            colorBag.GreenMagazine?.FillMagazine();
            colorBag.BlueMagazine?.FillMagazine();
            Debug.Log("[Example] All base magazines filled");
        }
    }

    /// <summary>
    /// 显示系统状态
    /// </summary>
    public void ShowSystemStatus()
    {
        if (colorBag != null)
        {
            Debug.Log("[Example] System Status:");
            Debug.Log(colorBag.GetAllMagazineInfo());
        }
        
        if (colorPicker != null)
        {
            Debug.Log($"Pickable objects in range: {colorPicker.GetPickableObjectCount()}");
        }
    }

    #endregion

    #region Debug

    private void OnGUI()
    {
        // 显示示例控制面板
        float panelX = 10f;
        float panelY = Screen.height - 200f;
        float panelWidth = 200f;
        float panelHeight = 180f;

        GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), "Example Controls");

        float buttonY = panelY + 25f;
        float buttonHeight = 25f;
        float buttonSpacing = 30f;

        if (GUI.Button(new Rect(panelX + 10f, buttonY, panelWidth - 20f, buttonHeight), "Manual Pick Color"))
        {
            ManualPickColor();
        }
        buttonY += buttonSpacing;

        if (GUI.Button(new Rect(panelX + 10f, buttonY, panelWidth - 20f, buttonHeight), "Clear All Magazines"))
        {
            ClearAllMagazines();
        }
        buttonY += buttonSpacing;

        if (GUI.Button(new Rect(panelX + 10f, buttonY, panelWidth - 20f, buttonHeight), "Fill Base Magazines"))
        {
            FillAllBaseMagazines();
        }
        buttonY += buttonSpacing;

        if (GUI.Button(new Rect(panelX + 10f, buttonY, panelWidth - 20f, buttonHeight), "Show Status"))
        {
            ShowSystemStatus();
        }
        buttonY += buttonSpacing;

        if (GUI.Button(new Rect(panelX + 10f, buttonY, panelWidth - 20f, buttonHeight), "Create Test Objects"))
        {
            CreateTestObjects();
        }
    }

    #endregion
}