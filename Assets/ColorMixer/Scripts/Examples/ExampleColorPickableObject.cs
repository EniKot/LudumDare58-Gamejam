using UnityEngine;

/// <summary>
/// 3D 物体颜色拾取组件示例
/// 实现 IColorPickable<GameObject> 接口用于新的 ColorPicker 系统
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
        
        // 确保有碰撞体用于检测
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"{gameObject.name} needs a Collider component for ColorPicker detection!");
        }
    }

    private void Start()
    {
        // 如果设置为使用渲染器颜色，从材质获取颜色
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
    /// 获取物体颜色
    /// </summary>
    public Color GetColor()
    {
        if (useRendererColor && objectRenderer != null)
        {
            // 优先从 MaterialPropertyBlock 获取
            objectRenderer.GetPropertyBlock(propertyBlock);
            
            if (propertyBlock.HasProperty("_Color"))
            {
                return propertyBlock.GetColor("_Color");
            }
            else if (propertyBlock.HasProperty("_BaseColor"))
            {
                return propertyBlock.GetColor("_BaseColor");
            }
            
            // 从材质获取
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
    /// 当颜色被拾取时调用
    /// </summary>
    public void OnColorPicked(Color color, GameObject target)
    {
        Debug.Log($"{gameObject.name} color was picked: {color}");
        
        // 播放音效
        PlayPickupSound();
        
        // 显示拾取效果
        if (showPickupEffect)
        {
            ShowPickupEffect();
        }
        
        // 可以在这里添加其他效果，比如粒子特效、UI提示等
    }

    #endregion

    #region Visual Effects

    /// <summary>
    /// 播放拾取音效
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
    /// 显示拾取效果
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
    /// 拾取效果协程
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
        
        // 恢复原始大小
        transform.localScale = originalScale;
        effectCoroutine = null;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 设置物体颜色
    /// </summary>
    public void SetColor(Color color)
    {
        objectColor = color;
        
        if (useRendererColor && objectRenderer != null)
        {
            objectRenderer.GetPropertyBlock(propertyBlock);
            
            // 尝试设置不同的颜色属性
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
    /// 获取物体信息
    /// </summary>
    public string GetObjectInfo()
    {
        return $"Object: {gameObject.name}, Color: {GetColor()}, Position: {transform.position}";
    }

    #endregion

    #region Debug

    private void OnDrawGizmos()
    {
        // 绘制物体颜色指示
        Gizmos.color = GetColor();
        Gizmos.DrawWireCube(transform.position, transform.localScale * 1.1f);
        
        // 绘制颜色标签
        #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"Color: {GetColor()}");
        #endif
    }

    #endregion
}
