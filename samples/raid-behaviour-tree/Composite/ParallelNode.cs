using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace ProjectRaid.Runtime.BehaviourTree.Composite
{
    public enum ResolvePolicy { Any, All }
    public enum ResolvePriority { FailureFirst, SuccessFirst }

    public class ParallelNode : BTComposite
    {
        [Title("Resolution")]
        [LabelText("Success Policy")] public ResolvePolicy successPolicy = ResolvePolicy.Any;
        [LabelText("Failure Policy")] public ResolvePolicy failurePolicy = ResolvePolicy.Any;
        [LabelText("Resolve Priority")] public ResolvePriority priority = ResolvePriority.FailureFirst;

        [Title("Control")]
        [LabelText("Abort Others On Resolve")] public bool abortOthersOnResolve = true;
        [LabelText("Skip Finished Children")] public bool skipFinished = true;

        private List<BTStatus> _states = new();

        protected override void OnEnter(BTContext ctx)
        {
            EnsureStateSize();
            for (int i = 0; i < _states.Count; i++) _states[i] = BTStatus.Running;
        }

        public override BTStatus OnTick(BTContext ctx)
        {
            EnsureStateSize();

            int success = 0, failure = 0, running = 0;

            for (int i = 0; i < _children.Count; i++)
            {
                var ch = _children[i];
                if (ch == null) { failure++; continue; }

                // 이미 Success/Failure이고 skip 설정이면 재틱 안 함
                if (skipFinished && (_states[i] == BTStatus.Success || _states[i] == BTStatus.Failure))
                {
                    if (_states[i] == BTStatus.Success) success++;
                    else failure++;
                    continue;
                }

                var s = ch.OnTick(ctx);
                _states[i] = s;

                if (s == BTStatus.Success) success++;
                else if (s == BTStatus.Failure) failure++;
                else running++;
            }

            // 해결 판정
            bool successResolved = (successPolicy == ResolvePolicy.All) ? (success == _children.Count)
                                                                       : (success > 0);
            bool failureResolved = (failurePolicy == ResolvePolicy.All) ? (failure == _children.Count)
                                                                       : (failure > 0);

            BTStatus result = BTStatus.Running;

            if (priority == ResolvePriority.FailureFirst)
            {
                if (failureResolved) result = BTStatus.Failure;
                else if (successResolved) result = BTStatus.Success;
                else if (running > 0) result = BTStatus.Running;
                else result = BTStatus.Failure; // 모두 끝났는데 정책으로 성공 아님 → 실패
            }
            else // SuccessFirst
            {
                if (successResolved) result = BTStatus.Success;
                else if (failureResolved) result = BTStatus.Failure;
                else if (running > 0) result = BTStatus.Running;
                else result = BTStatus.Failure;
            }

            if (abortOthersOnResolve && (result == BTStatus.Success || result == BTStatus.Failure))
            {
                // 아직 Running인 자식들 중단
                for (int i = 0; i < _children.Count; i++)
                {
                    if (_states[i] == BTStatus.Running)
                    {
                        _children[i].Reset();
                        _states[i] = BTStatus.Failure; // 어떤 값이든 더 이상 틱 안 하도록 마킹
                    }
                }
            }

            return Mark(result);
        }

        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            // Parallel이 종료되면 모든 자식 리셋
            for (int i = 0; i < _children.Count; i++) _children[i]?.Reset();
        }

        public override BTNode Clone()
        {
            var node = new ParallelNode()
            {
                successPolicy = successPolicy,
                failurePolicy = failurePolicy,
                priority = priority,
                abortOthersOnResolve = abortOthersOnResolve,
                skipFinished = skipFinished
            };
            foreach (var child in _children)
            {
                node.Add(child.Clone());
            }
            return node;
        }

        void EnsureStateSize()
        {
            int n = _children?.Count ?? 0;
            if (_states == null) _states = new List<BTStatus>(n);
            while (_states.Count < n) _states.Add(BTStatus.Running);
            if (_states.Count > n) _states.RemoveRange(n, _states.Count - n);
        }
    }
}