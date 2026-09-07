using System;

namespace ProjectRaid.Runtime.BehaviourTree.Composite
{
    [Serializable]
    public class SelectorNode : BTComposite
    {
        public override BTStatus OnTick(BTContext ctx)
        {
            while (_currentIndex < _children.Count)
            {
                BTStatus result = _children[_currentIndex].OnTick(ctx);
                if (result == BTStatus.Running) { return Mark(BTStatus.Running); }
                if (result == BTStatus.Success) { _currentIndex = 0; return Mark(BTStatus.Success); }
                _currentIndex++;
            }
            _currentIndex = 0;
            return Mark(BTStatus.Failure);
        }

        public override BTNode Clone()
        {
            var node = new SelectorNode();
            foreach (var child in _children)
            {
                node.Add(child.Clone());
            }
            return node;
        }
    }
}