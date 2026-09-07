using FishNet.Component.Animating;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Service
{
    public enum AnimationParameterType
    {
        Bool,
        Float,
        Int,
        Trigger
    }

    public class SetAnimationParameter : BTService
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
            base.OnInit(ctx);
        }

        protected override void Execute(BTContext ctx)
        {
            switch(_parameterType)
            {
                case AnimationParameterType.Bool:
                    _networkAnimator.Animator.SetBool(_parameterName, _boolValue);
                    break;
                case AnimationParameterType.Float:
                    _networkAnimator.Animator.SetFloat(_parameterName, _floatValue);
                    break;
                case AnimationParameterType.Int:
                    _networkAnimator.Animator.SetInteger(_parameterName, _intValue);
                    break;
                case AnimationParameterType.Trigger:
                    _networkAnimator.SetTrigger(_parameterName);
                    break;
            }
        }

        public override BTNode Clone()
        {
            var clone = new SetAnimationParameter
            {
                policy = this.policy,
                interval = this.interval,
                accumulateAcrossRuns = this.accumulateAcrossRuns,
                executeOnEnterForInterval = this.executeOnEnterForInterval,
                _parameterType = this._parameterType,
                _parameterName = this._parameterName,
                _floatValue = this._floatValue,
                _intValue = this._intValue,
                _boolValue = this._boolValue
            };
            clone.SetChild(_child?.Clone());
            return clone;
        }
    }
}