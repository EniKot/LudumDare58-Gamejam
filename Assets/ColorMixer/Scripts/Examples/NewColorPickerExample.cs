using UnityEngine;

/// <summary>
/// �� ColorPicker ϵͳ��ʹ��ʾ��
/// ��ʾ������ú�ʹ�û�����ײ������ɫʰȡϵͳ
/// </summary>
public class NewColorPickerExample : MonoBehaviour
{
    #region Serialized Fields
    [Header("System References")]
    [SerializeField] private ColorPicker colorPicker;
    [SerializeField] private ColorBag colorBag;
    [SerializeField] private ColorMixController mixController;
    
    [Header("Example Objects")]
    [SerializeField] private ExampleColorPickableObject[] colorObjects;
    
    [Header("Demo Settings")]
    [SerializeField] private bool autoCreateTestObjects = true;
    [SerializeField] private int testObjectCount = 5;
    [SerializeField] private float spawnRadius = 5f;
    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        SetupSystem();
        
        if (autoCreateTestObjects)
        {
            CreateTestObjects();
        }
        
        SubscribeToEvents();
        
        Debug.Log("=== New ColorPicker System Example Started ===");
        Debug.Log("Controls:");
        Debug.Log("- E: Pick color from nearby objects");
        Debug.Log("- 1: Mix Red + Green");
        Debug.Log("- 2: Mix Red + Blue");
        Debug.Log("- 3: Mix Green + Blue");
        Debug.Log("- 4: Clear Mix Magazine");
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region System Setup

    /// <summary>
    /// ����ϵͳ���
    /// </summary>
    private void SetupSystem()
    {
        // ���û���ҵ�����������Զ�����
        if (colorPicker == null)
            colorPicker = FindObjectOfType<ColorPicker>();
        
        if (colorBag == null)
            colorBag = FindObjectOfType<ColorBag>();
        
        if (mixController == null)
            mixController = FindObjectOfType<ColorMixController>();

        // ȷ��ϵͳ�������
        if (colorPicker == null)
        {
            Debug.LogError("ColorPicker not found! Please add ColorPicker component to the scene.");
        }

        if (colorBag == null)
        {
            Debug.LogError("ColorBag not found! Please add ColorBag component to the scene.");
        }

        if (mixController == null)
        {
            Debug.LogError("ColorMixController not found! Please add ColorMixController component to the scene.");
        }

        // ��������������
        if (colorPicker != null && colorBag != null)
        {
            colorPicker.SetColorBag(colorBag);
        }

        if (mixController != null && colorBag != null)
        {
            mixController.SetColorBag(colorBag);
        }
    }

    /// <summary>
    /// �������Զ���
    /// </summary>
    private void CreateTestObjects()
    {
        Color[] testColors = {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            new Color(1f, 0.5f, 0f), // Orange
            new Color(0.5f, 0f, 0.5f), // Purple
        };

        for (int i = 0; i < testObjectCount; i++)
        {
            // �������Զ���
            GameObject testObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObj.name = $"TestColorObject_{i}";
            
            // ���λ��
            Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
            randomPos.y = Mathf.Abs(randomPos.y) + 1f; // ȷ���ڵ����Ϸ�
            testObj.transform.position = transform.position + randomPos;
            
            // �����ת
            testObj.transform.rotation = Random.rotation;
            
            // �����ɫʰȡ���
            var pickable = testObj.AddComponent<ExampleColorPickableObject>();
            
            // ���������ɫ
            Color randomColor = testColors[Random.Range(0, testColors.Length)];
            pickable.SetColor(randomColor);
            
            // ���ò㼶����ѡ��
            testObj.layer = LayerMask.NameToLayer("Default");
        }
        
        Debug.Log($"Created {testObjectCount} test objects for color picking demonstration.");
    }

    #endregion

    #region Event Handling

    /// <summary>
    /// ����ϵͳ�¼�
    /// </summary>
    private void SubscribeToEvents()
    {
        if (colorPicker != null)
        {
            colorPicker.OnColorPicked += HandleColorPicked;
            colorPicker.OnPickableObjectFound += HandlePickableObjectFound;
            colorPicker.OnNoPickableObjectFound += HandleNoPickableObjectFound;
        }

        if (colorBag != null)
        {
            colorBag.OnColorsMixed += HandleColorsMixed;
            colorBag.OnMagazineUpdated += HandleMagazineUpdated;
        }
    }

    /// <summary>
    /// ȡ������ϵͳ�¼�
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (colorPicker != null)
        {
            colorPicker.OnColorPicked -= HandleColorPicked;
            colorPicker.OnPickableObjectFound -= HandlePickableObjectFound;
            colorPicker.OnNoPickableObjectFound -= HandleNoPickableObjectFound;
        }

