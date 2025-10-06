using UnityEngine;

public class Chest : Enemy, IColorDyeable
{
    public Color color;

    private SpriteRenderer spriteRenderer;
    public Sprite openedSprite;
    private bool isOpened = false;
    //public int maxHealth = 4;

    //private int currentHealth;
    private void Awake()
    {
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }


    }

    public void OnColorDye(Color receivedColor)
    {
        // 可以根据 receivedColor 改变宝箱颜色
        // 补色
        Color desireColor = Color.white - color;
        if (ColorMixingStrategies.IsColorSimilar(receivedColor, desireColor))
        {
            Debug.Log("Chest "+gameObject.name+" hit!");
            base.TakeDamage(1);
        }


    }
    public override void Die()
    {
        spriteRenderer.sprite = openedSprite;
        base.Die();
    }

}
