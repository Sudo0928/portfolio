using UnityEngine.AI;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public sealed class Idle : BTActionNode
    {
        public override BTStatus OnExecute(BTContext ctx)
        {
            return BTStatus.Success;
        }

        public override BTNode Clone()
        {
            return new Idle();
        }
    }
}