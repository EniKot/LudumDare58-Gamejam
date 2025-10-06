using System;
using System.Collections.Generic;
using UnityEngine;

public class ColorBag : MonoBehaviour
{
    #region Serialized Fields
    [Header("Magazine Data")]
    [SerializeField] private ColorMagazine redMagazine;
    [SerializeField] private ColorMagazine greenMagazine;
    [SerializeField] private ColorMagazine blueMagazine;
    [SerializeField] private ColorMagazine mixMagazine;
    
    [Header("Color Settings")]
    [SerializeField] private Color baseRedColor = Color.red;
    [SerializeField] private Color baseGreenColor = Color.green;
    [SerializeField] private Color baseBlueColor = Color.blue;
    
    [Header("Current Selection")]
    [SerializeField] private int currentMagazineIndex = 0;
    
    [Header("Debug")]
    [SerializeField] private bool showDebug = false;
    #endregion

    #region Public Properties
    public ColorMagazine RedMagazine => redMagazine;
    public ColorMagazine GreenMagazine => greenMagazine;
    public ColorMagazine BlueMagazine => blueMagazine;
    public ColorMagazine MixMagazine => mixMagazine;
    
    public ColorMagazine CurrentColorMagazine 
    { 
        get => GetMagazineByIndex(currentMagazineIndex);
        private set => currentMagazineIndex = GetMagazineIndex(value);
    }
    
    public int CurrentMagazineIndex => currentMagazineIndex;
    public bool IsMixMode { get; private set; } = false;
    public ColorMagazine FirstMixMagazine { get; private set; }
    
    public ColorMagazine[] BaseMagazines => new ColorMagazine[] { redMagazine, greenMagazine, blueMagazine };
    public ColorMagazine[] AllMagazines => new ColorMagazine[] { redMagazine, greenMagazine, blueMagazine, mixMagazine };
    #endregion




    #region Events
    public event Action<ColorMagazine, ColorMagazine, Color> OnColorsMixed;
    public event Action<ColorMagazine> OnMagazineUpdated;
    public event Action<ColorMagazine> OnCurrentMagazineChanged;
    public event Action<bool> OnMixModeChanged;
    public event Action<ColorMagazine> OnFirstMixMagazineSelected;
    #endregion




    #region Unity Lifecycle
    
