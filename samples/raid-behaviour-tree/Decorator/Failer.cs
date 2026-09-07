namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    public sealed class Failer : BTDecorator
    {
        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            status = BTStatus.Failure;
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new Failer { _child = clonedChild };
        }
    }
}