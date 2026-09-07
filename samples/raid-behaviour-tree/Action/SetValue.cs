using ProjectRaid.Character.Module;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public sealed class SetValue : BTActionNode
    {
        enum ValueType
        {
            String,
            Float,
            Int,
            Bool,
            Vector3,
            AnimationModeEnum,
        }
        
        public string key;

        [SerializeField] private ValueType valueType;
        [SerializeField, ShowIf("@valueType == ValueType.String")] private string stringValue;
        [SerializeField, ShowIf("@valueType == ValueType.Float")] private float floatValue;
        [SerializeField, ShowIf("@valueType == ValueType.Int")] private int intValue;
        [SerializeField, ShowIf("@valueType == ValueType.Bool")] private bool boolValue;
        [SerializeField, ShowIf("@valueType == ValueType.Vector3")] private Vector3 vector3Value;
        [SerializeField, ShowIf("@valueType == ValueType.AnimationModeEnum")] private MovementModule.AnimationMode animationModeEnumValue;

        public override BTStatus OnExecute(BTContext ctx)
        {
            switch (valueType)
            {
                case ValueType.String:
                    ctx.Blackboard.Set(key, stringValue);
                    break;
                case ValueType.Float:
                    ctx.Blackboard.Set(key, floatValue);
                    break;
                case ValueType.Int:
                    ctx.Blackboard.Set(key, intValue);
                    break;
                case ValueType.Bool:
                    ctx.Blackboard.Set(key, boolValue);
                    break;
                case ValueType.Vector3:
                    ctx.Blackboard.Set(key, vector3Value);
                    break;
                case ValueType.AnimationModeEnum:
                    ctx.Blackboard.Set(key, animationModeEnumValue);
                    break;
            }
            return BTStatus.Success;
        }

        public override BTNode Clone()
        {
            return new SetValue { key = key, valueType = valueType, stringValue = stringValue, floatValue = floatValue, intValue = intValue, boolValue = boolValue, vector3Value = vector3Value, animationModeEnumValue = animationModeEnumValue };
        }
    }
}