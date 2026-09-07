using System;
using ProjectRaid.Character.Module;

namespace ProjectRaid.Runtime.BehaviourTree
{
    public class AnimationModeEnumValue : Value<MovementModule.AnimationMode>
    {
        public AnimationModeEnumValue() { value = MovementModule.AnimationMode.Normal; }

        public override string ValueString
        {
            get
            {
                return value.ToString();
            }
            set
            {
                SetValue(value);
            }
        }

        public override void SetValue(string value)
        {
            if(CanParse(value, out var animationMode))
            {
                base.value = animationMode;
            }
        }

        public override bool CanParse(string value)
        {
            if(CanParse(value, out _)) return true;
            return false;
        }

        public bool CanParse(string value, out MovementModule.AnimationMode animationMode)
        {
            try
            {
                animationMode = Enum.Parse<MovementModule.AnimationMode>(value);
                return true;
            }
            catch
            {
                animationMode = default;
                return false;
            }
        }
    }
}