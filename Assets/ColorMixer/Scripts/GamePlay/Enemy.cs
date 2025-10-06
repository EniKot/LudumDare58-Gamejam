using UnityEngine;

/// <summary>
/// 敌人AI - 实现巡逻、追逐、攻击等行为
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
    // 组件引用
    private Rigidbody2D rb;
    private Transform playerTransform;
    
    // 状态
    private EnemyState currentState = EnemyState.Patrol;
    private int currentHealth;
    
    // 巡逻
    private Vector3 currentPatrolTarget;
    private bool isMovingToPointB = true;
    private float patrolWaitTimer = 0f;
    
    // 朝向
    private float baseScaleX = 1f;
    private int facingDirection = 1; // 1=右, -1=左
    
    // 攻击
    private float lastAttackTime = -999f;
    
    // 常量
    private const float DIRECTION_EPSILON = 0.05f;
    private const string PLAYER_TAG = "Player";
    #endregion

    #region Enums
    private enum EnemyState
    {
        Patrol,      // 巡逻
        Chase,       // 追逐
        Attack,      // 攻击
        ReturnToPatrol // 返回巡逻
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
    /// 初始化敌人
    /// </summary>
    private void InitializeEnemy()
    {
        // 组件
        rb = GetComponent<Rigidbody2D>();
        
        // 玩家引用
        var playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
        if (playerObj != null)
            playerTransform = playerObj.transform;
        
        // 初始化数据
        currentHealth = maxHealth;
        
        // 初始化朝向
        baseScaleX = Mathf.Abs(transform.localScale.x);
        if (baseScaleX < 0.01f) baseScaleX = 1f;
        facingDirection = transform.localScale.x >= 0 ? 1 : -1;
        
        // 初始化巡逻
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
    /// 验证巡逻点是否有效
    /// </summary>
    private bool ValidatePatrolPoints()
    {
        return pointA != null && pointB != null;
    }

    #endregion

    #region State Machine

    /// <summary>
    /// 更新状态
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
                // 发现玩家
                if (distanceToPlayer <= detectionRange)
                {
                    TransitionToChase();
                }
                break;

            case EnemyState.Chase:
                // 进入攻击范围
                if (distanceToPlayer <= attackRange)
                {
                    TransitionToAttack();
                }
                // 丢失目标
                else if (distanceToPlayer > loseTargetRange)
                {
                    TransitionToReturnToPatrol();
                }
                break;

            case EnemyState.Attack:
                // 脱离攻击范围
                if (distanceToPlayer > attackRange)
                {
                    TransitionToChase();
                }
                // 丢失目标
                else if (distanceToPlayer > loseTargetRange)
                {
                    TransitionToReturnToPatrol();
                }
                break;

            case EnemyState.ReturnToPatrol:
                // 玩家再次接近
                if (distanceToPlayer <= detectionRange)
                {
                    TransitionToChase();
                }
                // 到达巡逻点
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
    /// 执行当前状态
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
        
        // 选择最近的巡逻点
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
    /// 执行巡逻
    /// </summary>
    private void ExecutePatrol()
    {
        if (!ValidatePatrolPoints())
        {
            StopMovement();
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, currentPatrolTarget);

        // 到达巡逻点
        if (distanceToTarget <= arriveThreshold)
        {
            StopMovement();
            
            // 等待计时
            if (patrolWaitTimer < patrolWaitTime)
            {
                patrolWaitTimer += Time.deltaTime;
            }
            else
            {
                // 切换巡逻目标
                SwitchPatrolTarget();
                patrolWaitTimer = 0f;
            }
        }
        else
        {
            // 继续移动
            MoveTowards(currentPatrolTarget, patrolSpeed);
        }
    }

    /// <summary>
    /// 执行追逐
    /// </summary>
    private void ExecuteChase()
    {
        if (playerTransform == null) return;
        
        MoveTowards(playerTransform.position, chaseSpeed);
    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    private void ExecuteAttack()
    {
        StopMovement();
        
        // 面向玩家
        if (playerTransform != null)
        {
            FaceTarget(playerTransform.position);
        }
        
        // 尝试攻击
        TryAttack();
    }

    /// <summary>
    /// 执行返回巡逻
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
    /// 移动向目标
    /// </summary>
    private void MoveTowards(Vector3 target, float speed)
    {
        Vector2 direction = (target - transform.position);
        direction.y = 0f; // 仅水平移动
        
        if (direction.sqrMagnitude < 0.001f)
        {
            StopMovement();
            return;
        }
        
        direction.Normalize();
        
        // 设置速度
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
        
        // 更新朝向
        UpdateFacing(direction.x);
    }

    /// <summary>
    /// 停止移动
    /// </summary>
    private void StopMovement()
    {
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    /// <summary>
    /// 切换巡逻目标
    /// </summary>
    private void SwitchPatrolTarget()
    {
        isMovingToPointB = !isMovingToPointB;
        currentPatrolTarget = isMovingToPointB ? pointB.position : pointA.position;
        
        // 预先更新朝向，避免抖动
        float directionX = currentPatrolTarget.x - transform.position.x;
        if (Mathf.Abs(directionX) > DIRECTION_EPSILON)
        {
            UpdateFacing(directionX);
        }
    }

    /// <summary>
    /// 面向目标
    /// </summary>
    private void FaceTarget(Vector3 target)
    {
        float directionX = target.x - transform.position.x;
        UpdateFacing(directionX);
    }

    /// <summary>
    /// 更新朝向
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
    /// 应用朝向
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
    /// 尝试攻击
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
    /// 执行攻击
    /// </summary>
    private void PerformAttack()
    {
        lastAttackTime = Time.time;

        // 播放音效
        AudioManager.Instance?.PlaySFX("EnemyAttack");

        // 对玩家造成伤害
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
    /// 受到伤害
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
    /// 应用击退效果
    /// </summary>
    private void ApplyKnockback()
    {
        // 根据面向方向施加击退力
        Vector2 knockbackDirection = new Vector2(-facingDirection * 5f, 1f);
        rb.AddForce(knockbackDirection, ForceMode2D.Impulse);
    }

    /// <summary>
    /// 死亡
    /// </summary>
    public virtual void Die()
    {
        // 播放死亡音效
        AudioManager.Instance?.PlaySFX("EnemyDie");
        
        // 销毁对象
        Destroy(gameObject, 0.5f);
    }

    #endregion

    #region Debug

    /// <summary>
    /// 绘制调试信息
    /// </summary>
    private void DrawGizmos()
    {
        // 攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 丢失目标范围
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
        
        // 巡逻点连线
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pointA.position, 0.3f);
            Gizmos.DrawWireSphere(pointB.position, 0.3f);
        }
        
        // 当前目标
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
    /// 获取当前状态
    /// </summary>
    public string GetCurrentState()
    {
        return currentState.ToString();
    }

    /// <summary>
    /// 获取当前生命值
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// 是否正在追逐玩家
    /// </summary>
    public bool IsChasingPlayer()
    {
        return currentState == EnemyState.Chase || currentState == EnemyState.Attack;
    }

    #endregion
}
