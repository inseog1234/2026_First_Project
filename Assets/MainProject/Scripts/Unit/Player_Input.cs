using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerControll
{
    public class Player_Input : MonoBehaviour
    {
        // 캐릭터 입력 값
        public Vector2 move { get; private set; }
        public Vector2 mouseDirection { get; private set; }
        public bool sprint { get; private set; }
        public bool attack { get; private set; }
        public bool interaction { get; private set; }
        
        // UI 입력 값
        public bool cancle { get; private set; }


        private void Update()
        {
            MousePositionUpdate();
        }

        //   Player   ////////////////////////////////////

        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        public void OnAttack(InputValue value)
        {
            AttackInput(value.isPressed);
        }

        public void OnInteraction(InputValue value)
        {
            InteractionInput(value.isPressed);
        }

        //   UI   ////////////////////////////////////

        public void OnCancle(InputValue value)
        {
            CancelInput(value.isPressed);
        }

        public void MousePositionUpdate()
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
            mouseWorld.z = 0f;

            mouseDirection = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
        }


        /// <summary>
        /// ///////////////////////////////////////////
        /// </summary>

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection.normalized;
        } 

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void AttackInput(bool newAttackState)
        {
            attack = newAttackState;
        }

        public void InteractionInput(bool newInteraction)
        {
            interaction = newInteraction;
        }

        public void CancelInput(bool newCancle)
        {
            cancle = newCancle;
        }
    }
}
