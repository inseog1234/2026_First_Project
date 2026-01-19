using System;
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
        if (isDead)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            transform.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(transform.eulerAngles.z, 180f, Time.deltaTime * 5f));
            if (Mathf.Abs(180f - transform.eulerAngles.z) <= 70f)
            {
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, target.position.y - 8f, Time.deltaTime), transform.position.z);
            }
            
            return;
        }

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
        _rb.AddForce(dir * force, ForceMode2D.Impulse);
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

        if (_am != null)
        {
            _am.Rebind();
            _am.Update(0);
        }

        transform.rotation = Quaternion.Euler(0, 0, 0);
        gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    protected override void Die()
    {
        base.Die();

        // 투두(To do): 경험치 드랍
        // ExpOrbPool.Get().Init(transform.position, expValue);
    }
}
