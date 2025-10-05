using System;
using UnityEngine;

[System.Serializable]
public class ColorMagazine : MonoBehaviour
{
    #region Serialized Fields
    [Header("Magazine Settings")]
    [SerializeField] private int bulletCapacity = 6;
    [SerializeField] private Color magazineColor = Color.white;
    [SerializeField] private int currentBullets = 0;
    
    [Header("Debug")]
    [SerializeField] private bool showDebug = false;
    #endregion

    #region Public Properties
    public int BulletCapacity => bulletCapacity;
    public Color MagazineColor => magazineColor;
    public int CurrentBullets => currentBullets;
    public bool IsFull => currentBullets >= bulletCapacity;
    public bool IsEmpty => currentBullets <= 0;
    public float FillPercentage => (float)currentBullets / bulletCapacity;
    #endregion

    #region Events
    public event Action<ColorMagazine> OnBulletAdded;
    public event Action<ColorMagazine> OnBulletRemoved;
    public event Action<ColorMagazine> OnMagazineFull;
    public event Action<ColorMagazine> OnMagazineEmpty;
    public event Action<ColorMagazine> OnMagazineCleared;
    #endregion

    #region Public Methods

    /// <summary>
    /// 添加一发子弹到弹匣
    /// </summary>
    /// <param name="color">子弹颜色</param>
    /// <returns>是否成功添加</returns>
    public bool AddBullet(Color color)
    {
        if (IsFull)
        {
            if (showDebug)
                Debug.LogWarning($"Magazine is full! Cannot add more bullets. Current: {currentBullets}/{bulletCapacity}");
            return false;
        }

        // 如果是第一发子弹，设置弹匣颜色
        if (IsEmpty)
        {
            magazineColor = color;
        }
        // 检查颜色是否匹配（允许一定误差）
        else if (!ColorsMatch(magazineColor, color))
        {
            if (showDebug)
                Debug.LogWarning($"Color mismatch! Magazine color: {magazineColor}, Bullet color: {color}");
            return false;
        }

        currentBullets++;
        OnBulletAdded?.Invoke(this);

        if (showDebug)
            Debug.Log($"Bullet added! Current: {currentBullets}/{bulletCapacity}, Color: {color}");

        if (IsFull)
        {
            OnMagazineFull?.Invoke(this);
            if (showDebug)
                Debug.Log($"Magazine is now full!");
        }

        return true;
    }

    /// <summary>
    /// 从弹匣移除一发子弹
    /// </summary>
    /// <returns>是否成功移除</returns>
    public bool RemoveBullet()
    {
        if (IsEmpty)
        {
            if (showDebug)
                Debug.LogWarning("Magazine is empty! Cannot remove bullets.");
            return false;
        }

        currentBullets--;
        OnBulletRemoved?.Invoke(this);

        if (showDebug)
            Debug.Log($"Bullet removed! Current: {currentBullets}/{bulletCapacity}");

        if (IsEmpty)
        {
            magazineColor = Color.clear;
            OnMagazineEmpty?.Invoke(this);
            if (showDebug)
                Debug.Log("Magazine is now empty!");
        }

        return true;
    }

    /// <summary>
    /// 清空弹匣
    /// </summary>
    public void ClearMagazine()
    {
        int previousBullets = currentBullets;
        currentBullets = 0;
        magazineColor = Color.clear;
        
        OnMagazineCleared?.Invoke(this);

        if (showDebug)
            Debug.Log($"Magazine cleared! Removed {previousBullets} bullets.");
    }

    /// <summary>
    /// 设置弹匣颜色（通常用于初始化基础颜色弹匣）
    /// </summary>
    /// <param name="color">弹匣颜色</param>
    public void SetMagazineColor(Color color)
    {
        magazineColor = color;
        if (showDebug)
            Debug.Log($"Magazine color set to: {color}");
    }

    /// <summary>
    /// 填满弹匣（调试用）
    /// </summary>
    public void FillMagazine()
    {
        int bulletsToAdd = bulletCapacity - currentBullets;
        currentBullets = bulletCapacity;
        
        if (bulletsToAdd > 0)
        {
            OnMagazineFull?.Invoke(this);
            if (showDebug)
                Debug.Log($"Magazine filled! Added {bulletsToAdd} bullets. Total: {currentBullets}/{bulletCapacity}");
        }
    }

    /// <summary>
    /// 检查两个颜色是否匹配（允许小误差）
    /// </summary>
    private bool ColorsMatch(Color color1, Color color2, float tolerance = 0.1f)
    {
        return Vector4.Distance(color1, color2) <= tolerance;
    }

    /// <summary>
    /// 获取弹匣状态信息
    /// </summary>
    public string GetMagazineInfo()
    {
        return $"Color: {magazineColor}, Bullets: {currentBullets}/{bulletCapacity} ({FillPercentage:P0})";
    }

    #endregion

    #region Unity Lifecycle
    
    private void Start()
    {
        // 初始化事件监听
        if (showDebug)
        {
            OnBulletAdded += (mag) => Debug.Log($"[{gameObject.name}] Bullet added");
            OnBulletRemoved += (mag) => Debug.Log($"[{gameObject.name}] Bullet removed");
            OnMagazineFull += (mag) => Debug.Log($"[{gameObject.name}] Magazine full!");
            OnMagazineEmpty += (mag) => Debug.Log($"[{gameObject.name}] Magazine empty!");
            OnMagazineCleared += (mag) => Debug.Log($"[{gameObject.name}] Magazine cleared!");
        }
    }

    #endregion
}
