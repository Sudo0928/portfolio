using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public class RecoveryHp : BTActionNode
    {
        [SerializeField] private float _maxRecoveryAmount = 100f;
        [SerializeField] private float _recoveryAmount = 10f;
        [SerializeField] private float _recoveryInterval = 1f;

        private NPCController _npcController;

        private float _timer = 0f;

        public override void OnInit(BTContext ctx)
        {
            base.OnInit(ctx);
            _npcController = ctx.Owner.GetComponent<NPCController>();
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            _timer += ctx.DeltaTime;
            if(_timer >= _recoveryInterval)
            {
                _npcController.Heal(_recoveryAmount);
                _timer = 0f;

                ctx.Blackboard.TryGet<float>("CurrentHealth", out var currentHealth);
                Debug.Log($"RecoveryHp: {currentHealth}");

                if(currentHealth >= _maxRecoveryAmount) return Mark(BTStatus.Success);
                return Mark(BTStatus.Running);
            }
            return BTStatus.Running;
        }

        public override BTNode Clone()
        {
            return new RecoveryHp() { _maxRecoveryAmount = _maxRecoveryAmount, _recoveryAmount = _recoveryAmount, _recoveryInterval = _recoveryInterval };
        }
    }
}