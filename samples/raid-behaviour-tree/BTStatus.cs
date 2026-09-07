using System;

namespace ProjectRaid.Runtime.BehaviourTree
{
    [Serializable]
    public enum BTStatus
    {
        Success,
        Failure,
        Running,
    }
}