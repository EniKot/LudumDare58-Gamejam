using UnityEngine;

/// <summary>
/// 3D ������ɫʰȡ���ʾ��
/// ʵ�� IColorPickable<GameObject> �ӿ������µ� ColorPicker ϵͳ
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExampleColorPickableObject : MonoBehaviour, IColorPickable<GameObject>
{
    #region Serialized Fields
    [Header("Color Settings")]
    [SerializeField] private Color objectColor = Color.white;
    [SerializeField] private bool useRendererColor = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showPickupEffect = true;
    [SerializeField] private float effectDuration = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);
    
    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    #endregion

    #region Private Fields
    private Renderer objectRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Vector3 originalScale;
    private AudioSource audioSource;
    private Coroutine effectCoroutine;
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        originalScale = transform.localScale;
        audioSource = GetComponent<AudioSource>();
        
        // ȷ������ײ�����ڼ��
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"{gameObject.name} needs a Collider component for ColorPicker detection!");
        }
    }

    private void Start()
    {
        // �������Ϊʹ����Ⱦ����ɫ���Ӳ��ʻ�ȡ��ɫ
        if (useRendererColor && objectRenderer != null && objectRenderer.sharedMaterial != null)
        {
            if (objectRenderer.sharedMaterial.HasProperty("_Color"))
            {
                objectColor = objectRenderer.sharedMaterial.GetColor("_Color");
            }
            else if (objectRenderer.sharedMaterial.HasProperty("_BaseColor"))
            {
                objectColor = objectRenderer.sharedMaterial.GetColor("_BaseColor");
            }
        }
    }

    #endregion

    #region IColorPickable Implementation

    /// <summary>
    /// ��ȡ������ɫ
    /// </summary>
    public Color GetColor()
    {
        if (useRendererColor && objectRenderer != null)
        {
            // ���ȴ� MaterialPropertyBlock ��ȡ
            objectRenderer.GetPropertyBlock(propertyBlock);
            
            if (propertyBlock.HasProperty("_Color"))
            {
                return propertyBlock.GetColor("_Color");
            }
            else if (propertyBlock.HasProperty("_BaseColor"))
            {
                return propertyBlock.GetColor("_BaseColor");
            }
            
            // �Ӳ��ʻ�ȡ
            if (objectRenderer.sharedMaterial != null)
            {
                if (objectRenderer.sharedMaterial.HasProperty("_Color"))
                {
                    return objectRenderer.sharedMaterial.GetColor("_Color");
                }
                else if (objectRenderer.sharedMaterial.HasProperty("_BaseColor"))
                {
                    return objectRenderer.sharedMaterial.GetColor("_BaseColor");
                }
            }
        }
        
        return objectColor;
    }

    /// <summary>
    /// ����ɫ��ʰȡʱ����
    /// </summary>
    public void OnColorPicked(Color color, GameObject target)
    {
        Debug.Log($"{gameObject.name} color was picked: {color}");
        
        // ������Ч
        PlayPickupSound();
        
        // ��ʾʰȡЧ��
        if (showPickupEffect)
        {
            ShowPickupEffect();
        }
        
        // �����������������Ч��������������Ч��UI��ʾ��
    }

    #endregion

    #region Visual Effects

    /// <summary>
    /// ����ʰȡ��Ч
    /// </summary>
    private void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
        }
    }

    /// <summary>
    /// ��ʾʰȡЧ��
    /// </summary>
    private void ShowPickupEffect()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }
        
        effectCoroutine = StartCoroutine(PickupEffectCoroutine());
    }

    /// <summary>
    /// ʰȡЧ��Э��
    /// </summary>
    private System.Collections.IEnumerator PickupEffectCoroutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < effectDuration)
        {
            float progress = elapsedTime / effectDuration;
            float scaleMultiplier = scaleCurve.Evaluate(progress);
            
            transform.localScale = originalScale * scaleMultiplier;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // �ָ�ԭʼ��С
        transform.localScale = originalScale;
        effectCoroutine = null;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// ����������ɫ
    /// </summary>
    public void SetColor(Color color)
    {
        objectColor = color;
        
        if (useRendererColor && objectRenderer != null)
        {
            objectRenderer.GetPropertyBlock(propertyBlock);
            
            // �������ò�ͬ����ɫ����
            if (objectRenderer.sharedMaterial != null)
            {
                if (objectRenderer.sharedMaterial.HasProperty("_Color"))
                {
                    propertyBlock.SetColor("_Color", color);
                }
                else if (objectRenderer.sharedMaterial.HasProperty("_BaseColor"))
                {
                    propertyBlock.SetColor("_BaseColor", color);
                }
            }
            
            objectRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    /// <summary>
    /// ��ȡ������Ϣ
    /// </summary>
    public string GetObjectInfo()
    {
        return $"Object: {gameObject.name}, Color: {GetColor()}, Position: {transform.position}";
    }

    #endregion

    #region Debug

    private void OnDrawGizmos()
    {
        // ����������ɫָʾ
        Gizmos.color = GetColor();
        Gizmos.DrawWireCube(transform.position, transform.localScale * 1.1f);
        
        // ������ɫ��ǩ
        #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"Color: {GetColor()}");
        #endif
    }

    #endregion
}
