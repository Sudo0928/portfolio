using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GenshinImpactMovementSystem
{
    public class PlayerStoppingState : PlayerGroundedState
    {
        public PlayerStoppingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        #region IState Methods
        public override void Enter()
        {
            stateMachine.ReusableData.MovementSpeedModifier = 0f;

            SetBaseCameraRecenteringData();

            base.Enter();

            StartAnimation(stateMachine.Player.AnimationData.StoppingParameterHash);
        }

        public override void Exit()
        {
            base.Exit();

            StopAnimation(stateMachine.Player.AnimationData.StoppingParameterHash);
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            RotateTowardsTargetRotatoin();

            if (!IsMovingHorizontally()) return;

            DecelerateHorizontally();
        }

        public override void OnAnimationTransitionEvent()
        {
            stateMachine.ChangeState(stateMachine.IdlingState);
        }
        #endregion

        #region Reusable Methods
        protected override void AddInputActionsCallbacks()
        {
            base.AddInputActionsCallbacks();

            Managers.Input.MovementInput.started += OnMovementStarted;
        }

        protected override void RemoveInputActionsCallbacks()
        {
            base.RemoveInputActionsCallbacks();

            Managers.Input.MovementInput.started -= OnMovementStarted;
        }
        #endregion

        #region Input Methods
        private void OnMovementStarted(Vector2 movementInput)
        {
            OnMove();
        }
        #endregion
    }
}