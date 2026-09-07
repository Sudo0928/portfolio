using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GenshinImpactMovementSystem
{
    public class PlayerHardLandingState : PlayerLandingState
    {
        public PlayerHardLandingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        #region IState Methods
        public override void Enter()
        {
            stateMachine.ReusableData.MovementSpeedModifier = 0f;

            base.Enter();

            StartAnimation(stateMachine.Player.AnimationData.HardLandParameterHash);

            Managers.Input.MovementInput.Disable();

            ResetVelocity();
        }

        public override void Exit()
        {
            base.Exit();

            StopAnimation(stateMachine.Player.AnimationData.HardLandParameterHash);

            Managers.Input.MovementInput.Enable();
        }

        public override void OnAnimationExitEvent()
        {
            Managers.Input.MovementInput.Enable();
        }

        public override void OnAnimationTransitionEvent()
        {
            stateMachine.ChangeState(stateMachine.IdlingState);
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            if (!IsMovingHorizontally()) return;

            ResetVelocity();
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

        protected override void OnMove()
        {
            if (stateMachine.ReusableData.ShouldWalk) return;

            stateMachine.ChangeState(stateMachine.RunningState);
        }
        #endregion

        #region Input Methods
        protected override void OnJumpStarted(bool pressed)
        {
        }

        private void OnMovementStarted(Vector2 movementInput)
        {
            OnMove();
        }
        #endregion
    }
}
