using ProjectRaid.Character;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Service
{
    public class UpdatePlayerStateService : BTService
    {
        private PlayerCharacter player;
        [SerializeField] private string PlayerMode = "PlayerMode";
        [SerializeField] private string PlayerState = "PlayerState";

        public override void OnInit(BTContext ctx)
        {
            player = ctx.Owner.GetComponent<NPCController>().Player;
            base.OnInit(ctx);
        }

        protected override void Execute(BTContext ctx)
        {
            ctx.Blackboard.Set(PlayerMode, player.Movement.CurrentAnimationMode);
            ctx.Blackboard.Set(PlayerState, player.Movement.CurrentCharacterState);
        }

        public override BTNode Clone()
        {
            var clone = new UpdatePlayerStateService
            {
                policy = this.policy,
                interval = this.interval,
                accumulateAcrossRuns = this.accumulateAcrossRuns,
                executeOnEnterForInterval = this.executeOnEnterForInterval,
                PlayerMode = this.PlayerMode,
                PlayerState = this.PlayerState
            };
            clone.SetChild(_child?.Clone());
            return clone;
        }
    }
}