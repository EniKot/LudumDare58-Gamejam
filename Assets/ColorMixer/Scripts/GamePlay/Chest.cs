using UnityEngine;

public class Chest : MonoBehaviour, IInteractable,IColorDyeable
{
    public Animator anim;
    public int reward = 100;
    public Color color;
   
    private bool isOpened = false;

    public int maxHealth = 4;

    private int currentHealth;


    public void Start()
    {
        currentHealth = maxHealth;
    }
    public void OnColorDye(Color receivedColor)
    {
        // 可以根据 receivedColor 改变宝箱颜色
        // 补色
        Color desireColor = Color.white - color;
        if (ColorMixingStrategies.IsColorSimilar(receivedColor, desireColor)) { 
            
            currentHealth--;
            if (currentHealth <= 0)
            {
                Opened();
            }
        }
            

    }
    public void Opened()
    {
        gameObject.SetActive(false);
    }
    public void OnInteract(PlayerController player)
    {
        if (isOpened) return;

        Debug.Log("Chest opened! Reward +" + reward);
        if (anim != null)
            anim.SetTrigger("Open");

        isOpened = true;
        // 在这里加金币逻辑（例如 player.AddScore(reward);）
    }
}
