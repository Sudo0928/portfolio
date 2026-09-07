using System;

namespace ProjectRaid.Runtime.BehaviourTree
{
    public interface IValue
    {
        Type Type { get; }
        string ValueString { get; set; }
        void SetValue(string value);
        bool CanParse(string value);
        bool TryGet<T>(out T value);
        IValue Clone();
    }
}