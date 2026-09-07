namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    public sealed class RepeatUntilSuccess : BTDecorator
    {
        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            if(status == BTStatus.Success) return;
            else if(status == BTStatus.Failure) { _child?.Reset(); status = BTStatus.Running; }
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new RepeatUntilSuccess { _child = clonedChild };
        }
    }
}