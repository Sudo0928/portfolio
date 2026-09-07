namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    public sealed class Succeeder : BTDecorator
    {
        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            status = BTStatus.Success;
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new Succeeder { _child = clonedChild };
        }
    }
}