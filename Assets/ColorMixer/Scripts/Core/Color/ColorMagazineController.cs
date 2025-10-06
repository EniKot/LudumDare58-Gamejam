using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 颜色弹匣UI控制器
/// 负责显示弹匣的子弹数量和颜色进度条
/// </summary>
public class ColorMagazineController : MonoBehaviour
{
    #region Serialized Fields
    [Header("UI References")]
    //[SerializeField] private Slider progressBar;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI bulletCountText;
    [SerializeField] private TextMeshProUGUI magazineNameText;
    [SerializeField] private Image backgroundImage;
    
    [Header("Visual Settings")]
    [SerializeField] private Color emptyColor = Color.gray;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private float selectedAlpha = 0.3f;
    [SerializeField] private bool showMagazineName = true;
    
    [Header("Animation")]
    [SerializeField] private bool enableAnimation = true;
    [SerializeField] private float animationSpeed = 5f;
    #endregion

    #region Private Fields
    private ColorMagazine colorMagazine;
    private bool isSelected = false;
    private float targetFillAmount = 0f;
    private Color targetColor = Color.white;
    #endregion

    #region Public Properties
    public ColorMagazine ColorMagazine => colorMagazine;
    public bool IsSelected => isSelected;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (fillImage != null)
            fillImage = GetComponentInChildren<Image>();
        if (bulletCountText != null)
            bulletCountText.text = "0/0";
        if (magazineNameText != null)
            magazineNameText.gameObject.SetActive(showMagazineName);
        UpdateBackgroundColor();
    }

    private void Update()
    {
        if (enableAnimation)
        {
            UpdateAnimations();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 初始化弹匣控制器
    /// </summary>
    /// <param name="magazine">要控制的弹匣数据</param>
    public void Initialize(ColorMagazine magazine)
    {
        colorMagazine = magazine;
        
        if (colorMagazine != null)
        {
            // 订阅弹匣事件
            colorMagazine.OnBulletAdded += HandleMagazineUpdated;
            colorMagazine.OnBulletRemoved += HandleMagazineUpdated;
            colorMagazine.OnMagazineCleared += HandleMagazineUpdated;
            colorMagazine.OnMagazineColorChanged += HandleMagazineUpdated;
            colorMagazine.OnMagazineFull += HandleMagazineFull;
            colorMagazine.OnMagazineEmpty += HandleMagazineEmpty;
            
            // 初始化UI
            UpdateUI();
        }
    }

    /// <summary>
    /// 设置选中状态
    /// </summary>
    /// <param name="selected">是否选中</param>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateBackgroundColor();
    }

    /// <summary>
    /// 立即更新UI（无动画）
    /// </summary>
    public void UpdateUIImmediate()
    {
        if (colorMagazine == null) return;

        targetFillAmount = colorMagazine.FillPercentage;
        targetColor = colorMagazine.IsEmpty ? emptyColor : colorMagazine.MagazineColor;

        //if (progressBar != null)
        //    progressBar.value = targetFillAmount;

        if (fillImage != null)
            fillImage.color = targetColor;

        UpdateTextDisplays();
        UpdateBackgroundColor();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 处理弹匣更新事件
    /// </summary>
    private void HandleMagazineUpdated(ColorMagazine magazine)
    {
        UpdateUI();
    }

    /// <summary>
    /// 处理弹匣满事件
    /// </summary>
    private void HandleMagazineFull(ColorMagazine magazine)
    {
        // 可以添加满弹匣特效
        Debug.Log($"Magazine {magazine.MagazineName} is full!");
    }

    /// <summary>
    /// 处理弹匣空事件
    /// </summary>
    private void HandleMagazineEmpty(ColorMagazine magazine)
    {
        // 可以添加空弹匣特效
        Debug.Log($"Magazine {magazine.MagazineName} is empty!");
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    private void UpdateUI()
    {
        if (colorMagazine == null) return;

        targetFillAmount = colorMagazine.FillPercentage;
        targetColor = colorMagazine.IsEmpty ? emptyColor : colorMagazine.MagazineColor;

        if (!enableAnimation)
        {
            UpdateUIImmediate();
        }

        UpdateTextDisplays();
        UpdateBackgroundColor();
    }

    /// <summary>
    /// 更新动画
    /// </summary>
    private void UpdateAnimations()
    {
        //if (progressBar != null)
        //{
        //    progressBar.value = Mathf.Lerp(progressBar.value, targetFillAmount, Time.deltaTime * animationSpeed);
        //}

        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(fillImage.color, targetColor, Time.deltaTime * animationSpeed);
        }
    }

    /// <summary>
    /// 更新文本显示
    /// </summary>
    private void UpdateTextDisplays()
    {
        if (colorMagazine == null) return;

        if (bulletCountText != null)
        {
            bulletCountText.text = $"{colorMagazine.CurrentBullets}/{colorMagazine.BulletCapacity}";
        }

        if (magazineNameText != null && showMagazineName)
        {
            magazineNameText.text = colorMagazine.MagazineName;
        }
    }

    /// <summary>
    /// 更新背景颜色
    /// </summary>
    private void UpdateBackgroundColor()
    {
        if (backgroundImage == null) return;

        if (isSelected)
        {
            Color bgColor = selectedColor;
            bgColor.a = selectedAlpha;
            backgroundImage.color = bgColor;
        }
        else
        {
            Color bgColor = Color.white;
            bgColor.a = 0f;
            backgroundImage.color = bgColor;
        }
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        if (colorMagazine != null)
        {
            colorMagazine.OnBulletAdded -= HandleMagazineUpdated;
            colorMagazine.OnBulletRemoved -= HandleMagazineUpdated;
            colorMagazine.OnMagazineCleared -= HandleMagazineUpdated;
            colorMagazine.OnMagazineColorChanged -= HandleMagazineUpdated;
            colorMagazine.OnMagazineFull -= HandleMagazineFull;
            colorMagazine.OnMagazineEmpty -= HandleMagazineEmpty;
        }
    }

    #endregion

    #region Debug

    [ContextMenu("Test Fill Magazine")]
    private void TestFillMagazine()
    {
        if (colorMagazine != null)
        {
            colorMagazine.FillMagazine();
        }
    }

    [ContextMenu("Test Clear Magazine")]
    private void TestClearMagazine()
    {
        if (colorMagazine != null)
        {
            colorMagazine.ClearMagazine();
        }
    }

    #endregion
}