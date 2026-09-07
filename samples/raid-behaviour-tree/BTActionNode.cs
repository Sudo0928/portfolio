using System;

namespace ProjectRaid.Runtime.BehaviourTree
{
    [Serializable]
    public abstract class BTActionNode : BTNode
    {
        private bool _entered = false;
        public abstract BTStatus OnExecute(BTContext ctx);

        public sealed override BTStatus OnTick(BTContext ctx)
        {
            if(!_entered) { OnEnter(ctx); _entered = true; }
            var result = OnExecute(ctx);
            if(result != BTStatus.Running) { OnExit(ctx, ref result); _entered = false; }
            return Mark(result);
        }

        public override void Reset()
        {
            _entered = false;
        }

        public override void OnInit(BTContext ctx) { }
    }
}