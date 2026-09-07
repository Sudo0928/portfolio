using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    public class IntCondition : ConditionGuardBase
    {
        [SerializeField] private string _intValueKey;
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetOps))]
#endif
        [SerializeField] private CompareOperator _op = CompareOperator.GreaterOrEqual;
        [ShowIf("@_op != CompareOperator.Exists && _op != CompareOperator.NotExists")]
        [SerializeField] private int _intValue;

        protected override bool Evaluate(BTContext ctx)
        {
            int value = 0;

            if (_op == CompareOperator.Exists || _op == CompareOperator.NotExists)
            {
                if (ctx.Blackboard.TryGet(_intValueKey, out value)) return _op == CompareOperator.Exists ? true : false;
                return false;
            }

            if (ctx.Blackboard.TryGet(_intValueKey, out value))
            {
                return _op switch
                {
                    CompareOperator.Equal => value == _intValue,
                    CompareOperator.NotEqual => value != _intValue,
                    CompareOperator.Less => value < _intValue,
                    CompareOperator.LessOrEqual => value <= _intValue,
                    CompareOperator.Greater => value > _intValue,
                    CompareOperator.GreaterOrEqual => value >= _intValue,
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
            return new IntCondition()
            {
                _child = clonedChild,
                _reevalPolicy = _reevalPolicy,
                _useFallback = _useFallback,
                _fallbackNode = _fallbackNode?.Clone(),
                _triggerMode = _triggerMode,
                _fallbackWhen = _fallbackWhen,
                _blockedReturn = _blockedReturn,

                _intValueKey = _intValueKey,
                _op = _op,
                _intValue = _intValue,
            };
        }
    }
}