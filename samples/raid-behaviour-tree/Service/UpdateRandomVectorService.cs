using UnityEngine;
using ProjectRaid.Runtime.BehaviourTree;
using Sirenix.OdinInspector;

namespace ProjectRaid.Runtime.BehaviourTree.Service
{
    [System.Serializable]
    public class UpdateRandomVectorService : BTService
    {
        [Title("Blackboard")]
        [LabelText("Target Key")]
        public string key = "RandomLocation";

        [LabelText("Random Range")]
        public Vector2 rangeX = new Vector2(-5, 5);
        public Vector2 rangeY = new Vector2(0, 1);
        public Vector2 rangeZ = new Vector2(-5, 5);

        protected override void Execute(BTContext ctx)
        {
            Vector3 random = new Vector3(
                Random.Range(rangeX.x, rangeX.y),
                Random.Range(rangeY.x, rangeY.y),
                Random.Range(rangeZ.x, rangeZ.y)
            );
            ctx.Blackboard.Set(key, random);
        }

        public override BTNode Clone()
        {
            var clone = new UpdateRandomVectorService
            {
                policy = this.policy,
                interval = this.interval,
                accumulateAcrossRuns = this.accumulateAcrossRuns,
                executeOnEnterForInterval = this.executeOnEnterForInterval,
                key = this.key,
                rangeX = this.rangeX,
                rangeY = this.rangeY,
                rangeZ = this.rangeZ
            };
            clone.SetChild(_child?.Clone());
            return clone;
        }
    }
}