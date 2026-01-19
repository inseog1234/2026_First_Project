using System;
using UnityEngine;

namespace PlayerControll
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Player_Input))]
    [RequireComponent(typeof(SkillController))]

    public class Player : Unit
    {
        [Header("플레이어 세팅")]
        [SerializeField] private float First_Max_Exp = 4;
        [SerializeField] private PlayerUI _ui;

        private Player_Input _input;
        private SkillController _skillController;
        

        public float curExp {get; private set;}
        public float maxExp {get; private set;}
        public int Level {get; private set;}

        protected override void Start()
        {
            _input = GetComponent<Player_Input>();
            _skillController = GetComponent<SkillController>();

            base.Start();

            _skillController.Set_Offset(offsetProperty);
            Level = 1;
            
            maxExp = First_Max_Exp;
            
            _ui.SetHP(Hp, MaxHp);
            _ui.SetExp(curExp * 1.0f, maxExp * 1.0f);
        }

        protected override void Update()
        {
            base.Update();
            Set_MoveDirect();
        }

        public void AddExp(float v)
        {
            curExp += v;
            if (curExp >= maxExp)
            {
                LevelUp();
            }

            _ui.SetExp(curExp * 1.0f, maxExp * 1.0f);
            _ui.SetLevel(Level);
        }

        void LevelUp()
        {
            curExp = 0;
            maxExp *= 1.2f;
            Level++;
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
            _ui.SetHP(Hp, MaxHp);
        }
    }
}

