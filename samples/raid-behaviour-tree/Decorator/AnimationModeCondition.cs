using System;
using System.Collections.Generic;
using ProjectRaid.Character.Module;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    [Serializable]
    public sealed class AnimationModeCondition : ConditionGuardBase
    {
        [Title("Animation Mode")]
        [SerializeField] private string _animModeKey;
        
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetOps))]
#endif
        [SerializeField] private CompareOperator _op = CompareOperator.Equal;
        [ShowIf("@_op != CompareOperator.Exists && _op != CompareOperator.NotExists")]
        [SerializeField] private MovementModule.AnimationMode _animModeValue = default;

        protected override bool Evaluate(BTContext ctx)
        {
            if (ctx.Blackboard.TryGet<MovementModule.AnimationMode>(_animModeKey, out var a))
            {
                return _op switch
                {
                    CompareOperator.Equal => a == _animModeValue,
                    CompareOperator.NotEqual => a != _animModeValue,
                    _ => false
                };
            }
            return false;
        }

#if UNITY_EDITOR
        private IEnumerable<CompareOperator> GetOps()
        {
            yield return CompareOperator.Exists;
            yield return CompareOperator.NotExists;
            yield return CompareOperator.Equal;
            yield return CompareOperator.NotEqual;
        }
#endif

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new AnimationModeCondition
            {
                _child = clonedChild,
                _reevalPolicy = _reevalPolicy,
                _useFallback = _useFallback,
                _fallbackNode = _fallbackNode?.Clone(),
                _triggerMode = _triggerMode,
                _fallbackWhen = _fallbackWhen,
                _blockedReturn = _blockedReturn,

                _animModeKey = _animModeKey,
                _op = _op,
                _animModeValue = _animModeValue
            };
        }
    }
}

