using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour
{
    public static Transform LastCheckpoint; // 全局存储
    public GameObject flagActiveEffect; // 可选，播放特效

    private bool activated = false;

    private void Start()
    {
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (other.CompareTag("Player"))
        {
            activated = true;
            LastCheckpoint = transform;
            AudioManager.Instance.PlaySFX("Checkpoint");
            Debug.Log("✅ 新的复活点已激活");

            if (flagActiveEffect)
                Instantiate(flagActiveEffect, transform.position, Quaternion.identity);
        }
    }
}
