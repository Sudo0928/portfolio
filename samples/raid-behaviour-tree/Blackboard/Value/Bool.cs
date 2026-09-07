using System;

namespace ProjectRaid.Runtime.BehaviourTree
{
    [Serializable]
    public class Bool : Value<bool>
    {
        public Bool() { value = false; }
        public override void SetValue(string value)
        {
            base.value = bool.Parse(value);
        }
        public override bool CanParse(string value)
        {
            return bool.TryParse(value, out _);
        }
    }
}