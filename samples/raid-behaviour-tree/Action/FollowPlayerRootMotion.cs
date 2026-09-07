// FollowPlayerRootMotion_Pure.cs
// Pure steering + root motion follower (no NavMeshAgent.SetDestination, no agent path following)
// - Uses Animator root motion to move (blend tree speed param)
// - Simple obstacle avoidance (spherecast + whiskers)
// - Manual NavMeshLink traversal by proximity (optional)
// Unity 2022.3+, AI Navigation 2.x (NavMeshLink), ECM2

using System;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation; // NavMeshLink (AI Navigation 2.x)
using ECM2;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    [Serializable]
    public sealed class FollowPlayerRootMotion : BTActionNode
    {
        // --- Target ---
        [SerializeField] private Transform _explicitTarget;

        // --- Arrival / steering ---
        [Header("Arrival")]
        [SerializeField] private float _stoppingDistance = 0.7f;   // success radius
        [SerializeField] private float _brakingDistance = 2.5f;    // forward param scales down when closer than this

        [Header("Steering")]
        [SerializeField] private float _turnSpeed = 540f;          // degrees/sec when not using root-rot
        [SerializeField] private bool  _applyRootRotation = true;  // if false → ECM2 rotation assist

        // --- Animator (RootMotion) ---
        [Header("Animator Params")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _pMoveForward = "MoveForward"; // 0..1 forward blend
        [SerializeField] private string _pMoveRight   = "MoveRight";   // optional strafe
        [SerializeField] private string _pIsMoving    = "IsMoving";
        [SerializeField] private string _pJumpTrig    = "Jump";
        [SerializeField] private string _pDropTrig    = "Drop";

        // --- Obstacle avoidance (simple) ---
        [Header("Avoidance (Whiskers)")]
        [SerializeField] private LayerMask _obstacleMask = ~0;     // which layers to avoid
        [SerializeField] private float _probeRadius = 0.25f;
        [SerializeField] private float _probeDistance = 1.5f;
        [SerializeField] private float _whiskerAngle = 30f;        // left/right probe from desired dir
        [SerializeField] private float _whiskerMultiplier = 0.8f;  // side probes are shorter
        [SerializeField] private float _probeYOffset = 0.3f;       // lift from feet to reduce ground hits

        // --- NavMeshLink traversal (optional) ---
        [Header("NavMeshLink Traversal")]
        [SerializeField] private float _linkActivationRadius = 0.6f;   // how close to entry to trigger
        [SerializeField] private float _linkExitSnapXZ = 0.35f;        // snap tolerance to finish traversal
        [SerializeField] private float _jumpUnconstrainTime = 0.15f;   // relax ground stick to start jump
        [SerializeField] private float _linkCacheInterval = 1.0f;

        // Runtime
        private Transform _target;
        private GameObject _owner;
        private ECM2.Character _ch;                // ECM2 facade
        private CharacterMovement _cm;        // ECM2 movement

        private int _hForward, _hRight, _hIsMoving, _hJump, _hDrop;
        private NavMeshLink[] _links = Array.Empty<NavMeshLink>();
        private float _linkCacheTimer;

        private bool _isTraversingLink;
        private NavMeshLink _activeLink;
        private Vector3 _entryWorld, _exitWorld;

        public override void OnInit(BTContext ctx)
        {
            _owner = ctx.Owner;
            _ch = _owner.GetComponent<ECM2.Character>();
            _cm = _owner.GetComponent<CharacterMovement>();
            if (_animator == null) _animator = _owner.GetComponentInChildren<Animator>();
            if (_animator != null) _animator.applyRootMotion = true;

            _hForward = string.IsNullOrEmpty(_pMoveForward) ? 0 : Animator.StringToHash(_pMoveForward);
            _hRight   = string.IsNullOrEmpty(_pMoveRight)   ? 0 : Animator.StringToHash(_pMoveRight);
            _hIsMoving= string.IsNullOrEmpty(_pIsMoving)    ? 0 : Animator.StringToHash(_pIsMoving);
            _hJump    = string.IsNullOrEmpty(_pJumpTrig)    ? 0 : Animator.StringToHash(_pJumpTrig);
            _hDrop    = string.IsNullOrEmpty(_pDropTrig)    ? 0 : Animator.StringToHash(_pDropTrig);

            _target = _explicitTarget;
            if (_target == null)
            {
                var npc = _owner.GetComponent<NPCController>();
                if (npc && npc.Player) _target = npc.Player.transform;
            }
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            if (_target == null || _animator == null || _ch == null || _cm == null)
                return BTStatus.Failure;

            // Cache links periodically
            _linkCacheTimer -= Time.deltaTime;
            if (_linkCacheTimer <= 0f)
            {
                _links = UnityEngine.Object.FindObjectsOfType<NavMeshLink>(includeInactive: false);
                _linkCacheTimer = _linkCacheInterval;
            }

            if (_isTraversingLink)
            {
                TraverseActiveLink();
                UpdateAnimatorMoveState();
                return BTStatus.Running;
            }

            Vector3 pos = _owner.transform.position;
            Vector3 tgt = _target.position;

            // Arrival
            float distXZ = HorizontalDistance(pos, tgt);
            if (distXZ <= _stoppingDistance)
            {
                UpdateAnimatorMoveState(0f);
                return BTStatus.Success;
            }

            // Desired move dir (seek)
            Vector3 desiredDir = (tgt - pos); desiredDir.y = 0f;
            if (desiredDir.sqrMagnitude < 1e-6f)
            {
                UpdateAnimatorMoveState(0f);
                return BTStatus.Running;
            }
            desiredDir.Normalize();

            // Avoidance
            Vector3 steerDir = AvoidObstacles(pos, desiredDir);

            // Rotate toward steer direction
            SteerTowards(steerDir);

            // Scale forward param for braking near target
            float forwardScale = Mathf.Clamp01((distXZ - _stoppingDistance) / Mathf.Max(0.001f, _brakingDistance));
            UpdateAnimatorMoveState(forwardScale);

            // Check NavMeshLink proximity (optional jump / drop)
            if (TryGetNearbyNavLink(pos, steerDir, out var link, out var entry, out var exit))
                BeginLinkTraversal(link, entry, exit);

            return BTStatus.Running;
        }

        public override BTNode Clone()
        {
            return new FollowPlayerRootMotion
            {
                _explicitTarget = _explicitTarget,
                _stoppingDistance = _stoppingDistance,
                _brakingDistance = _brakingDistance,
                _turnSpeed = _turnSpeed,
                _applyRootRotation = _applyRootRotation,
                _animator = _animator,
                _pMoveForward = _pMoveForward,
                _pMoveRight = _pMoveRight,
                _pIsMoving = _pIsMoving,
                _pJumpTrig = _pJumpTrig,
                _pDropTrig = _pDropTrig,
                _obstacleMask = _obstacleMask,
                _probeRadius = _probeRadius,
                _probeDistance = _probeDistance,
                _whiskerAngle = _whiskerAngle,
                _whiskerMultiplier = _whiskerMultiplier,
                _probeYOffset = _probeYOffset,
                _linkActivationRadius = _linkActivationRadius,
                _linkExitSnapXZ = _linkExitSnapXZ,
                _jumpUnconstrainTime = _jumpUnconstrainTime,
                _linkCacheInterval = _linkCacheInterval
            };
        }

        // --------------------- helpers ---------------------
        void SteerTowards(Vector3 dir)
        {
            if (_applyRootRotation)
            {
                // feed a signed strafe/turn hint if needed
                if (_hRight != 0)
                {
                    var fwd = _owner.transform.forward;
                    float signed = SignedAngleOnPlane(fwd, dir, Vector3.up) / 180f; // -1..1
                    _animator.SetFloat(_hRight, signed);
                }
            }
            else
            {
                _cm.RotateTowards(dir, _turnSpeed, updateYawOnly: true);
            }
        }

        void UpdateAnimatorMoveState(float forwardScale = 1f)
        {
            if (_hIsMoving != 0) _animator.SetBool(_hIsMoving, forwardScale > 0.01f);
            if (_hForward != 0)  _animator.SetFloat(_hForward, forwardScale);
            // keep lateral 0 unless you have strafe blends
            if (_hRight != 0 && _applyRootRotation == false) _animator.SetFloat(_hRight, 0f);
        }

        Vector3 AvoidObstacles(Vector3 origin, Vector3 desiredDir)
        {
            // primary probe straight ahead
            Vector3 probeOrigin = origin + Vector3.up * _probeYOffset;
            if (SphereProbe(probeOrigin, desiredDir, _probeDistance, out var hit))
            {
                // try whiskers (left / right)
                Vector3 leftDir = Quaternion.AngleAxis(-_whiskerAngle, Vector3.up) * desiredDir;
                Vector3 rightDir= Quaternion.AngleAxis(+_whiskerAngle, Vector3.up) * desiredDir;

                bool leftFree  = !SphereProbe(probeOrigin, leftDir,  _probeDistance * _whiskerMultiplier, out _);
                bool rightFree = !SphereProbe(probeOrigin, rightDir, _probeDistance * _whiskerMultiplier, out _);

                if (leftFree && rightFree)
                {
                    // choose the side whose normal faces away from obstacle more
                    float lDot = Vector3.Dot(leftDir, Vector3.Reflect(desiredDir, hit.normal));
                    float rDot = Vector3.Dot(rightDir, Vector3.Reflect(desiredDir, hit.normal));
                    return (lDot > rDot) ? leftDir : rightDir;
                }
                if (leftFree) return leftDir;
                if (rightFree) return rightDir;

                // fallback: slide along obstacle using reflection
                Vector3 reflect = Vector3.Reflect(desiredDir, hit.normal);
                reflect.y = 0f; if (reflect.sqrMagnitude < 1e-6f) return desiredDir;
                return reflect.normalized;
            }
            return desiredDir;
        }

        bool SphereProbe(Vector3 origin, Vector3 dir, float dist, out RaycastHit hit)
        {
            return Physics.SphereCast(origin, _probeRadius, dir, out hit, dist, _obstacleMask, QueryTriggerInteraction.Ignore);
        }

        // --- NavMeshLink handling (optional) ---
        bool TryGetNearbyNavLink(Vector3 pos, Vector3 steerDir, out NavMeshLink link, out Vector3 entry, out Vector3 exit)
        {
            link = null; entry = exit = default;
            if (_links == null || _links.Length == 0) return false;

            for (int i = 0; i < _links.Length; i++)
            {
                var l = _links[i];
                if (!l || !l.enabled || !l.gameObject.activeInHierarchy) continue;

                var s = l.transform.TransformPoint(l.startPoint);
                var e = l.transform.TransformPoint(l.endPoint);

                // choose nearest end as entry based on current steering direction
                Vector3 toS = s - pos; toS.y = 0f;
                Vector3 toE = e - pos; toE.y = 0f;

                Vector3 entryCandidate = (Vector3.Dot(toS.normalized, steerDir) > Vector3.Dot(toE.normalized, steerDir)) ? s : e;
                Vector3 exitCandidate  = (entryCandidate == s) ? e : s;

                if (HorizontalDistance(pos, entryCandidate) <= _linkActivationRadius)
                {
                    link = l; entry = entryCandidate; exit = exitCandidate; return true;
                }
            }
            return false;
        }

        void BeginLinkTraversal(NavMeshLink link, Vector3 entry, Vector3 exit)
        {
            _isTraversingLink = true;
            _activeLink = link; _entryWorld = entry; _exitWorld = exit;

            // face toward exit and trigger appropriate animation
            var dir = (_exitWorld - _owner.transform.position); dir.y = 0f;
            if (dir.sqrMagnitude > 1e-6f)
            {
                dir.Normalize();
                SteerTowards(dir);
            }

            bool goingUp = _exitWorld.y > _entryWorld.y + 0.1f;
            _ch.PauseGroundConstraint(_jumpUnconstrainTime * (goingUp ? 1f : 0.5f));

            var action = link.GetComponent<NavLinkAction>();
            if (action != null)
            {
                if (action.actionType == NavLinkAction.ActionType.Jump && _hJump != 0) _animator.SetTrigger(_hJump);
                else if (action.actionType == NavLinkAction.ActionType.Drop && _hDrop != 0) _animator.SetTrigger(_hDrop);
                else
                {
                    if (goingUp && _hJump != 0) _animator.SetTrigger(_hJump);
                    else if (!goingUp && _hDrop != 0) _animator.SetTrigger(_hDrop);
                }
            }
            else
            {
                if (goingUp && _hJump != 0) _animator.SetTrigger(_hJump);
                else if (!goingUp && _hDrop != 0) _animator.SetTrigger(_hDrop);
            }
        }

        void TraverseActiveLink()
        {
            // Keep orientation guided towards exit while RM plays
            var dir = (_exitWorld - _owner.transform.position); dir.y = 0f;
            if (dir.sqrMagnitude > 1e-6f)
            {
                dir.Normalize();
                SteerTowards(dir);
            }

            // finish when grounded and at exit
            var exitXZ = new Vector3(_exitWorld.x, _owner.transform.position.y, _exitWorld.z);
            bool nearExit = HorizontalDistance(_owner.transform.position, exitXZ) <= _linkExitSnapXZ;
            if (_cm.isGrounded && nearExit)
            {
                _isTraversingLink = false;
                _activeLink = null;
                UpdateAnimatorMoveState(0f);
            }
        }

        // --- math utils ---
        static float HorizontalDistance(Vector3 a, Vector3 b)
        { a.y = b.y; return Vector3.Distance(a, b); }

        static float SignedAngleOnPlane(Vector3 from, Vector3 to, Vector3 up)
        {
            var a = Vector3.ProjectOnPlane(from, up);
            var b = Vector3.ProjectOnPlane(to, up);
            return Vector3.SignedAngle(a, b, up);
        }
    }

    /// <summary>
    /// Optional helper for annotating links with a specific traversal animation type
    /// </summary>
    public sealed class NavLinkAction : MonoBehaviour
    {
        public enum ActionType { Auto = 0, Jump, Drop, Climb, Ladder }
        public ActionType actionType = ActionType.Auto;
    }
}
