using System;

namespace ProjectRaid.Runtime.BehaviourTree
{
    public interface IBlackboard
    {
        bool TryGet<T>(string key, out T value);
        void Set<T>(string key, T value);
        event Action<string> OnKeyChanged;
        IBlackboard Clone();

        // 🔥 신규: Guard 등 핫패스용
        bool ContainsKey(string key);
    }
}