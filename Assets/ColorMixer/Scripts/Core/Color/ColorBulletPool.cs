using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// ��ɫ�ӵ�����ع�����
/// ʹ�� SingletonMono ����� Unity �Դ��� ObjectPool
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
    /// ��ʼ�������
    /// </summary>
    private void InitializePool()
    {
        // ��������
        if (poolContainer == null)
        {
            poolContainer = new GameObject("BulletContainer").transform;
            poolContainer.SetParent(transform);
        }
        
        // ��֤�ӵ�Ԥ����
        if (bulletPrefab == null)
        {
            Debug.LogWarning("ColorBulletPool: No bullet prefab assigned! Pool may not work properly.");
            return;
        }

        // ��ʼ�� Unity ObjectPool
        bulletPool = new ObjectPool<ColorBullet>(
            createFunc: CreateBullet,
            actionOnGet: OnGetBullet,
            actionOnRelease: OnReleaseBullet,
            actionOnDestroy: OnDestroyBullet,
            collectionCheck: collectionChecks,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        // Ԥ�ȳ���
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
    /// �������ӵ�
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

        // ���û��ջص�
        bullet.OnReturnToPool = ReturnBullet;

        if (showDebugInfo)
            Debug.Log($"Created new bullet. Total created: {TotalCount}");

        return bullet;
    }

    /// <summary>
    /// �ӳ��л�ȡ�ӵ�ʱ����
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
    /// �ӵ����س���ʱ����
    /// </summary>
    private void OnReleaseBullet(ColorBullet bullet)
    {
        if (bullet != null)
        {
            bullet.gameObject.SetActive(false);
            bullet.transform.SetParent(poolContainer);
            
            // �ӻ�Ծ�б��Ƴ�
            if (activeBullets.Contains(bullet))
            {
                activeBullets.Remove(bullet);
            }
            
            if (showDebugInfo)
                Debug.Log($"Released bullet to pool. Active: {ActiveCount}, Inactive: {InactiveCount}");
        }
    }

    /// <summary>
    /// �����ӵ�ʱ����
    /// </summary>
    private void OnDestroyBullet(ColorBullet bullet)
    {
        if (bullet != null)
        {
            // �ӻ�Ծ�б��Ƴ���������ڣ�
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
    /// �����ӵ�
    /// </summary>
    /// <param name="position">����λ��</param>
    /// <param name="direction">���䷽��1=�ң�-1=��</param>
    /// <param name="color">�ӵ���ɫ</param>
    /// <param name="speed">�ӵ��ٶȣ���ѡ��</param>
    /// <param name="damage">�ӵ��˺�����ѡ��</param>
    /// <param name="pierce">��͸��������ѡ��</param>
    /// <param name="lifetime">���ʱ�䣨��ѡ��</param>
    /// <returns>������ӵ�ʵ����ʧ�ܷ���null</returns>
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

        // ȷ������ֻ����1��-1
        int bulletDirection = direction >= 0 ? 1 : -1;

        // ��ʼ���ӵ�
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
    /// �����ӵ��������
    /// </summary>
    /// <param name="bullet">Ҫ���յ��ӵ�</param>
    public void ReturnBullet(ColorBullet bullet)
    {
        if (bullet == null || bulletPool == null) return;

        // ����ص��������ظ����ã�
        bullet.OnReturnToPool = ReturnBullet;

        // �ͷŵ�����
        bulletPool.Release(bullet);
    }

    /// <summary>
    /// ������л�Ծ�ӵ�
    /// </summary>
    public void ClearAllActiveBullets()
    {
        if (bulletPool == null) return;

        // �������������ڱ���ʱ�޸��б�
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
    /// ��ȡ��״̬��Ϣ
    /// </summary>
    /// <returns>��״̬�ַ���</returns>
    public string GetPoolInfo()
    {
        return $"ColorBulletPool - Active: {ActiveCount}, Inactive: {InactiveCount}, Total: {TotalCount}, MaxSize: {maxSize}";
    }

    /// <summary>
    /// �ͷŶ������Դ
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
        // ���ѡ�����ҷ���
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
