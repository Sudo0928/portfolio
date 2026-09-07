namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    public sealed class Inverter : BTDecorator
    {
        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            status = status == BTStatus.Success ? BTStatus.Failure : BTStatus.Success;
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new Inverter { _child = clonedChild };
        }
    }
}