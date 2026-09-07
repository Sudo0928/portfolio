using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public class OnOffStun : BTActionNode
    {
        [SerializeField] private bool _isOnStun = false;

        private NPCController _npcController;

        public override void OnInit(BTContext ctx)
        {
            base.OnInit(ctx);
            _npcController = ctx.Owner.GetComponent<NPCController>();
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            ctx.Blackboard.Set("IsStuned", _isOnStun);
            _npcController.isStun = _isOnStun;
            return BTStatus.Success;
        }

        public override BTNode Clone()
        {
            return new OnOffStun() { _isOnStun = _isOnStun };
        }
    }
}