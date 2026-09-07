using UnityEngine;
using UnityEngine.AI;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public sealed class MoveNode : BTActionNode
    {
#pragma warning disable CS0414 // HACK: 변수가 사용되지 않고있어서 경고 무시 (나중에 사용 할 수 있음)
        private bool isInitialized = false;
#pragma warning restore CS0414

        private NavMeshAgent _navMeshAgent;

        public override void OnInit(BTContext ctx)
        {
            base.OnInit(ctx);

            _navMeshAgent = ctx.Owner.GetComponent<NavMeshAgent>();
            isInitialized = true;
        }

        protected override void OnEnter(BTContext ctx)
        {
            
            _navMeshAgent.isStopped = false;
            
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            if(!ctx.Blackboard.TryGet<Vector3>("targetPosition", out var targetPos)) return BTStatus.Failure;
            
            // MoveNode 예시
            if (!_navMeshAgent.isOnNavMesh) return BTStatus.Failure;

            _navMeshAgent.SetDestination(targetPos);

            // 같은 프레임에 남은 거리 확인 X
            // 경로가 준비된 다음 프레임들에서만 판정
            if (!_navMeshAgent.pathPending && _navMeshAgent.hasPath)
            {
                if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
                    return BTStatus.Success; // 도착
                else
                    return BTStatus.Running; // 이동 중
            }

            return BTStatus.Running; // 아직 경로 계산 중
        }

        public override BTNode Clone()
        {
            return new MoveNode();
        }
    }
}