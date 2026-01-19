using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 dir;
    private float speed;
    private float damage;
    private float lifeTime;
    private float knockback;
    private int pierce;

    private bool isActive;

    public void Init(Vector2 dir, float damage, float speed, float lifeTime, float knockback, int pierce = 0)
    {
        this.dir = dir.normalized;
        this.damage = damage;
        this.speed = speed;
        this.lifeTime = lifeTime;
        this.pierce = pierce;
        this.knockback = knockback;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

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
        
        enemy.TakeDamage(damage);
        enemy.Knockback(dir, knockback);

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
        ProjectilePooling.Instance.Return(this);
    }
}
