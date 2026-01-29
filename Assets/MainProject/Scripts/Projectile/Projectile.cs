using UnityEngine;

public class Projectile : MonoBehaviour
{
    private SkillProjectilePool ownerPool;
    private ActiveSkill ownerSkill;

    private Vector2 dir;
    private float speed;
    private float damage;
    private float lifeTime;
    private float knockback;
    private int pierce;
    private bool ignoreHitReturn;
    private bool isActive;

    public void Init(Vector2 dir, float damage, float speed, float lifeTime, float knockback, ActiveSkill skill, 
    SkillProjectilePool pool, int pierce = 0, bool rotate = true, bool ignoreHitReturn = false)
    {
        this.dir = dir.normalized;
        this.damage = damage;
        this.speed = speed;
        this.lifeTime = lifeTime;
        this.knockback = knockback;
        this.pierce = pierce;
        this.ownerSkill = skill;
        this.ownerPool = pool;
        this.ignoreHitReturn = ignoreHitReturn;

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

        transform.position += (Vector3)(dir * (speed * Time.deltaTime));

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
            ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null || enemy.isDead) return;

        enemy.TakeDamage(damage);
        ownerSkill?.AddDamage(damage);

        if (knockback > 0f)
        {
            Vector2 kbDir =
                ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
            enemy.Knockback(kbDir, knockback);
        }

        if (ignoreHitReturn) return;

        if (pierce > 0)
            pierce--;
        else
            ReturnToPool();
    }

    private void ResetState()
    {
        isActive = false;
        ownerSkill = null;
    }

    private void ReturnToPool()
    {
        if (!isActive) return;

        ResetState();
        ownerPool.Return(this);
    }
}
