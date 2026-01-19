using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Enemy : Unit
{
    private Transform target;
    private Vector2 target_Offset;

    [Header("경험치")]
    public int expValue = 1;

    [Header("데미지 세팅")]
    public float Damage;
    public float Damage_Interval; // 1초마다

    private float DamageTimer;
    private bool isTouchingPlayer;

    protected override void Start()
    {
        base.Start();
        target = GameObject.FindWithTag("Player").transform;
        target_Offset = target.GetComponent<Unit>().Get_Offset();
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

        StopAllCoroutines(); // 기존 연출 중지 (안전)
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
        _MoveDirect = Vector2.zero;
        gameObject.layer = LayerMask.NameToLayer("Default");
        ExpOrbPooling.Instance.DropExp(transform.position, expValue);
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        float t = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(transform.position.x, target.position.y - 8f, transform.position.z);

        float startRot = transform.eulerAngles.z;
        float endRot = 180f;

        while (t < 1f)
        {
            t += Time.deltaTime;

            float rot = Mathf.LerpAngle(startRot, endRot, t);
            transform.rotation = Quaternion.Euler(0, 0, rot);

            transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        // 경험치 드랍

        // 풀 반환
        EnemyPooling.Instance.Return(this);
    }

}
