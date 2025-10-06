using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Enemy : MonoBehaviour
{
    //Ñ²Âßµã
    public Transform pointA, pointB;
    public float speed = 2f;
    public int hp = 3;
    public int damage = 1;
    public float attackRange = 1.2f;
    public float chaseRange = 5f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 target;
    private bool chasing = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        target = pointB.position;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist < attackRange)
        {
            anim.SetTrigger("Attack");
            player.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 4f, ForceMode2D.Impulse);
            AudioManager.Instance.PlaySFX("EnemyAttack");
            return;
        }
        else if (dist < chaseRange)
        {
            chasing = true;
            MoveToward(player.position);
        }
        else
        {
            chasing = false;
            Patrol();
        }
    }

    void Patrol()
    {
        MoveToward(target);
        if (Vector2.Distance(transform.position, target) < 0.2f)
            target = (target == pointA.position) ? pointB.position : pointA.position;
    }

    void MoveToward(Vector3 destination)
    {
        Vector2 dir = (destination - transform.position).normalized;
        rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        anim.SetBool("Run", true);
        transform.localScale = new Vector3(Mathf.Sign(dir.x), 1, 1);
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        anim.SetTrigger("Hit");
        if (hp <= 0) Die();
    }

    void Die()
    {
        anim.SetTrigger("Die");
        AudioManager.Instance.PlaySFX("EnemyDie");
        Destroy(gameObject, 1f);
    }
}
