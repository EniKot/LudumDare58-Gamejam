using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 颜色子弹对象池管理器
/// 使用 SingletonMono 基类和 Unity 自带的 ObjectPool
/// </summary>
public class ColorBulletPool : SingletonMono<ColorBulletPool>
{
    #region Serialized Fields
    [Header("Pool Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int defaultCapacity = 24;
    [SerializeField] private int maxSize = 100;
    [SerializeField] private bool collectionChecks = false;
    
    [Header("Bullet Default Settings")]
    [SerializeField] private float defaultSpeed = 15f;
    [SerializeField] private int defaultDamage = 1;
    [SerializeField] private int defaultPierce = 0;
    [SerializeField] private float defaultLifetime = 5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    #endregion

    #region Private Fields
    private ObjectPool<ColorBullet> bulletPool;
    private List<ColorBullet> activeBullets = new List<ColorBullet>();
    private Transform poolContainer;
    #endregion

    #region Public Properties
    public int ActiveCount => activeBullets.Count;
    public int InactiveCount => bulletPool?.CountInactive ?? 0;
    public int TotalCount => ActiveCount + InactiveCount;
    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();
        InitializePool();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void InitializePool()
    {
        // 创建容器
        if (poolContainer == null)
        {
            poolContainer = new GameObject("BulletContainer").transform;
            poolContainer.SetParent(transform);
        }
        
        // 验证子弹预制体
        if (bulletPrefab == null)
        {
            Debug.LogWarning("ColorBulletPool: No bullet prefab assigned! Pool may not work properly.");
            return;
        }

        // 初始化 Unity ObjectPool
        bulletPool = new ObjectPool<ColorBullet>(
            createFunc: CreateBullet,
            actionOnGet: OnGetBullet,
            actionOnRelease: OnReleaseBullet,
            actionOnDestroy: OnDestroyBullet,
            collectionCheck: collectionChecks,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        // 预热池子
        var warmupBullets = new List<ColorBullet>();
        for (int i = 0; i < defaultCapacity; i++)
        {
            warmupBullets.Add(bulletPool.Get());
        }
        
        foreach (var bullet in warmupBullets)
        {
            bulletPool.Release(bullet);
        }

        if (showDebugInfo)
            Debug.Log($"ColorBulletPool initialized with capacity {defaultCapacity}, maxSize {maxSize}");
    }

    #endregion

    #region ObjectPool Callbacks

    /// <summary>
    /// 创建新子弹
    /// </summary>
    private ColorBullet CreateBullet()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("ColorBulletPool: Cannot create bullet - no prefab assigned!");
            return null;
        }

        GameObject bulletObj = Instantiate(bulletPrefab, poolContainer);
        ColorBullet bullet = bulletObj.GetComponent<ColorBullet>();
        
        if (bullet == null)
        {
            Debug.LogError("ColorBulletPool: Bullet prefab must have ColorBullet component!");
            Destroy(bulletObj);
            return null;
        }

        // 设置回收回调
        bullet.OnReturnToPool = ReturnBullet;

        if (showDebugInfo)
            Debug.Log($"Created new bullet. Total created: {TotalCount}");

        return bullet;
    }

    /// <summary>
    /// 从池中获取子弹时调用
    /// </summary>
    private void OnGetBullet(ColorBullet bullet)
    {
        if (bullet != null)
        {
            bullet.gameObject.SetActive(true);
            activeBullets.Add(bullet);
            
            if (showDebugInfo)
                Debug.Log($"Got bullet from pool. Active: {ActiveCount}, Inactive: {InactiveCount}");
        }
    }

    /// <summary>
    /// 子弹返回池中时调用
    /// </summary>
    private void OnReleaseBullet(ColorBullet bullet)
    {
        if (bullet != null)
        {
            bullet.gameObject.SetActive(false);
            bullet.transform.SetParent(poolContainer);
            
            // 从活跃列表移除
            if (activeBullets.Contains(bullet))
            {
                activeBullets.Remove(bullet);
            }
            
            if (showDebugInfo)
                Debug.Log($"Released bullet to pool. Active: {ActiveCount}, Inactive: {InactiveCount}");
        }
    }

    /// <summary>
    /// 销毁子弹时调用
    /// </summary>
    private void OnDestroyBullet(ColorBullet bullet)
    {
        if (bullet != null)
        {
            // 从活跃列表移除（如果存在）
            if (activeBullets.Contains(bullet))
            {
                activeBullets.Remove(bullet);
            }
            
            Destroy(bullet.gameObject);
            
            if (showDebugInfo)
                Debug.Log($"Destroyed bullet. Active: {ActiveCount}, Inactive: {InactiveCount}");
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 发射子弹
    /// </summary>
    /// <param name="position">发射位置</param>
    /// <param name="direction">发射方向（1=右，-1=左）</param>
    /// <param name="color">子弹颜色</param>
    /// <param name="speed">子弹速度（可选）</param>
    /// <param name="damage">子弹伤害（可选）</param>
    /// <param name="pierce">穿透次数（可选）</param>
    /// <param name="lifetime">存活时间（可选）</param>
    /// <returns>发射的子弹实例，失败返回null</returns>
    public ColorBullet FireBullet(Vector3 position, int direction, Color color,
        float? speed = null, int? damage = null, int? pierce = null, float? lifetime = null)
    {
        if (bulletPool == null)
        {
            Debug.LogError("ColorBulletPool: Pool not initialized!");
            return null;
        }

        ColorBullet bullet = bulletPool.Get();
        if (bullet == null)
        {
            if (showDebugInfo)
                Debug.LogWarning("ColorBulletPool: Failed to get bullet from pool");
            return null;
        }

        // 确保方向只能是1或-1
        int bulletDirection = direction >= 0 ? 1 : -1;

        // 初始化子弹
        bullet.Initialize(
            position,
            bulletDirection,
            color,
            speed ?? defaultSpeed,
            damage ?? defaultDamage,
            pierce ?? defaultPierce,
            lifetime ?? defaultLifetime
        );

        if (showDebugInfo)
        {
            string directionText = bulletDirection > 0 ? "Right" : "Left";
            Debug.Log($"Fired bullet at {position} towards {directionText} with color {color}");
        }

        return bullet;
    }

    /// <summary>
    /// 回收子弹到对象池
    /// </summary>
    /// <param name="bullet">要回收的子弹</param>
    public void ReturnBullet(ColorBullet bullet)
    {
        if (bullet == null || bulletPool == null) return;

        // 清除回调（避免重复调用）
        bullet.OnReturnToPool = ReturnBullet;

        // 释放到池中
        bulletPool.Release(bullet);
    }

    /// <summary>
    /// 清空所有活跃子弹
    /// </summary>
    public void ClearAllActiveBullets()
    {
        if (bulletPool == null) return;

        // 创建副本避免在遍历时修改列表
        var bulletsToReturn = new List<ColorBullet>(activeBullets);
        
        foreach (var bullet in bulletsToReturn)
        {
            if (bullet != null)
            {
                bullet.ReturnToPool();
            }
        }

        if (showDebugInfo)
            Debug.Log($"Cleared all active bullets. Active: {ActiveCount}, Inactive: {InactiveCount}");
    }

    /// <summary>
    /// 获取池状态信息
    /// </summary>
    /// <returns>池状态字符串</returns>
    public string GetPoolInfo()
    {
        return $"ColorBulletPool - Active: {ActiveCount}, Inactive: {InactiveCount}, Total: {TotalCount}, MaxSize: {maxSize}";
    }

    /// <summary>
    /// 释放对象池资源
    /// </summary>
    public void DisposePool()
    {
        if (bulletPool != null)
        {
            ClearAllActiveBullets();
            bulletPool.Dispose();
            bulletPool = null;
        }
    }

    #endregion

    #region Debug

    [ContextMenu("Test Fire Random Bullet")]
    private void TestFireRandomBullet()
    {
        // 随机选择左右方向
        int direction = Random.value > 0.5f ? 1 : -1;
        Color randomColor = new Color(Random.value, Random.value, Random.value, 1f);
        
        FireBullet(transform.position, direction, randomColor);
    }

    [ContextMenu("Clear All Bullets")]
    private void TestClearAllBullets()
    {
        ClearAllActiveBullets();
    }

    [ContextMenu("Show Pool Info")]
    private void TestShowPoolInfo()
    {
        Debug.Log(GetPoolInfo());
    }

    [ContextMenu("Dispose Pool")]
    private void TestDisposePool()
    {
        DisposePool();
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;

        float yOffset = 200f;
        float lineHeight = 20f;
        
        GUI.color = Color.white;
        GUI.Label(new Rect(10, yOffset, 300, lineHeight), "=== ColorBulletPool Debug ===");
        yOffset += lineHeight;
        
        GUI.Label(new Rect(10, yOffset, 300, lineHeight), $"Active: {ActiveCount}");
        yOffset += lineHeight;
        
        GUI.Label(new Rect(10, yOffset, 300, lineHeight), $"Inactive: {InactiveCount}");
        yOffset += lineHeight;
        
        GUI.Label(new Rect(10, yOffset, 300, lineHeight), $"Total: {TotalCount} / {maxSize}");
        yOffset += lineHeight;
        
        GUI.Label(new Rect(10, yOffset, 300, lineHeight), $"Pool Status: {(bulletPool != null ? "Initialized" : "Not Initialized")}");
        yOffset += lineHeight;

        if (GUI.Button(new Rect(10, yOffset, 100, 25), "Clear All"))
        {
            ClearAllActiveBullets();
        }
        
        if (GUI.Button(new Rect(120, yOffset, 100, 25), "Fire Test"))
        {
            TestFireRandomBullet();
        }
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        DisposePool();
    }

    #endregion
}
