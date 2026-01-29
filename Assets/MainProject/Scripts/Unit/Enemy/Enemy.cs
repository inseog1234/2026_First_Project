using System;
using System.Collections;
using UnityEngine;


public class Enemy : Unit
{
    private Transform target;
    private Vector2 target_Offset;

    public Enemy prefabKey { get; private set; }
    public void SetPrefabKey(Enemy key) => prefabKey = key;

    [Header("경험치")]
    public int expValue = 1;

    [Header("데미지 세팅")]
    public float Damage;
    public float Damage_Interval;

    private float DamageTimer;
    private bool isTouchingPlayer;

    protected override void Start()
    {
        base.Start();
        target = GameObject.FindWithTag("Player").transform;
        target_Offset = target.GetComponent<Unit>().Get_Offset();
    }

    private void OnEnable()
    {
        EnemyManager.Register(this);
    }

    private void OnDisable()
    {
        EnemyManager.Unregister(this);
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        SetMoveDirection();
        CheckContactDamage();
    }


    private void SetMoveDirection()
    {
        if (target == null) return;

        _MoveDirect = (target.position + new Vector3(0, target_Offset.y) - transform.position).normalized;
    }

    public void Knockback(Vector2 dir, float force)
    {
        if (isDead) return;

        StopAllCoroutines(); // 기존 연출 중지
        StartCoroutine(KnockbackRoutine(dir, force));
    }

    private IEnumerator KnockbackRoutine(Vector2 dir, float force)
    {
        isKnockbacking = true;

        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(dir.normalized * (force * 5f), ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.15f); // 넉백 유지 시간

        _rb.linearVelocity = Vector2.zero;
        isKnockbacking = false;
    }


    private void CheckContactDamage()
    {
        if (!isTouchingPlayer) return;

        DamageTimer -= Time.deltaTime;
        if (DamageTimer <= 0f)
        {
            target.GetComponent<Unit>().TakeDamage(Damage);
            DamageTimer = Damage_Interval;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = true;
            DamageTimer = 0f;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = false;
        }
    }

    public void ResetState()
    {
        isDead = false;
        Hp = MaxHp;
        _MoveDirect = Vector2.zero;

        transform.rotation = Quaternion.identity;
        gameObject.layer = LayerMask.NameToLayer("Enemy");

        if (_am != null)
        {
            _am.Rebind();
            _am.Update(0);
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;
        KillManager.Instance.AddKill();
        _MoveDirect = Vector2.zero;
        gameObject.layer = LayerMask.NameToLayer("Default");
        ExpOrbPooling.Instance.DropExp(transform.position, 1, expValue);
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (_am != null)
            _am.enabled = false;

        float duration = 0.5f;
        float t = 0f;

        Vector3 startScale = transform.localScale;
        float rotateSpeed = 720f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float rate = t / duration;

            transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, rate);

            yield return null;
        }

        transform.localScale = startScale;
        transform.rotation = Quaternion.identity;
        if (_am != null)
            _am.enabled = true;

        EnemyPooling.Instance.Return(this);
    }


}
