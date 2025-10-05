using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 颜色混合控制器
/// 处理玩家的混色输入操作
/// </summary>
public class ColorMixController : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private ColorBag colorBag;
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode mixRedGreenKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode mixRedBlueKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode mixGreenBlueKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode clearMixMagazineKey = KeyCode.Alpha4;
    
    [Header("Debug")]
    [SerializeField] private bool showMixGUI = true;
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // 查找 ColorBag 如果未设置
        if (colorBag == null)
        {
            colorBag = FindObjectOfType<ColorBag>();
            if (colorBag == null)
            {
                Debug.LogError("ColorBag not found! Please assign it in the inspector.");
            }
        }
    }

    private void Start()
    {
        // 订阅混色事件
        if (colorBag != null)
        {
            colorBag.OnColorsMixed += HandleColorsMixed;
        }
    }

    private void Update()
    {
        HandleMixInput();
    }

    private void OnGUI()
    {
        if (showMixGUI)
        {
            DrawMixGUI();
        }
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        if (colorBag != null)
        {
            colorBag.OnColorsMixed -= HandleColorsMixed;
        }
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// 处理混色输入
    /// </summary>
    private void HandleMixInput()
    {
        if (colorBag == null) return;

        // 混合红绿
        if (Input.GetKeyDown(mixRedGreenKey))
        {
            TryMixRedGreen();
        }

        // 混合红蓝
        if (Input.GetKeyDown(mixRedBlueKey))
        {
            TryMixRedBlue();
        }

        // 混合绿蓝
        if (Input.GetKeyDown(mixGreenBlueKey))
        {
            TryMixGreenBlue();
        }

        // 清空混合弹匣
        if (Input.GetKeyDown(clearMixMagazineKey))
        {
            ClearMixMagazine();
        }

        // 支持新输入系统
        HandleNewInputSystem();
    }

    /// <summary>
    /// 处理新输入系统
    /// </summary>
    private void HandleNewInputSystem()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            TryMixRedGreen();
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            TryMixRedBlue();
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            TryMixGreenBlue();
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            ClearMixMagazine();
        }
    }

    #endregion

    #region Mix Operations

    /// <summary>
    /// 尝试混合红色和绿色
    /// </summary>
    private void TryMixRedGreen()
    {
        bool canMix = colorBag.CanMix(colorBag.RedMagazine, colorBag.GreenMagazine);
        
        if (canMix)
        {
            colorBag.MixRedGreen();
        }
        else
        {
            Debug.LogWarning("Cannot mix Red + Green: Magazines not full or Mix magazine not empty");
            ShowMixFailureMessage("Red + Green", colorBag.RedMagazine, colorBag.GreenMagazine);
        }
    }

    /// <summary>
    /// 尝试混合红色和蓝色
    /// </summary>
    private void TryMixRedBlue()
    {
        bool canMix = colorBag.CanMix(colorBag.RedMagazine, colorBag.BlueMagazine);
        
        if (canMix)
        {
            colorBag.MixRedBlue();
        }
        else
        {
            Debug.LogWarning("Cannot mix Red + Blue: Magazines not full or Mix magazine not empty");
            ShowMixFailureMessage("Red + Blue", colorBag.RedMagazine, colorBag.BlueMagazine);
        }
    }

    /// <summary>
    /// 尝试混合绿色和蓝色
    /// </summary>
    private void TryMixGreenBlue()
    {
        bool canMix = colorBag.CanMix(colorBag.GreenMagazine, colorBag.BlueMagazine);
        
        if (canMix)
        {
            colorBag.MixGreenBlue();
        }
        else
        {
            Debug.LogWarning("Cannot mix Green + Blue: Magazines not full or Mix magazine not empty");
            ShowMixFailureMessage("Green + Blue", colorBag.GreenMagazine, colorBag.BlueMagazine);
        }
    }

    /// <summary>
    /// 清空混合弹匣
    /// </summary>
    private void ClearMixMagazine()
    {
        if (colorBag.MixMagazine != null && !colorBag.MixMagazine.IsEmpty)
        {
            colorBag.MixMagazine.ClearMagazine();
            Debug.Log("Mix magazine cleared");
        }
        else
        {
            Debug.Log("Mix magazine is already empty");
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 处理混色完成事件
    /// </summary>
    private void HandleColorsMixed(ColorMagazine mag1, ColorMagazine mag2, Color mixedColor)
    {
        Debug.Log($"Colors successfully mixed! {mag1.gameObject.name} + {mag2.gameObject.name} = {mixedColor}");
        
        // 这里可以添加更多效果，比如：
        // - 播放混色音效
        // - 显示混色特效
        // - 更新UI显示
        // - 触发成就系统等
        
        PlayMixEffect(mixedColor);
    }

    /// <summary>
    /// 播放混色效果
    /// </summary>
    private void PlayMixEffect(Color mixedColor)
    {
        // 可以在这里添加粒子效果、音效等
        Debug.Log($"Playing mix effect for color: {mixedColor}");
    }

    /// <summary>
    /// 显示混色失败信息
    /// </summary>
    private void ShowMixFailureMessage(string mixType, ColorMagazine mag1, ColorMagazine mag2)
    {
        string message = $"Cannot mix {mixType}:\n";
        message += $"- {mag1.gameObject.name}: {mag1.CurrentBullets}/{mag1.BulletCapacity} bullets\n";
        message += $"- {mag2.gameObject.name}: {mag2.CurrentBullets}/{mag2.BulletCapacity} bullets\n";
        message += $"- Mix Magazine: {(colorBag.MixMagazine.IsEmpty ? "Empty" : "Not Empty")}";
        
        Debug.Log(message);
    }

    #endregion

    #region GUI

    /// <summary>
    /// 绘制混色GUI
    /// </summary>
    private void DrawMixGUI()
    {
        float startX = Screen.width - 250f;
        float startY = 10f;
        float lineHeight = 25f;
        float yOffset = startY;

        GUI.color = Color.white;
        GUI.Label(new Rect(startX, yOffset, 240f, lineHeight), "=== Color Mixing ===");
        yOffset += lineHeight + 5f;

        // 显示混色选项
        DrawMixOption(startX, ref yOffset, lineHeight, "Red + Green", mixRedGreenKey, 
            colorBag.CanMix(colorBag.RedMagazine, colorBag.GreenMagazine));

        DrawMixOption(startX, ref yOffset, lineHeight, "Red + Blue", mixRedBlueKey, 
            colorBag.CanMix(colorBag.RedMagazine, colorBag.BlueMagazine));

        DrawMixOption(startX, ref yOffset, lineHeight, "Green + Blue", mixGreenBlueKey, 
            colorBag.CanMix(colorBag.GreenMagazine, colorBag.BlueMagazine));

        yOffset += 5f;

        // 清空混合弹匣选项
        GUI.color = colorBag.MixMagazine != null && !colorBag.MixMagazine.IsEmpty ? Color.yellow : Color.gray;
        GUI.Label(new Rect(startX, yOffset, 240f, lineHeight), 
            $"[{clearMixMagazineKey}] Clear Mix Magazine");

        GUI.color = Color.white;
    }

    /// <summary>
    /// 绘制单个混色选项
    /// </summary>
    private void DrawMixOption(float x, ref float y, float lineHeight, string mixName, KeyCode key, bool canMix)
    {
        GUI.color = canMix ? Color.green : Color.red;
        GUI.Label(new Rect(x, y, 240f, lineHeight), $"[{key}] {mixName}");
        y += lineHeight;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 设置 ColorBag 引用
    /// </summary>
    public void SetColorBag(ColorBag bag)
    {
        // 取消旧的事件订阅
        if (colorBag != null)
        {
            colorBag.OnColorsMixed -= HandleColorsMixed;
        }

        colorBag = bag;

        // 订阅新的事件
        if (colorBag != null)
        {
            colorBag.OnColorsMixed += HandleColorsMixed;
        }
    }

    /// <summary>
    /// 检查指定混色是否可用
    /// </summary>
    public bool CanMix(ColorBag.MagazineType type1, ColorBag.MagazineType type2)
    {
        if (colorBag == null) return false;

        var mag1 = colorBag.GetMagazine(type1);
        var mag2 = colorBag.GetMagazine(type2);

        return colorBag.CanMix(mag1, mag2);
    }

    #endregion
}