    private void Start()
    {
        InitializeMagazines();
        SetupEventListeners();
        SetCurrentMagazine(0); // 默认选择红色弹匣
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// 初始化弹匣
    /// </summary>
    private void InitializeMagazines()
    {
        // 创建弹匣实例（如果为null）
        if (redMagazine == null)
            redMagazine = new ColorMagazine("Red Magazine", 6);
        if (greenMagazine == null)
            greenMagazine = new ColorMagazine("Green Magazine", 6);
        if (blueMagazine == null)
            blueMagazine = new ColorMagazine("Blue Magazine", 6);
        if (mixMagazine == null)
            mixMagazine = new ColorMagazine("Mix Magazine", 6);

        // 设置基础颜色
        redMagazine.SetMagazineColor(baseRedColor);
        redMagazine.SetMagazineName("Red Magazine");
        
        greenMagazine.SetMagazineColor(baseGreenColor);
        greenMagazine.SetMagazineName("Green Magazine");
        
        blueMagazine.SetMagazineColor(baseBlueColor);
        blueMagazine.SetMagazineName("Blue Magazine");
        
        mixMagazine.SetMagazineName("Mix Magazine");

        if (showDebug)
            Debug.Log("ColorBag initialized with base colors");
    }

    /// <summary>
    /// 设置事件监听
    /// </summary>
    private void SetupEventListeners()
    {
        foreach (var magazine in AllMagazines)
        {
            if (magazine != null)
            {
                magazine.OnBulletAdded += HandleMagazineUpdated;
                magazine.OnBulletRemoved += HandleMagazineUpdated;
                magazine.OnMagazineCleared += HandleMagazineUpdated;
            }
        }
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        foreach (var magazine in AllMagazines)
        {
            if (magazine != null)
            {
                magazine.OnBulletAdded -= HandleMagazineUpdated;
                magazine.OnBulletRemoved -= HandleMagazineUpdated;
                magazine.OnMagazineCleared -= HandleMagazineUpdated;
            }
        }
    }

    private void HandleMagazineUpdated(ColorMagazine magazine)
    {
        OnMagazineUpdated?.Invoke(magazine);
    }

    #endregion

    #region Magazine Selection

    /// <summary>
    /// 设置当前选中的弹匣（通过索引）
    /// </summary>
    /// <param name="index">弹匣索引 (0=红, 1=绿, 2=蓝, 3=混合)</param>
    public bool SetCurrentMagazine(int index)
    {
        if (index < 0 || index >= AllMagazines.Length)
        {
            if (showDebug)
                Debug.LogWarning($"Invalid magazine index: {index}");
            return false;
        }

        var previousMagazine = CurrentColorMagazine;
        currentMagazineIndex = index;
        var newMagazine = CurrentColorMagazine;

        if (previousMagazine != newMagazine)
        {
            OnCurrentMagazineChanged?.Invoke(newMagazine);
            
            if (showDebug)
                Debug.Log($"Current magazine changed to: {newMagazine?.MagazineName}");
        }

        return true;
    }

    /// <summary>
    /// 根据索引获取弹匣
    /// </summary>
    private ColorMagazine GetMagazineByIndex(int index)
    {
        return index switch
        {
            0 => redMagazine,
            1 => greenMagazine,
            2 => blueMagazine,
            3 => mixMagazine,
            _ => null
        };
    }

    /// <summary>
    /// 获取弹匣的索引
    /// </summary>
    private int GetMagazineIndex(ColorMagazine magazine)
    {
        if (magazine == redMagazine) return 0;
        if (magazine == greenMagazine) return 1;
        if (magazine == blueMagazine) return 2;
        if (magazine == mixMagazine) return 3;
        return -1;
    }

    #endregion

    #region Mix Mode Management

 

    /// <summary>
    /// 进入混色模式
    /// </summary>
    public void EnterMixMode()
    {
        Debug.Log("Try Enter Mix Mode");
        ColorMagazine currentMag = CurrentColorMagazine;
        
        // 只有基础颜色弹匣才能进入混色模式
        if (currentMag != redMagazine && currentMag != greenMagazine && currentMag != blueMagazine)
        {
            if (showDebug)
                Debug.LogWarning("Can only enter mix mode with base color magazines");
            return;
        }

        // 检查当前弹匣是否满弹
        if (!currentMag.IsFull)
        {
            if (showDebug)
                Debug.LogWarning("Current magazine must be full to enter mix mode");
            return;
        }

        // 检查混合弹匣是否为空
        if (!mixMagazine.IsEmpty)
        {
            if (showDebug)
                Debug.LogWarning("Mix magazine must be empty to enter mix mode");
            return;
        }

        Debug.Log("Enter Mix Mode Success");
        IsMixMode = true;
        FirstMixMagazine = currentMag;
        OnMixModeChanged?.Invoke(true);
        OnFirstMixMagazineSelected?.Invoke(FirstMixMagazine);

        if (showDebug)
            Debug.Log($"Entered mix mode with {FirstMixMagazine.MagazineName}");
    }

    /// <summary>
    /// 退出混色模式
    /// </summary>
    public void ExitMixMode()
    {
        Debug.Log("Exit Mix Mode");
        IsMixMode = false;
        FirstMixMagazine = null;
        OnMixModeChanged?.Invoke(false);

        if (showDebug)
            Debug.Log("Exited mix mode");
    }

    /// <summary>
    /// 尝试与当前选中的弹匣进行混色
    /// </summary>
    public bool TryMixWithCurrentMagazine()
    {
        if (!IsMixMode || FirstMixMagazine == null)
        {
            if (showDebug)
                Debug.LogWarning("Not in mix mode or no first magazine selected");
            return false;
        }

        var secondMagazine = CurrentColorMagazine;
        
        // 检查第二个弹匣是否有效
        if (secondMagazine == null || secondMagazine == FirstMixMagazine)
        {
            if (showDebug)
                Debug.LogWarning("Invalid second magazine for mixing");
            return false;
        }

        // 只能与基础颜色弹匣混合
        if (secondMagazine != redMagazine && secondMagazine != greenMagazine && secondMagazine != blueMagazine)
        {
            if (showDebug)
                Debug.LogWarning("Can only mix with base color magazines");
            return false;
        }

        // 执行混色
        bool success = MixColors(FirstMixMagazine, secondMagazine);
        
        if (success)
        {
            ExitMixMode();
        }

        return success;
    }

    #endregion

    #region Color Management

    /// <summary>
    /// 根据颜色添加子弹到对应的弹匣
    /// </summary>
    /// <param name="color">要添加的颜色</param>
    /// <returns>是否成功添加</returns>
    public bool AddColorBullet(Color color)
    {
        ColorMagazine targetMagazine = GetMagazineForColor(color);
        
        if (targetMagazine == null)
        {
            if (showDebug)
                Debug.LogWarning($"No suitable magazine found for color: {color}");
            return false;
        }

        bool success = targetMagazine.AddBullet();
        
        if (showDebug && success)
            Debug.Log($"Added {color} bullet to {targetMagazine.MagazineName}");
            
        return success;
    }

    /// <summary>
    /// 根据颜色获取对应的弹匣
    /// </summary>
    /// <param name="color">颜色</param>
    /// <returns>对应的弹匣</returns>
    private ColorMagazine GetMagazineForColor(Color color)
    {
        // 首先检查是否匹配基础颜色
        if (IsColorSimilar(color, baseRedColor))
            return redMagazine;
        if (IsColorSimilar(color, baseGreenColor))
            return greenMagazine;
        if (IsColorSimilar(color, baseBlueColor))
            return blueMagazine;

        //// 检查混合弹匣是否匹配（如果混合弹匣已有颜色）
        //if (mixMagazine != null && !mixMagazine.IsEmpty && IsColorSimilar(color, mixMagazine.MagazineColor))
        //    return mixMagazine;

        //// 如果混合弹匣为空，可以接受任何非基础颜色
        //if (mixMagazine != null && mixMagazine.IsEmpty && 
        //    !IsColorSimilar(color, baseRedColor) && 
        //    !IsColorSimilar(color, baseGreenColor) && 
        //    !IsColorSimilar(color, baseBlueColor))
        //{
        //    return mixMagazine;
        //}

        return null;
    }

    /// <summary>
    /// 检查两个颜色是否相似
    /// </summary>
    private bool IsColorSimilar(Color color1, Color color2, float threshold = 0.2f)
    {
        return Vector4.Distance(color1, color2) <= threshold;
    }

    #endregion

    #region Color Mixing

    /// <summary>
    /// 混合两个弹匣的颜色
    /// </summary>
    /// <param name="magazine1">第一个弹匣</param>
    /// <param name="magazine2">第二个弹匣</param>
    /// <returns>是否成功混合</returns>
    public bool MixColors(ColorMagazine magazine1, ColorMagazine magazine2)
    {
        //// 检查输入有效性
        //if (magazine1 == null || magazine2 == null)
        //{
        //    if (showDebug)
        //        Debug.LogError("Cannot mix: One or both magazines are null");
        //    return false;
        //}

        //// 检查两个弹匣是否都满
        //if (!magazine1.IsFull || !magazine2.IsFull)
        //{
        //    if (showDebug)
        //        Debug.LogWarning($"Cannot mix: Both magazines must be full. {magazine1.MagazineName}: {magazine1.CurrentBullets}/{magazine1.BulletCapacity}, {magazine2.MagazineName}: {magazine2.CurrentBullets}/{magazine2.BulletCapacity}");
        //    return false;
        //}

        //// 检查混合弹匣是否为空
        //if (mixMagazine != null && !mixMagazine.IsEmpty)
        //{
        //    if (showDebug)
        //        Debug.LogWarning("Cannot mix: Mix magazine is not empty");
        //    return false;
        //}
        if(!CanMix(magazine1, magazine2))
        {
            if (showDebug)
                Debug.LogWarning("Cannot mix: Conditions not met for mixing magazines");
            return false;
        }

        // 执行混色
        Color mixedColor = PerformColorMixing(magazine1.MagazineColor, magazine2.MagazineColor);
        
        // 清空参与混色的弹匣
        magazine1.ClearMagazine();
        magazine2.ClearMagazine();
        
        // 将混合颜色填满混合弹匣
        if (mixMagazine != null)
        {
            mixMagazine.ClearMagazine();
            mixMagazine.SetMagazineColor(mixedColor);
            mixMagazine.FillMagazine();
        }

        // 触发事件
        OnColorsMixed?.Invoke(magazine1, magazine2, mixedColor);

        if (showDebug)
            Debug.Log($"Successfully mixed {magazine1.MagazineName} and {magazine2.MagazineName} to create {mixedColor}");

        return true;
    }

    /// <summary>
    /// 执行颜色混合计算
    /// </summary>
    private Color PerformColorMixing(Color color1, Color color2)
    {
        return ColorMixingStrategies.AverageMix(color1, color2);
    }

    #endregion




    #region Public Utility Methods

    /// <summary>
    /// 获取所有弹匣状态信息
    /// </summary>
    public string GetAllMagazineInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine("=== ColorBag Status ===");
        info.AppendLine($"Current Magazine: {CurrentColorMagazine?.MagazineName ?? "None"}");
        info.AppendLine($"Mix Mode: {IsMixMode}");
        if (IsMixMode && FirstMixMagazine != null)
            info.AppendLine($"First Mix Magazine: {FirstMixMagazine.MagazineName}");
        
        foreach (var magazine in AllMagazines)
        {
            if (magazine != null)
            {
                info.AppendLine($"{magazine.MagazineName}: {magazine.GetMagazineInfo()}");
            }
        }
        
        return info.ToString();
    }

    /// <summary>
    /// 检查是否可以进行混色
    /// </summary>
    public bool CanMix(ColorMagazine mag1, ColorMagazine mag2)
    {
        return (mag1 != null && mag2 != null) &&
               (mag1.IsFull && mag2.IsFull) &&
               (mixMagazine == null || mixMagazine.IsEmpty);
    }

    /// <summary>
    /// 清空所有弹匣
    /// </summary>
    public void ClearAllMagazines()
    {
        foreach (var magazine in AllMagazines)
        {
            magazine?.ClearMagazine();
        }
        
        ExitMixMode(); // 退出混色模式
        
        if (showDebug)
            Debug.Log("All magazines cleared");
    }

    /// <summary>
    /// 获取指定弹匣
    /// </summary>
    public ColorMagazine GetMagazine(MagazineType type)
    {
        return type switch
        {
            MagazineType.Red => redMagazine,
            MagazineType.Green => greenMagazine,
            MagazineType.Blue => blueMagazine,
            MagazineType.Mix => mixMagazine,
            _ => null
        };
    }

    #endregion

    #region Enums
    
    public enum MagazineType
    {
        Red,
        Green,
        Blue,
        Mix
    }

    #endregion
}
