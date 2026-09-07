using System;
using Sirenix.OdinInspector;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    /// <summary>
    /// 자식을 N회 반복 실행합니다.
    /// - allowFailureRepeat=false: 자식이 한 번이라도 Failure면 즉시 Failure 전파
    /// - allowFailureRepeat=true : 자식이 Success든 Failure든 회차를 소진할 때까지 반복 후 Success 반환
    /// </summary>
    [Serializable]
    public sealed class RepeatCount : BTDecorator
    {
        [LabelText("Allow Failure Repeat")] public bool allowFailureRepeat = false;
        [MinValue(1), LabelText("Count")] public int count = 1;

        [NonSerialized] private int _remaining;

        protected override void OnEnter(BTContext ctx)
        {
            _remaining = Math.Max(1, count);
        }

        /// <summary>
        /// 자식이 Running이 아닌 상태로 종료될 때마다 호출됩니다.
        /// 여기서 반복 여부를 결정하고 status를 변형합니다.
        /// </summary>
        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            // 자식이 실패했는데 실패 반복을 허용하지 않으면 즉시 실패 전파
            if (status == BTStatus.Failure && !allowFailureRepeat)
            {
                // status 그대로 Failure 유지
                return;
            }

            // 한 회차 종료(Success 또는 Failure-허용)
            _remaining--;

            if (_remaining > 0)
            {
                // 회차가 남았으면 자식을 리셋하고, 반복을 계속하기 위해 Running으로 변환
                _child?.Reset();
                status = BTStatus.Running;
            }
            else
            {
                // 모든 회차 완료 → 최종 성공으로 처리
                status = BTStatus.Success;
            }
        }

        public override void Reset()
        {
            _remaining = 0;
            base.Reset(); // BTDecorator: _entered=false 및 자식 Reset
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new RepeatCount
            {
                allowFailureRepeat = this.allowFailureRepeat,
                count = this.count,
                _child = clonedChild
            };
        }
    }
}
