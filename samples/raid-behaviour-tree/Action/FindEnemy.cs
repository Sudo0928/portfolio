using Sirenix.Utilities;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public sealed class FindEnemy : BTActionNode
    {
        [SerializeField] private float _searchRange = 10f;

        public override BTStatus OnExecute(BTContext ctx)
        {
            Collider[] _results =  Physics.OverlapSphere(ctx.Owner.transform.position, _searchRange, LayerMask.GetMask("Enemy"), QueryTriggerInteraction.Ignore);
            if(_results.Length <= 0) return BTStatus.Failure;
            _results.Sort((a, b) => Vector3.SqrMagnitude(a.transform.position - ctx.Owner.transform.position).CompareTo(Vector3.SqrMagnitude(b.transform.position - ctx.Owner.transform.position)));
            ctx.Blackboard.Set("enemy", _results[0]);
            return BTStatus.Success;
        }

        public override BTNode Clone()
        {
            return new FindEnemy() { _searchRange = _searchRange };
        }
    }
}