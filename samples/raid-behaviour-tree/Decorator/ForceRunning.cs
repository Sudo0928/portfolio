using System;
using Sirenix.OdinInspector;
using ProjectRaid.Runtime.BehaviourTree;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    [Serializable]
    public sealed class ForceRunning : BTDecorator
    {
        public enum ForceWhen
        {
            Always,     // Success/Failure 모두 Running으로 강제
            OnSuccess,  // Success 일 때만 Running으로 강제
            OnFailure   // Failure 일 때만 Running으로 강제
        }

        [LabelText("When")]
        [SerializeField] private ForceWhen _when = ForceWhen.Always;

        [LabelText("Reset child when forced?")]
        [SerializeField] private bool _resetChildOnForce = true;

        // 선택: Safety - 한 프레임에 너무 많이 재시작 방지
        [LabelText("Max force per frame (0=unlimit)")]
        [SerializeField] private int _maxForcePerFrame = 0;

        [NonSerialized] private int _forceCountFrame = -1;
        [NonSerialized] private int _forceCount = 0;

        protected override bool TryTickOverride(BTContext ctx, out BTStatus status)
        {
            if (_child == null)
            {
                status = BTStatus.Failure;
                return true;
            }

            // per-frame 카운터 초기화
            int frame = UnityEngine.Time.frameCount;
            if (_forceCountFrame != frame) { _forceCountFrame = frame; _forceCount = 0; }

            var s = _child.OnTick(ctx);

            if (s == BTStatus.Running)
            {
                status = BTStatus.Running;
                return true;
            }

            bool shouldForce =
                _when == ForceWhen.Always ||
               (_when == ForceWhen.OnSuccess && s == BTStatus.Success) ||
               (_when == ForceWhen.OnFailure && s == BTStatus.Failure);

            if (shouldForce && (_maxForcePerFrame == 0 || _forceCount < _maxForcePerFrame))
            {
                _forceCount++;

                if (_resetChildOnForce)
                    _child.Reset(); // ← 여기! 강제 Running 시 자식 즉시 리셋해서 다음 틱에 재실행됨

                status = BTStatus.Running;
                return true;
            }

            // 강제하지 않는 경우엔 원래 상태 전달
            status = s;
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            _child?.Reset();
            _forceCount = 0;
            _forceCountFrame = -1;
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new ForceRunning
            {
                _child = clonedChild,
                _when = _when,
                _resetChildOnForce = _resetChildOnForce,
                _maxForcePerFrame = _maxForcePerFrame
            };
        }
    }
}
