using System;
using UnityEngine;

[System.Serializable]
public class ColorMagazine
{
    #region Serialized Fields
    private int bulletCapacity = 6;
    private Color magazineColor = Color.clear;
    private int currentBullets = 0;
    private string magazineName = "Magazine";
    private bool showDebug = false;
    #endregion

    #region Public Properties
    public int BulletCapacity => bulletCapacity;
    public Color MagazineColor => magazineColor;
    public int CurrentBullets => currentBullets;
    public bool IsFull => currentBullets >= bulletCapacity;
    public bool IsEmpty => currentBullets <= 0;
    public float FillPercentage => (float)currentBullets / bulletCapacity;
    public string MagazineName => magazineName;
    #endregion

    #region Events
    public event Action<ColorMagazine> OnBulletAdded;
    public event Action<ColorMagazine> OnBulletRemoved;
    public event Action<ColorMagazine> OnMagazineFull;
    public event Action<ColorMagazine> OnMagazineEmpty;
    public event Action<ColorMagazine> OnMagazineCleared;
    public event Action<ColorMagazine> OnMagazineColorChanged;
    #endregion

    #region Constructor
    public ColorMagazine(string name = "Magazine", int capacity = 6)
    { 
        Debug.Log("create color magazine " + name);
        magazineName = name;
        bulletCapacity = capacity;
        currentBullets = 0;
        magazineColor = Color.clear;
    }
    #endregion

    #region Public Methods

    public bool AddBullet()   {
        if (IsFull)
        {
            if (showDebug)
                Debug.LogWarning($"Magazine is full! Cannot add more bullets. Current: {currentBullets}/{bulletCapacity}");
            return false;
        }

        //else if (!ColorsMatch(magazineColor, color))
        //{
        //    if (showDebug)
        //        Debug.LogWarning($"Color mismatch! Magazine color: {magazineColor}, Bullet color: {color}");
        //    return false;
        //}

        currentBullets++;
        OnBulletAdded?.Invoke(this);

        if (IsFull)
            OnMagazineFull?.Invoke(this);

        return true;
    }

    public bool RemoveBullet()
    {
        if (IsEmpty) return false;

        currentBullets--;
        OnBulletRemoved?.Invoke(this);

        if (IsEmpty)
        {
            OnMagazineEmpty?.Invoke(this);
        }

        return true;
    }

    public void ClearMagazine()
    {
        currentBullets = 0;
        //magazineColor = Color.clear;
        OnMagazineCleared?.Invoke(this);
        //OnMagazineColorChanged?.Invoke(this);
    }

    public void SetMagazineColor(Color color)
    {
        magazineColor = color;
        OnMagazineColorChanged?.Invoke(this);
    }

    public void SetMagazineName(string name)
    {
        magazineName = name;
    }
    public void SetMagazineCapacity(int capacity)
    {
        bulletCapacity = capacity;
        if (currentBullets > bulletCapacity)
            currentBullets = bulletCapacity;
    }
    public void FillMagazine()
    {
        int bulletsToAdd = bulletCapacity - currentBullets;
        currentBullets = bulletCapacity;
        
        if (bulletsToAdd > 0)
            OnMagazineFull?.Invoke(this);
    }


    public string GetMagazineInfo()
    {
        return $"Color: {magazineColor}, Bullets: {currentBullets}/{bulletCapacity} ({FillPercentage:P0})";
    }

    #endregion
}
