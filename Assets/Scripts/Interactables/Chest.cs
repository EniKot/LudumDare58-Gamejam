using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public Animator anim;
    public int reward = 100;

    private bool isOpened = false;

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
