// OnFlagEnterOnce.cs
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    /// <summary>
    /// 블랙보드의 bool 플래그가 false->true로 바뀌는 "진입(상승 에지)" 시점에만
    /// _onEnter(예: Weapon_Equip)를 1회 실행하고, 그 이후에는 자식(Combat 루프)만 틱한다.
    /// 플래그가 다시 false가 되면 1회 실행 메모리를 해제한다.
    /// </summary>
    [Serializable]
    public sealed class OnFlagEnterOnce : BTDecorator
    {
        [Title("Flag (Blackboard)")]
        [LabelText("Bool Flag Key")]
        public string flagKey = "IsCombat";

        [Title("On Enter (runs once when flag turns true)")]
        [SerializeReference, InlineProperty, HideLabel]
        private BTNode _onEnter; // 예: Weapon_Equip

        [Title("Memory (optional)")]
        [LabelText("Use Blackboard Memory Key (bool)")]
        public string memoryKey = "_combatEquipped"; // 비우면 내부 메모리만 사용

        [Title("When Flag is FALSE")]
        [LabelText("Return Failure (block child)")]
        public bool returnFailureWhenFlagOff = true;

        // 내부 메모리: 이번 Combat 세션에서 onEnter를 이미 수행했는가?
        [NonSerialized] private bool _doneThisSession = false;

        public override void OnInit(BTContext ctx)
        {
            _onEnter?.OnInit(ctx);
            base.OnInit(ctx);
        }

        // 자식 틱 전에 개입
        protected override bool TryTickOverride(BTContext ctx, out BTStatus status)
        {
            bool inFlag = false;
            ctx.Blackboard.TryGet(flagKey, out inFlag); // 없는 키면 기본 false 취급

            // Combat 아님: 메모리 해제(+ 필요시 실패로 막기)
            if (!inFlag)
            {
                SetMemory(ctx, false);
                _onEnter?.Reset();

                if (returnFailureWhenFlagOff)
                {
                    status = BTStatus.Failure;
                    return true; // 자식 틱 금지
                }

                status = default; // 통과해서 자식으로
                return false;
            }

            // Combat 중: onEnter 1회만 실행
            if (!GetMemory(ctx))
            {
                if (_onEnter == null)
                {
                    SetMemory(ctx, true);
                }
                else
                {
                    var s = _onEnter.OnTick(ctx);
                    if (s == BTStatus.Running)
                    {
                        status = BTStatus.Running;
                        return true; // onEnter 도는 동안 자식 금지
                    }
                    if (s == BTStatus.Failure)
                    {
                        // 장비 장착 실패 등: 상위 정책상 막고 싶으면 Failure 반환
                        _onEnter.Reset();
                        status = BTStatus.Failure;
                        return true;
                    }

                    // Success → 이번 Combat 세션 완료 표시
                    SetMemory(ctx, true);
                }
            }

            // onEnter가 끝났거나 이미 완료 → 자식 틱
            status = default;
            return false;
        }

        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            // 여기서는 _doneThisSession을 유지한다(Combat이 유지되는 동안은 다시 실행 금지).
            // Combat이 끝나면(= flag가 false가 되는 틱) TryTickOverride에서 메모리를 해제한다.
        }

        public override void Reset()
        {
            // Reset이 들어오더라도 Combat 중에는 재장전을 막고 싶다면
            // 내부 플래그를 바로 지우지 않는 편이 안전하다(블랙보드 메모리 사용을 권장).
            _onEnter?.Reset();
            base.Reset();
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new OnFlagEnterOnce
            {
                flagKey = flagKey,
                _onEnter = _onEnter?.Clone(),
                memoryKey = memoryKey,
                returnFailureWhenFlagOff = returnFailureWhenFlagOff,
                _child = clonedChild
            };
        }

        // ===== 메모리 도우미 =====
        private bool GetMemory(BTContext ctx)
        {
            if (!string.IsNullOrEmpty(memoryKey))
            {
                if (ctx.Blackboard.TryGet(memoryKey, out bool v)) return v;
                return false;
            }
            return _doneThisSession;
        }

        private void SetMemory(BTContext ctx, bool v)
        {
            if (!string.IsNullOrEmpty(memoryKey))
                ctx.Blackboard.Set(memoryKey, v);
            else
                _doneThisSession = v;
        }
    }
}
