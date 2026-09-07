// SequenceWithObserver.cs
using System;
using ProjectRaid.Runtime.BehaviourTree;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Composite
{
    [Serializable]
    public class SequenceWithObserver : SequenceNode
    {
        private IBlackboard _bb;
        private bool _aborted;

        [SerializeField] private string[] _observeKeys; // 감시할 블랙보드 키들

        public override void OnInit(BTContext ctx)
        {
            base.OnInit(ctx);
            _bb = ctx.Blackboard;
            if (_bb != null && _observeKeys != null)
            {
                foreach (var key in _observeKeys)
                    _bb.OnKeyChanged += __OnBBChanged;
            }
        }

        private void __OnBBChanged(string key)
        {
            if (_observeKeys == null || Array.IndexOf(_observeKeys, key) == -1)
                return;

            // 현재 실행 중이던 자식 중단 요청
            _aborted = true;
        }

        public override BTStatus OnTick(BTContext ctx)
        {
            if (_aborted)
            {
                // 현재 자식 중단 및 전체 재평가
                if (_currentIndex < _children.Count)
                {
                    _children[_currentIndex]?.Reset();
                }
                _currentIndex = 0;
                _aborted = false;
            }

            while (_currentIndex < _children.Count)
            {
                BTStatus result = _children[_currentIndex].OnTick(ctx);
                if (result == BTStatus.Running) return Mark(BTStatus.Running);
                if (result == BTStatus.Failure)
                {
                    _currentIndex = 0;
                    return Mark(BTStatus.Failure);
                }
                _currentIndex++;
            }

            _currentIndex = 0;
            return Mark(BTStatus.Success);
        }

        public override void Reset()
        {
            base.Reset();
            _aborted = false;
        }

        public override BTNode Clone()
        {
            var node = new SequenceWithObserver()
            {
                _observeKeys = (string[])_observeKeys?.Clone()
            };
            foreach (var child in _children)
                node.Add(child.Clone());
            return node;
        }
    }
}
