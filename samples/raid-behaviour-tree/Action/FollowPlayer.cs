using ECM2;
using KinematicCharacterController;
using ProjectRaid.Character;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public class FollowPlayer : BTActionNode
    {
        private NavMeshCharacter _navMeshCharacter;
        private PlayerCharacter player;
        private KinematicCharacterMotor moter;
        private ECM2.Character character;

        public override void OnInit(BTContext ctx)
        {
            _navMeshCharacter = ctx.Owner.GetComponent<NavMeshCharacter>();
            player = ctx.Owner.GetComponent<NPCController>().Player;
            moter = player.GetComponent<KinematicCharacterMotor>();
            character = ctx.Owner.GetComponent<ECM2.Character>();
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            if(player == null) return BTStatus.Failure;

            character.maxWalkSpeed = Mathf.Max(5f, moter.Velocity.magnitude);
            
            // MoveNode 예시
            if (!_navMeshCharacter.agent.isOnNavMesh) return BTStatus.Failure;

            _navMeshCharacter.MoveToDestination(player.transform.position);

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
            return new FollowPlayer();
        }
    }
}