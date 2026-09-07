using ECM2;
using ProjectRaid.Character;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public class TeleportToPlayer : BTActionNode
    {
        private NPCController npcController;
        private NavMeshCharacter navMeshCharacter;
        private PlayerCharacter player;

        public override void OnInit(BTContext ctx)
        {
            npcController = ctx.Owner.GetComponent<NPCController>();
            player = npcController.Player;
            navMeshCharacter = npcController.NavMeshCharacter;
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            if(player == null) return BTStatus.Failure;

            Vector3 targetPosition = player.transform.position + player.transform.forward * -1f;
            navMeshCharacter.agent.Warp(targetPosition);
            navMeshCharacter.character.characterMovement.position = targetPosition;
            return BTStatus.Success;
        }

        public override BTNode Clone()
        {
            return new TeleportToPlayer();
        }
    }
}