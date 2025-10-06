using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SpikeTrap : MonoBehaviour
{
    [Header("复活点 (Checkpoint)")]
    public Transform fallbackPoint; // 默认复活点（可不填）

    private void Start()
    {
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 获取玩家控制器
            var player = other.GetComponent<PlayerController>();
            if (player == null) return;

            // 获取当前全局复活点
            Transform respawn = Checkpoint.LastCheckpoint != null
                ? Checkpoint.LastCheckpoint
                : fallbackPoint;

            if (respawn != null)
            {
                other.transform.position = respawn.position;
                AudioManager.Instance.PlaySFX("SpikeHit");
                Debug.Log("💀 玩家触碰尖刺，被送回复活点。");
            }
        }
    }
}
