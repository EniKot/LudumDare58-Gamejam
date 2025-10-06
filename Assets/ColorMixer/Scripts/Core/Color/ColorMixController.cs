using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 颜色混合控制器
/// 处理弹匣选择和混色操作
/// 键盘1234控制弹匣选择，Tab进入混色模式
/// </summary>
public class ColorMixController : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private ColorBag colorBag;
    //[SerializeField] private PlayerController playerController;
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode selectMagazine1Key = KeyCode.Alpha1; // 红色
    [SerializeField] private KeyCode selectMagazine2Key = KeyCode.Alpha2; // 绿色
    [SerializeField] private KeyCode selectMagazine3Key = KeyCode.Alpha3; // 蓝色
    [SerializeField] private KeyCode selectMagazine4Key = KeyCode.Alpha4; // 混合
    [SerializeField] private KeyCode mixModeKey = KeyCode.Tab;
    
    [Header("Debug")]
    [SerializeField] private bool showMixGUI = true;
    [SerializeField] private bool showDebug = true;
    #endregion

    #region Private Fields
    private InputService inputService;
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // 查找 ColorBag 如果未设置
        if (colorBag == null)
            colorBag = FindObjectOfType<ColorBag>();
        
        //if (playerController == null)
        //    playerController = FindObjectOfType<PlayerController>();
            
        inputService = InputService.Instance;
    }

    private void Start()
    {
        // 订阅 ColorBag 事件
        if (colorBag != null)
        {
            colorBag.OnColorsMixed += HandleColorsMixed;
            colorBag.OnCurrentMagazineChanged += HandleCurrentMagazineChanged;
            colorBag.OnMixModeChanged += HandleMixModeChanged;
            colorBag.OnFirstMixMagazineSelected += HandleFirstMixMagazineSelected;
        }
    }

    private void Update()
    {
        HandleMixInput();
    }

    private void OnGUI()
    {
        if (showMixGUI)
            DrawMixGUI();
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        if (colorBag != null)
        {
            colorBag.OnColorsMixed -= HandleColorsMixed;
            colorBag.OnCurrentMagazineChanged -= HandleCurrentMagazineChanged;
            colorBag.OnMixModeChanged -= HandleMixModeChanged;
            colorBag.OnFirstMixMagazineSelected -= HandleFirstMixMagazineSelected;
        }
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// 处理混色和弹匣选择输入 旧版输入系统，新输入系统请自行修改
    /// </summary>
    private void HandleMixInput()
    {
        if (colorBag == null) return;

        // 弹匣选择
        if (Input.GetKeyDown(selectMagazine1Key))
        {
            HandleMagazineSelection(0); // 红色
        }
        else if (Input.GetKeyDown(selectMagazine2Key))
        {
            HandleMagazineSelection(1); // 绿色
        }
        else if (Input.GetKeyDown(selectMagazine3Key))
        {
            HandleMagazineSelection(2); // 蓝色
        }
        else if (Input.GetKeyDown(selectMagazine4Key))
        {
            HandleMagazineSelection(3); // 混合
        }

        // 混色模式切换
        if (Input.GetKeyDown(mixModeKey))
        {
            HandleMixModeToggle();
        }

        
    }


    #endregion


    #region Magazine Selection

    /// <summary>
    /// 处理弹匣选择
    /// </summary>
    /// <param name="magazineIndex">弹匣索引</param>
    public void HandleMagazineSelection(int magazineIndex)
    {
        if (colorBag.IsMixMode)
        {
            // 在混色模式下，选择第二个弹匣并尝试混色
            if (colorBag.SetCurrentMagazine(magazineIndex))
            {
                bool mixSuccess = colorBag.TryMixWithCurrentMagazine();
                if (!mixSuccess && showDebug)
                {
                    Debug.LogWarning("Failed to mix colors - check magazine requirements");
                }
            }
        }
        else
        {
            // 正常模式下，切换当前弹匣
            colorBag.SetCurrentMagazine(magazineIndex);
        }
    }

    /// <summary>
    /// 处理混色模式切换
    /// </summary>
    public void HandleMixModeToggle()
    {
        if (colorBag.IsMixMode)
        {
            // 退出混色模式
            colorBag.ExitMixMode();

        }
        else
        {
            // 进入混色模式
            colorBag.EnterMixMode();
        }
    }

    #endregion




    #region Event Handlers

    /// <summary>
    /// 处理混色完成事件
    /// </summary>
    private void HandleColorsMixed(ColorMagazine mag1, ColorMagazine mag2, Color mixedColor)
    {
        if (showDebug)
            Debug.Log($"[ColorMixController] Colors successfully mixed! {mag1.MagazineName} + {mag2.MagazineName} = {mixedColor}");
        
        PlayMixEffect(mixedColor);
    }

    /// <summary>
    /// 处理当前弹匣改变事件
    /// </summary>
    private void HandleCurrentMagazineChanged(ColorMagazine newMagazine)
    {
        if (showDebug)
            Debug.Log($"[ColorMixController] Current magazine changed to: {newMagazine?.MagazineName}");
    }

    /// <summary>
    /// 处理混色模式改变事件
    /// </summary>
    private void HandleMixModeChanged(bool inMixMode)
    {
        if (showDebug)
            Debug.Log($"[ColorMixController] Mix mode: {(inMixMode ? "ENTERED" : "EXITED")}");
    }

    /// <summary>
    /// 处理第一个混色弹匣选择事件
    /// </summary>
    private void HandleFirstMixMagazineSelected(ColorMagazine firstMagazine)
    {
        if (showDebug)
            Debug.Log($"[ColorMixController] First mix magazine selected: {firstMagazine.MagazineName}");
    }

    /// <summary>
    /// 播放混色效果
    /// </summary>
    private void PlayMixEffect(Color mixedColor)
    {
        // 可以在这里添加粒子效果、音效等
        if (showDebug)
            Debug.Log($"Playing mix effect for color: {mixedColor}");
    }

    #endregion


















    #region GUI

    /// <summary>
    /// 绘制混色GUI Generate By Claude4
    /// </summary>
    private void DrawMixGUI()
    {
        if (colorBag == null) return;

        float startX = Screen.width - 300f;
        float startY = 10f;
        float lineHeight = 25f;
        float yOffset = startY;

        GUI.color = Color.white;
        GUI.Label(new Rect(startX, yOffset, 290f, lineHeight), "=== Color Mix Controller ===");
        yOffset += lineHeight + 5f;

        // 显示当前选中的弹匣
        var currentMag = colorBag.CurrentColorMagazine;
        if (currentMag != null)
        {
            GUI.color = currentMag.MagazineColor;
            GUI.Box(new Rect(startX, yOffset, 20, 20), "");
            GUI.color = Color.white;
            GUI.Label(new Rect(startX + 25, yOffset, 260f, lineHeight), 
                $"Current: {currentMag.MagazineName} ({currentMag.CurrentBullets}/{currentMag.BulletCapacity})");
            yOffset += lineHeight;
        }

        // 显示混色模式状态
        if (colorBag.IsMixMode)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(startX, yOffset, 290f, lineHeight), "*** MIX MODE ACTIVE ***");
            yOffset += lineHeight;
            
            if (colorBag.FirstMixMagazine != null)
            {
                GUI.color = colorBag.FirstMixMagazine.MagazineColor;
                GUI.Box(new Rect(startX, yOffset, 20, 20), "");
                GUI.color = Color.white;
                GUI.Label(new Rect(startX + 25, yOffset, 260f, lineHeight), 
                    $"First: {colorBag.FirstMixMagazine.MagazineName}");
                yOffset += lineHeight;
                
                GUI.color = Color.cyan;
                GUI.Label(new Rect(startX, yOffset, 290f, lineHeight), "Select second magazine to mix!");
                yOffset += lineHeight;
            }
        }

        yOffset += 5f;

        // 显示控制说明
        GUI.color = Color.white;
        GUI.Label(new Rect(startX, yOffset, 290f, lineHeight), "=== Controls ===");
        yOffset += lineHeight;

        DrawControlHint(startX, ref yOffset, lineHeight, $"[{selectMagazine1Key}]", "Red Magazine", 
            colorBag.CurrentMagazineIndex == 0);
        DrawControlHint(startX, ref yOffset, lineHeight, $"[{selectMagazine2Key}]", "Green Magazine", 
            colorBag.CurrentMagazineIndex == 1);
        DrawControlHint(startX, ref yOffset, lineHeight, $"[{selectMagazine3Key}]", "Blue Magazine", 
            colorBag.CurrentMagazineIndex == 2);
        DrawControlHint(startX, ref yOffset, lineHeight, $"[{selectMagazine4Key}]", "Mix Magazine", 
            colorBag.CurrentMagazineIndex == 3);

        yOffset += 5f;
        GUI.color = colorBag.IsMixMode ? Color.yellow : Color.gray;
        GUI.Label(new Rect(startX, yOffset, 290f, lineHeight), $"[{mixModeKey}] Toggle Mix Mode");

        GUI.color = Color.white;
    }

    /// <summary>
    /// 绘制单个控制提示
    /// </summary>
    private void DrawControlHint(float x, ref float y, float lineHeight, string key, string description, bool isSelected)
    {
        GUI.color = isSelected ? Color.green : Color.white;
        GUI.Label(new Rect(x, y, 290f, lineHeight), $"{key} {description}");
        y += lineHeight;
    }

    #endregion

    //#region Public Methods
    /// <summary>
    /// 设置 ColorBag 引用
    /// </summary>
    //public void SetColorBag(ColorBag bag)
    //{
    //    // 取消旧的事件订阅
    //    if (colorBag != null)
    //    {
    //        colorBag.OnColorsMixed -= HandleColorsMixed;
    //        colorBag.OnCurrentMagazineChanged -= HandleCurrentMagazineChanged;
    //        colorBag.OnMixModeChanged -= HandleMixModeChanged;
    //        colorBag.OnFirstMixMagazineSelected -= HandleFirstMixMagazineSelected;
    //    }

    //    colorBag = bag;

    //    // 订阅新的事件
    //    if (colorBag != null)
    //    {
    //        colorBag.OnColorsMixed += HandleColorsMixed;
    //        colorBag.OnCurrentMagazineChanged += HandleCurrentMagazineChanged;
    //        colorBag.OnMixModeChanged += HandleMixModeChanged;
    //        colorBag.OnFirstMixMagazineSelected += HandleFirstMixMagazineSelected;
    //    }
    //}
    //#endregion
}