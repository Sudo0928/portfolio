using System.Collections.Generic;
using ProjectRaid.Combat;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    /// <summary>
    /// 선딜(Startup) → 시전(Action) → 후딜(Recovery) 3단계 공격 액션.
    /// - 각 위상은 단 한 번만 CrossFade 호출.
    /// - 진행률 매칭: 풀경로 이름/해시 + (옵션) 태그 감지.
    /// - Action 위상에서 AnimationCurve(0..1)로 히트 ON/OFF 윈도우 제어.
    /// - 새 히트창이 열릴 때(커브 0→1) 동일 대상도 다시 히트 가능(옵션).
    /// - 외부 전이를 막기 위한 Animator Bool(예: "IsAttacking") 제공.
    /// - 타임아웃 기본 0(미사용). 필요 시에만 사용.
    /// </summary>
    public sealed class Attack : BTActionNode
    {
        public enum HitColliderType { Object, Box, Capsule, Sphere }
        public enum CapsuleDirection { YAxis, XAxis, ZAxis }
        private enum Phase { None, Startup, Action, Recovery, Done }

        [Title("General")]
        [FoldoutGroup("General"), LabelText("Layer")] [SerializeField] private int _layer = 0;

        [FoldoutGroup("General"), LabelText("IsAttacking Param"), Tooltip("공격 중 외부 전이를 막기 위한 Animator Bool")]
        [SerializeField] private string _pIsAttacking = "IsAttacking";

        [FoldoutGroup("General"), LabelText("Use Tag Detection"), Tooltip("스테이트 매칭을 태그로도 허용")]
        [SerializeField] private bool _useTagDetection = false;

        [FoldoutGroup("General"), ShowIf(nameof(_useTagDetection)), LabelText("Startup Tag")]
        [SerializeField] private string _startupTag = "Startup";
        [FoldoutGroup("General"), ShowIf(nameof(_useTagDetection)), LabelText("Action Tag")]
        [SerializeField] private string _actionTag = "AttackActive";
        [FoldoutGroup("General"), ShowIf(nameof(_useTagDetection)), LabelText("Recovery Tag")]
        [SerializeField] private string _recoveryTag = "Recovery";

        [FoldoutGroup("General"), LabelText("Phase Timeout (sec)"), Tooltip("0이면 미사용. 매칭 실패시 안전 탈출용")]
        [SerializeField, Min(0f)] private float _phaseTimeout = 0f;

        [Title("Startup")]
        [FoldoutGroup("Startup"), LabelText("Use Startup")] [SerializeField] private bool _useAttackDelay = false;
        [FoldoutGroup("Startup"), ShowIf(nameof(_useAttackDelay)), LabelText("State (Full Path)")]
        [SerializeField] private string _startUpStateName;
        [FoldoutGroup("Startup"), ShowIf(nameof(_useAttackDelay)), LabelText("Fade")]
        [SerializeField] private float _startUpCrossFadeDuration = 0.1f;
        [FoldoutGroup("Startup"), ShowIf(nameof(_useAttackDelay)), LabelText("Exit @")]
        [SerializeField, Range(0f, 1f)] private float _startUpExitNormalized = 0.98f;

        [Title("Action")]
        [FoldoutGroup("Action"), LabelText("State (Full Path)")]
        [SerializeField] private string _actionStateName;
        [FoldoutGroup("Action"), LabelText("Fade")]
        [SerializeField] private float _actionCrossFadeDuration = 0.1f;
        [FoldoutGroup("Action"), LabelText("Exit @")]
        [SerializeField, Range(0f, 1f)] private float _actionExitNormalized = 1f;

        [FoldoutGroup("Action"), LabelText("Hit Point Curve"), Tooltip("x: 0..1(정규 프레임), y: 0..1(히트 ON)")]
        [SerializeField] private AnimationCurve _hitPointCurve = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.45f, 0f),
            new Keyframe(0.45f, 1f), new Keyframe(0.65f, 1f),
            new Keyframe(0.65f, 0f), new Keyframe(1f, 0f));

        [FoldoutGroup("Action"), Tooltip("히트창이 닫혔다가(1→0) 다시 열리면(0→1) 동일 대상 재히트 허용")]
        [SerializeField] private bool _rehitOnNewWindow = true;

        [Title("Recovery")]
        [FoldoutGroup("Recovery"), LabelText("Use Recovery")] [SerializeField] private bool _useRecoveryDelay = false;
        [FoldoutGroup("Recovery"), ShowIf(nameof(_useRecoveryDelay)), LabelText("State (Full Path)")]
        [SerializeField] private string _recoveryStateName;
        [FoldoutGroup("Recovery"), ShowIf(nameof(_useRecoveryDelay)), LabelText("Fade")]
        [SerializeField] private float _recoveryCrossFadeDuration = 0.1f;
        [FoldoutGroup("Recovery"), ShowIf(nameof(_useRecoveryDelay)), LabelText("Exit @")]
        [SerializeField, Range(0f, 1f)] private float _recoveryExitNormalized = 0.98f;

        [Title("Hit Collider")]
        [FoldoutGroup("Hit Collider"), LabelText("Type")]
        [SerializeField] private HitColliderType _hitColliderType;

        [FoldoutGroup("Hit Collider"), ShowIf("@_hitColliderType == HitColliderType.Object")]
        [SerializeField] private Collider _collider;

        [FoldoutGroup("Hit Collider"), HideIf("@_hitColliderType == HitColliderType.Object"), LabelText("Use Gizmos")]
        [SerializeField] private bool _useGizmos = true;
        [FoldoutGroup("Hit Collider"), HideIf("@_hitColliderType == HitColliderType.Object"), LabelText("Pivot (Local)")]
        [SerializeField] private Vector3 _pivotPoint = Vector3.up;

        [FoldoutGroup("Hit Collider"), ShowIf("@_hitColliderType == HitColliderType.Box"), LabelText("Box Size")]
        [SerializeField] private Vector3 _boxSize = new Vector3(1f, 1f, 1f);

        [FoldoutGroup("Hit Collider"), ShowIf("@_hitColliderType == HitColliderType.Capsule"), LabelText("Radius")]
        [SerializeField] private float _capsuleRadius = 0.5f;
        [FoldoutGroup("Hit Collider"), ShowIf("@_hitColliderType == HitColliderType.Capsule"), LabelText("Height")]
        [SerializeField] private float _capsuleHeight = 2f;
        [FoldoutGroup("Hit Collider"), ShowIf("@_hitColliderType == HitColliderType.Capsule"), LabelText("Axis")]
        [SerializeField] private CapsuleDirection _capsuleDirection = CapsuleDirection.YAxis;

        [FoldoutGroup("Hit Collider"), ShowIf("@_hitColliderType == HitColliderType.Sphere"), LabelText("Radius")]
        [SerializeField] private float _sphereRadius = 1f;

        [FoldoutGroup("Hit Collider"), LabelText("Hit Mask")]
        [SerializeField] private LayerMask _hitMask = ~0;
        [FoldoutGroup("Hit Collider"), LabelText("Damage Info")]
        [SerializeField] private DamageInfo _damageInfo;

        [FoldoutGroup("Hit Collider"), LabelText("Prevent Multiple Hits")]
        [SerializeField] private bool _preventMultipleHitsPerAction = true;

        // -------- Runtime --------
        private Animator _anim;
        private Phase _phase = Phase.None;

        private int _hSU, _hAC, _hRC;
        private bool _playedSU, _playedAC, _playedRC;

        private bool _hitActive;
        private float _lastCurve;
        private readonly HashSet<GameObject> _alreadyHit = new HashSet<GameObject>(64);
        private int _hitWindowIndex = 0; // 디버깅용(필수 아님)

        private float _phaseStartTime;

        public override void OnInit(BTContext ctx)
        {
            base.OnInit(ctx);
            _anim = ctx.Owner.GetComponentInChildren<Animator>();
            _hSU = Animator.StringToHash(_startUpStateName ?? string.Empty);
            _hAC = Animator.StringToHash(_actionStateName ?? string.Empty);
            _hRC = Animator.StringToHash(_recoveryStateName ?? string.Empty);
        }

        protected override void OnEnter(BTContext ctx)
        {
            base.OnEnter(ctx);

            _phase = _useAttackDelay ? Phase.Startup : Phase.Action;
            _playedSU = _playedAC = _playedRC = false;

            _hitActive = false;
            _lastCurve = 0f;
            _alreadyHit.Clear();
            _hitWindowIndex = 0;
            _phaseStartTime = Time.time;

            if (_anim && !string.IsNullOrEmpty(_pIsAttacking))
                _anim.SetBool(_pIsAttacking, true);
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            if (_anim == null) return Mark(BTStatus.Failure);

            switch (_phase)
            {
                case Phase.Startup:
                {
                    PlayOnce(_startUpStateName, _startUpCrossFadeDuration, ref _playedSU);
                    float prog = GetProgress(_hSU, _startUpStateName, _startupTag);
                    if (Reached(prog, _startUpExitNormalized) || TimedOut())
                        NextPhase(Phase.Action);
                    return Mark(BTStatus.Running);
                }

                case Phase.Action:
                {
                    PlayOnce(_actionStateName, _actionCrossFadeDuration, ref _playedAC);

                    float prog = Mathf.Clamp01(GetProgress(_hAC, _actionStateName, _actionTag));
                    float c = Mathf.Clamp01(_hitPointCurve.Evaluate(prog));

                    // 커브 엣지 감지: 0→1 시작, 1→0 종료
                    if (!_hitActive && _lastCurve <= 0f && c > 0f) BeginHit();
                    if (_hitActive && c <= 0f && _lastCurve > 0f) EndHit();

                    if (_hitActive) DoHitSweep(ctx);
                    _lastCurve = c;

                    if (Reached(prog, _actionExitNormalized) || TimedOut())
                        NextPhase(_useRecoveryDelay ? Phase.Recovery : Phase.Done);
                    return Mark(BTStatus.Running);
                }

                case Phase.Recovery:
                {
                    PlayOnce(_recoveryStateName, _recoveryCrossFadeDuration, ref _playedRC);
                    float prog = GetProgress(_hRC, _recoveryStateName, _recoveryTag);
                    if (Reached(prog, _recoveryExitNormalized) || TimedOut())
                        NextPhase(Phase.Done);
                    return Mark(BTStatus.Running);
                }

                case Phase.Done:
                {
                    if (_hitActive) EndHit();
                    if (_anim && !string.IsNullOrEmpty(_pIsAttacking))
                        _anim.SetBool(_pIsAttacking, false);
                    return Mark(BTStatus.Success);
                }
            }

            return Mark(BTStatus.Success);
        }

        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            if (_hitActive) EndHit();
            _alreadyHit.Clear();
            if (_anim && !string.IsNullOrEmpty(_pIsAttacking))
                _anim.SetBool(_pIsAttacking, false);
        }

        // ===== Helpers =====
        private void PlayOnce(string stateName, float fade, ref bool playedFlag)
        {
            if (playedFlag || string.IsNullOrEmpty(stateName)) return;
            _anim.CrossFadeInFixedTime(stateName, fade, _layer, 0f);
            playedFlag = true;
        }

        /// <summary>현재/다음 스테이트에서 이름/해시/태그로 진행률(0..1)을 찾는다. 실패 시 -1.</summary>
        private float GetProgress(int stateHash, string stateName, string tagIfUsed)
        {
            var cur = _anim.GetCurrentAnimatorStateInfo(_layer);
            var nxt = _anim.GetNextAnimatorStateInfo(_layer);

            if (_useTagDetection && !string.IsNullOrEmpty(tagIfUsed))
            {
                if (_anim.IsInTransition(_layer))
                {
                    if (nxt.IsTag(tagIfUsed)) return nxt.normalizedTime;
                    if (cur.IsTag(tagIfUsed)) return cur.normalizedTime;
                }
                else if (cur.IsTag(tagIfUsed)) return cur.normalizedTime;
            }

            if (_anim.IsInTransition(_layer))
            {
                if (nxt.fullPathHash == stateHash || (!string.IsNullOrEmpty(stateName) && nxt.IsName(stateName)))
                    return nxt.normalizedTime;
                if (cur.fullPathHash == stateHash || (!string.IsNullOrEmpty(stateName) && cur.IsName(stateName)))
                    return cur.normalizedTime;
            }
            else if (cur.fullPathHash == stateHash || (!string.IsNullOrEmpty(stateName) && cur.IsName(stateName)))
            {
                return cur.normalizedTime;
            }

            return -1f;
        }

        private bool Reached(float normalized, float exitAt)
        {
            if (normalized < 0f) return false;
            if (exitAt <= 0f) return true;
            return normalized >= exitAt;
        }

        private bool TimedOut()
        {
            return _phaseTimeout > 0f && (Time.time - _phaseStartTime) >= _phaseTimeout;
        }

        private void NextPhase(Phase p)
        {
            _phase = p;
            _phaseStartTime = Time.time;

            if (_hitActive && p != Phase.Action) EndHit();
            _lastCurve = 0f;
        }

        private void BeginHit()
        {
            _hitActive = true;
            _hitWindowIndex++;

            // 새 히트창이 열릴 때 동일 대상 재히트 허용
            if (_rehitOnNewWindow)
                _alreadyHit.Clear();

            if (_hitColliderType == HitColliderType.Object && _collider)
                _collider.enabled = true;
        }

        private void EndHit()
        {
            _hitActive = false;
            if (_hitColliderType == HitColliderType.Object && _collider)
                _collider.enabled = false;
        }

        private void DoHitSweep(BTContext ctx)
        {
            var root = ctx.Owner.transform;
            var pivotWorld = root.TransformPoint(_pivotPoint);

            Collider[] hits = null;

            switch (_hitColliderType)
            {
                case HitColliderType.Object:
                    return; // 트리거 콜라이더가 자체 처리
                case HitColliderType.Box:
                    hits = Physics.OverlapBox(pivotWorld, _boxSize * 0.5f, root.rotation, _hitMask);
                    break;
                case HitColliderType.Sphere:
                    hits = Physics.OverlapSphere(pivotWorld, _sphereRadius, _hitMask);
                    break;
                case HitColliderType.Capsule:
                {
                    GetCapsuleWorldEnds(root, pivotWorld, out var a, out var b);
                    hits = Physics.OverlapCapsule(a, b, _capsuleRadius, _hitMask);
                    break;
                }
            }

            if (hits == null) return;

            for (int i = 0; i < hits.Length; i++)
            {
                var col = hits[i];
                if (!col) continue;

                Entity entity = col.GetComponentInParent<Entity>();
                if (entity == null) continue;
                if (col.CompareTag("Player") || col.CompareTag("NPC")) continue;
                if (_preventMultipleHitsPerAction && _alreadyHit.Contains(entity.gameObject)) continue;

                _alreadyHit.Add(entity.gameObject);
                
                _damageInfo.attacker = ctx.Owner.transform;
                _damageInfo.receiver = entity.transform;

                // TODO: 실제 대미지 처리 연결
                col.GetComponent<IDamageable>()?.TakeDamage(_damageInfo);
                Debug.Log($"Hit: {col.name}");
            }
        }

        private void GetCapsuleWorldEnds(Transform root, Vector3 pivotWorld, out Vector3 a, out Vector3 b)
        {
            float half = Mathf.Max(_capsuleRadius, _capsuleHeight * 0.5f) - _capsuleRadius;
            Vector3 axis = _capsuleDirection switch
            {
                CapsuleDirection.XAxis => root.right,
                CapsuleDirection.ZAxis => root.forward,
                _ => root.up
            };
            a = pivotWorld + axis * half;
            b = pivotWorld - axis * half;
        }

        public override BTNode Clone()
        {
            return new Attack
            {
                // General
                _layer = _layer,
                _pIsAttacking = _pIsAttacking,
                _useTagDetection = _useTagDetection,
                _startupTag = _startupTag,
                _actionTag = _actionTag,
                _recoveryTag = _recoveryTag,
                _phaseTimeout = _phaseTimeout,

                // Startup
                _useAttackDelay = _useAttackDelay,
                _startUpStateName = _startUpStateName,
                _startUpCrossFadeDuration = _startUpCrossFadeDuration,
                _startUpExitNormalized = _startUpExitNormalized,

                // Action
                _actionStateName = _actionStateName,
                _actionCrossFadeDuration = _actionCrossFadeDuration,
                _actionExitNormalized = _actionExitNormalized,
                _hitPointCurve = _hitPointCurve,
                _rehitOnNewWindow = _rehitOnNewWindow,

                // Recovery
                _useRecoveryDelay = _useRecoveryDelay,
                _recoveryStateName = _recoveryStateName,
                _recoveryCrossFadeDuration = _recoveryCrossFadeDuration,
                _recoveryExitNormalized = _recoveryExitNormalized,

                // Hit Collider
                _hitColliderType = _hitColliderType,
                _collider = _collider,
                _useGizmos = _useGizmos,
                _pivotPoint = _pivotPoint,
                _boxSize = _boxSize,
                _capsuleRadius = _capsuleRadius,
                _capsuleHeight = _capsuleHeight,
                _capsuleDirection = _capsuleDirection,
                _sphereRadius = _sphereRadius,
                _hitMask = _hitMask,
                _preventMultipleHitsPerAction = _preventMultipleHitsPerAction,

                // Damage Info
                _damageInfo = _damageInfo,
            };
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos(GameObject obj)
        {
            if (!_useGizmos) return;

            var old = Gizmos.matrix;
            Gizmos.matrix = obj.transform.localToWorldMatrix * Matrix4x4.TRS(_pivotPoint, Quaternion.identity, Vector3.one);

            switch (_hitColliderType)
            {
                case HitColliderType.Object:
                    if (_collider)
                        Gizmos.DrawWireCube(
                            obj.transform.InverseTransformPoint(_collider.bounds.center),
                            _collider.bounds.size);
                    break;

                case HitColliderType.Box:
                    Gizmos.DrawWireCube(Vector3.zero, _boxSize);
                    break;

                case HitColliderType.Sphere:
                    Gizmos.DrawWireSphere(Vector3.zero, _sphereRadius);
                    break;

                case HitColliderType.Capsule:
                {
                    float half = Mathf.Max(_capsuleRadius, _capsuleHeight * 0.5f) - _capsuleRadius;
                    Vector3 axis = _capsuleDirection switch
                    {
                        CapsuleDirection.XAxis => Vector3.right,
                        CapsuleDirection.ZAxis => Vector3.forward,
                        _ => Vector3.up
                    };
                    var top = axis * half; var bottom = -axis * half;

                    Gizmos.DrawWireSphere(top, _capsuleRadius);
                    Gizmos.DrawWireSphere(bottom, _capsuleRadius);

                    Vector3 right = (axis == Vector3.up) ? Vector3.right : Vector3.up;
                    Vector3 fwd   = (axis == Vector3.forward) ? Vector3.right : Vector3.forward;
                    Gizmos.DrawLine(top + right * _capsuleRadius,   bottom + right * _capsuleRadius);
                    Gizmos.DrawLine(top - right * _capsuleRadius,   bottom - right * _capsuleRadius);
                    Gizmos.DrawLine(top + fwd   * _capsuleRadius,   bottom + fwd   * _capsuleRadius);
                    Gizmos.DrawLine(top - fwd   * _capsuleRadius,   bottom - fwd   * _capsuleRadius);
                    break;
                }
            }

            Gizmos.matrix = old;
        }
#endif
    }
}
