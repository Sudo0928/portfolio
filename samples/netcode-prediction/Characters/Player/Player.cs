using System;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

namespace GenshinImpactMovementSystem
{
    public class Player : NetworkBehaviour
    {
        [field: Header("References")]
        [field: SerializeField] public PlayerSO Data { get; private set; }

        [field: Header("Collisions")]
        [field: SerializeField] public PlayerCapsuleColliderUtility ColliderUtility { get; private set; }
        [field: SerializeField] public PlayerLayerData LayerData { get; private set; }

        [field: Header("Camera")]
        [field: SerializeField] public PlayerCameraUtility CameraUtility { get; private set; }

        [field: Header("Animations")]
        [field: SerializeField] public PlayerAnimationData AnimationData { get; private set; }
        
        public Rigidbody Rigidbody {  get; private set; }
        public Animator Animator { get; private set; }
        public Transform MainCameraTransform { get; private set; }

        private PlayerMovementStateMachine movementStateMachine;

        public override void OnStartNetwork()
        {
            Rigidbody = GetComponent<Rigidbody>();

            Init();

            TimeManager.OnTick += TimeManager_OnTick;
            TimeManager.OnPostTick += TimeManager_OnPostTick;
        }

        private void TimeManager_OnTick()
        {
            if(IsOwner)
            {
                ReplicateData data = new ReplicateData(Managers.Input.CurrentMovementInput, Managers.Input.CurrentOnWalkToggle, Managers.Input.CurrentDash, Managers.Input.CurrentSprint, Managers.Input.CurrentJump, Managers.Input.CurrentYawInput);
    
                Replicate(data);
            }
            else
            {
                Replicate(default(ReplicateData));
            }
        }

        private void TimeManager_OnPostTick()
        {
            CreateReconcile();
        }

        protected override void OnValidate()
        {
            ColliderUtility.Initialize(gameObject);
            ColliderUtility.CalculateCapsuleColliderDimensions();
        }

        public override void OnStartClient()
        {
            CameraUtility.Initialize();
            Init();
        }

        public override void OnStartServer()
        {
            CameraUtility.Initialize();
            Init();
        }

        private void Init()
        {
            if(movementStateMachine != null) return;

            MainCameraTransform = Camera.main?.transform;
            if (MainCameraTransform == null)
            {
                Debug.LogWarning("Main camera not found during Player initialization");
            }

            Animator = GetComponentInChildren<Animator>();

            ColliderUtility.Initialize(gameObject);
            ColliderUtility.CalculateCapsuleColliderDimensions();
            AnimationData.Initialize();

            movementStateMachine = new PlayerMovementStateMachine(this);
            movementStateMachine.ChangeState(movementStateMachine.IdlingState);
        }

        private void OnTriggerEnter(Collider collider)
        {
            movementStateMachine.OnTriggerEnter(collider);
        }

        private void OnTriggerExit(Collider collider)
        {
            movementStateMachine.OnTriggerExit(collider);
        }

        public void OnMovementStateAnimationEnterEvent()
        {
            movementStateMachine.OnAnimationEnterEvent();
        }

        public void OnMovementStateAnimationExitEvent()
        {
            movementStateMachine.OnAnimationExitEvent();
        }

        public void OnMovementStateAnimationTransitionEvent()
        {
            movementStateMachine.OnAnimationTransitionEvent();
        }

        [Replicate]
        private void Replicate(ReplicateData data, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
            float tickDelta = (float)TimeManager.TickDelta;
    
            data.MovementInput.Normalize();
            
            Managers.Input.MovementInput.Value = data.MovementInput;
            Managers.Input.WalkToggleInput.Value = data.OnWalkToggle;
            Managers.Input.DashInput.Value = data.Dash;
            Managers.Input.SprintInput.Value = data.Sprint;
            Managers.Input.JumpInput.Value = data.Jump;
            Managers.Input.YawInput.Value = data.YawInput;

            movementStateMachine.PhysicsUpdate();
            movementStateMachine.Update();
        }
    
        public override void CreateReconcile()
        {
            transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
    
            ReconcileData data = new ReconcileData(position, rotation, Rigidbody.velocity);
    
            Reconcile(data);
        }
    
        [Reconcile]
        private void Reconcile(ReconcileData data, Channel channel = Channel.Unreliable)
        {
            transform.SetPositionAndRotation(data.Position, data.Rotation);
    
            Rigidbody.velocity = data.Velocity;
        }
    }

    public struct ReplicateData : IReplicateData
    {
        private uint _tick;
        public readonly Vector2 MovementInput;
        public readonly bool OnWalkToggle;
        public readonly bool Dash;
        public readonly bool Sprint;
        public readonly bool Jump;
        public readonly float YawInput;

        public ReplicateData(Vector2 movementInput, bool walkToggle, bool dash, bool sprint, bool jump, float yawInput) : this()
        {
            MovementInput = movementInput;
            OnWalkToggle = walkToggle;
            Dash = dash;
            Sprint = sprint;
            Jump = jump;
            YawInput = yawInput;
        }

        public uint GetTick() => _tick;

        public void SetTick(uint value) => _tick = value;

        public void Dispose()
        {
            // Used internally by Fish-Networking
        }
    }

    public struct ReconcileData : IReconcileData
    {
        private uint _tick;
        
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Velocity;

        public ReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity) : this()
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
        }
        public uint GetTick() => _tick;

        public void SetTick(uint value) => _tick = value;

        public void Dispose()
        {
            // Used internally by Fish-Networking
        }
    }
}
