using System;

namespace ProjectRaid.Runtime.BehaviourTree.Composite
{
    [Serializable]
    public class SequenceNode : BTComposite
    {
        public override BTStatus OnTick(BTContext ctx)
        {
            while (_currentIndex < _children.Count)
            {
                BTStatus result = _children[_currentIndex].OnTick(ctx);
                if (result == BTStatus.Running) { return Mark(BTStatus.Running); }
                if (result == BTStatus.Failure) { _currentIndex = 0; return Mark(BTStatus.Failure);}
                _currentIndex++;
            }
            _currentIndex = 0;
            return Mark(BTStatus.Success);
        }

        public override BTNode Clone()
        {
            var node = new SequenceNode();
            foreach (var child in _children)
            {
                node.Add(child.Clone());
            }
            return node;
        }
    }
}