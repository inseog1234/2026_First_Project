using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Player Component Settings")]
    [SerializeField] protected Rigidbody2D _rb;

    protected Vector2 _MoveDirect;
    protected Vector2 _AtkDirect;
    protected float Hp;

    [Header("Unit Param Settings")]
    [SerializeField] protected float _MoveSpeed;
    [SerializeField] protected float _AttackCoolTime;
    [SerializeField] protected float MaxHp;
    [SerializeField] protected float ATK;

    protected float _AttackCoolTime_Timer;
    protected bool _isAttackPossible;
    

    protected virtual void Start()
    {
        Hp = MaxHp;
    }

    protected virtual void Update() {
        
    }

    protected virtual void Move()
    {
        _rb.MovePosition(_rb.position + _MoveDirect.normalized * _MoveSpeed * (Time.fixedDeltaTime * 5));
    }
   private void FixedUpdate()
    {
        Move();
    }


    private void CheckAttackCoolTime()
    {
        if (!_isAttackPossible)
        {
            if (_AttackCoolTime_Timer < _AttackCoolTime)
            {
                _AttackCoolTime_Timer += Time.deltaTime;
            }
            
            _isAttackPossible = true;
        }
    }

    protected virtual void Attack()
    {
        
    }
}
