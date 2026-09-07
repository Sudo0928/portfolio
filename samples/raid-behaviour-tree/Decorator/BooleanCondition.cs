using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    public class BooleanCondition : ConditionGuardBase
    {
        [SerializeField] private string _boolValueKey;
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetOps))]
#endif
        [SerializeField] private CompareOperator _op = CompareOperator.GreaterOrEqual;
        [ShowIf("@_op != CompareOperator.Exists && _op != CompareOperator.NotExists")]
        [SerializeField] private bool _boolValue = false;

        protected override bool Evaluate(BTContext ctx)
        {
            bool value = false;

            if (_op == CompareOperator.Exists || _op == CompareOperator.NotExists)
            {
                if (ctx.Blackboard.TryGet(_boolValueKey, out value)) return _op == CompareOperator.Exists ? true : false;
                return false;
            }

            if (ctx.Blackboard.TryGet(_boolValueKey, out value))
            {
                return _op switch
                {
                    CompareOperator.Equal => value == _boolValue,
                    CompareOperator.NotEqual => value != _boolValue,
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
            return new BooleanCondition
            {
                _child = clonedChild,
                _reevalPolicy = _reevalPolicy,
                _useFallback = _useFallback,
                _fallbackNode = _fallbackNode?.Clone(),
                _triggerMode = _triggerMode,
                _fallbackWhen = _fallbackWhen,
                _blockedReturn = _blockedReturn,

                _boolValueKey = _boolValueKey,
                _op = _op,
                _boolValue = _boolValue,
            };
        }
    }
}