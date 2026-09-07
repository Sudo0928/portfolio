using FishNet.Component.Animating;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public enum AnimationParameterType
    {
        Bool,
        Float,
        Int,
        Trigger
    }

    public class SetAnimationParameter : BTActionNode
    {
        [SerializeField, EnumToggleButtons] private AnimationParameterType _parameterType;
        [SerializeField] private string _parameterName;
        [SerializeField, ShowIf("@_parameterType == AnimationParameterType.Float")] private float _floatValue;
        [SerializeField, ShowIf("@_parameterType == AnimationParameterType.Int")] private int _intValue;
        [SerializeField, ShowIf("@_parameterType == AnimationParameterType.Bool")] private bool _boolValue;
        
        private NetworkAnimator _networkAnimator;

        public override void OnInit(BTContext ctx)
        {
            _networkAnimator = ctx.Owner.GetComponentInChildren<NetworkAnimator>();
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            switch(_parameterType)
            {
                case AnimationParameterType.Bool:
                    _networkAnimator.Animator.SetBool(_parameterName, true);
                    break;
                case AnimationParameterType.Float:
                    _networkAnimator.Animator.SetFloat(_parameterName, 1f);
                    break;
                case AnimationParameterType.Int:
                    _networkAnimator.Animator.SetInteger(_parameterName, 1);
                    break;
                case AnimationParameterType.Trigger:
                    _networkAnimator.SetTrigger(_parameterName);
                    break;
            }
            return BTStatus.Success;
        }

        public override BTNode Clone()
        {
            return new SetAnimationParameter();
        }
    }
}