using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ��ɫ��ϻUI������
/// ������ʾ��ϻ���ӵ���������ɫ������
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
    /// ��ʼ����ϻ������
    /// </summary>
    /// <param name="magazine">Ҫ���Ƶĵ�ϻ����</param>
    public void Initialize(ColorMagazine magazine)
    {
        colorMagazine = magazine;
        
        if (colorMagazine != null)
        {
            // ���ĵ�ϻ�¼�
            colorMagazine.OnBulletAdded += HandleMagazineUpdated;
            colorMagazine.OnBulletRemoved += HandleMagazineUpdated;
            colorMagazine.OnMagazineCleared += HandleMagazineUpdated;
            colorMagazine.OnMagazineColorChanged += HandleMagazineUpdated;
            colorMagazine.OnMagazineFull += HandleMagazineFull;
            colorMagazine.OnMagazineEmpty += HandleMagazineEmpty;
            
            // ��ʼ��UI
            UpdateUI();
        }
    }

    /// <summary>
    /// ����ѡ��״̬
    /// </summary>
    /// <param name="selected">�Ƿ�ѡ��</param>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateBackgroundColor();
    }

    /// <summary>
    /// ��������UI���޶�����
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
    /// ����ϻ�����¼�
    /// </summary>
    private void HandleMagazineUpdated(ColorMagazine magazine)
    {
        UpdateUI();
    }

    /// <summary>
    /// ����ϻ���¼�
    /// </summary>
    private void HandleMagazineFull(ColorMagazine magazine)
    {
        // �����������ϻ��Ч
        Debug.Log($"Magazine {magazine.MagazineName} is full!");
    }

    /// <summary>
    /// ����ϻ���¼�
    /// </summary>
    private void HandleMagazineEmpty(ColorMagazine magazine)
    {
        // ������ӿյ�ϻ��Ч
        Debug.Log($"Magazine {magazine.MagazineName} is empty!");
    }

    /// <summary>
    /// ����UI��ʾ
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
    /// ���¶���
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
    /// �����ı���ʾ
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
    /// ���±�����ɫ
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