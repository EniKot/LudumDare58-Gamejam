using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform targetPoint;
    public GameObject portalEffectPrefab;
    public float teleportDelay = 0.3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && targetPoint != null)
        {
            StartCoroutine(TeleportSequence(other.transform));
        }
    }

    private System.Collections.IEnumerator TeleportSequence(Transform player)
    {
        if (portalEffectPrefab)
            Instantiate(portalEffectPrefab, transform.position, Quaternion.identity);

        AudioManager.Instance.PlaySFX("Teleport");
        yield return new WaitForSeconds(teleportDelay);

        player.position = targetPoint.position;

        if (portalEffectPrefab)
            Instantiate(portalEffectPrefab, targetPoint.position, Quaternion.identity);
    }
}
