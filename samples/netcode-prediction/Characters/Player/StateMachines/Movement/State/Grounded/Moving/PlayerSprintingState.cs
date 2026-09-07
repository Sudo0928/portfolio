using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GenshinImpactMovementSystem
{
    public class PlayerSprintingState : PlayerMovingState
    {
        private PlayerSprintData sprintData;

        private float startTime;

        private bool keepSprinting;

        private bool shouldResetSprintState;

        public PlayerSprintingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
            sprintData = movementData.SprintData;
        }

        #region IState Methods
        public override void Enter()
        {
            stateMachine.ReusableData.MovementSpeedModifier = sprintData.SpeedModifier;

            base.Enter();

            StartAnimation(stateMachine.Player.AnimationData.SprintParameterHash);

            stateMachine.ReusableData.CurrentJumpForce = airborneData.JumpData.StrongForce;

            shouldResetSprintState = true;

            startTime = Time.time;
        }

        public override void Exit()
        {
            base.Exit();

            StopAnimation(stateMachine.Player.AnimationData.SprintParameterHash);

            if (shouldResetSprintState)
            {
                keepSprinting = false;

                stateMachine.ReusableData.ShouldSprint = false;
            }
        }

        public override void Update()
        {
            base.Update();

            if (keepSprinting) return;

            if (Time.time < startTime + sprintData.SprintToRunTime) return;

            StopSprinting();
        }
        #endregion

        #region Main Methods
        private void StopSprinting()
        {
            if(stateMachine.ReusableData.MovementInput == Vector2.zero)
            {
                stateMachine.ChangeState(stateMachine.IdlingState);

                return;
            }

            stateMachine.ChangeState(stateMachine.RunningState);
        }
        #endregion

        #region Reusable Methods
        protected override void AddInputActionsCallbacks()
        {
            base.AddInputActionsCallbacks();

            Managers.Input.SprintInput.performed += OnSprintPerformed;
        }

        protected override void RemoveInputActionsCallbacks()
        {
            base.RemoveInputActionsCallbacks();

            Managers.Input.SprintInput.performed -= OnSprintPerformed;
        }

        protected override void OnFall()
        {
            shouldResetSprintState = false;

            base.OnFall();
        }
        #endregion

        #region Input Methods
        protected override void OnMovementCanceled(Vector2 movementInput)
        {
            stateMachine.ChangeState(stateMachine.HardStoppingState);

            base.OnMovementCanceled(movementInput);
        }

        protected override void OnJumpStarted(bool pressed)
        {
            shouldResetSprintState = false;

            base.OnJumpStarted(pressed);
        }

        private void OnSprintPerformed(bool pressed)
        {
            keepSprinting = true;

            stateMachine.ReusableData.ShouldSprint = true;
        }
        #endregion
    }
}
