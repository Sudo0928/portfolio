using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree
{
    public sealed class BTContext
    {
        public GameObject Owner { get; }
        public IBlackboard Blackboard { get; }
        public float DeltaTime { get; set; }
        public BTContext(GameObject owner, IBlackboard blackboard, float deltaTime) => (Owner, Blackboard, DeltaTime) = (owner, blackboard, deltaTime);
    }
}