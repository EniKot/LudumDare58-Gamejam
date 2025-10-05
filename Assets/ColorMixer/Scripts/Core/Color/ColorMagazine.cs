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
    /// ���һ���ӵ�����ϻ
    /// </summary>
    /// <param name="color">�ӵ���ɫ</param>
    /// <returns>�Ƿ�ɹ����</returns>
    public bool AddBullet(Color color)
    {
        if (IsFull)
        {
            if (showDebug)
                Debug.LogWarning($"Magazine is full! Cannot add more bullets. Current: {currentBullets}/{bulletCapacity}");
            return false;
        }

        // ����ǵ�һ���ӵ������õ�ϻ��ɫ
        if (IsEmpty)
        {
            magazineColor = color;
        }
        // �����ɫ�Ƿ�ƥ�䣨����һ����
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
    /// �ӵ�ϻ�Ƴ�һ���ӵ�
    /// </summary>
    /// <returns>�Ƿ�ɹ��Ƴ�</returns>
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
    /// ��յ�ϻ
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
    /// ���õ�ϻ��ɫ��ͨ�����ڳ�ʼ��������ɫ��ϻ��
    /// </summary>
    /// <param name="color">��ϻ��ɫ</param>
    public void SetMagazineColor(Color color)
    {
        magazineColor = color;
        if (showDebug)
            Debug.Log($"Magazine color set to: {color}");
    }

    /// <summary>
    /// ������ϻ�������ã�
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
    /// ���������ɫ�Ƿ�ƥ�䣨����С��
    /// </summary>
    private bool ColorsMatch(Color color1, Color color2, float tolerance = 0.1f)
    {
        return Vector4.Distance(color1, color2) <= tolerance;
    }

    /// <summary>
    /// ��ȡ��ϻ״̬��Ϣ
    /// </summary>
    public string GetMagazineInfo()
    {
        return $"Color: {magazineColor}, Bullets: {currentBullets}/{bulletCapacity} ({FillPercentage:P0})";
    }

    #endregion

    #region Unity Lifecycle
    
    private void Start()
    {
        // ��ʼ���¼�����
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
