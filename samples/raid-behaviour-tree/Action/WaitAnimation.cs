using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public class WaitAnimation : BTActionNode
    {
        private Animator _animator;

        public override void OnInit(BTContext ctx)
        {
            _animator = ctx.Owner.GetComponentInChildren<Animator>();
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            if(_animator == null) return BTStatus.Failure;

            if(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) return BTStatus.Running;

            return BTStatus.Success;
        }

        public override BTNode Clone()
        {
            return new WaitAnimation();
        }
    }
}