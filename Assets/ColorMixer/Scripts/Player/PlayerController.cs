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
    [Tooltip("�Ƿ�����Ծʱ����ˮƽ�ٶ�")]
    public bool preserveHorizontalVelocityOnJump = true;
    
    [Tooltip("��Ծʱ�����ˮƽ�ٶ�����")]
    public float maxHorizontalVelocityOnJump = 15f;

    [Header("Ground Detection")]
    [Tooltip("ѡ������ⷽʽ")]
    public GroundDetectionMethod detectionMethod = GroundDetectionMethod.MultiRaycast;
    
    [Tooltip("�������Layer")]
    public LayerMask groundLayer = -1;
    
    [Header("Raycast Detection Settings")]
    [Tooltip("���߼������ƫ�ƣ�����ڽ�ɫ���ģ�")]
    public Vector2 raycastOffset = new Vector2(0, -0.5f);
    
    [Tooltip("���߼��ĳ���")]
    public float raycastDistance = 0.15f;
    
    [Tooltip("�����߼�������")]
    public int raycastCount = 3;
    
    [Tooltip("�����߼��Ŀ��")]
    public float raycastWidth = 0.6f;
    
    [Header("BoxCast Detection Settings")]
    [Tooltip("BoxCast�Ĵ�С")]
    public Vector2 boxCastSize = new Vector2(0.8f, 0.1f);
    
    [Tooltip("BoxCast�����ƫ��")]
    public Vector2 boxCastOffset = new Vector2(0, -0.5f);
    
    [Tooltip("BoxCast�ļ�����")]
    public float boxCastDistance = 0.1f;
    
    [Header("OverlapBox Detection Settings")]
    [Tooltip("OverlapBox�Ĵ�С")]
    public Vector2 overlapBoxSize = new Vector2(0.8f, 0.1f);
    
    [Tooltip("OverlapBox��λ��ƫ��")]
    public Vector2 overlapBoxOffset = new Vector2(0, -0.5f);
    
    [Header("Debug")]
    [Tooltip("��ʾ�����������Ϣ")]
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
        Raycast,              // �����߼��
        MultiRaycast,         // �����߼�⣨���ȶ���
        BoxCast,              // BoxCast���
        OverlapBox,           // OverlapBox��⣨�ȷ��
        VelocityBased         // �����ٶȵļ�⣨�򵥵���̫׼ȷ��
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
        
        // ���û������groundLayer�������û�
        if (groundLayer == -1)
        {
            Debug.LogWarning("Ground Layer not set! Ground detection may not work correctly.");
        }
    }

    private void Update()
    {
        moveVector = inputService.Move;

        // ÿ֡���µ�����
        CheckGroundStatus();

        Restart();
    }

    private void FixedUpdate()
    {
        if (alive)
        {
            Hurt();
            Die();
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

        // ���ݲ�ͬ�ļ�ⷽ�����Ʋ�ͬ��Gizmos
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
    /// ����ɫ�Ƿ��ڵ�����
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
            case GroundDetectionMethod.VelocityBased:
                isGrounded = CheckGroundWithVelocity();
                break;
        }

        // ����ɫ���ŵ�ʱ
        if (!wasGrounded && isGrounded)
        {
            OnLanded();
        }

        // ����ɫ���뿪����ʱ
        if (wasGrounded && !isGrounded)
        {
            OnLeftGround();
        }
    }

    /// <summary>
    /// ����1: �����߼�⣨��򵥣�
    /// </summary>
    private bool CheckGroundWithRaycast()
    {
        Vector2 origin = (Vector2)transform.position + raycastOffset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, raycastDistance, groundLayer);

        if (showGroundDebug && hit.collider != null)
        {
            Debug.DrawRay(origin, Vector2.down * raycastDistance, Color.green);
        }

        return hit.collider != null;
    }

    /// <summary>
    /// ����2: �����߼�⣨���ȶ����Ƽ���
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

        // ������һ�����߻��е���
        return hitCount > 0;
    }

    /// <summary>
    /// ����3: BoxCast��⣨�ʺϲ�������Σ�
    /// </summary>
    private bool CheckGroundWithBoxCast()
    {
        Vector2 origin = (Vector2)transform.position + boxCastOffset;
        RaycastHit2D hit = Physics2D.BoxCast(origin, boxCastSize, 0f, Vector2.down, boxCastDistance, groundLayer);

        return hit.collider != null;
    }

    /// <summary>
    /// ����4: OverlapBox��⣨�ȷ���Ƽ����ڸ�����ײ��
    /// </summary>
    private bool CheckGroundWithOverlapBox()
    {
        Vector2 boxCenter = (Vector2)transform.position + overlapBoxOffset;
        Collider2D hit = Physics2D.OverlapBox(boxCenter, overlapBoxSize, 0f, groundLayer);

        return hit != null;
    }

    /// <summary>
    /// ����5: �����ٶȵļ�⣨�򵥵���׼ȷ��
    /// </summary>
    private bool CheckGroundWithVelocity()
    {
        // �����ֱ�ٶȽӽ�0����һ֡�ڵ����ϣ���Ϊ���ڵ���
        // ���ַ�����̫׼ȷ�����Ƽ�ʹ��
        return Mathf.Abs(rb.velocity.y) < 0.1f && isGrounded;
    }

    /// <summary>
    /// ����ɫ�ŵ�ʱ����
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
    /// ����ɫ�뿪����ʱ����
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

            // �ڿ��к͵���ʹ�ò�ͬ���ƶ��߼�
            if (isGrounded)
            {
                // �����ƶ���ֱ�������ٶ�
                rb.velocity = new Vector2(moveVector.x * movePower, rb.velocity.y);
            }

            transform.localScale = new Vector3(direction, 1, 1);
            
            if (isGrounded && !anim.GetBool("isJump"))
                anim.SetBool("isRun", true);
        }
    }
    void Shoot()
    {

    }
    void Jump(InputAction.CallbackContext obj)
    {
        if (!alive || !isGrounded) return;

        //isJumping = true;
        anim.SetBool("isJump", true);

        if (preserveHorizontalVelocityOnJump)
        {
            // ����ˮƽ�ٶȣ�ֻ���ô�ֱ�ٶ�
            float currentHorizontalVelocity = rb.velocity.x;
            
            // ��ѡ��������Ծʱ�����ˮƽ�ٶ�
            if (Mathf.Abs(currentHorizontalVelocity) > maxHorizontalVelocityOnJump)
            {
                currentHorizontalVelocity = Mathf.Sign(currentHorizontalVelocity) * maxHorizontalVelocityOnJump;
            }
            
            rb.velocity = new Vector2(currentHorizontalVelocity, 0);
        }
        else
        {
            // ��ͳ��ʽ�����������ٶ�
            rb.velocity = Vector2.zero;
        }
        
        // �����Ծ��
        rb.AddForce(transform.up * jumpPower, ForceMode2D.Impulse);
        
        if (showGroundDebug)
        {
            Debug.Log($"Jump! Horizontal velocity preserved: {rb.velocity.x}");
        }
    }

    void Attack(InputAction.CallbackContext obj)
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Shoot!");
            anim.SetTrigger("attack");
        }
    }

    void Hurt()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            anim.SetTrigger("hurt");
            if (direction == 1)
                rb.AddForce(new Vector2(-5f, 1f), ForceMode2D.Impulse);
            else
                rb.AddForce(new Vector2(5f, 1f), ForceMode2D.Impulse);
            Debug.Log("Hurt!");
        }
    }

    void Die()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            anim.SetTrigger("die");
            alive = false;
        }
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
    /// ��ȡ��ǰ�Ƿ��ڵ�����
    /// </summary>
    public bool IsGrounded => isGrounded;

    /// <summary>
    /// �ֶ����õ���״̬���������������
    /// </summary>
    public void SetGroundedState(bool grounded)
    {
        isGrounded = grounded;
    }

    /// <summary>
    /// �л��Ƿ�����Ծʱ����ˮƽ�ٶ�
    /// </summary>
    public void SetPreserveHorizontalVelocity(bool preserve)
    {
        preserveHorizontalVelocityOnJump = preserve;
    }

    #endregion
}
