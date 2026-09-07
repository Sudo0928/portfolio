using System;

namespace ProjectRaid.Runtime.BehaviourTree
{
    [Serializable]
    public class Float : Value<float>
    {
        public Float() { value = 0f; }
        public override void SetValue(string value)
        {
            base.value = float.Parse(value);
        }
        public override bool CanParse(string value)
        {
            return float.TryParse(value, out _);
        }
    }
}