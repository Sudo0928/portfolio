using System;
using System.Collections.Generic;
using ProjectRaid.Runtime.BehaviourTree;
using ProjectRaid.Runtime.BehaviourTree.Decorator;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    [Serializable]
    public sealed class FloatCondition : ConditionGuardBase
    {
        [SerializeField] private string _floatValueKey;
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetOps))]
#endif
        [SerializeField] private CompareOperator _op = CompareOperator.GreaterOrEqual;
        [ShowIf("@_op != CompareOperator.Exists && _op != CompareOperator.NotExists")]
        [SerializeField] private float _floatValue = 0f;

        protected override bool Evaluate(BTContext ctx)
        {
            float value = 0f;

            if (_op == CompareOperator.Exists || _op == CompareOperator.NotExists)
            {
                if (ctx.Blackboard.TryGet(_floatValueKey, out value)) return _op == CompareOperator.Exists ? true : false;
                return false;
            }

            if (ctx.Blackboard.TryGet<float>(_floatValueKey, out value))
            {
                return _op switch
                {
                    CompareOperator.Equal => Mathf.Approximately(value, _floatValue),
                    CompareOperator.NotEqual => !Mathf.Approximately(value, _floatValue),
                    CompareOperator.Less => value < _floatValue,
                    CompareOperator.LessOrEqual => value <= _floatValue,
                    CompareOperator.Greater => value > _floatValue,
                    CompareOperator.GreaterOrEqual => value >= _floatValue,
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
            yield return CompareOperator.Greater;
            yield return CompareOperator.GreaterOrEqual;
            yield return CompareOperator.Less;
            yield return CompareOperator.LessOrEqual;
        }
#endif

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new FloatCondition
            {
                _child = clonedChild,
                _reevalPolicy = _reevalPolicy,
                _useFallback = _useFallback,
                _fallbackNode = _fallbackNode?.Clone(),
                _triggerMode = _triggerMode,
                _fallbackWhen = _fallbackWhen,
                _blockedReturn = _blockedReturn,

                _floatValueKey = _floatValueKey,
                _op = _op,
                _floatValue = _floatValue,
            };
        }
    }
}