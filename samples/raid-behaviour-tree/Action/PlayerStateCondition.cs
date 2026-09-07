using ProjectRaid.Character;
using ProjectRaid.Character.Module;
using UnityEngine;
using static ProjectRaid.Character.Module.MovementModule;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public class PlayerStateCondition : BTActionNode
    {
        [SerializeField] private AnimationMode _animationMode;
        private MovementModule _movementModule;

        public override void OnInit(BTContext ctx)
        {
            _movementModule = ctx.Owner.GetComponent<NPCController>().Player.Movement;
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            return _movementModule.CurrentAnimationMode == _animationMode ? BTStatus.Success : BTStatus.Failure;
        }

        public override BTNode Clone()
        {
            return new PlayerStateCondition() { _animationMode = _animationMode };
        }
    }
}