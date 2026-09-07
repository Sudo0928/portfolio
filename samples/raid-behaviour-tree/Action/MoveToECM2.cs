using System.Collections;
using System.Collections.Generic;
using ECM2;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public class MoveToECM2 : BTActionNode
    {
        [FoldoutGroup("MoveToECM2")][SerializeField] private string _targetPosKey;
        private NavMeshCharacter _navMeshCharacter;

        public override void OnInit(BTContext ctx)
        {
            _navMeshCharacter = ctx.Owner.GetComponent<NavMeshCharacter>();
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            if(!ctx.Blackboard.TryGet<Vector3>(_targetPosKey, out var targetPos)) return BTStatus.Failure;
            
            // MoveNode 예시
            if (!_navMeshCharacter.agent.isOnNavMesh) return BTStatus.Failure;

            _navMeshCharacter.MoveToDestination(targetPos);

            // 같은 프레임에 남은 거리 확인 X
            // 경로가 준비된 다음 프레임들에서만 판정
            if (!_navMeshCharacter.agent.pathPending && _navMeshCharacter.agent.hasPath)
            {
                if (_navMeshCharacter.agent.remainingDistance <= _navMeshCharacter.agent.stoppingDistance)
                    return BTStatus.Success; // 도착
                else
                    return BTStatus.Running; // 이동 중
            }

            return BTStatus.Running; // 아직 경로 계산 중
        }

        public override BTNode Clone()
        {
            return new MoveToECM2();
        }
    }
}
