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
    /// ��ʼ����ϻ
    /// </summary>
    private void InitializeMagazines()
    {
        // ���û�����ɫ��ϻ����ɫ
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
    /// �����¼�����
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
    /// ������ɫ����ӵ�����Ӧ�ĵ�ϻ
    /// </summary>
    /// <param name="color">Ҫ��ӵ���ɫ</param>
    /// <returns>�Ƿ�ɹ����</returns>
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
    /// ������ɫ��ȡ��Ӧ�ĵ�ϻ
    /// </summary>
    /// <param name="color">��ɫ</param>
    /// <returns>��Ӧ�ĵ�ϻ</returns>
    private ColorMagazine GetMagazineForColor(Color color)
    {
        // ���ȼ���Ƿ�ƥ�������ɫ
        if (IsColorSimilar(color, baseRedColor))
            return redMagazine;
        if (IsColorSimilar(color, baseGreenColor))
            return greenMagazine;
        if (IsColorSimilar(color, baseBlueColor))
            return blueMagazine;

        // ����ϵ�ϻ�Ƿ�ƥ�䣨�����ϵ�ϻ������ɫ��
        if (mixMagazine != null && !mixMagazine.IsEmpty && IsColorSimilar(color, mixMagazine.MagazineColor))
            return mixMagazine;

        // �����ϵ�ϻΪ�գ����Խ����κηǻ�����ɫ
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
    /// ���������ɫ�Ƿ�����
    /// </summary>
    private bool IsColorSimilar(Color color1, Color color2, float threshold = 0.2f)
    {
        return Vector4.Distance(color1, color2) <= threshold;
    }

    #endregion

    #region Color Mixing

    /// <summary>
    /// ���������ϻ����ɫ
    /// </summary>
    /// <param name="magazine1">��һ����ϻ</param>
    /// <param name="magazine2">�ڶ�����ϻ</param>
    /// <returns>�Ƿ�ɹ����</returns>
    public bool MixColors(ColorMagazine magazine1, ColorMagazine magazine2)
    {
        // ���������Ч��
        if (magazine1 == null || magazine2 == null)
        {
            if (showDebug)
                Debug.LogError("Cannot mix: One or both magazines are null");
            return false;
        }

        // ���������ϻ�Ƿ���
        if (!magazine1.IsFull || !magazine2.IsFull)
        {
            if (showDebug)
                Debug.LogWarning($"Cannot mix: Both magazines must be full. {magazine1.gameObject.name}: {magazine1.CurrentBullets}/{magazine1.BulletCapacity}, {magazine2.gameObject.name}: {magazine2.CurrentBullets}/{magazine2.BulletCapacity}");
            return false;
        }

        // ����ϵ�ϻ�Ƿ�Ϊ��
        if (mixMagazine != null && !mixMagazine.IsEmpty)
        {
            if (showDebug)
                Debug.LogWarning("Cannot mix: Mix magazine is not empty");
            return false;
        }

        // ִ�л�ɫ
        Color mixedColor = PerformColorMixing(magazine1.MagazineColor, magazine2.MagazineColor);
        
        // ��ղ����ɫ�ĵ�ϻ
        magazine1.ClearMagazine();
        magazine2.ClearMagazine();
        
        // �������ɫ������ϵ�ϻ
        if (mixMagazine != null)
        {
            mixMagazine.SetMagazineColor(mixedColor);
            mixMagazine.FillMagazine();
        }

        // �����¼�
        OnColorsMixed?.Invoke(magazine1, magazine2, mixedColor);

        if (showDebug)
            Debug.Log($"Successfully mixed {magazine1.gameObject.name} and {magazine2.gameObject.name} to create {mixedColor}");

        return true;
    }

    /// <summary>
    /// ��Ϻ�ɫ����ɫ��ϻ
    /// </summary>
    public bool MixRedGreen()
    {
        return MixColors(redMagazine, greenMagazine);
    }

    /// <summary>
    /// ��Ϻ�ɫ����ɫ��ϻ
    /// </summary>
    public bool MixRedBlue()
    {
        return MixColors(redMagazine, blueMagazine);
    }

    /// <summary>
    /// �����ɫ����ɫ��ϻ
    /// </summary>
    public bool MixGreenBlue()
    {
        return MixColors(greenMagazine, blueMagazine);
    }

    /// <summary>
    /// ִ����ɫ��ϼ���
    /// </summary>
    private Color PerformColorMixing(Color color1, Color color2)
    {
        // ʹ�� ColorMixingStrategies �еĻ�Ϸ���
        return ColorMixingStrategies.AverageMix(color1, color2);
    }

    #endregion

    #region Public Utility Methods

    /// <summary>
    /// ��ȡ���е�ϻ״̬��Ϣ
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
    /// ����Ƿ���Խ��л�ɫ
    /// </summary>
    public bool CanMix(ColorMagazine mag1, ColorMagazine mag2)
    {
        return mag1 != null && mag2 != null && 
               mag1.IsFull && mag2.IsFull && 
               (mixMagazine == null || mixMagazine.IsEmpty);
    }

    /// <summary>
    /// ������е�ϻ
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
    /// ��ȡָ����ϻ
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
