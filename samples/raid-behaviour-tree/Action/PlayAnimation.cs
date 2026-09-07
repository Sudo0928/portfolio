using FishNet.Component.Animating;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public class PlayAnimation : BTActionNode
    {
        private NetworkAnimator _networkAnimator;

        [FoldoutGroup("PlayAnimation")][SerializeField] private string _animationName;

        public override void OnInit(BTContext ctx)
        {
            _networkAnimator = ctx.Owner.GetComponentInChildren<NetworkAnimator>();
        }

        public override BTStatus OnExecute(BTContext context)
        {
            if(!_networkAnimator.Animator.IsInTransition(0)) _networkAnimator.CrossFade(_animationName, 0.1f, 0);
            else return BTStatus.Running;
            return BTStatus.Success;
        }

        public override BTNode Clone()
        {
            return new PlayAnimation() { _animationName = this._animationName };
        }
    }
}
