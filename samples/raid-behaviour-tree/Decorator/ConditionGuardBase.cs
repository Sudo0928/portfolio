using System;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree
{
    /// <summary>
    /// 조건 가드 공통 베이스 + 선택형 Fallback 트리거
    /// - When: GuardBlockedOnly / GuardBlockedOrChildFailure (변경 없음)
    /// - Trigger: Guard 차단 시 동작 + (신규) Child Success 종료 시 동작
    /// </summary>
    [Serializable]
    public abstract class ConditionGuardBase : BTDecorator
    {
        // ─────────────────────────────────────────────────────────────────────
        // 정책 Enum
        // ─────────────────────────────────────────────────────────────────────

        [Serializable]
        public enum GuardReevalPolicy
        {
            EveryTickAbort,    // 실행 중 조건이 깨지면 즉시 Abort
            LatchUntilFinish,  // 시작했으면 끝까지 유지 (조건 변동 무시)
            WaitThenFail       // 끝까지 감, 종료 프레임에 Failure로 덮기
        }

        [Serializable]
        public enum BlockedReturnPolicy
        {
            Failure = 0,
            Success = 1,
            FallbackResult = 2
        }

        /// <summary>
        /// Fallback을 언제 시도할지 (가드 차단 상황 + 자식 성공 종료 케이스 포함)
        /// </summary>
        [Serializable]
        public enum FallbackTriggerMode
        {
            AlwaysOnGuardFailure = 0,          // (기존) 가드가 막히면 항상
            OnlyWhenAbortingFromRunning = 1,   // (기존) Running 중 Abort일 때만

            // (신규) Guard 차단은 "Running 중 Abort일 때만", 추가로 "자식이 Success로 끝나도" Fallback
            AbortingFromRunningOrChildSuccess = 2,

            // (신규) 자식이 Success로 끝났을 때만 (Guard 차단에선 X)
            OnlyOnChildSuccess = 3
        }

        /// <summary>
        /// (변경 없음) Fallback을 가드 차단에만 쓸지, 자식 Failure에도 쓸지
        /// </summary>
        [Serializable]
        public enum FallbackWhen
        {
            GuardBlockedOnly = 0,
            GuardBlockedOrChildFailure = 1
        }

        // ─────────────────────────────────────────────────────────────────────
        // 인스펙터 설정
        // ─────────────────────────────────────────────────────────────────────

        [Title("Reevaluation (When Condition Breaks During Running)")]
        [SerializeField, EnumToggleButtons, LabelText("Reevaluation Policy")]
        protected GuardReevalPolicy _reevalPolicy = GuardReevalPolicy.EveryTickAbort;

        [Title("Fallback")]
        [ToggleLeft, LabelText("Use Fallback")]
        [SerializeField] protected bool _useFallback = true;

        [ShowIf(nameof(_useFallback))]
        [SerializeReference, InlineProperty, HideLabel]
        protected BTNode _fallbackNode;

        [ShowIf(nameof(_useFallback))]
        [LabelText("Trigger")]
        [SerializeField] protected FallbackTriggerMode _triggerMode = FallbackTriggerMode.AlwaysOnGuardFailure;

        [ShowIf(nameof(_useFallback))]
        [LabelText("When")]
        [SerializeField] protected FallbackWhen _fallbackWhen = FallbackWhen.GuardBlockedOnly;

        [ShowIf(nameof(_useFallback))]
        [LabelText("Final Return")]
        [SerializeField] protected BlockedReturnPolicy _blockedReturn = BlockedReturnPolicy.FallbackResult;

        // ─────────────────────────────────────────────────────────────────────
        // 런타임 상태
        // ─────────────────────────────────────────────────────────────────────
        [NonSerialized] private bool _checkedThisTick;
        [NonSerialized] private bool _cachedPass;
        [NonSerialized] private int  _lastEvalFrame = -1;

        [NonSerialized] private bool _usingFallback;   // Fallback 진행 중
        [NonSerialized] private bool _latched;         // 자식 진입~종료 구간
        [NonSerialized] private bool _pendingAbort;    // WaitThenFail 예약

        // ─────────────────────────────────────────────────────────────────────

        public override void OnInit(BTContext ctx)
        {
            base.OnInit(ctx);
            _fallbackNode?.OnInit(ctx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void __SyncTickFrame()
        {
            int f = Time.frameCount;
            if (_lastEvalFrame != f)
            {
                _lastEvalFrame = f;
                _checkedThisTick = false;
            }
        }

        /// <summary>
        /// Decorator의 CanTick: 가드 조건을 확인한다.
        /// 라칭 중 정책에 따라 자식 틱 허용/차단을 결정.
        /// </summary>
        protected override bool CanTick(BTContext ctx)
        {
            __SyncTickFrame();

            // 라칭 중이면 정책 우선
            if (_latched)
            {
                if (_reevalPolicy == GuardReevalPolicy.LatchUntilFinish) return true;
                if (_reevalPolicy == GuardReevalPolicy.WaitThenFail && _pendingAbort) return true;
            }

            // 프레임 캐시
            if (_checkedThisTick)
            {
                var v = _cachedPass;
                _checkedThisTick = false; // 동일 프레임 1회 소비
                return v;
            }

            _cachedPass = Evaluate(ctx);
            return _cachedPass;
        }

        /// <summary>
        /// Decorator의 틱 오버라이드: 가드 차단/Abort/Fallback 전환 처리
        /// true를 반환하면 여기서 결과를 확정함(자식 호출 안 함), false면 자식으로 진행.
        /// </summary>
        protected override bool TryTickOverride(BTContext ctx, out BTStatus status)
        {
            __SyncTickFrame();

            // 0) 이미 Fallback 모드면 Fallback부터 틱
            if (_usingFallback)
            {
                var s = _fallbackNode?.OnTick(ctx) ?? BTStatus.Failure;
                if (s == BTStatus.Running)
                {
                    status = BTStatus.Running;
                    _checkedThisTick = false;
                    return true;
                }

                _fallbackNode?.Reset();
                _usingFallback = false;
                status = DecideFinalStatus(s);
                _checkedThisTick = false;
                return true;
            }

            // 1) 이번 프레임 조건 선평가
            if (!_checkedThisTick)
            {
                _cachedPass = Evaluate(ctx);
                _checkedThisTick = true;
            }

            // 2) 조건 통과 → 자식으로 진행
            if (_cachedPass)
            {
                status = default;
                return false;
            }

            // 3) 라칭 중 조건이 깨짐(=Running 중 Abort 후보)
            if (_latched)
            {
                switch (_reevalPolicy)
                {
                    case GuardReevalPolicy.LatchUntilFinish:
                        status = default; // 계속 진행
                        return false;

                    case GuardReevalPolicy.WaitThenFail:
                        _pendingAbort = true;
                        status = default; // 계속 진행, 종료 프레임에 Failure로 덮음
                        return false;

                    case GuardReevalPolicy.EveryTickAbort:
                        // Running 중 Abort 상황에서만 트리거할지 여부 확인
                        if (ShouldRunFallbackOnGuardBlock(abortingFromRunning: true))
                        {
                            SafeAbortChild(ctx);
                            return StartOrFinishFallback(ctx, out status);
                        }
                        else
                        {
                            SafeAbortChild(ctx);
                            status = BTStatus.Failure;
                            _checkedThisTick = false;
                            return true;
                        }
                }
            }

            // 4) 라칭 전 가드 차단(자식 미진입)
            if (ShouldRunFallbackOnGuardBlock(abortingFromRunning: false))
            {
                return StartOrFinishFallback(ctx, out status);
            }

            // 5) 기본 실패
            status = BTStatus.Failure;
            _checkedThisTick = false;
            return true;
        }

        protected override void OnEnter(BTContext ctx)
        {
            _latched = true;
            _pendingAbort = false;
        }

        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            // WaitThenFail: 종료 프레임에 실패로 덮기
            if (_reevalPolicy == GuardReevalPolicy.WaitThenFail && _pendingAbort)
            {
                status = BTStatus.Failure;
            }

            // ── 자식 종료 프레임에서 Fallback 트리거 처리 ──────────────────
            // 1) 자식 실패(Failure) → When이 ChildFailure 허용이면 Fallback
            bool wantsOnChildFailure =
                _useFallback
                && _fallbackNode != null
                && _fallbackWhen == FallbackWhen.GuardBlockedOrChildFailure
                && status == BTStatus.Failure;

            // 2) 자식 성공(Success) → Trigger가 Success를 허용하면 Fallback
            bool wantsOnChildSuccess =
                _useFallback
                && _fallbackNode != null
                && ( _triggerMode == FallbackTriggerMode.AbortingFromRunningOrChildSuccess
                  || _triggerMode == FallbackTriggerMode.OnlyOnChildSuccess )
                && status == BTStatus.Success;

            if ((wantsOnChildFailure || wantsOnChildSuccess) && !_usingFallback)
            {
                var fs = _fallbackNode.OnTick(ctx);
                if (fs == BTStatus.Running)
                {
                    // Fallback 모드로 전환
                    _usingFallback = true;
                    status = BTStatus.Running;
                    _latched = false;
                    _pendingAbort = false;
                    _checkedThisTick = false;
                    return;
                }

                // Fallback이 즉시 종료됨 → 최종 결과 확정
                _fallbackNode.Reset();
                status = DecideFinalStatus(fs);
                _latched = false;
                _pendingAbort = false;
                _checkedThisTick = false;
                return;
            }

            // 클린업
            if (_usingFallback)
            {
                _fallbackNode?.Reset();
                _usingFallback = false;
            }

            _latched = false;
            _pendingAbort = false;
            _checkedThisTick = false;
        }

        public override void Reset()
        {
            base.Reset();
            _fallbackNode?.Reset();
            _checkedThisTick = false;
            _usingFallback = false;
            _latched = false;
            _pendingAbort = false;
            _lastEvalFrame = -1;
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            var copy = (ConditionGuardBase)MemberwiseClone();
            copy._child = clonedChild;
            copy._fallbackNode = _fallbackNode?.Clone();

            // 런타임 상태 초기화
            copy._checkedThisTick = false;
            copy._usingFallback = false;
            copy._latched = false;
            copy._pendingAbort = false;
            copy._lastEvalFrame = -1;

            return copy;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 내부 유틸
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 가드 차단(자식 미진입 또는 Running 중 Abort)에서 Fallback을 트리거할지
        /// </summary>
        private bool ShouldRunFallbackOnGuardBlock(bool abortingFromRunning)
        {
            if (!_useFallback || _fallbackNode == null) return false;

            switch (_triggerMode)
            {
                case FallbackTriggerMode.AlwaysOnGuardFailure:
                    return true;

                case FallbackTriggerMode.OnlyWhenAbortingFromRunning:
                    return abortingFromRunning;

                case FallbackTriggerMode.AbortingFromRunningOrChildSuccess:
                    // 자식 Success는 OnExit에서 처리, Guard 차단에선 "Abort일 때만"
                    return abortingFromRunning;

                case FallbackTriggerMode.OnlyOnChildSuccess:
                    // Guard 차단 상황에서는 트리거하지 않음
                    return false;
            }
            return false;
        }

        private bool StartOrFinishFallback(BTContext ctx, out BTStatus status)
        {
            var fs = _fallbackNode?.OnTick(ctx) ?? BTStatus.Failure;
            if (fs == BTStatus.Running)
            {
                _usingFallback = true;
                status = BTStatus.Running;
                _checkedThisTick = false;
                return true;
            }

            _fallbackNode?.Reset();
            status = DecideFinalStatus(fs);
            _checkedThisTick = false;
            return true;
        }

        /// <summary>EveryTickAbort 경로에서 자식 누수 방지.</summary>
        private void SafeAbortChild(BTContext ctx)
        {
            if (_latched)
            {
                _child?.Reset();
            }
            _latched = false;
            _pendingAbort = false;
        }

        private BTStatus DecideFinalStatus(BTStatus fb)
        {
            return _blockedReturn switch
            {
                BlockedReturnPolicy.Success        => BTStatus.Success,
                BlockedReturnPolicy.FallbackResult => (fb == BTStatus.Running ? BTStatus.Running : fb),
                _                                   => BTStatus.Failure
            };
        }

        /// <summary>
        /// 파생 클래스에서 구현: true면 조건 통과, false면 차단.
        /// 박싱/언박싱 없이 빠르게 구현하도록 주의.
        /// </summary>
        protected abstract bool Evaluate(BTContext ctx);
    }
}
