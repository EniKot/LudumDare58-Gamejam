using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ��ɫ��Ͽ�����
/// ������ҵĻ�ɫ�������
/// </summary>
public class ColorMixController : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private ColorBag colorBag;
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode mixRedGreenKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode mixRedBlueKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode mixGreenBlueKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode clearMixMagazineKey = KeyCode.Alpha4;
    
    [Header("Debug")]
    [SerializeField] private bool showMixGUI = true;
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // ���� ColorBag ���δ����
        if (colorBag == null)
        {
            colorBag = FindObjectOfType<ColorBag>();
            if (colorBag == null)
            {
                Debug.LogError("ColorBag not found! Please assign it in the inspector.");
            }
        }
    }

    private void Start()
    {
        // ���Ļ�ɫ�¼�
        if (colorBag != null)
        {
            colorBag.OnColorsMixed += HandleColorsMixed;
        }
    }

    private void Update()
    {
        HandleMixInput();
    }

    private void OnGUI()
    {
        if (showMixGUI)
        {
            DrawMixGUI();
        }
    }

    private void OnDestroy()
    {
        // ȡ�������¼�
        if (colorBag != null)
        {
            colorBag.OnColorsMixed -= HandleColorsMixed;
        }
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// �����ɫ����
    /// </summary>
    private void HandleMixInput()
    {
        if (colorBag == null) return;

        // ��Ϻ���
        if (Input.GetKeyDown(mixRedGreenKey))
        {
            TryMixRedGreen();
        }

        // ��Ϻ���
        if (Input.GetKeyDown(mixRedBlueKey))
        {
            TryMixRedBlue();
        }

        // �������
        if (Input.GetKeyDown(mixGreenBlueKey))
        {
            TryMixGreenBlue();
        }

        // ��ջ�ϵ�ϻ
        if (Input.GetKeyDown(clearMixMagazineKey))
        {
            ClearMixMagazine();
        }

        // ֧��������ϵͳ
        HandleNewInputSystem();
    }

    /// <summary>
    /// ����������ϵͳ
    /// </summary>
    private void HandleNewInputSystem()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            TryMixRedGreen();
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            TryMixRedBlue();
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            TryMixGreenBlue();
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            ClearMixMagazine();
        }
    }

    #endregion

    #region Mix Operations

    /// <summary>
    /// ���Ի�Ϻ�ɫ����ɫ
    /// </summary>
    private void TryMixRedGreen()
    {
        bool canMix = colorBag.CanMix(colorBag.RedMagazine, colorBag.GreenMagazine);
        
        if (canMix)
        {
            colorBag.MixRedGreen();
        }
        else
        {
            Debug.LogWarning("Cannot mix Red + Green: Magazines not full or Mix magazine not empty");
            ShowMixFailureMessage("Red + Green", colorBag.RedMagazine, colorBag.GreenMagazine);
        }
    }

    /// <summary>
    /// ���Ի�Ϻ�ɫ����ɫ
    /// </summary>
    private void TryMixRedBlue()
    {
        bool canMix = colorBag.CanMix(colorBag.RedMagazine, colorBag.BlueMagazine);
        
        if (canMix)
        {
            colorBag.MixRedBlue();
        }
        else
        {
            Debug.LogWarning("Cannot mix Red + Blue: Magazines not full or Mix magazine not empty");
            ShowMixFailureMessage("Red + Blue", colorBag.RedMagazine, colorBag.BlueMagazine);
        }
    }

    /// <summary>
    /// ���Ի����ɫ����ɫ
    /// </summary>
    private void TryMixGreenBlue()
    {
        bool canMix = colorBag.CanMix(colorBag.GreenMagazine, colorBag.BlueMagazine);
        
        if (canMix)
        {
            colorBag.MixGreenBlue();
        }
        else
        {
            Debug.LogWarning("Cannot mix Green + Blue: Magazines not full or Mix magazine not empty");
            ShowMixFailureMessage("Green + Blue", colorBag.GreenMagazine, colorBag.BlueMagazine);
        }
    }

    /// <summary>
    /// ��ջ�ϵ�ϻ
    /// </summary>
    private void ClearMixMagazine()
    {
        if (colorBag.MixMagazine != null && !colorBag.MixMagazine.IsEmpty)
        {
            colorBag.MixMagazine.ClearMagazine();
            Debug.Log("Mix magazine cleared");
        }
        else
        {
            Debug.Log("Mix magazine is already empty");
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// �����ɫ����¼�
    /// </summary>
    private void HandleColorsMixed(ColorMagazine mag1, ColorMagazine mag2, Color mixedColor)
    {
        Debug.Log($"Colors successfully mixed! {mag1.gameObject.name} + {mag2.gameObject.name} = {mixedColor}");
        
        // ���������Ӹ���Ч�������磺
        // - ���Ż�ɫ��Ч
        // - ��ʾ��ɫ��Ч
        // - ����UI��ʾ
        // - �����ɾ�ϵͳ��
        
        PlayMixEffect(mixedColor);
    }

    /// <summary>
    /// ���Ż�ɫЧ��
    /// </summary>
    private void PlayMixEffect(Color mixedColor)
    {
        // �����������������Ч������Ч��
        Debug.Log($"Playing mix effect for color: {mixedColor}");
    }

    /// <summary>
    /// ��ʾ��ɫʧ����Ϣ
    /// </summary>
    private void ShowMixFailureMessage(string mixType, ColorMagazine mag1, ColorMagazine mag2)
    {
        string message = $"Cannot mix {mixType}:\n";
        message += $"- {mag1.gameObject.name}: {mag1.CurrentBullets}/{mag1.BulletCapacity} bullets\n";
        message += $"- {mag2.gameObject.name}: {mag2.CurrentBullets}/{mag2.BulletCapacity} bullets\n";
        message += $"- Mix Magazine: {(colorBag.MixMagazine.IsEmpty ? "Empty" : "Not Empty")}";
        
        Debug.Log(message);
    }

    #endregion

    #region GUI

    /// <summary>
    /// ���ƻ�ɫGUI
    /// </summary>
    private void DrawMixGUI()
    {
        float startX = Screen.width - 250f;
        float startY = 10f;
        float lineHeight = 25f;
        float yOffset = startY;

        GUI.color = Color.white;
        GUI.Label(new Rect(startX, yOffset, 240f, lineHeight), "=== Color Mixing ===");
        yOffset += lineHeight + 5f;

        // ��ʾ��ɫѡ��
        DrawMixOption(startX, ref yOffset, lineHeight, "Red + Green", mixRedGreenKey, 
            colorBag.CanMix(colorBag.RedMagazine, colorBag.GreenMagazine));

        DrawMixOption(startX, ref yOffset, lineHeight, "Red + Blue", mixRedBlueKey, 
            colorBag.CanMix(colorBag.RedMagazine, colorBag.BlueMagazine));

        DrawMixOption(startX, ref yOffset, lineHeight, "Green + Blue", mixGreenBlueKey, 
            colorBag.CanMix(colorBag.GreenMagazine, colorBag.BlueMagazine));

        yOffset += 5f;

        // ��ջ�ϵ�ϻѡ��
        GUI.color = colorBag.MixMagazine != null && !colorBag.MixMagazine.IsEmpty ? Color.yellow : Color.gray;
        GUI.Label(new Rect(startX, yOffset, 240f, lineHeight), 
            $"[{clearMixMagazineKey}] Clear Mix Magazine");

        GUI.color = Color.white;
    }

    /// <summary>
    /// ���Ƶ�����ɫѡ��
    /// </summary>
    private void DrawMixOption(float x, ref float y, float lineHeight, string mixName, KeyCode key, bool canMix)
    {
        GUI.color = canMix ? Color.green : Color.red;
        GUI.Label(new Rect(x, y, 240f, lineHeight), $"[{key}] {mixName}");
        y += lineHeight;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// ���� ColorBag ����
    /// </summary>
    public void SetColorBag(ColorBag bag)
    {
        // ȡ���ɵ��¼�����
        if (colorBag != null)
        {
            colorBag.OnColorsMixed -= HandleColorsMixed;
        }

        colorBag = bag;

        // �����µ��¼�
        if (colorBag != null)
        {
            colorBag.OnColorsMixed += HandleColorsMixed;
        }
    }

    /// <summary>
    /// ���ָ����ɫ�Ƿ����
    /// </summary>
    public bool CanMix(ColorBag.MagazineType type1, ColorBag.MagazineType type2)
    {
        if (colorBag == null) return false;

        var mag1 = colorBag.GetMagazine(type1);
        var mag2 = colorBag.GetMagazine(type2);

        return colorBag.CanMix(mag1, mag2);
    }

    #endregion
}