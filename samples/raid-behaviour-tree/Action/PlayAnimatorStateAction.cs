// PlayAnimatorStateAction.cs
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    /// <summary>
    /// Animator의 특정 상태(이름/경로 해시)를 재생한다.
    /// - 시작: Play 또는 CrossFade로 상태 진입
    /// - 대기: WaitMode에 따라 Running 유지
    /// - 완료: Success (또는 NotFound 정책에 따라 Failure)
    ///
    /// 사용 예:
    /// 1) 게이트 전환 모션: GateDecorator의 _gate 쪽에 넣어서 모션이 끝날 때까지 Running
    /// 2) 단독 사용: 즉시 성공(Immediate) or 지정 초/정규화 시간까지 대기
    /// </summary>
    [Serializable]
    public sealed class PlayAnimatorStateAction : BTActionNode
    {
        // === Play Target ===
        public enum HashType { ShortNameHash, FullPathHash }
        [Title("State")]
        [Tooltip("AnimatorController의 상태명(또는 풀패스). 예: Idle 또는 Base Layer.Locomotion.Idle")]
        [SerializeField] private string _stateName = "Idle";
        [SerializeField, LabelText("Hash Type")] private HashType _hashType = HashType.ShortNameHash;
        [SerializeField, LabelText("Layer")] private int _layer = 0;

        // === Start Method ===
        public enum StartMethod { Play, CrossFade }
        [Title("Start")]
        [SerializeField, LabelText("Method")] private StartMethod _startMethod = StartMethod.CrossFade;
        [SerializeField, ShowIf("@_startMethod == StartMethod.CrossFade")]
        [LabelText("Fade (sec)")] private float _crossFadeDuration = 0.1f;

        [SerializeField, LabelText("Start Normalized Time")]
        [Tooltip("Play: 0이면 처음부터. CrossFade: -1이면 유지(Default).")]
        private float _startNormalizedTime = 0f; // CrossFade는 -1이면 유지

        // === Wait / Completion Policy ===
        public enum WaitMode
        {
            ImmediateSuccess,          // 재생만 하고 바로 성공
            UntilStateEntered,         // 대상 상태로 들어가면 성공
            UntilNormalizedTime,       // 대상 상태로 들어간 뒤 normalizedTime >= threshold
            FixedSeconds,              // 지정 시간(sec) 경과 시 성공
            WhileInState               // 대상 상태에 머무르는 동안 Running, 벗어나면 성공
        }

        [Title("Wait")]
        [SerializeField] private WaitMode _waitMode = WaitMode.UntilNormalizedTime;
        [SerializeField, ShowIf("@_waitMode == WaitMode.UntilNormalizedTime")]
        [LabelText("Min Normalized Time")] private float _minNormalizedTime = 1.0f;
        [SerializeField, ShowIf("@_waitMode == WaitMode.FixedSeconds")]
        [LabelText("Duration (sec)")] private float _waitSeconds = 0.6f;

        // === Error / Edge Policy ===
        [Title("Policy")]
        [SerializeField, LabelText("Fail If Animator Missing")] private bool _failIfNoAnimator = true;
        [SerializeField, LabelText("Fail If State Not Found")] private bool _failIfNotFound = false;

        // === Runtime ===
        [NonSerialized] private Animator _anim;
        [NonSerialized] private int _stateHash;
        [NonSerialized] private bool _started;
        [NonSerialized] private bool _waitingForEntry; // CrossFade 진입 대기 등
        [NonSerialized] private float _elapsed;

        public override void OnInit(BTContext ctx)
        {
            // Hash는 문자열 비교를 피하기 위해 미리 계산
            _anim = ctx.Owner.GetComponentInChildren<Animator>();
            _stateHash = Animator.StringToHash(_stateName);
        }

        protected override void OnEnter(BTContext ctx)
        {
            _started = false;
            _waitingForEntry = false;
            _elapsed = 0f;
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            if (_anim == null)
                return Mark(_failIfNoAnimator ? BTStatus.Failure : BTStatus.Success);

            // 최초 1회 시작
            if (!_started)
            {
                // 상태 시작
                bool ok = StartState();
                if (!ok)
                    return Mark(_failIfNotFound ? BTStatus.Failure : BTStatus.Success);

                // 대기 모드 설정
                switch (_waitMode)
                {
                    case WaitMode.ImmediateSuccess:
                        return Mark(BTStatus.Success);
                    case WaitMode.UntilStateEntered:
                    case WaitMode.UntilNormalizedTime:
                    case WaitMode.WhileInState:
                        _waitingForEntry = true; // 대상 상태로 실제 진입했는지 확인 필요
                        break;
                    case WaitMode.FixedSeconds:
                        _waitingForEntry = false;
                        _elapsed = 0f;
                        break;
                }

                _started = true;
                // 시작 프레임에도 Running 유지 가능
                return Mark(BTStatus.Running);
            }

            // 진행/완료 판정
            switch (_waitMode)
            {
                case WaitMode.FixedSeconds:
                    _elapsed += ctx.DeltaTime;
                    if (_elapsed >= _waitSeconds)
                        return Mark(BTStatus.Success);
                    return Mark(BTStatus.Running);

                case WaitMode.UntilStateEntered:
                    if (HasEnteredTargetState())
                        return Mark(BTStatus.Success);
                    return Mark(BTStatus.Running);

                case WaitMode.UntilNormalizedTime:
                {
                    if (_waitingForEntry)
                    {
                        if (!HasEnteredTargetState())
                            return Mark(BTStatus.Running);
                        _waitingForEntry = false;
                    }

                    if(!TryGetTargetStateNormalizedTime(out float normalizedTime)) return Mark(BTStatus.Running);

                    if (normalizedTime >= _minNormalizedTime)
                            return Mark(BTStatus.Success);
                        return Mark(BTStatus.Running);
                }

                case WaitMode.WhileInState:
                {
                    if (_waitingForEntry)
                    {
                        if (!HasEnteredTargetState())
                            return Mark(BTStatus.Running);
                        _waitingForEntry = false;
                    }

                    // 대상 상태에 있는 동안엔 Running, 벗어나면 Success
                    bool inTarget = IsInTargetState();
                    return Mark(inTarget ? BTStatus.Running : BTStatus.Success);
                }

                default: // ImmediateSuccess는 위에서 이미 반환됨
                    return Mark(BTStatus.Success);
            }
        }

        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            // 별도 정리 필요 없음. (원하면 여기서 파라미터/트리거 Reset 가능)
        }

        public override void Reset()
        {
            base.Reset();
            _started = false;
            _waitingForEntry = false;
            _elapsed = 0f;
        }

        public override BTNode Clone()
        {
            return new PlayAnimatorStateAction
            {
                _stateName = _stateName,
                _hashType = _hashType,
                _layer = _layer,
                _startMethod = _startMethod,
                _crossFadeDuration = _crossFadeDuration,
                _startNormalizedTime = _startNormalizedTime,
                _waitMode = _waitMode,
                _minNormalizedTime = _minNormalizedTime,
                _waitSeconds = _waitSeconds,
                _failIfNoAnimator = _failIfNoAnimator,
                _failIfNotFound = _failIfNotFound
            };
        }

        // ===== Helpers =====

        bool StartState()
        {
            // Hash 재계산(에디터에서 이름 바뀐 뒤 OnInit을 안 탔을 수 있으니 안전하게)
            _stateHash = Animator.StringToHash(_stateName);

            // Animator.Play/CrossFade는 해시가 short/풀패스 모두 허용(해당 문자열로 만든 해시여야 함)
            if (_startMethod == StartMethod.Play)
            {
                float t = Mathf.Clamp(_startNormalizedTime, 0f, float.MaxValue);
                _anim.Play(_stateHash, _layer, t);
                // Play는 즉시 상태가 바뀌는 경향이 있지만, 안전하게 다음 틱까지 확인
                return true;
            }
            else // CrossFade
            {
                float t = (_startNormalizedTime < 0f) ? -1f : Mathf.Clamp(_startNormalizedTime, 0f, float.MaxValue);
                _anim.CrossFade(_stateHash, Mathf.Max(0f, _crossFadeDuration), _layer, t);
                // 전이 후 진입 확인 필요
                return true;
            }
        }

        bool HasEnteredTargetState()
        {
            // 현재 또는 Next 상태 중 하나가 목표 상태인지 빠르게 확인
            if (_anim.IsInTransition(_layer))
            {
                var next = _anim.GetNextAnimatorStateInfo(_layer);
                if (MatchHash(next)) return true;
            }
            var cur = _anim.GetCurrentAnimatorStateInfo(_layer);
            return MatchHash(cur);
        }

        bool IsInTargetState()
        {
            if (_anim.IsInTransition(_layer))
            {
                var cur = _anim.GetCurrentAnimatorStateInfo(_layer);
                var next = _anim.GetNextAnimatorStateInfo(_layer);
                return MatchHash(cur) || MatchHash(next);
            }
            else
            {
                var cur = _anim.GetCurrentAnimatorStateInfo(_layer);
                return MatchHash(cur);
            }
        }

        public bool TryGetTargetStateNormalizedTime(out float normalizedTime)
        {
            if (_anim.IsInTransition(_layer))
            {
                var next = _anim.GetNextAnimatorStateInfo(_layer);
                if (MatchHash(next))
                {
                    normalizedTime = next.normalizedTime;
                    return true;
                }
            }
            var cur = _anim.GetCurrentAnimatorStateInfo(_layer);
            if (MatchHash(cur))
            {
                normalizedTime = cur.normalizedTime;
                return true;
            }
            normalizedTime = 0f;
            return false;
        }

        bool MatchHash(AnimatorStateInfo s)
        {
            // 사용자가 어떤 해시 문자열로 넣었는지에 따라 비교 대상 선택
            return _hashType == HashType.FullPathHash ? s.fullPathHash == _stateHash : s.shortNameHash == _stateHash;
        }
    }
}
