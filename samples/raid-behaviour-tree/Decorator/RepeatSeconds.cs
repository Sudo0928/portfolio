using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    public sealed class RepeatSeconds : BTDecorator
    {
        [SerializeField] private float _seconds;
        private bool _isRunning = false;

        protected override void OnEnter(BTContext ctx)
        {
            if(!_isRunning) 
            {
                _isRunning = true;
                WaitForSeconds(_seconds).Forget();
            }
        }

        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            if (status == BTStatus.Success && _isRunning)
            {
                // 자식이 성공했지만 아직 목표 횟수 미달 → 반복 진행
                // 자식은 자체적으로 완료 처리했으므로 다음 틱에 재-진입됨
                status = BTStatus.Running;
                // 필요 시: _child.Reset();  // Reset 훅을 도입했다면 여기서 호출
                _child?.Reset();
            }
        }

        private async UniTaskVoid WaitForSeconds(float seconds)
        {
            await UniTask.WaitForSeconds(seconds);
            _isRunning = false;
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new RepeatSeconds() { _child = clonedChild, _seconds = _seconds };
        }
    }
}