        if (colorBag != null)
        {
            colorBag.OnColorsMixed -= HandleColorsMixed;
            colorBag.OnMagazineUpdated -= HandleMagazineUpdated;
        }
    }

    /// <summary>
    /// ������ɫʰȡ�¼�
    /// </summary>
    private void HandleColorPicked(Color color, GameObject target)
    {
        Debug.Log($"[Example] Color picked: {color} from {target.name}");
        
        // �������������ʰȡЧ�������磺
        // - ������Ч
        // - ��ʾUI��ʾ
        // - ���ӷ�����
    }

    /// <summary>
    /// �����ҵ���ʰȡ�����¼�
    /// </summary>
    private void HandlePickableObjectFound(IColorPickable<GameObject> pickable)
    {
        // �������������UI��ʾ����ʾ����ʰȡ�Ķ���
        if (pickable is MonoBehaviour component)
        {
            // Debug.Log($"[Example] Pickable object in range: {component.gameObject.name}");
        }
    }

    /// <summary>
    /// ����û�п�ʰȡ�����¼�
    /// </summary>
    private void HandleNoPickableObjectFound()
    {
        Debug.Log("[Example] No pickable objects in range");
    }

    /// <summary>
    /// ������ɫ����¼�
    /// </summary>
    private void HandleColorsMixed(ColorMagazine mag1, ColorMagazine mag2, Color mixedColor)
    {
        Debug.Log($"[Example] Colors mixed: {mag1.gameObject.name} + {mag2.gameObject.name} = {mixedColor}");
        
        // ������������ӻ��Ч�������磺
        // - ���Ż����Ч
        // - ��ʾ��Ч
        // - ����UI��
    }

    /// <summary>
    /// ����ϻ�����¼�
    /// </summary>
    private void HandleMagazineUpdated(ColorMagazine magazine)
    {
        Debug.Log($"[Example] Magazine updated: {magazine.gameObject.name} - {magazine.CurrentBullets}/{magazine.BulletCapacity}");
    }

    #endregion

    #region Public Methods (������UI��ť��)

    /// <summary>
    /// �ֶ�ʰȡ��ɫ���ɰ󶨵�UI��ť��
    /// </summary>
    public void ManualPickColor()
    {
        if (colorPicker != null)
        {
            colorPicker.ManualPickColor();
        }
    }

    /// <summary>
    /// ������е�ϻ�������ã�
    /// </summary>
    public void ClearAllMagazines()
    {
        if (colorBag != null)
        {
            colorBag.ClearAllMagazines();
            Debug.Log("[Example] All magazines cleared");
        }
    }

    /// <summary>
    /// �������л�����ϻ�������ã�
    /// </summary>
    public void FillAllBaseMagazines()
    {
        if (colorBag != null)
        {
            colorBag.RedMagazine?.FillMagazine();
            colorBag.GreenMagazine?.FillMagazine();
            colorBag.BlueMagazine?.FillMagazine();
            Debug.Log("[Example] All base magazines filled");
        }
    }

    /// <summary>
    /// ��ʾϵͳ״̬
    /// </summary>
    public void ShowSystemStatus()
    {
        if (colorBag != null)
        {
            Debug.Log("[Example] System Status:");
            Debug.Log(colorBag.GetAllMagazineInfo());
        }
        
        if (colorPicker != null)
        {
            Debug.Log($"Pickable objects in range: {colorPicker.GetPickableObjectCount()}");
        }
    }

    #endregion

    #region Debug

    private void OnGUI()
    {
        // ��ʾʾ���������
        float panelX = 10f;
        float panelY = Screen.height - 200f;
        float panelWidth = 200f;
        float panelHeight = 180f;

        GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), "Example Controls");

        float buttonY = panelY + 25f;
        float buttonHeight = 25f;
        float buttonSpacing = 30f;

        if (GUI.Button(new Rect(panelX + 10f, buttonY, panelWidth - 20f, buttonHeight), "Manual Pick Color"))
        {
            ManualPickColor();
        }
        buttonY += buttonSpacing;

        if (GUI.Button(new Rect(panelX + 10f, buttonY, panelWidth - 20f, buttonHeight), "Clear All Magazines"))
        {
            ClearAllMagazines();
        }
        buttonY += buttonSpacing;

        if (GUI.Button(new Rect(panelX + 10f, buttonY, panelWidth - 20f, buttonHeight), "Fill Base Magazines"))
        {
            FillAllBaseMagazines();
        }
        buttonY += buttonSpacing;

        if (GUI.Button(new Rect(panelX + 10f, buttonY, panelWidth - 20f, buttonHeight), "Show Status"))
        {
            ShowSystemStatus();
        }
        buttonY += buttonSpacing;

        if (GUI.Button(new Rect(panelX + 10f, buttonY, panelWidth - 20f, buttonHeight), "Create Test Objects"))
        {
            CreateTestObjects();
        }
    }

    #endregion
}