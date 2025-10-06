using UnityEngine;

/// <summary>
/// ����AI - ʵ��Ѳ�ߡ�׷�𡢹�������Ϊ
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    #region Serialized Fields
    [Header("Patrol Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    
    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float arriveThreshold = 0.3f;
    [SerializeField] private float patrolWaitTime = 1f;
    
    [Header("Combat")]
    [SerializeField] private int maxHealth = 4;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float loseTargetRange = 7f;
    [SerializeField] private float attackCooldown = 1.5f;
    
    [Header("Visual")]
    [SerializeField] private bool showGizmos = true;
    #endregion

    #region Private Fields
    // �������
    private Rigidbody2D rb;
    private Transform playerTransform;
    
    // ״̬
    private EnemyState currentState = EnemyState.Patrol;
    private int currentHealth;
    
    // Ѳ��
    private Vector3 currentPatrolTarget;
    private bool isMovingToPointB = true;
    private float patrolWaitTimer = 0f;
    
    // ����
    private float baseScaleX = 1f;
    private int facingDirection = 1; // 1=��, -1=��
    
    // ����
    private float lastAttackTime = -999f;
    
    // ����
    private const float DIRECTION_EPSILON = 0.05f;
    private const string PLAYER_TAG = "Player";
    #endregion

    #region Enums
    private enum EnemyState
    {
        Patrol,      // Ѳ��
        Chase,       // ׷��
        Attack,      // ����
        ReturnToPatrol // ����Ѳ��
    }
    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        InitializeEnemy();
    }

    private void Update()
    {
        UpdateState();
        ExecuteCurrentState();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        DrawGizmos();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// ��ʼ������
    /// </summary>
    private void InitializeEnemy()
    {
        // ���
        rb = GetComponent<Rigidbody2D>();
        
        // �������
        var playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
        if (playerObj != null)
            playerTransform = playerObj.transform;
        
        // ��ʼ������
        currentHealth = maxHealth;
        
        // ��ʼ������
        baseScaleX = Mathf.Abs(transform.localScale.x);
        if (baseScaleX < 0.01f) baseScaleX = 1f;
        facingDirection = transform.localScale.x >= 0 ? 1 : -1;
        
        // ��ʼ��Ѳ��
        if (ValidatePatrolPoints())
        {
            currentPatrolTarget = pointB.position;
            isMovingToPointB = true;
            currentState = EnemyState.Patrol;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Patrol points not set. Enemy will remain stationary.");
            currentState = EnemyState.Patrol;
        }
    }

    /// <summary>
    /// ��֤Ѳ�ߵ��Ƿ���Ч
    /// </summary>
    private bool ValidatePatrolPoints()
    {
        return pointA != null && pointB != null;
    }

    #endregion

    #region State Machine

    /// <summary>
    /// ����״̬
    /// </summary>
    private void UpdateState()
    {
        if (playerTransform == null)
        {
            if (currentState != EnemyState.Patrol)
                TransitionToPatrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case EnemyState.Patrol:
                // �������
                if (distanceToPlayer <= detectionRange)
                {
                    TransitionToChase();
                }
                break;

            case EnemyState.Chase:
                // ���빥����Χ
                if (distanceToPlayer <= attackRange)
                {
                    TransitionToAttack();
                }
                // ��ʧĿ��
                else if (distanceToPlayer > loseTargetRange)
                {
                    TransitionToReturnToPatrol();
                }
                break;

            case EnemyState.Attack:
                // ���빥����Χ
                if (distanceToPlayer > attackRange)
                {
                    TransitionToChase();
                }
                // ��ʧĿ��
                else if (distanceToPlayer > loseTargetRange)
                {
                    TransitionToReturnToPatrol();
                }
                break;

            case EnemyState.ReturnToPatrol:
                // ����ٴνӽ�
                if (distanceToPlayer <= detectionRange)
                {
                    TransitionToChase();
                }
                // ����Ѳ�ߵ�
                else if (ValidatePatrolPoints())
                {
                    float distToTarget = Vector2.Distance(transform.position, currentPatrolTarget);
                    if (distToTarget <= arriveThreshold)
                    {
                        TransitionToPatrol();
                    }
                }
                break;
        }
    }

    /// <summary>
    /// ִ�е�ǰ״̬
    /// </summary>
    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                ExecutePatrol();
                break;

            case EnemyState.Chase:
                ExecuteChase();
                break;

            case EnemyState.Attack:
                ExecuteAttack();
                break;

            case EnemyState.ReturnToPatrol:
                ExecuteReturnToPatrol();
                break;
        }
    }

    #endregion

    #region State Transitions

    private void TransitionToPatrol()
    {
        currentState = EnemyState.Patrol;
        patrolWaitTimer = 0f;
    }

    private void TransitionToChase()
    {
        currentState = EnemyState.Chase;
    }

    private void TransitionToAttack()
    {
        currentState = EnemyState.Attack;
        StopMovement();
    }

    private void TransitionToReturnToPatrol()
    {
        currentState = EnemyState.ReturnToPatrol;
        
        // ѡ�������Ѳ�ߵ�
        if (ValidatePatrolPoints())
        {
            float distToA = Vector2.Distance(transform.position, pointA.position);
            float distToB = Vector2.Distance(transform.position, pointB.position);
            
            if (distToA < distToB)
            {
                currentPatrolTarget = pointA.position;
                isMovingToPointB = false;
            }
            else
            {
                currentPatrolTarget = pointB.position;
                isMovingToPointB = true;
            }
        }
    }

    #endregion

    #region State Execution

    /// <summary>
    /// ִ��Ѳ��
    /// </summary>
    private void ExecutePatrol()
    {
        if (!ValidatePatrolPoints())
        {
            StopMovement();
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, currentPatrolTarget);

        // ����Ѳ�ߵ�
        if (distanceToTarget <= arriveThreshold)
        {
            StopMovement();
            
            // �ȴ���ʱ
            if (patrolWaitTimer < patrolWaitTime)
            {
                patrolWaitTimer += Time.deltaTime;
            }
            else
            {
                // �л�Ѳ��Ŀ��
                SwitchPatrolTarget();
                patrolWaitTimer = 0f;
            }
        }
        else
        {
            // �����ƶ�
            MoveTowards(currentPatrolTarget, patrolSpeed);
        }
    }

    /// <summary>
    /// ִ��׷��
    /// </summary>
    private void ExecuteChase()
    {
        if (playerTransform == null) return;
        
        MoveTowards(playerTransform.position, chaseSpeed);
    }

    /// <summary>
    /// ִ�й���
    /// </summary>
    private void ExecuteAttack()
    {
        StopMovement();
        
        // �������
        if (playerTransform != null)
        {
            FaceTarget(playerTransform.position);
        }
        
        // ���Թ���
        TryAttack();
    }

    /// <summary>
    /// ִ�з���Ѳ��
    /// </summary>
    private void ExecuteReturnToPatrol()
    {
        if (!ValidatePatrolPoints())
        {
            TransitionToPatrol();
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, currentPatrolTarget);

        if (distanceToTarget <= arriveThreshold)
        {
            TransitionToPatrol();
        }
        else
        {
            MoveTowards(currentPatrolTarget, patrolSpeed);
        }
    }

    #endregion

    #region Movement

    /// <summary>
    /// �ƶ���Ŀ��
    /// </summary>
    private void MoveTowards(Vector3 target, float speed)
    {
        Vector2 direction = (target - transform.position);
        direction.y = 0f; // ��ˮƽ�ƶ�
        
        if (direction.sqrMagnitude < 0.001f)
        {
            StopMovement();
            return;
        }
        
        direction.Normalize();
        
        // �����ٶ�
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
        
        // ���³���
        UpdateFacing(direction.x);
    }

    /// <summary>
    /// ֹͣ�ƶ�
    /// </summary>
    private void StopMovement()
    {
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    /// <summary>
    /// �л�Ѳ��Ŀ��
    /// </summary>
    private void SwitchPatrolTarget()
    {
        isMovingToPointB = !isMovingToPointB;
        currentPatrolTarget = isMovingToPointB ? pointB.position : pointA.position;
        
        // Ԥ�ȸ��³��򣬱��ⶶ��
        float directionX = currentPatrolTarget.x - transform.position.x;
        if (Mathf.Abs(directionX) > DIRECTION_EPSILON)
        {
            UpdateFacing(directionX);
        }
    }

    /// <summary>
    /// ����Ŀ��
    /// </summary>
    private void FaceTarget(Vector3 target)
    {
        float directionX = target.x - transform.position.x;
        UpdateFacing(directionX);
    }

    /// <summary>
    /// ���³���
    /// </summary>
    private void UpdateFacing(float directionX)
    {
        if (directionX > DIRECTION_EPSILON && facingDirection != 1)
        {
            facingDirection = 1;
            ApplyFacing();
        }
        else if (directionX < -DIRECTION_EPSILON && facingDirection != -1)
        {
            facingDirection = -1;
            ApplyFacing();
        }
    }

    /// <summary>
    /// Ӧ�ó���
    /// </summary>
    private void ApplyFacing()
    {
        transform.localScale = new Vector3(
            baseScaleX * facingDirection,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    #endregion

    #region Combat

    /// <summary>
    /// ���Թ���
    /// </summary>
    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        if (playerTransform == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > attackRange + 0.1f)
            return;

        PerformAttack();
    }

    /// <summary>
    /// ִ�й���
    /// </summary>
    private void PerformAttack()
    {
        lastAttackTime = Time.time;

        // ������Ч
        AudioManager.Instance?.PlaySFX("EnemyAttack");

        // ���������˺�
        if (playerTransform != null)
        {
            var playerController = playerTransform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.Hurt();
            }
        }
    }

    /// <summary>
    /// �ܵ��˺�
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        
        ApplyKnockback();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Ӧ�û���Ч��
    /// </summary>
    private void ApplyKnockback()
    {
        // ����������ʩ�ӻ�����
        Vector2 knockbackDirection = new Vector2(-facingDirection * 5f, 1f);
        rb.AddForce(knockbackDirection, ForceMode2D.Impulse);
    }

    /// <summary>
    /// ����
    /// </summary>
    public virtual void Die()
    {
        // ����������Ч
        AudioManager.Instance?.PlaySFX("EnemyDie");
        
        // ���ٶ���
        Destroy(gameObject, 0.5f);
    }

    #endregion

    #region Debug

    /// <summary>
    /// ���Ƶ�����Ϣ
    /// </summary>
    private void DrawGizmos()
    {
        // ������Χ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // ��ⷶΧ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // ��ʧĿ�귶Χ
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
        
        // Ѳ�ߵ�����
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pointA.position, 0.3f);
            Gizmos.DrawWireSphere(pointB.position, 0.3f);
        }
        
        // ��ǰĿ��
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentPatrolTarget);
            Gizmos.DrawWireSphere(currentPatrolTarget, 0.2f);
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// ��ȡ��ǰ״̬
    /// </summary>
    public string GetCurrentState()
    {
        return currentState.ToString();
    }

    /// <summary>
    /// ��ȡ��ǰ����ֵ
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// �Ƿ�����׷�����
    /// </summary>
    public bool IsChasingPlayer()
    {
        return currentState == EnemyState.Chase || currentState == EnemyState.Attack;
    }

    #endregion
}
