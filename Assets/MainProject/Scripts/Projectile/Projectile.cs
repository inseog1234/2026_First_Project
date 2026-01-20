using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    private bool ignoreHitReturn;

    // 같은 적 한 번만 타격
    // private HashSet<int> hitSet;

    private ActiveSkill ownerSkill;

    private Vector2 dir;
    private float speed;
    private float damage;
    private float lifeTime;
    private float knockback;
    private int pierce;

    private bool isActive;

    public void Init(
        Vector2 dir, float damage, float speed, float lifeTime, float knockback,
        ActiveSkill skill, int pierce = 0, bool rotate = true, bool ignoreHitReturn = false
    )
    {
        this.dir = dir.normalized;
        this.damage = damage;
        this.speed = speed;
        this.lifeTime = lifeTime;
        this.pierce = pierce;
        this.knockback = knockback;
        this.ownerSkill = skill;
        this.ignoreHitReturn = ignoreHitReturn;

        // 같은 적 한 번만 타격
        // hitSet ??= new HashSet<int>();
        // hitSet.Clear();

        if (rotate && dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }

        isActive = true;
        gameObject.SetActive(true);
    }


    private void Update()
    {
        if (!isActive) return;

        transform.position += (Vector3)(dir * speed * Time.deltaTime);

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy.isDead) return;

        // 같은 적 한 번만 타격
        // int id = other.GetInstanceID();
        // if (!hitSet.Add(id)) return;

        enemy.TakeDamage(damage);
        ownerSkill?.AddDamage(damage);
        enemy.Knockback(dir, knockback);

        if (knockback > 0f)
        {
            Vector2 kbDir = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
            enemy.Knockback(kbDir, knockback);
        }

        if (ignoreHitReturn) return;
        
        if (pierce > 0)
        {
            pierce--;
        }
        else
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        isActive = false;
        ownerSkill = null;
        ProjectilePooling.Instance.Return(this);
    }
}
