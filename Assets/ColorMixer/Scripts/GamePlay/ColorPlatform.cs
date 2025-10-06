using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class ColorPlatform : MonoBehaviour,IColorDyeable
{
    

    [Header("目标颜色 (平台需要被激活的最终颜色)")]
    public Color targetColor;

    private SpriteRenderer sr;
    private BoxCollider2D col;
    public bool fillComplete = false;
    public float initialAlpha = 0.15f;
    private float alpha;
    public int hitCount = 3;

    public void OnColorDye(Color receivedColor)
    {
        if (fillComplete) return;
       

        // 判断是否达成目标混色
        if (ColorMixingStrategies.IsColorSimilar(targetColor,receivedColor))
        {


            alpha += (1.0f - initialAlpha) / hitCount;
            alpha = Mathf.Clamp(alpha, 0.15f, 1.0f);
            UpdateTransparency();

            if (alpha >= 1.0f)
            {
                fillComplete = true;
                OnFillComplete();
            }
           
        }

    }
    private void OnFillComplete()
    {
        var move = GetComponent<MovingPlatform>();
        if (move) move.active = true;
        col.isTrigger = false;
        AudioManager.Instance.PlaySFX("PlatformActive");
    }
    void Awake()
    {
        alpha = initialAlpha;
    }
    void Start()
    { 
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        UpdateTransparency();
    }
    
    // 模拟被子弹击中颜色
   

    private void UpdateTransparency()
    {
        
        sr.color = new Color(targetColor.r,targetColor.g,targetColor.b, alpha);
    }

}
