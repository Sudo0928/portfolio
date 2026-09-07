using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GenshinImpactMovementSystem
{
    public class PlayerWalkingState : PlayerMovingState
    {
        private PlayerWalkData walkData;

        public PlayerWalkingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
            walkData = movementData.WalkData;
        }

        #region IState Methods
        public override void Enter()
        {
            stateMachine.ReusableData.MovementSpeedModifier = movementData.WalkData.SpeedModifier;

            stateMachine.ReusableData.BackwardsCameraRecenteringData = walkData.BackwardsCameraRecenteringData;

            base.Enter();

            StartAnimation(stateMachine.Player.AnimationData.WalkParameterHash);

            stateMachine.ReusableData.CurrentJumpForce = airborneData.JumpData.WeakForce;
        }

        public override void Exit()
        {
            base.Exit();

            StopAnimation(stateMachine.Player.AnimationData.WalkParameterHash);

            SetBaseCameraRecenteringData();
        }
        #endregion

        #region Input Methods
        protected override void OnMovementCanceled(Vector2 movementInput)
        {
            stateMachine.ChangeState(stateMachine.LightStoppingState);

            base.OnMovementCanceled(movementInput);
        }

        protected override void OnWalkToggleStarted(bool pressed)
        {
            base.OnWalkToggleStarted(pressed);

            stateMachine.ChangeState(stateMachine.RunningState);
        }
        #endregion
    }
}
