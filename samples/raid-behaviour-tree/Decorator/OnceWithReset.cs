// OnceWithReset.cs
using System;
using ProjectRaid.Runtime.BehaviourTree;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    [Serializable]
    public sealed class OnceWithReset : BTDecorator
    {
        [SerializeField] private bool _onlyOnceOnFailure = false;
        [SerializeField] private string _resetKey; // 이 키 값이 변경되면 다시 실행 허용

        private bool _alreadyRun = false;
        private IBlackboard _bb;

        public override void OnInit(BTContext ctx)
        {
            base.OnInit(ctx);
            _bb = ctx.Blackboard;
            if (_bb != null && !string.IsNullOrEmpty(_resetKey))
                _bb.OnKeyChanged += __OnKeyChanged;
        }

        private void __OnKeyChanged(string key)
        {
            if (key == _resetKey)
                _alreadyRun = false;
        }

        protected override bool CanTick(BTContext ctx)
        {
            return !_alreadyRun;
        }

        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            if (!_alreadyRun)
            {
                if (!_onlyOnceOnFailure || status == BTStatus.Failure)
                    _alreadyRun = true;
            }
        }

        public override void Reset()
        {
            base.Reset();
            // Note: _alreadyRun remains as-is unless reset key triggers it
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new OnceWithReset
            {
                _child = clonedChild,
                _onlyOnceOnFailure = _onlyOnceOnFailure,
                _resetKey = _resetKey
            };
        }
    }
}
