using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 颜色射击器
/// 负责处理所有射击相关的逻辑，从ColorPicker中分离出来
/// </summary>
public class ColorShooter : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private ColorBag colorBag;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ColorPicker colorPicker;
    
    [Header("Shooting Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float bulletLifetime = 5f;
    [SerializeField] private Vector2 fireOffset = new Vector2(0.5f, 0f);
    
    [Header("Auto Fire Settings")]
    [SerializeField] private bool enableAutoFire = false;
    [SerializeField] private float fireRate = 0.2f; // 射击间隔（秒）
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private bool showFirePoint = true;
    #endregion

    #region Private Fields
    private InputService inputService;
    private float lastFireTime = 0f;
    #endregion

    #region Events
    public event Action OnBulletFired;
    public event Action OnShootAttempted;
    public event Action OnShootFailed;
    #endregion

    #region Public Properties
    public bool CanShoot => CanShootInternal();
    public Color CurrentBulletColor => GetCurrentBulletColor();
    public Vector3 FirePosition => GetFirePosition();
    public Vector2 FireDirection => GetFireDirection();
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
        if (playerController != null)
        {
            OnBulletFired += playerController.Attack;
        }
    }

    private void Start()
    {
        SetupInputEvents();
        
        if (showDebugInfo)
            Debug.Log("ColorShooter initialized");
    }

    private void Update()
    {
        HandleAutoFire();
    }

    private void OnDestroy()
    {
        CleanupInputEvents();
    }

    private void OnDrawGizmos()
    {
        if (showFirePoint)
        {
            DrawFirePointGizmos();
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void InitializeComponents()
    {
        // 自动查找组件
        if (colorBag == null)
            colorBag = FindObjectOfType<ColorBag>();
        
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
            
        if (colorPicker == null)
            colorPicker = FindObjectOfType<ColorPicker>();
            
        inputService = InputService.Instance;
        
        // 验证必要组件
        ValidateComponents();
    }

    /// <summary>
    /// 验证必要组件
    /// </summary>
    private void ValidateComponents()
    {
        if (colorBag == null)
            Debug.LogWarning("ColorShooter: ColorBag not found! Shooting may not work properly.");
            
        if (inputService == null)
            Debug.LogWarning("ColorShooter: InputService not found! Input handling may not work.");
            
        if (ColorBulletPool.Instance == null)
            Debug.LogWarning("ColorShooter: ColorBulletPool not found! Bullet creation may fail.");
    }

    /// <summary>
    /// 设置输入事件
    /// </summary>
    private void SetupInputEvents()
    {
        if (inputService?.inputMap?.Player != null && inputService.inputMap.Player.Fire != null)
        {
            inputService.inputMap.Player.Fire.started += OnFireInput;
        }
    }

    /// <summary>
    /// 清理输入事件
    /// </summary>
    private void CleanupInputEvents()
    {
        if (inputService?.inputMap?.Player != null && inputService.inputMap.Player.Fire != null)
        {
            inputService.inputMap.Player.Fire.started -= OnFireInput;
        }
        if(playerController != null)
        {
            OnBulletFired -= playerController.Attack;
        }
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// 处理射击输入
    /// </summary>
    private void OnFireInput(InputAction.CallbackContext context)
    {
        // 只有在ColorPicker没有检测到可拾取对象时才射击
        if (colorPicker == null || !colorPicker.HasPickableInRange())
        {
            TryShoot();
        }
    }

    #endregion

    #region Shooting Logic

    /// <summary>
    /// 尝试射击
    /// </summary>
    public bool TryShoot()
    {
        OnShootAttempted?.Invoke();
        
        // 检查射击条件
        if (!CanShootInternal())
        {
            OnShootFailed?.Invoke();
            
            if (showDebugInfo)
            {
                string reason = GetShootFailReason();
                Debug.LogWarning($"ColorShooter: Cannot shoot - {reason}");
            }
            
            return false;
        }

        // 检查射击间隔
        if (!CheckFireRate())
        {
            if (showDebugInfo)
                Debug.Log("ColorShooter: Fire rate limit exceeded");
            return false;
        }

        // 执行射击
        return PerformShoot();
    }

    /// <summary>
    /// 执行射击
    /// </summary>
    private bool PerformShoot()
    {
        var currentMagazine = colorBag.CurrentColorMagazine;
        
        // 获取射击参数
        Vector3 firePos = GetFirePosition();
        int fireDirection = GetFireDirectionInt();
        Color bulletColor = currentMagazine.MagazineColor;


        // 发射子弹
        ColorBullet bullet = ColorBulletPool.Instance.FireBullet(
            firePos,
            fireDirection,
            bulletColor,
            bulletSpeed,
            bulletDamage,
            lifetime: bulletLifetime
        );

        if (bullet != null)
        {
            // 从弹匣中消耗一发子弹
            currentMagazine.RemoveBullet();
            
            // 更新射击时间
            lastFireTime = Time.time;
            
            // 触发事件
            OnBulletFired?.Invoke();
            
            if (showDebugInfo)
            {
                Debug.Log($"ColorShooter: Fired {bulletColor} bullet from {currentMagazine.MagazineName}! " +
                         $"Remaining: {currentMagazine.CurrentBullets}/{currentMagazine.BulletCapacity}");
            }
            
            return true;
        }
        else
        {
            OnShootFailed?.Invoke();
            
            if (showDebugInfo)
                Debug.LogWarning("ColorShooter: Failed to fire bullet from pool!");
            
            return false;
        }
    }

    /// <summary>
    /// 检查是否可以射击
    /// </summary>
    private bool CanShootInternal()
    {
        return colorBag?.CurrentColorMagazine != null && 
               !colorBag.CurrentColorMagazine.IsEmpty &&
               ColorBulletPool.Instance != null;
    }

    /// <summary>
    /// 检查射击频率限制
    /// </summary>
    private bool CheckFireRate()
    {
        return Time.time - lastFireTime >= fireRate;
    }

    /// <summary>
    /// 获取射击失败原因
    /// </summary>
    private string GetShootFailReason()
    {
        if (colorBag == null)
            return "No ColorBag assigned";
        
        if (colorBag.CurrentColorMagazine == null)
            return "No magazine selected";
            
        if (colorBag.CurrentColorMagazine.IsEmpty)
            return "Magazine is empty";
            
        if (ColorBulletPool.Instance == null)
            return "ColorBulletPool not found";
            
        return "Unknown reason";
    }

    #endregion

    #region Position and Direction

    /// <summary>
    /// 获取发射位置
    /// </summary>
    private Vector3 GetFirePosition()
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }

        // 如果没有设置发射点，使用玩家位置加上偏移
        Vector3 basePosition = playerController != null ? playerController.transform.position : transform.position;
        
        // 根据玩家朝向调整偏移
        Vector2 adjustedOffset = fireOffset;
        if (playerController != null)
        {
            // 获取玩家朝向（从PlayerController的localScale判断）
            float direction = playerController.transform.localScale.x;
            adjustedOffset.x *= direction; // 根据朝向调整X偏移
        }
        
        return basePosition + (Vector3)adjustedOffset;
    }

    /// <summary>
    /// 获取发射方向
    /// </summary>
    private Vector2 GetFireDirection()
    {
        if (playerController != null)
        {
            // 根据玩家朝向确定发射方向
            float direction = playerController.transform.localScale.x;
            return new Vector2(direction, 0f).normalized;
        }

        // 默认向右发射
        return Vector2.right;
    }

    /// <summary>
    /// 获取发射方向（整数）
    /// </summary>
    private int GetFireDirectionInt()
    {
        if (playerController != null)
        {
            // 根据玩家朝向确定发射方向
            float direction = playerController.transform.localScale.x;
            return direction >= 0 ? 1 : -1;
        }

        // 默认向右发射
        return 1;
    }

    /// <summary>
    /// 获取当前子弹颜色
    /// </summary>
    private Color GetCurrentBulletColor()
    {
        return colorBag?.CurrentColorMagazine?.MagazineColor ?? Color.clear;
    }

    #endregion

    #region Auto Fire

    /// <summary>
    /// 处理自动射击
    /// </summary>
    private void HandleAutoFire()
    {
        if (!enableAutoFire) return;
        
        // 检查是否应该自动射击
        if (ShouldAutoFire())
        {
            TryShoot();
        }
    }

    /// <summary>
    /// 检查是否应该自动射击
    /// </summary>
    private bool ShouldAutoFire()
    {
        // 自动射击条件：
        // 1. 没有可拾取对象
        // 2. 有子弹可射击
        // 3. 达到射击频率
        return (colorPicker == null || !colorPicker.HasPickableInRange()) &&
               CanShootInternal() &&
               CheckFireRate();
    }

    #endregion




    #region Debug and Visualization

    /// <summary>
    /// 绘制发射点Gizmos
    /// </summary>
    private void DrawFirePointGizmos()
    {
        Vector3 firePos = GetFirePosition();
        Vector2 fireDir = GetFireDirection();
        
        // 发射点
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(firePos, 0.1f);
        
        // 发射方向
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(firePos, fireDir * 1.5f);
        
        // 发射方向箭头
        Vector3 arrowEnd = firePos + (Vector3)(fireDir * 1.5f);
        Vector3 arrowSide1 = arrowEnd + (Vector3)(Quaternion.Euler(0, 0, 135f) * fireDir * 0.3f);
        Vector3 arrowSide2 = arrowEnd + (Vector3)(Quaternion.Euler(0, 0, -135f) * fireDir * 0.3f);
        
        Gizmos.DrawLine(arrowEnd, arrowSide1);
        Gizmos.DrawLine(arrowEnd, arrowSide2);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 设置ColorBag引用
    /// </summary>
    public void SetColorBag(ColorBag bag)
    {
        colorBag = bag;
    }

    /// <summary>
    /// 设置PlayerController引用
    /// </summary>
    public void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
    }

    /// <summary>
    /// 设置ColorPicker引用
    /// </summary>
    public void SetColorPicker(ColorPicker picker)
    {
        colorPicker = picker;
    }

    /// <summary>
    /// 设置发射点
    /// </summary>
    public void SetFirePoint(Transform point)
    {
        firePoint = point;
    }

    /// <summary>
    /// 设置子弹参数
    /// </summary>
    public void SetBulletParameters(float speed, int damage, float lifetime)
    {
        bulletSpeed = speed;
        bulletDamage = damage;
        bulletLifetime = lifetime;
    }

    /// <summary>
    /// 设置发射偏移
    /// </summary>
    public void SetFireOffset(Vector2 offset)
    {
        fireOffset = offset;
    }

    /// <summary>
    /// 设置射击频率
    /// </summary>
    public void SetFireRate(float rate)
    {
        fireRate = Mathf.Max(0f, rate);
    }

    /// <summary>
    /// 启用/禁用自动射击
    /// </summary>
    public void SetAutoFire(bool enable)
    {
        enableAutoFire = enable;
    }

    /// <summary>
    /// 强制射击（忽略频率限制）
    /// </summary>
    public bool ForceShoot()
    {
        if (!CanShootInternal())
        {
            OnShootFailed?.Invoke();
            return false;
        }

        return PerformShoot();
    }

    /// <summary>
    /// 获取射击器状态信息
    /// </summary>
    public string GetShooterInfo()
    {
        var mag = colorBag?.CurrentColorMagazine;
        return $"ColorShooter - CanShoot: {CanShoot}, " +
               $"Magazine: {mag?.MagazineName ?? "None"} ({mag?.CurrentBullets ?? 0}/{mag?.BulletCapacity ?? 0}), " +
               $"FireRate: {fireRate}s, AutoFire: {enableAutoFire}";
    }

    #endregion
}