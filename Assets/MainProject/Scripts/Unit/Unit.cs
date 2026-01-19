using UnityEngine;

public class Unit : MonoBehaviour
{
    protected Rigidbody2D _rb;
    protected SpriteRenderer _sr;
    protected Animator _am;
    protected Vector2 _MoveDirect;
    protected Vector2 _AtkDirect;
    protected bool isKnockbacking;


    [SerializeField] protected float Hp;

    [Header("Unit Param Settings")]
    [SerializeField] protected float _MoveSpeed;
    [SerializeField] protected float MaxHp;
    [SerializeField] protected Vector2 offset;
    [SerializeField] protected Vector2 offsetProperty;

    protected float _AttackCoolTime_Timer;
    protected bool _isAttackPossible;
    
    public bool isDead {get; protected set;}
    private float hitValue;

    protected virtual void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _am = GetComponentInChildren<Animator>();

        offsetProperty = offset;

        Hp = MaxHp;
    }

    protected virtual void Update() 
    {
        UpdateAnimation();
        UpdateLookDirection();
    }

    protected void UpdateAnimation()
    {
        bool isMoving = _MoveDirect.sqrMagnitude > 0.01f;
        _am.SetBool("isMove", isMoving);
        

        hitValue = Mathf.MoveTowards(hitValue, 0f, Time.deltaTime * 3f);
        _sr.material.SetFloat("_Value", hitValue);
        _sr.material.SetTexture("_Texture2D", _sr.sprite.texture);

    }

    protected virtual void Move()
    {
        if (isKnockbacking) return;

        _rb.MovePosition(
            _rb.position + _MoveDirect.normalized * _MoveSpeed * Time.fixedDeltaTime * 5f
        );
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y);
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        Move();
    }

    private void UpdateLookDirection()
    {
        if (_MoveDirect.x == 0) return;

        _sr.flipX = _MoveDirect.x < 0;
        offsetProperty = _sr.flipX ? new Vector2(-offset.x, offset.y) : offset;
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        Hp -= damage;
        _am.SetTrigger("hit");
        hitValue = 0.8f;

        OnHpChanged();

        if (Hp <= 0)
        {
            Die();
        }
    }

    protected virtual void OnHpChanged() {}

    protected virtual void Die()
    {
        isDead = true;
        _am.SetTrigger("die");
        Invoke(nameof(ReturnToPool), 5f);
    }

    protected virtual void ReturnToPool()
    {
        EnemyPooling.Instance.Return(this as Enemy);
    }

    public Vector2 Get_Offset() => offsetProperty;

}
