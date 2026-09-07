using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree
{
    [CreateAssetMenu(fileName = "New BTBlackBoard", menuName = "BehaviourTree/BTBlackBoard")]
    public sealed class BTBlackBoard : SerializedScriptableObject, IBlackboard
    {
        [ShowInInspector][OdinSerialize] 
        private Dictionary<string, BlackboardHolder> _values = new();

        public event Action<string> OnKeyChanged;

        public bool TryGet<T>(string key, out T value)
        {
            if (_values.TryGetValue(key, out var holder) && holder._blackBoard.TryGet<T>(out var v))
            {
                value = v;
                return true;
            }
            value = default;
            return false;
        }

        // 🔥 신규: 키 존재 여부 빠른 경로
        public bool ContainsKey(string key) => _values.ContainsKey(key);

        public void Add<T>(string key, T value)
        {
            _values[key] = new BlackboardHolder { _blackBoard = new Value<T> { value = value } };
        }

        public void Remove(string key)
        {
            if (_values.Remove(key))
                OnKeyChanged?.Invoke(key);
        }

        // 🔧 동일 값이면 이벤트/쓰기 생략
        public void Set<T>(string key, T value)
        {
            if (_values.TryGetValue(key, out var holder) && holder._blackBoard is Value<T> box)
            {
                if (EqualityComparer<T>.Default.Equals(box.value, value))
                    return; // no-op
                box.value = value;
                OnKeyChanged?.Invoke(key);
                return;
            }

            _values[key] = new BlackboardHolder { _blackBoard = new Value<T> { value = value } };
            OnKeyChanged?.Invoke(key);
        }

        public IReadOnlyList<(string, Type)> GetAllKeysAndTypes()
            => _values.Select(kv => (kv.Key, kv.Value._blackBoard.Type)).ToList();

        public IBlackboard Clone()
        {
            var clone = ScriptableObject.CreateInstance<BTBlackBoard>();
            clone._values = new Dictionary<string, BlackboardHolder>(_values.Count);
            foreach (var kv in _values)
            {
                var holder = kv.Value;
                var copied = new BlackboardHolder
                {
                    _blackBoard = holder?._blackBoard?.Clone()
                };
                clone._values[kv.Key] = copied;
            }
            return clone;
        }
    }
}