using System;

namespace ProjectRaid.Runtime.BehaviourTree
{
    [Serializable]
    public class Int : Value<int>
    {
        public Int() { value = 0; }
        public override void SetValue(string value)
        {
            base.value = int.Parse(value);
        }

        public override bool CanParse(string value)
        {
            return int.TryParse(value, out _);
        }
    }
}