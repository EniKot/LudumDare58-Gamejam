using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class ColorBullet : MonoBehaviour
{
    [Header("Bullet����")]
    public float speed = 10f;                 // ���ٶȣ���λ/�룩
    public int damage = 1;                    // �˺�ֵ ����ʱ�ò��ϣ�
    public int pierce = 0;                    // ��͸������0 = ����͸��>0 ��ʾ�ɴ�͸���� ��ʱ�ò��ϣ�
    public float lifetime = 5f;               // ���ʱ�䣨�룩����ʱ����
    //public bool useTrigger = true;            // �Ƿ�ʹ�� Trigger������ʹ��������ײ��
    public int direction = 1;
    public Color color;                     // �ӵ���ɫ
    [Header("���")]
    public SpriteRenderer spriteRenderer;     // ���� Inspector ָ��
    public ParticleSystem hitEffectPrefab;    // ����Ч������ѡ��

    // �ػ����ⲿ���ջص����ⲿ�ع�������ע�ᣩ
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

        // �����������ٶȣ���Ҫ��
        rb.velocity = Vector2.zero;
    }

    void Update()
    {
        // ���ʹ�÷�����������������Ĭ�����������ɸĳ� transform.Translate
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
    /// �����ӵ�����ɫ���˺��ȣ�������
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

    // ����/��ײ�ص�
    void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }


    void HandleHit(Collider2D other)
    {
        if (other == null) return;

        // �������Լ��㼶���Լ���ײ����ظ���⣨�ɸ�����Ŀ�㼶���ԸĽ���
        if (other.attachedRigidbody != null && other.attachedRigidbody == rb) return;
        
        // ���ȳ��Զ�Ŀ����� IDamageable �ӿ�

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
        //    // ���û�� IDamageable�����Ը��ݱ�ǩ�����������������
        //    // Example: if (other.CompareTag("Environment")) { ... }
        //}
        SpawnHitEffect(other.ClosestPoint(transform.position));


        // Ĭ������һ�ξͻ���
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
    /// �����ӵ���ͨ���ص������أ�
    /// </summary>
    public void ReturnToPool()
    {
        // ֹͣ�˶�
        rb.velocity = Vector2.zero;
        // ȡ���ص�ע�ᣨ���գ�
        OnReturnToPool?.Invoke(this);
    }

    void OnDisable()
    {
        // ���������������
        OnReturnToPool = null;
    }
}

