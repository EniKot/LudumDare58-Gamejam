using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 1.5f;
    public LayerMask interactLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, interactLayer);
            if (hit)
            {
                var interactable = hit.GetComponent<IInteractable>();
                if (interactable != null)
                    interactable.OnInteract(GetComponent<PlayerController>());
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
