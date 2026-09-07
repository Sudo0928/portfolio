using ProjectRaid.Character;
using UnityEngine.AI;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public class CheckVailidTeleport : BTActionNode
    {
        private NavMeshAgent agent;
        private PlayerCharacter player;

        public override void OnInit(BTContext ctx)
        {
            base.OnInit(ctx);
            agent = ctx.Owner.GetComponent<NavMeshAgent>();
            player = ctx.Owner.GetComponent<NPCController>().Player;
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            if (!NavMesh.SamplePosition(player.transform.position, out var hit, 1, agent.areaMask))
                return BTStatus.Failure;

            // 2) 에이전트 타입/마스크 필터로 '로컬 자체 경로' 검사
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };

            var path = new NavMeshPath();
            if (NavMesh.CalculatePath(hit.position, hit.position, filter, path) &&
            path.status == NavMeshPathStatus.PathComplete)
                return BTStatus.Success;
            return BTStatus.Failure;
        }

        public override BTNode Clone()
        {
            return new CheckVailidTeleport();
        }
    }
}