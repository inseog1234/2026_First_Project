using System;
using UnityEngine;

namespace PlayerControll
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Player_Input))]
    [RequireComponent(typeof(SkillController))]

    public class Player : Unit
    {
        private Player_Input _input;
        private SkillController _skillController;

        protected override void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _input = GetComponent<Player_Input>();
            _skillController = 
            
            base.Start();
        }

        protected override void Update()
        {
            base.Update();

            PlayerLookRotation();
            Set_MoveDirect();
        }

        private void PlayerLookRotation()
        {
            Debug.Log(_input.mouseDirection.x >= 0);
        }

        private void Set_MoveDirect()
        {
            if (_MoveDirect != _input.move)
            {
                _MoveDirect = _input.move;
            }
        }

        protected override void Attack()
        {
            if (_input.attack)
            {
                if (_isAttackPossible)
                {
                    Debug.Log("공격!");
                }
                
                _input.AttackInput(false);
            }
        }
    }
}

