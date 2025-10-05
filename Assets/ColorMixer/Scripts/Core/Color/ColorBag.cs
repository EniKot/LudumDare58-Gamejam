using System;
using System.Collections.Generic;
using UnityEngine;

public class ColorBag : MonoBehaviour
{
    #region Serialized Fields
    [Header("Magazine References")]
    [SerializeField] private ColorMagazine redMagazine;
    [SerializeField] private ColorMagazine greenMagazine;
    [SerializeField] private ColorMagazine blueMagazine;
    [SerializeField] private ColorMagazine mixMagazine;
    
    [Header("Color Settings")]
    [SerializeField] private Color baseRedColor = Color.red;
    [SerializeField] private Color baseGreenColor = Color.green;
    [SerializeField] private Color baseBlueColor = Color.blue;
    
    [Header("Debug")]
    [SerializeField] private bool showDebug = false;
    #endregion

    #region Public Properties
    public ColorMagazine RedMagazine => redMagazine;
    public ColorMagazine GreenMagazine => greenMagazine;
    public ColorMagazine BlueMagazine => blueMagazine;
    public ColorMagazine MixMagazine => mixMagazine;
    
    public ColorMagazine[] BaseMagazines => new ColorMagazine[] { redMagazine, greenMagazine, blueMagazine };
    public ColorMagazine[] AllMagazines => new ColorMagazine[] { redMagazine, greenMagazine, blueMagazine, mixMagazine };
    #endregion

    #region Events
    public event Action<ColorMagazine, ColorMagazine, Color> OnColorsMixed;
    public event Action<ColorMagazine> OnMagazineUpdated;
    #endregion

    #region Unity Lifecycle
    
    private void Start()
    {
        InitializeMagazines();
        SetupEventListeners();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// 初始化弹匣
    /// </summary>
    private void InitializeMagazines()
    {
        // 设置基础颜色弹匣的颜色
        if (redMagazine != null)
        {
            redMagazine.SetMagazineColor(baseRedColor);
            redMagazine.gameObject.name = "Red Magazine";
        }
        
        if (greenMagazine != null)
        {
            greenMagazine.SetMagazineColor(baseGreenColor);
            greenMagazine.gameObject.name = "Green Magazine";
        }
        
        if (blueMagazine != null)
        {
            blueMagazine.SetMagazineColor(baseBlueColor);
            blueMagazine.gameObject.name = "Blue Magazine";
        }
        
        if (mixMagazine != null)
        {
            mixMagazine.gameObject.name = "Mix Magazine";
        }

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

    private void HandleMagazineUpdated(ColorMagazine magazine)
    {
        OnMagazineUpdated?.Invoke(magazine);
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

        bool success = targetMagazine.AddBullet(color);
        
        if (showDebug && success)
            Debug.Log($"Added {color} bullet to {targetMagazine.gameObject.name}");
            
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

        // 检查混合弹匣是否匹配（如果混合弹匣已有颜色）
        if (mixMagazine != null && !mixMagazine.IsEmpty && IsColorSimilar(color, mixMagazine.MagazineColor))
            return mixMagazine;

        // 如果混合弹匣为空，可以接受任何非基础颜色
        if (mixMagazine != null && mixMagazine.IsEmpty && 
            !IsColorSimilar(color, baseRedColor) && 
            !IsColorSimilar(color, baseGreenColor) && 
            !IsColorSimilar(color, baseBlueColor))
        {
            return mixMagazine;
        }

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
        // 检查输入有效性
        if (magazine1 == null || magazine2 == null)
        {
            if (showDebug)
                Debug.LogError("Cannot mix: One or both magazines are null");
            return false;
        }

        // 检查两个弹匣是否都满
        if (!magazine1.IsFull || !magazine2.IsFull)
        {
            if (showDebug)
                Debug.LogWarning($"Cannot mix: Both magazines must be full. {magazine1.gameObject.name}: {magazine1.CurrentBullets}/{magazine1.BulletCapacity}, {magazine2.gameObject.name}: {magazine2.CurrentBullets}/{magazine2.BulletCapacity}");
            return false;
        }

        // 检查混合弹匣是否为空
        if (mixMagazine != null && !mixMagazine.IsEmpty)
        {
            if (showDebug)
                Debug.LogWarning("Cannot mix: Mix magazine is not empty");
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
            mixMagazine.SetMagazineColor(mixedColor);
            mixMagazine.FillMagazine();
        }

        // 触发事件
        OnColorsMixed?.Invoke(magazine1, magazine2, mixedColor);

        if (showDebug)
            Debug.Log($"Successfully mixed {magazine1.gameObject.name} and {magazine2.gameObject.name} to create {mixedColor}");

        return true;
    }

    /// <summary>
    /// 混合红色和绿色弹匣
    /// </summary>
    public bool MixRedGreen()
    {
        return MixColors(redMagazine, greenMagazine);
    }

    /// <summary>
    /// 混合红色和蓝色弹匣
    /// </summary>
    public bool MixRedBlue()
    {
        return MixColors(redMagazine, blueMagazine);
    }

    /// <summary>
    /// 混合绿色和蓝色弹匣
    /// </summary>
    public bool MixGreenBlue()
    {
        return MixColors(greenMagazine, blueMagazine);
    }

    /// <summary>
    /// 执行颜色混合计算
    /// </summary>
    private Color PerformColorMixing(Color color1, Color color2)
    {
        // 使用 ColorMixingStrategies 中的混合方法
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
        
        foreach (var magazine in AllMagazines)
        {
            if (magazine != null)
            {
                info.AppendLine($"{magazine.gameObject.name}: {magazine.GetMagazineInfo()}");
            }
        }
        
        return info.ToString();
    }

    /// <summary>
    /// 检查是否可以进行混色
    /// </summary>
    public bool CanMix(ColorMagazine mag1, ColorMagazine mag2)
    {
        return mag1 != null && mag2 != null && 
               mag1.IsFull && mag2.IsFull && 
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
