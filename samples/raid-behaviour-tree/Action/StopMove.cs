using ECM2;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public class StopMove : BTActionNode
    {
        private NavMeshCharacter _navMeshCharacter;

        public override void OnInit(BTContext ctx)
        {
            _navMeshCharacter = ctx.Owner.GetComponent<NavMeshCharacter>();
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            _navMeshCharacter.StopMovement();
            return BTStatus.Success;
        }

        public override BTNode Clone()
        {
            return new StopMove();
        }
    }
}