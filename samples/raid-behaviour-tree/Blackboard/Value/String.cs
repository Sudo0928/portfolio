using System;

namespace ProjectRaid.Runtime.BehaviourTree
{
    [Serializable]
    public class String : Value<string>
    {
        public String() { value = string.Empty; }
        public override void SetValue(string value)
        {
            base.value = value;
        }
        public override bool CanParse(string value)
        {
            return true;
        }
    }
}