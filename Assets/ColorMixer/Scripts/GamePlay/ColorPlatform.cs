using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class ColorPlatform : MonoBehaviour
{
    public enum BasicColor { Red, Yellow, Blue }

    [Header("目标颜色 (平台需要被激活的最终颜色)")]
    public BasicColor targetColor;

    [Header("当前填充状态")]
    public List<BasicColor> currentHits = new List<BasicColor>();

    private SpriteRenderer sr;
    private BoxCollider2D col;
    private bool solidified = false;
    private float alpha = 0.15f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        col.enabled = false;
        UpdateTransparency();
    }

    // 模拟被子弹击中颜色
    public void OnHitByColor(BasicColor color)
    {
        if (solidified) return;
        if (!currentHits.Contains(color))
            currentHits.Add(color);

        // 判断是否达成目标混色
        if (IsColorMatch())
        {
            solidified = true;
            col.enabled = true;
            alpha = 1f;
            UpdateTransparency();

            var move = GetComponent<MovingPlatform>();
            if (move) move.active = true;
            AudioManager.Instance.PlaySFX("PlatformActive");
        }
        else
        {
            alpha += 0.25f;
            alpha = Mathf.Clamp(alpha, 0.15f, 0.7f);
            UpdateTransparency();
        }
    }

    private bool IsColorMatch()
    {
        // 颜色组合规则
        if (targetColor == BasicColor.Red && currentHits.Contains(BasicColor.Red)) return true;
        if (targetColor == BasicColor.Yellow && currentHits.Contains(BasicColor.Yellow)) return true;
        if (targetColor == BasicColor.Blue && currentHits.Contains(BasicColor.Blue)) return true;
        // 混色逻辑
        if (targetColor == BasicColor.Red && currentHits.Contains(BasicColor.Yellow) && currentHits.Contains(BasicColor.Blue)) return true;
        if (targetColor == BasicColor.Yellow && currentHits.Contains(BasicColor.Red) && currentHits.Contains(BasicColor.Blue)) return true;
        if (targetColor == BasicColor.Blue && currentHits.Contains(BasicColor.Red) && currentHits.Contains(BasicColor.Yellow)) return true;
        return false;
    }

    private void UpdateTransparency()
    {
        Color baseColor = GetBaseColor(targetColor);
        sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }

    private Color GetBaseColor(BasicColor c)
    {
        switch (c)
        {
            case BasicColor.Red: return Color.red;
            case BasicColor.Yellow: return Color.yellow;
            case BasicColor.Blue: return Color.blue;
            default: return Color.white;
        }
    }
}
