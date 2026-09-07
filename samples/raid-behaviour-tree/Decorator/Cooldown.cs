using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    public sealed class Cooldown : BTDecorator
    {
        /// <summary>
        /// 쿨다운 시간(seconds)
        /// </summary>
        [SerializeField] private float _cooldown;
        /// <summary>
        /// 성공 시에만 쿨다운 적용
        /// </summary>
        [SerializeField] private bool successOnly = false;
        private bool _isCooldown = false;

        protected override bool CanTick(BTContext ctx)
        {
            if(!_isCooldown) return true;
            return false;
        }

        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            if (successOnly)
            {
                if (status == BTStatus.Success)
                    WaitForCooldown().Forget();
            }
            else
            {
                if (status != BTStatus.Running)
                    WaitForCooldown().Forget();
            }
        }

        private async UniTaskVoid WaitForCooldown()
        {
            _isCooldown = true;
            await UniTask.WaitForSeconds(_cooldown);
            _isCooldown = false;
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new Cooldown { _child = clonedChild, _cooldown = _cooldown, successOnly = successOnly };
        }
    }
}