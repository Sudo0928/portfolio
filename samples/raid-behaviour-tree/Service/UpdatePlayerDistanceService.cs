using ProjectRaid.Character;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Service
{
    public class UpdatePlayerDistanceService : BTService
    {
        [Title("Blackboard")]
        [LabelText("Target Key")]
        public string key = "PlayerSqrDistance";

        private PlayerCharacter player;

        public override void OnInit(BTContext ctx)
        {
            player = ctx.Owner.GetComponent<NPCController>().Player;
            base.OnInit(ctx);
        }

        protected override void Execute(BTContext ctx)
        {
            Debug.Log(player == null);

            if(player != null)
            {
                var distance = Vector3.SqrMagnitude(ctx.Owner.transform.position - player.transform.position);
                ctx.Blackboard.Set(key, distance);
            }
        }

        public override BTNode Clone()
        {
            var clone = new UpdatePlayerDistanceService
            {
                policy = this.policy,
                interval = this.interval,
                accumulateAcrossRuns = this.accumulateAcrossRuns,
                executeOnEnterForInterval = this.executeOnEnterForInterval,
                key = this.key
            };
            clone.SetChild(_child?.Clone());
            return clone;
        }
    }
}