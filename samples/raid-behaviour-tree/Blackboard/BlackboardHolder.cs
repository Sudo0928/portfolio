using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree
{
    [Serializable]
    public class BlackboardHolder
    {
        [SerializeReference] public IValue _blackBoard;
        [ShowInInspector, ShowIf("@_blackBoard != null")]
        public string Value
        {
            get => _blackBoard?.ValueString;
            set
            {
                if (_blackBoard?.CanParse(value) ?? false) _blackBoard?.SetValue(value);
            }
        }

        public BlackboardHolder Clone()
        {
            return new BlackboardHolder() { _blackBoard = _blackBoard ?? null };
        }
    }
}