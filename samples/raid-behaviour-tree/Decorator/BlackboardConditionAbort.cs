// BlackboardConditionAbort.cs
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    [Serializable]
    public sealed class BlackboardConditionAbort : ConditionGuardBase
    {
        enum ValueType
        {
            String,
            Float,
            Int,
            Bool,
            Vector3,
        }
        
        [SerializeField] private string _key;

        [SerializeField] private ValueType valueType;
        [SerializeField, ShowIf("@valueType == ValueType.String")] private string stringValue;
        [SerializeField, ShowIf("@valueType == ValueType.Float")] private float floatValue;
        [SerializeField, ShowIf("@valueType == ValueType.Int")] private int intValue;
        [SerializeField, ShowIf("@valueType == ValueType.Bool")] private bool boolValue;
        [SerializeField, ShowIf("@valueType == ValueType.Vector3")] private Vector3 vector3Value;

        public override void OnInit(BTContext ctx)
        {
            base.OnInit(ctx);
            // 필요 시 키 변경 이벤트 등록도 가능
        }

        protected override bool Evaluate(BTContext ctx)
        {
            var bb = ctx.Blackboard;
            if (bb == null) return false;

            switch (valueType)
            {
                case ValueType.String:
                    if (!bb.TryGet<string>(_key, out string stringVal)) return false;
                    return stringVal == stringValue;
                case ValueType.Float:
                    if (!bb.TryGet<float>(_key, out float floatVal)) return false;
                    return floatVal == floatValue;
                case ValueType.Int:
                    if (!bb.TryGet<int>(_key, out int intVal)) return false;
                    return intVal == intValue;
                case ValueType.Bool:
                    if (!bb.TryGet<bool>(_key, out bool boolVal)) return false;
                    return boolVal == boolValue;
                case ValueType.Vector3:
                    if (!bb.TryGet<Vector3>(_key, out Vector3 vector3Val)) return false;
                    return vector3Val == vector3Value;
                default:
                    return false;
            }
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new BlackboardConditionAbort
            {
                _child = clonedChild,
                _key = _key,
                valueType = valueType,
                stringValue = stringValue,
                floatValue = floatValue,
                intValue = intValue,
                boolValue = boolValue,
                vector3Value = vector3Value,
                // 상속 필드 복사
                _useFallback = _useFallback,
                _fallbackNode = _fallbackNode?.Clone(),
                _reevalPolicy = _reevalPolicy,
                _triggerMode = _triggerMode,
                _fallbackWhen = _fallbackWhen,
                _blockedReturn = _blockedReturn
            };
        }
    }
}
