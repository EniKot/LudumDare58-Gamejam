using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class ColorBullet : MonoBehaviour
{
    [Header("Bullet属性")]
    public float speed = 10f;                 // 初速度（单位/秒）
    public int damage = 1;                    // 伤害值 （暂时用不上）
    public int pierce = 0;                    // 穿透次数（0 = 不穿透，>0 表示可穿透几次 暂时用不上）
    public float lifetime = 5f;               // 存活时间（秒），超时回收
    //public bool useTrigger = true;            // 是否使用 Trigger（否则使用物理碰撞）
    public int direction = 1;
    public Color color;                     // 子弹颜色
    [Header("外观")]
    public SpriteRenderer spriteRenderer;     // 可在 Inspector 指定
    public ParticleSystem hitEffectPrefab;    // 命中效果（可选）

    // 池化或外部回收回调（外部池管理器会注册）
    public Action<ColorBullet> OnReturnToPool;

    Rigidbody2D rb;
    float bornTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.color = color;
    }

    void OnEnable()
    {
        bornTime = Time.time;

        // 清除物理残留速度（重要）
        rb.velocity = Vector2.zero;
    }

    void Update()
    {
        // 如果使用非物理驱动（但这里默认用物理），可改成 transform.Translate
        if (Time.time - bornTime > lifetime)
        {
            ReturnToPool();
        }
    }

    void FixedUpdate()
    {
        
        rb.velocity = Vector2.right*direction * speed;
    }

    /// <summary>
    /// 配置子弹（颜色、伤害等）并启动
    /// </summary>
    public void Initialize(Vector2 position, int direction,Color color, float speed, int damage, int pierce = 0, float lifetime = 5f)
    {
        transform.position = position;
        transform.localScale = new Vector3(direction, 1, 1);
        this.direction=direction;
        this.speed = speed;
        this.damage = damage;
        this.pierce = pierce;
        this.lifetime = lifetime;
       
        bornTime = Time.time;

        if (spriteRenderer != null)
            spriteRenderer.color = color;

        gameObject.SetActive(true);
    }

    // 触发/碰撞回调
    void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }


    void HandleHit(Collider2D other)
    {
        if (other == null) return;

        // 忽略与自己层级或自己碰撞体的重复检测（可根据项目层级策略改进）
        if (other.attachedRigidbody != null && other.attachedRigidbody == rb) return;
        
        // 优先尝试对目标调用 IDamageable 接口

        var dyableObject = other.GetComponentInParent<IColorDyeable>();

        if(dyableObject != null)
        {
            dyableObject.OnColorDye(color);
        }


        //var damageable = other.GetComponentInParent<IColorDyeable>();
        //if (damageable != null)
        //{
        //    damageable.TakeDamage(damage, gameObject);
        //}
        //else
        //{
        //    // 如果没有 IDamageable，可以根据标签或组件进行其他处理
        //    // Example: if (other.CompareTag("Environment")) { ... }
        //}
        SpawnHitEffect(other.ClosestPoint(transform.position));


        // 默认命中一次就回收
        ReturnToPool();
    }

    void SpawnHitEffect(Vector2 position)
        
    {
        //Debug.Log("PlayHit Effect");
        if (hitEffectPrefab == null) return;
        var ps = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        ps.Play();
        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }

    /// <summary>
    /// 回收子弹（通过回调交给池）
    /// </summary>
    public void ReturnToPool()
    {
        // 停止运动
        rb.velocity = Vector2.zero;
        // 取消回调注册（保险）
        OnReturnToPool?.Invoke(this);
    }

    void OnDisable()
    {
        // 清理，避免残留引用
        OnReturnToPool = null;
    }
}

