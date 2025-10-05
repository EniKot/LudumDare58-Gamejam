using UnityEngine;

public class Obstacle : MonoBehaviour, IInteractable
{
    public int damage = 10;

    public void OnInteract(PlayerController player)
    {
        Debug.Log("Player hit obstacle! -" + damage + " HP");
        // 这里可以触发动画/扣血逻辑
    }
}
