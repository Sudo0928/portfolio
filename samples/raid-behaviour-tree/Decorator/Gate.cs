// GateDecorator.cs
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    /// <summary>
    /// 최초 진입 시 게이트 노드를 완료(성공/실패)할 때까지 실행한다.
    /// - Gate가 Running: GateDecorator도 Running (자식 미틱)
    /// - Gate가 Failure: GateDecorator Failure (자식 미틱)
    /// - Gate가 Success: 같은 프레임부터 자식 틱 시작
    /// 자식이 끝나고(성공/실패) 데코레이터가 종료되면 다음 진입 때 게이트를 다시 실행한다.
    /// </summary>
    [Serializable]
    public sealed class Gate : BTDecorator
    {
        [Title("Gate (runs once on enter until completes)")]
        [SerializeReference, InlineProperty, HideLabel]
        private BTNode _gate;

        [NonSerialized] private bool _gatePassed;

        public override void OnInit(BTContext ctx)
        {
            // Gate 먼저 초기화, 그 다음 Child 초기화(기존 BTDecorator가 child를 초기화함)
            _gate?.OnInit(ctx);
            base.OnInit(ctx);
        }

        // ✅ 자식 틱 전에 한 번 개입해서 Gate를 돌린다.
        protected override bool TryTickOverride(BTContext ctx, out BTStatus status)
        {
            // 게이트가 아직 통과되지 않았다면 게이트부터 처리
            if (!_gatePassed)
            {
                if (_gate == null)
                {
                    // 게이트가 비어 있으면 즉시 통과로 간주
                    _gatePassed = true;
                    status = default;
                    return false; // 자식으로 진행
                }

                var s = _gate.OnTick(ctx);
                if (s == BTStatus.Running)
                {
                    status = BTStatus.Running; // 게이트 도는 동안은 자식 틱 금지
                    return true;
                }
                if (s == BTStatus.Failure)
                {
                    // 게이트 실패 → 데코레이터 실패, 다음 진입 때 다시 시도
                    _gate.Reset();
                    _gatePassed = false;
                    status = BTStatus.Failure;
                    return true;
                }

                // 게이트 성공 → 같은 프레임에 자식으로 바로 넘어감
                _gatePassed = true;
                status = default;
                return false;
            }

            status = default;
            return false; // 게이트 통과 후에는 자식만 틱
        }

        // 자식이 끝나면 다음 진입을 위해 게이트를 리암
        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            _gate?.Reset();
            _gatePassed = false;
        }

        public override void Reset()
        {
            base.Reset();
            _gate?.Reset();
            _gatePassed = false;
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new Gate
            {
                _child = clonedChild,
                _gate = _gate?.Clone()
            };
        }
    }
}
