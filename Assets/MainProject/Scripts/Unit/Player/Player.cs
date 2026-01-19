using System;
using UnityEngine;

namespace PlayerControll
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Player_Input))]
    [RequireComponent(typeof(SkillController))]

    public class Player : Unit
    {
        [SerializeField] private PlayerHPBar hpBar;

        private Player_Input _input;
        private SkillController _skillController;

        protected override void Start()
        {
            _input = GetComponent<Player_Input>();
            _skillController = GetComponent<SkillController>();

            base.Start();

            _skillController.Set_Offset(offsetProperty);
            hpBar.SetHP(Hp, MaxHp);
        }

        protected override void Update()
        {
            base.Update();
            Set_MoveDirect();
        }

        private void Set_MoveDirect()
        {
            if (_MoveDirect != _input.move)
            {
                _MoveDirect = _input.move;
            }
            
            if (_MoveDirect.x != 0)
            {
                _skillController.Set_Offset(offsetProperty);
            }
        }

        protected override void OnHpChanged()
        {
            hpBar.SetHP(Hp, MaxHp);
        }

    }
}

