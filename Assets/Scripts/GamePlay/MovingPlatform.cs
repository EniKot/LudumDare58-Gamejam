using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA, pointB;
    public float speed = 2f;
    public bool active = true;
    private bool goingToB = true;

    void Update()
    {
        if (!active) return;
        Transform target = goingToB ? pointB : pointA;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
            goingToB = !goingToB;
    }
}
