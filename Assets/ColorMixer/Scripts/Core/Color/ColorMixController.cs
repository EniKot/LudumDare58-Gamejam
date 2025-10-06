using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ��ɫ��Ͽ�����
/// ����ϻѡ��ͻ�ɫ����
/// ����1234���Ƶ�ϻѡ��Tab�����ɫģʽ
/// </summary>
public class ColorMixController : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private ColorBag colorBag;
    //[SerializeField] private PlayerController playerController;
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode selectMagazine1Key = KeyCode.Alpha1; // ��ɫ
    [SerializeField] private KeyCode selectMagazine2Key = KeyCode.Alpha2; // ��ɫ
    [SerializeField] private KeyCode selectMagazine3Key = KeyCode.Alpha3; // ��ɫ
    [SerializeField] private KeyCode selectMagazine4Key = KeyCode.Alpha4; // ���
    [SerializeField] private KeyCode mixModeKey = KeyCode.Tab;
    
    [Header("Debug")]
    [SerializeField] private bool showMixGUI = true;
    [SerializeField] private bool showDebug = true;
    #endregion

    #region Private Fields
    private InputService inputService;
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // ���� ColorBag ���δ����
        if (colorBag == null)
            colorBag = FindObjectOfType<ColorBag>();
        
        //if (playerController == null)
        //    playerController = FindObjectOfType<PlayerController>();
            
        inputService = InputService.Instance;
    }

    private void Start()
    {
        // ���� ColorBag �¼�
        if (colorBag != null)
        {
            colorBag.OnColorsMixed += HandleColorsMixed;
            colorBag.OnCurrentMagazineChanged += HandleCurrentMagazineChanged;
            colorBag.OnMixModeChanged += HandleMixModeChanged;
            colorBag.OnFirstMixMagazineSelected += HandleFirstMixMagazineSelected;
        }
    }

    private void Update()
    {
        HandleMixInput();
    }

    private void OnGUI()
    {
        if (showMixGUI)
            DrawMixGUI();
    }

    private void OnDestroy()
    {
        // ȡ�������¼�
        if (colorBag != null)
        {
            colorBag.OnColorsMixed -= HandleColorsMixed;
            colorBag.OnCurrentMagazineChanged -= HandleCurrentMagazineChanged;
            colorBag.OnMixModeChanged -= HandleMixModeChanged;
            colorBag.OnFirstMixMagazineSelected -= HandleFirstMixMagazineSelected;
        }
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// �����ɫ�͵�ϻѡ������ �ɰ�����ϵͳ��������ϵͳ�������޸�
    /// </summary>
    private void HandleMixInput()
    {
        if (colorBag == null) return;

        // ��ϻѡ��
        if (Input.GetKeyDown(selectMagazine1Key))
        {
            HandleMagazineSelection(0); // ��ɫ
        }
        else if (Input.GetKeyDown(selectMagazine2Key))
        {
            HandleMagazineSelection(1); // ��ɫ
        }
        else if (Input.GetKeyDown(selectMagazine3Key))
        {
            HandleMagazineSelection(2); // ��ɫ
        }
        else if (Input.GetKeyDown(selectMagazine4Key))
        {
            HandleMagazineSelection(3); // ���
        }

        // ��ɫģʽ�л�
        if (Input.GetKeyDown(mixModeKey))
        {
            HandleMixModeToggle();
        }

        
    }


    #endregion


    #region Magazine Selection

    /// <summary>
    /// ����ϻѡ��
    /// </summary>
    /// <param name="magazineIndex">��ϻ����</param>
    public void HandleMagazineSelection(int magazineIndex)
    {
        if (colorBag.IsMixMode)
        {
            // �ڻ�ɫģʽ�£�ѡ��ڶ�����ϻ�����Ի�ɫ
            if (colorBag.SetCurrentMagazine(magazineIndex))
            {
                bool mixSuccess = colorBag.TryMixWithCurrentMagazine();
                if (!mixSuccess && showDebug)
                {
                    Debug.LogWarning("Failed to mix colors - check magazine requirements");
                }
            }
        }
        else
        {
            // ����ģʽ�£��л���ǰ��ϻ
            colorBag.SetCurrentMagazine(magazineIndex);
        }
    }

    /// <summary>
    /// �����ɫģʽ�л�
    /// </summary>
    public void HandleMixModeToggle()
    {
        if (colorBag.IsMixMode)
        {
            // �˳���ɫģʽ
            colorBag.ExitMixMode();

        }
        else
        {
            // �����ɫģʽ
            colorBag.EnterMixMode();
        }
    }

    #endregion




    #region Event Handlers

    /// <summary>
    /// �����ɫ����¼�
    /// </summary>
    private void HandleColorsMixed(ColorMagazine mag1, ColorMagazine mag2, Color mixedColor)
    {
        if (showDebug)
            Debug.Log($"[ColorMixController] Colors successfully mixed! {mag1.MagazineName} + {mag2.MagazineName} = {mixedColor}");
        
        PlayMixEffect(mixedColor);
    }

    /// <summary>
    /// ����ǰ��ϻ�ı��¼�
    /// </summary>
    private void HandleCurrentMagazineChanged(ColorMagazine newMagazine)
    {
        if (showDebug)
            Debug.Log($"[ColorMixController] Current magazine changed to: {newMagazine?.MagazineName}");
    }

    /// <summary>
    /// �����ɫģʽ�ı��¼�
    /// </summary>
    private void HandleMixModeChanged(bool inMixMode)
    {
        if (showDebug)
            Debug.Log($"[ColorMixController] Mix mode: {(inMixMode ? "ENTERED" : "EXITED")}");
    }

    /// <summary>
    /// �����һ����ɫ��ϻѡ���¼�
    /// </summary>
    private void HandleFirstMixMagazineSelected(ColorMagazine firstMagazine)
    {
        if (showDebug)
            Debug.Log($"[ColorMixController] First mix magazine selected: {firstMagazine.MagazineName}");
    }

    /// <summary>
    /// ���Ż�ɫЧ��
    /// </summary>
    private void PlayMixEffect(Color mixedColor)
    {
        // �����������������Ч������Ч��
        if (showDebug)
            Debug.Log($"Playing mix effect for color: {mixedColor}");
    }

    #endregion


















    #region GUI

    /// <summary>
    /// ���ƻ�ɫGUI Generate By Claude4
    /// </summary>
    private void DrawMixGUI()
    {
        if (colorBag == null) return;

        float startX = Screen.width - 300f;
        float startY = 10f;
        float lineHeight = 25f;
        float yOffset = startY;

        GUI.color = Color.white;
        GUI.Label(new Rect(startX, yOffset, 290f, lineHeight), "=== Color Mix Controller ===");
        yOffset += lineHeight + 5f;

        // ��ʾ��ǰѡ�еĵ�ϻ
        var currentMag = colorBag.CurrentColorMagazine;
        if (currentMag != null)
        {
            GUI.color = currentMag.MagazineColor;
            GUI.Box(new Rect(startX, yOffset, 20, 20), "");
            GUI.color = Color.white;
            GUI.Label(new Rect(startX + 25, yOffset, 260f, lineHeight), 
                $"Current: {currentMag.MagazineName} ({currentMag.CurrentBullets}/{currentMag.BulletCapacity})");
            yOffset += lineHeight;
        }

        // ��ʾ��ɫģʽ״̬
        if (colorBag.IsMixMode)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(startX, yOffset, 290f, lineHeight), "*** MIX MODE ACTIVE ***");
            yOffset += lineHeight;
            
            if (colorBag.FirstMixMagazine != null)
            {
                GUI.color = colorBag.FirstMixMagazine.MagazineColor;
                GUI.Box(new Rect(startX, yOffset, 20, 20), "");
                GUI.color = Color.white;
                GUI.Label(new Rect(startX + 25, yOffset, 260f, lineHeight), 
                    $"First: {colorBag.FirstMixMagazine.MagazineName}");
                yOffset += lineHeight;
                
                GUI.color = Color.cyan;
                GUI.Label(new Rect(startX, yOffset, 290f, lineHeight), "Select second magazine to mix!");
                yOffset += lineHeight;
            }
        }

        yOffset += 5f;

        // ��ʾ����˵��
        GUI.color = Color.white;
        GUI.Label(new Rect(startX, yOffset, 290f, lineHeight), "=== Controls ===");
        yOffset += lineHeight;

        DrawControlHint(startX, ref yOffset, lineHeight, $"[{selectMagazine1Key}]", "Red Magazine", 
            colorBag.CurrentMagazineIndex == 0);
        DrawControlHint(startX, ref yOffset, lineHeight, $"[{selectMagazine2Key}]", "Green Magazine", 
            colorBag.CurrentMagazineIndex == 1);
        DrawControlHint(startX, ref yOffset, lineHeight, $"[{selectMagazine3Key}]", "Blue Magazine", 
            colorBag.CurrentMagazineIndex == 2);
        DrawControlHint(startX, ref yOffset, lineHeight, $"[{selectMagazine4Key}]", "Mix Magazine", 
            colorBag.CurrentMagazineIndex == 3);

        yOffset += 5f;
        GUI.color = colorBag.IsMixMode ? Color.yellow : Color.gray;
        GUI.Label(new Rect(startX, yOffset, 290f, lineHeight), $"[{mixModeKey}] Toggle Mix Mode");

        GUI.color = Color.white;
    }

    /// <summary>
    /// ���Ƶ���������ʾ
    /// </summary>
    private void DrawControlHint(float x, ref float y, float lineHeight, string key, string description, bool isSelected)
    {
        GUI.color = isSelected ? Color.green : Color.white;
        GUI.Label(new Rect(x, y, 290f, lineHeight), $"{key} {description}");
        y += lineHeight;
    }

    #endregion

    //#region Public Methods
    /// <summary>
    /// ���� ColorBag ����
    /// </summary>
    //public void SetColorBag(ColorBag bag)
    //{
    //    // ȡ���ɵ��¼�����
    //    if (colorBag != null)
    //    {
    //        colorBag.OnColorsMixed -= HandleColorsMixed;
    //        colorBag.OnCurrentMagazineChanged -= HandleCurrentMagazineChanged;
    //        colorBag.OnMixModeChanged -= HandleMixModeChanged;
    //        colorBag.OnFirstMixMagazineSelected -= HandleFirstMixMagazineSelected;
    //    }

    //    colorBag = bag;

    //    // �����µ��¼�
    //    if (colorBag != null)
    //    {
    //        colorBag.OnColorsMixed += HandleColorsMixed;
    //        colorBag.OnCurrentMagazineChanged += HandleCurrentMagazineChanged;
    //        colorBag.OnMixModeChanged += HandleMixModeChanged;
    //        colorBag.OnFirstMixMagazineSelected += HandleFirstMixMagazineSelected;
    //    }
    //}
    //#endregion
}