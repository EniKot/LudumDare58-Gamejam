using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ��ɫ�����
/// ���������������ص��߼�����ColorPicker�з������
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
    [SerializeField] private float fireRate = 0.2f; // ���������룩
    
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
    /// ��ʼ���������
    /// </summary>
    private void InitializeComponents()
    {
        // �Զ��������
        if (colorBag == null)
            colorBag = FindObjectOfType<ColorBag>();
        
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
            
        if (colorPicker == null)
            colorPicker = FindObjectOfType<ColorPicker>();
            
        inputService = InputService.Instance;
        
        // ��֤��Ҫ���
        ValidateComponents();
    }

    /// <summary>
    /// ��֤��Ҫ���
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
    /// ���������¼�
    /// </summary>
    private void SetupInputEvents()
    {
        if (inputService?.inputMap?.Player != null && inputService.inputMap.Player.Fire != null)
        {
            inputService.inputMap.Player.Fire.started += OnFireInput;
        }
    }

    /// <summary>
    /// ���������¼�
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
    /// �����������
    /// </summary>
    private void OnFireInput(InputAction.CallbackContext context)
    {
        // ֻ����ColorPickerû�м�⵽��ʰȡ����ʱ�����
        if (colorPicker == null || !colorPicker.HasPickableInRange())
        {
            TryShoot();
        }
    }

    #endregion

    #region Shooting Logic

    /// <summary>
    /// �������
    /// </summary>
    public bool TryShoot()
    {
        OnShootAttempted?.Invoke();
        
        // ����������
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

        // ���������
        if (!CheckFireRate())
        {
            if (showDebugInfo)
                Debug.Log("ColorShooter: Fire rate limit exceeded");
            return false;
        }

        // ִ�����
        return PerformShoot();
    }

    /// <summary>
    /// ִ�����
    /// </summary>
    private bool PerformShoot()
    {
        var currentMagazine = colorBag.CurrentColorMagazine;
        
        // ��ȡ�������
        Vector3 firePos = GetFirePosition();
        int fireDirection = GetFireDirectionInt();
        Color bulletColor = currentMagazine.MagazineColor;


        // �����ӵ�
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
            // �ӵ�ϻ������һ���ӵ�
            currentMagazine.RemoveBullet();
            
            // �������ʱ��
            lastFireTime = Time.time;
            
            // �����¼�
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
    /// ����Ƿ�������
    /// </summary>
    private bool CanShootInternal()
    {
        return colorBag?.CurrentColorMagazine != null && 
               !colorBag.CurrentColorMagazine.IsEmpty &&
               ColorBulletPool.Instance != null;
    }

    /// <summary>
    /// ������Ƶ������
    /// </summary>
    private bool CheckFireRate()
    {
        return Time.time - lastFireTime >= fireRate;
    }

    /// <summary>
    /// ��ȡ���ʧ��ԭ��
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
    /// ��ȡ����λ��
    /// </summary>
    private Vector3 GetFirePosition()
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }

        // ���û�����÷���㣬ʹ�����λ�ü���ƫ��
        Vector3 basePosition = playerController != null ? playerController.transform.position : transform.position;
        
        // ������ҳ������ƫ��
        Vector2 adjustedOffset = fireOffset;
        if (playerController != null)
        {
            // ��ȡ��ҳ��򣨴�PlayerController��localScale�жϣ�
            float direction = playerController.transform.localScale.x;
            adjustedOffset.x *= direction; // ���ݳ������Xƫ��
        }
        
        return basePosition + (Vector3)adjustedOffset;
    }

    /// <summary>
    /// ��ȡ���䷽��
    /// </summary>
    private Vector2 GetFireDirection()
    {
        if (playerController != null)
        {
            // ������ҳ���ȷ�����䷽��
            float direction = playerController.transform.localScale.x;
            return new Vector2(direction, 0f).normalized;
        }

        // Ĭ�����ҷ���
        return Vector2.right;
    }

    /// <summary>
    /// ��ȡ���䷽��������
    /// </summary>
    private int GetFireDirectionInt()
    {
        if (playerController != null)
        {
            // ������ҳ���ȷ�����䷽��
            float direction = playerController.transform.localScale.x;
            return direction >= 0 ? 1 : -1;
        }

        // Ĭ�����ҷ���
        return 1;
    }

    /// <summary>
    /// ��ȡ��ǰ�ӵ���ɫ
    /// </summary>
    private Color GetCurrentBulletColor()
    {
        return colorBag?.CurrentColorMagazine?.MagazineColor ?? Color.clear;
    }

    #endregion

    #region Auto Fire

    /// <summary>
    /// �����Զ����
    /// </summary>
    private void HandleAutoFire()
    {
        if (!enableAutoFire) return;
        
        // ����Ƿ�Ӧ���Զ����
        if (ShouldAutoFire())
        {
            TryShoot();
        }
    }

    /// <summary>
    /// ����Ƿ�Ӧ���Զ����
    /// </summary>
    private bool ShouldAutoFire()
    {
        // �Զ����������
        // 1. û�п�ʰȡ����
        // 2. ���ӵ������
        // 3. �ﵽ���Ƶ��
        return (colorPicker == null || !colorPicker.HasPickableInRange()) &&
               CanShootInternal() &&
               CheckFireRate();
    }

    #endregion




    #region Debug and Visualization

    /// <summary>
    /// ���Ʒ����Gizmos
    /// </summary>
    private void DrawFirePointGizmos()
    {
        Vector3 firePos = GetFirePosition();
        Vector2 fireDir = GetFireDirection();
        
        // �����
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(firePos, 0.1f);
        
        // ���䷽��
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(firePos, fireDir * 1.5f);
        
        // ���䷽���ͷ
        Vector3 arrowEnd = firePos + (Vector3)(fireDir * 1.5f);
        Vector3 arrowSide1 = arrowEnd + (Vector3)(Quaternion.Euler(0, 0, 135f) * fireDir * 0.3f);
        Vector3 arrowSide2 = arrowEnd + (Vector3)(Quaternion.Euler(0, 0, -135f) * fireDir * 0.3f);
        
        Gizmos.DrawLine(arrowEnd, arrowSide1);
        Gizmos.DrawLine(arrowEnd, arrowSide2);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// ����ColorBag����
    /// </summary>
    public void SetColorBag(ColorBag bag)
    {
        colorBag = bag;
    }

    /// <summary>
    /// ����PlayerController����
    /// </summary>
    public void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
    }

    /// <summary>
    /// ����ColorPicker����
    /// </summary>
    public void SetColorPicker(ColorPicker picker)
    {
        colorPicker = picker;
    }

    /// <summary>
    /// ���÷����
    /// </summary>
    public void SetFirePoint(Transform point)
    {
        firePoint = point;
    }

    /// <summary>
    /// �����ӵ�����
    /// </summary>
    public void SetBulletParameters(float speed, int damage, float lifetime)
    {
        bulletSpeed = speed;
        bulletDamage = damage;
        bulletLifetime = lifetime;
    }

    /// <summary>
    /// ���÷���ƫ��
    /// </summary>
    public void SetFireOffset(Vector2 offset)
    {
        fireOffset = offset;
    }

    /// <summary>
    /// �������Ƶ��
    /// </summary>
    public void SetFireRate(float rate)
    {
        fireRate = Mathf.Max(0f, rate);
    }

    /// <summary>
    /// ����/�����Զ����
    /// </summary>
    public void SetAutoFire(bool enable)
    {
        enableAutoFire = enable;
    }

    /// <summary>
    /// ǿ�����������Ƶ�����ƣ�
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
    /// ��ȡ�����״̬��Ϣ
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