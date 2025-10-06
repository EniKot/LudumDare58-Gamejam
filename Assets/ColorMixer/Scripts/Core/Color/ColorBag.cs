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
        SetCurrentMagazine(0); // Ĭ��ѡ���ɫ��ϻ
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// ��ʼ����ϻ
    /// </summary>
    private void InitializeMagazines()
    {
        // ������ϻʵ�������Ϊnull��
        if (redMagazine == null)
            redMagazine = new ColorMagazine("Red Magazine", 6);
        if (greenMagazine == null)
            greenMagazine = new ColorMagazine("Green Magazine", 6);
        if (blueMagazine == null)
            blueMagazine = new ColorMagazine("Blue Magazine", 6);
        if (mixMagazine == null)
            mixMagazine = new ColorMagazine("Mix Magazine", 6);

        // ���û�����ɫ
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

    /// <summary>
    /// ȡ�������¼�
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
    /// ���õ�ǰѡ�еĵ�ϻ��ͨ��������
    /// </summary>
    /// <param name="index">��ϻ���� (0=��, 1=��, 2=��, 3=���)</param>
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
    /// ����������ȡ��ϻ
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
    /// ��ȡ��ϻ������
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
    /// �����ɫģʽ
    /// </summary>
    public void EnterMixMode()
    {
        Debug.Log("Try Enter Mix Mode");
        ColorMagazine currentMag = CurrentColorMagazine;
        
        // ֻ�л�����ɫ��ϻ���ܽ����ɫģʽ
        if (currentMag != redMagazine && currentMag != greenMagazine && currentMag != blueMagazine)
        {
            if (showDebug)
                Debug.LogWarning("Can only enter mix mode with base color magazines");
            return;
        }

        // ��鵱ǰ��ϻ�Ƿ�����
        if (!currentMag.IsFull)
        {
            if (showDebug)
                Debug.LogWarning("Current magazine must be full to enter mix mode");
            return;
        }

        // ����ϵ�ϻ�Ƿ�Ϊ��
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
    /// �˳���ɫģʽ
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
    /// �����뵱ǰѡ�еĵ�ϻ���л�ɫ
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
        
        // ���ڶ�����ϻ�Ƿ���Ч
        if (secondMagazine == null || secondMagazine == FirstMixMagazine)
        {
            if (showDebug)
                Debug.LogWarning("Invalid second magazine for mixing");
            return false;
        }

        // ֻ���������ɫ��ϻ���
        if (secondMagazine != redMagazine && secondMagazine != greenMagazine && secondMagazine != blueMagazine)
        {
            if (showDebug)
                Debug.LogWarning("Can only mix with base color magazines");
            return false;
        }

        // ִ�л�ɫ
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

        bool success = targetMagazine.AddBullet();
        
        if (showDebug && success)
            Debug.Log($"Added {color} bullet to {targetMagazine.MagazineName}");
            
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

        //// ����ϵ�ϻ�Ƿ�ƥ�䣨�����ϵ�ϻ������ɫ��
        //if (mixMagazine != null && !mixMagazine.IsEmpty && IsColorSimilar(color, mixMagazine.MagazineColor))
        //    return mixMagazine;

        //// �����ϵ�ϻΪ�գ����Խ����κηǻ�����ɫ
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
        //// ���������Ч��
        //if (magazine1 == null || magazine2 == null)
        //{
        //    if (showDebug)
        //        Debug.LogError("Cannot mix: One or both magazines are null");
        //    return false;
        //}

        //// ���������ϻ�Ƿ���
        //if (!magazine1.IsFull || !magazine2.IsFull)
        //{
        //    if (showDebug)
        //        Debug.LogWarning($"Cannot mix: Both magazines must be full. {magazine1.MagazineName}: {magazine1.CurrentBullets}/{magazine1.BulletCapacity}, {magazine2.MagazineName}: {magazine2.CurrentBullets}/{magazine2.BulletCapacity}");
        //    return false;
        //}

        //// ����ϵ�ϻ�Ƿ�Ϊ��
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

        // ִ�л�ɫ
        Color mixedColor = PerformColorMixing(magazine1.MagazineColor, magazine2.MagazineColor);
        
        // ��ղ����ɫ�ĵ�ϻ
        magazine1.ClearMagazine();
        magazine2.ClearMagazine();
        
        // �������ɫ������ϵ�ϻ
        if (mixMagazine != null)
        {
            mixMagazine.ClearMagazine();
            mixMagazine.SetMagazineColor(mixedColor);
            mixMagazine.FillMagazine();
        }

        // �����¼�
        OnColorsMixed?.Invoke(magazine1, magazine2, mixedColor);

        if (showDebug)
            Debug.Log($"Successfully mixed {magazine1.MagazineName} and {magazine2.MagazineName} to create {mixedColor}");

        return true;
    }

    /// <summary>
    /// ִ����ɫ��ϼ���
    /// </summary>
    private Color PerformColorMixing(Color color1, Color color2)
    {
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
    /// ����Ƿ���Խ��л�ɫ
    /// </summary>
    public bool CanMix(ColorMagazine mag1, ColorMagazine mag2)
    {
        return (mag1 != null && mag2 != null) &&
               (mag1.IsFull && mag2.IsFull) &&
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
        
        ExitMixMode(); // �˳���ɫģʽ
        
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
