using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;

public class PlayerController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Input")]
    public InputService inputService;
    
    [Header("Movement")]
    public float movePower = 10f;
    public float jumpPower = 15f; //Set Gravity Scale in Rigidbody2D Component to 5

    [Header("Jump Settings")]
    [Tooltip("是否在跳跃时保持水平速度")]
    public bool preserveHorizontalVelocityOnJump = true;
    
    [Tooltip("跳跃时的最大水平速度限制")]
    public float maxHorizontalVelocityOnJump = 15f;

    [Header("Ground Detection")]
    [Tooltip("选择地面检测方式")]
    public GroundDetectionMethod detectionMethod = GroundDetectionMethod.MultiRaycast;
    
    [Tooltip("地面检测的Layer")]
    public LayerMask groundLayer = -1;
    
    [Header("Raycast Detection Settings")]
    [Tooltip("射线检测的起点偏移（相对于角色中心）")]
    public Vector2 raycastOffset = new Vector2(0, -0.5f);
    
    [Tooltip("射线检测的长度")]
    public float raycastDistance = 0.15f;
    
    [Tooltip("多射线检测的数量")]
    public int raycastCount = 3;
    
    [Tooltip("多射线检测的宽度")]
    public float raycastWidth = 0.6f;
    
    [Header("BoxCast Detection Settings")]
    [Tooltip("BoxCast的大小")]
    public Vector2 boxCastSize = new Vector2(0.8f, 0.1f);
    
    [Tooltip("BoxCast的起点偏移")]
    public Vector2 boxCastOffset = new Vector2(0, -0.5f);
    
    [Tooltip("BoxCast的检测距离")]
    public float boxCastDistance = 0.1f;
    
    [Header("OverlapBox Detection Settings")]
    [Tooltip("OverlapBox的大小")]
    public Vector2 overlapBoxSize = new Vector2(0.8f, 0.1f);
    
    [Tooltip("OverlapBox的位置偏移")]
    public Vector2 overlapBoxOffset = new Vector2(0, -0.5f);
    
    [Header("Debug")]
    [Tooltip("显示地面检测调试信息")]
    public bool showGroundDebug = false;
    #endregion

    #region Private Fields
    private Rigidbody2D rb;
    private Animator anim;
    private int direction = 1;


    //bool isJumping = false;
    private bool alive = true;

    bool isGrounded = true;

    Vector2 moveVector;

    public int Direction => direction;

    #endregion

    #region Enums
    public enum GroundDetectionMethod
    {
        Raycast,              // 单射线检测
        MultiRaycast,         // 多射线检测（更稳定）
        BoxCast,              // BoxCast检测
        OverlapBox,           // OverlapBox检测（最精确）
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {   
        rb = GetComponent<Rigidbody2D>();
        inputService = InputService.Instance;

        inputService.inputMap.Player.Jump.started += Jump;
        //inputService.inputMap.Player.Fire.started += Shoot;
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        
        // 如果没有设置groundLayer，警告用户
        if (groundLayer == -1)
        {
            Debug.LogWarning("Ground Layer not set! Ground detection may not work correctly.");
        }
    }

    private void Update()
    {
        moveVector = inputService.Move;

        // 每帧更新地面检测
        CheckGroundStatus();

        Restart();
    }

    private void FixedUpdate()
    {
        if (alive)
        {
            //Hurt();
            //Die();
            Move();
        }
    }
    private void OnDestroy()
    {
        inputService.inputMap.Player.Jump.started -= Jump;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        anim.SetBool("isJump", false);
    }

    private void OnDrawGizmos()
    {
        if (!showGroundDebug) return;

        // 根据不同的检测方法绘制不同的Gizmos
        switch (detectionMethod)
        {
            case GroundDetectionMethod.Raycast:
                DrawRaycastGizmo();
                break;
            case GroundDetectionMethod.MultiRaycast:
                DrawMultiRaycastGizmo();
                break;
            case GroundDetectionMethod.BoxCast:
                DrawBoxCastGizmo();
                break;
            case GroundDetectionMethod.OverlapBox:
                DrawOverlapBoxGizmo();
                break;
        }
    }
    #endregion

    #region Ground Detection Methods

    /// <summary>
    /// 检查角色是否在地面上
    /// </summary>
    private void CheckGroundStatus()
    {
        bool wasGrounded = isGrounded;

        switch (detectionMethod)
        {
            case GroundDetectionMethod.Raycast:
                isGrounded = CheckGroundWithRaycast();
                break;
            case GroundDetectionMethod.MultiRaycast:
                isGrounded = CheckGroundWithMultiRaycast();
                break;
            case GroundDetectionMethod.BoxCast:
                isGrounded = CheckGroundWithBoxCast();
                break;
            case GroundDetectionMethod.OverlapBox:
                isGrounded = CheckGroundWithOverlapBox();
                break;
                break;
        }

        // 当角色刚着地时
        if (!wasGrounded && isGrounded)
        {
            OnLanded();
        }

        // 当角色刚离开地面时
        if (wasGrounded && !isGrounded)
        {
            OnLeftGround();
        }
    }

    /// <summary>
    /// 方法1: 单射线检测（最简单）
    /// </summary>
    private bool CheckGroundWithRaycast()
    {
        Vector2 origin = (Vector2)transform.position + raycastOffset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, raycastDistance, groundLayer);
        if (hit.collider.gameObject.CompareTag("Player") || hit.collider.gameObject == gameObject)
        {
            return false;
        }
        if (showGroundDebug && hit.collider != null)
        {
            Debug.DrawRay(origin, Vector2.down * raycastDistance, Color.green);
        }

        return hit.collider != null;
    }
    
    /// <summary>
    /// 方法2: 多射线检测（更稳定，推荐）
    /// </summary>
    private bool CheckGroundWithMultiRaycast()
    {
        int hitCount = 0;
        
        for (int i = 0; i < raycastCount; i++)
        {
            float t = raycastCount > 1 ? (float)i / (raycastCount - 1) : 0.5f;
            float xOffset = (t - 0.5f) * raycastWidth;
            
            Vector2 origin = (Vector2)transform.position + raycastOffset + new Vector2(xOffset, 0);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, raycastDistance, groundLayer);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject.CompareTag("Player")||hit.collider.gameObject==gameObject){
                    continue;
                }
                //Debug.Log("Hit:"+hit.collider.name);
                hitCount++;
                if (showGroundDebug)
                {
                    Debug.DrawRay(origin, Vector2.down * raycastDistance, Color.green);
                }
            }
            else if (showGroundDebug)
            {
                Debug.DrawRay(origin, Vector2.down * raycastDistance, Color.red);
            }
        }

        // 至少有一条射线击中地面
        return hitCount > 0;
    }

    /// <summary>
    /// 方法3: BoxCast检测（适合不规则地形）
    /// </summary>
    private bool CheckGroundWithBoxCast()
    {
        Vector2 origin = (Vector2)transform.position + boxCastOffset;
        RaycastHit2D hit = Physics2D.BoxCast(origin, boxCastSize, 0f, Vector2.down, boxCastDistance, groundLayer);
        if(hit.collider != null)
        {
            if (hit.collider.gameObject.CompareTag("Player") || hit.collider.gameObject == gameObject)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        return false;

        
    }

    /// <summary>
    /// 方法4: OverlapBox检测（最精确，推荐用于复杂碰撞）
    /// </summary>
    private bool CheckGroundWithOverlapBox()
    {
        Vector2 boxCenter = (Vector2)transform.position + overlapBoxOffset;
        Collider2D hit = Physics2D.OverlapBox(boxCenter, overlapBoxSize, 0f, groundLayer);
        if (hit != null)
        {
            if (hit.gameObject.CompareTag("Player") || hit.gameObject == gameObject)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        
        return false;
    }


    /// <summary>
    /// 当角色着地时调用
    /// </summary>
    private void OnLanded()
    {
        anim.SetBool("isJump", false);
        //isJumping = false;
        
        if (showGroundDebug)
        {
            Debug.Log("Player Landed!");
        }
    }

    /// <summary>
    /// 当角色离开地面时调用
    /// </summary>
    private void OnLeftGround()
    {
        if (showGroundDebug)
        {
            Debug.Log("Player Left Ground!");
        }
    }

    #endregion

    #region Movement Methods

    void Move()
    {
        anim.SetBool("isRun", false);
        
        if (moveVector.x != 0)
        {
            direction = (int)Mathf.Sign(moveVector.x);

            // 在空中和地面使用不同的移动逻辑
            if (isGrounded)
            {
                // 地面移动：直接设置速度
                rb.velocity = new Vector2(moveVector.x * movePower, rb.velocity.y);
            }

            transform.localScale = new Vector3(direction* Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            
            if (isGrounded && !anim.GetBool("isJump"))
                anim.SetBool("isRun", true);
        }
    }
    void Jump(InputAction.CallbackContext obj)
    {
        if (!alive || !isGrounded) return;

        //isJumping = true;
        anim.SetBool("isJump", true);

        if (preserveHorizontalVelocityOnJump)
        {
            // 保持水平速度，只重置垂直速度
            float currentHorizontalVelocity = rb.velocity.x;
            
            // 可选：限制跳跃时的最大水平速度
            if (Mathf.Abs(currentHorizontalVelocity) > maxHorizontalVelocityOnJump)
            {
                currentHorizontalVelocity = Mathf.Sign(currentHorizontalVelocity) * maxHorizontalVelocityOnJump;
            }
            
            rb.velocity = new Vector2(currentHorizontalVelocity, 0);
        }
        else
        {
            // 传统方式：重置所有速度
            rb.velocity = Vector2.zero;
        }
        
        // 添加跳跃力
        rb.AddForce(transform.up * jumpPower, ForceMode2D.Impulse);
        
        if (showGroundDebug)
        {
            Debug.Log($"Jump! Horizontal velocity preserved: {rb.velocity.x}");
        }
    }

    public void Attack()
    {
        Debug.Log("Shoot!");
        anim.SetTrigger("attack");
        
    }

    public void Hurt()
    {
        anim.SetTrigger("hurt");
        if (direction == 1)
            rb.AddForce(new Vector2(-5f, 1f), ForceMode2D.Impulse);
        else
            rb.AddForce(new Vector2(5f, 1f), ForceMode2D.Impulse);
        Debug.Log("Hurt!");
       
    }

    public void Die()
    {
        anim.SetTrigger("die");
        alive = false;
        
    }

    void Restart()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            anim.SetTrigger("idle");
            alive = true;
        }
    }

    #endregion




    #region Debug Gizmos

    private void DrawRaycastGizmo()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }

        Vector2 origin = (Vector2)transform.position + raycastOffset;
        Gizmos.DrawLine(origin, origin + Vector2.down * raycastDistance);
        Gizmos.DrawWireSphere(origin, 0.05f);
    }

    private void DrawMultiRaycastGizmo()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }

        for (int i = 0; i < raycastCount; i++)
        {
            float t = raycastCount > 1 ? (float)i / (raycastCount - 1) : 0.5f;
            float xOffset = (t - 0.5f) * raycastWidth;
            
            Vector2 origin = (Vector2)transform.position + raycastOffset + new Vector2(xOffset, 0);
            Gizmos.DrawLine(origin, origin + Vector2.down * raycastDistance);
            Gizmos.DrawWireSphere(origin, 0.03f);
        }
    }

    private void DrawBoxCastGizmo()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }

        Vector2 origin = (Vector2)transform.position + boxCastOffset;
        Vector2 endPos = origin + Vector2.down * boxCastDistance;
        
        // Draw start box
        DrawWireBox(origin, boxCastSize);
        // Draw end box
        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);
        DrawWireBox(endPos, boxCastSize);
    }

    private void DrawOverlapBoxGizmo()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }

        Vector2 boxCenter = (Vector2)transform.position + overlapBoxOffset;
        DrawWireBox(boxCenter, overlapBoxSize);
    }

    private void DrawWireBox(Vector2 center, Vector2 size)
    {
        Vector2 halfSize = size / 2f;
        Vector2 topLeft = center + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = center + new Vector2(halfSize.x, halfSize.y);
        Vector2 bottomLeft = center + new Vector2(-halfSize.x, -halfSize.y);
        Vector2 bottomRight = center + new Vector2(halfSize.x, -halfSize.y);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 获取当前是否在地面上
    /// </summary>
    public bool IsGrounded => isGrounded;

    /// <summary>
    /// 手动设置地面状态（用于特殊情况）
    /// </summary>
    public void SetGroundedState(bool grounded)
    {
        isGrounded = grounded;
    }

    /// <summary>
    /// 切换是否在跳跃时保持水平速度
    /// </summary>
    public void SetPreserveHorizontalVelocity(bool preserve)
    {
        preserveHorizontalVelocityOnJump = preserve;
    }

    #endregion
}